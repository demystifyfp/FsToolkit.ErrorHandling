module RuntimeAsyncTestHelpers

#nowarn "9"

open System
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open System.Runtime.CompilerServices
open System.Reflection.Metadata
open System.Reflection.Metadata.Ecma335
open System.Threading
open System.Threading.Tasks
open Expecto

let deadline = TimeSpan.FromSeconds 30.

let newGate () =
    TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

let after (gate: Task<unit>) (value: 'T) : Task<'T> =
    gate.ContinueWith(
        (fun (_: Task<unit>) -> value),
        CancellationToken.None,
        TaskContinuationOptions.ExecuteSynchronously,
        TaskScheduler.Default
    )

let faultAfter (gate: Task<unit>) (error: exn) : Task<'T> =
    gate.ContinueWith(
        (fun (_: Task<unit>) -> raise error),
        CancellationToken.None,
        TaskContinuationOptions.ExecuteSynchronously,
        TaskScheduler.Default
    )

let isRuntimeAsync (method: MethodBase) =
    method.MethodImplementationFlags.HasFlag MethodImplAttributes.Async

let isStateMachine (t: Type) =
    typeof<IAsyncStateMachine>.IsAssignableFrom t

let rec nestedTypes (t: Type) : Type list = [
    for nested in
        t.GetNestedTypes(
            BindingFlags.Public
            ||| BindingFlags.NonPublic
        ) do
        nested
        yield! nestedTypes nested
]

let private declaredMembers =
    BindingFlags.Public
    ||| BindingFlags.NonPublic
    ||| BindingFlags.Instance
    ||| BindingFlags.Static
    ||| BindingFlags.DeclaredOnly

let methodsOf (t: Type) : MethodBase list = [
    yield!
        t.GetMethods declaredMembers
        |> Seq.cast<MethodBase>
    yield!
        t.GetConstructors declaredMembers
        |> Seq.cast<MethodBase>
]

let private opCodes =
    typeof<OpCodes>
        .GetFields(
            BindingFlags.Public
            ||| BindingFlags.Static
        )
    |> Array.map (fun field -> field.GetValue null :?> OpCode)
    |> Array.map (fun op -> uint16 op.Value, op)
    |> dict

let private operandSize (op: OpCode) (il: byte array) (offset: int) =
    match op.OperandType with
    | OperandType.InlineNone -> 0
    | OperandType.ShortInlineBrTarget
    | OperandType.ShortInlineI
    | OperandType.ShortInlineVar -> 1
    | OperandType.InlineVar -> 2
    | OperandType.InlineI8
    | OperandType.InlineR -> 8
    | OperandType.InlineSwitch ->
        4
        + 4
          * BitConverter.ToInt32(il, offset)
    | _ -> 4

let private genericArguments (t: Type | null) : Type array | null =
    match t with
    | null -> null
    | t when t.IsGenericType -> t.GetGenericArguments()
    | _ -> null

// A reference can name an instantiation that the generic context of its caller does not satisfy.
let private resolveInContext
    (method: MethodBase)
    (token: int)
    (methodArguments: Type array | null)
    : MemberInfo =
    try
        method.Module.ResolveMember(token, genericArguments method.DeclaringType, methodArguments)
        |> nonNull
    with
    | :? ArgumentException
    | :? TypeLoadException ->
        method.Module.ResolveMember token
        |> nonNull

let directCalls (method: MethodBase) : MethodBase list =
    match method.GetMethodBody() with
    | null -> []
    | body ->
        let il = nonNull (body.GetILAsByteArray())

        let methodArguments: Type array | null =
            if method.IsGenericMethod then
                method.GetGenericArguments()
            else
                null

        let rec decode offset calls =
            if
                offset
                >= il.Length
            then
                List.rev calls
            else
                let code, operand =
                    if il[offset] = 0xFEuy then
                        0xFE00us
                        ||| uint16 il[offset + 1],
                        offset + 2
                    else
                        uint16 il[offset], offset + 1

                let op = opCodes[code]

                let calls =
                    if
                        op = OpCodes.Call
                        || op = OpCodes.Callvirt
                    then
                        let token = BitConverter.ToInt32(il, operand)

                        resolveInContext method token methodArguments :?> MethodBase
                        |> fun target ->
                            target
                            :: calls
                    else
                        calls

                decode
                    (operand
                     + operandSize op il operand)
                    calls

        decode 0 []

let private metadataReaders =
    Collections.Concurrent.ConcurrentDictionary<Module, System.Reflection.Metadata.MetadataReader>()

let private metadataOf (m: Module) =
    metadataReaders.GetOrAdd(
        m,
        fun m ->
            let mutable blob = NativeInterop.NativePtr.nullPtr<byte>
            let mutable length = 0

            if
                not (
                    System.Reflection.Metadata.AssemblyExtensions.TryGetRawMetadata(
                        m.Assembly,
                        &blob,
                        &length
                    )
                )
            then
                failwith $"No metadata for %s{m.Name}"

            System.Reflection.Metadata.MetadataReader(blob, length)
    )


let rec private typeHandleName (reader: MetadataReader) (handle: EntityHandle) =
    match handle.Kind with
    | HandleKind.TypeReference ->
        let t = reader.GetTypeReference(TypeReferenceHandle.op_Explicit handle)
        reader.GetString t.Namespace, reader.GetString t.Name
    | HandleKind.TypeDefinition ->
        let t = reader.GetTypeDefinition(TypeDefinitionHandle.op_Explicit handle)
        reader.GetString t.Namespace, reader.GetString t.Name
    | _ -> "", ""

let rec private methodTargetName (reader: MetadataReader) (handle: EntityHandle) =
    match handle.Kind with
    | HandleKind.MethodSpecification ->
        let spec =
            reader.GetMethodSpecification(MethodSpecificationHandle.op_Explicit handle)

        methodTargetName reader spec.Method
    | HandleKind.MemberReference ->
        let r = reader.GetMemberReference(MemberReferenceHandle.op_Explicit handle)
        typeHandleName reader r.Parent, reader.GetString r.Name
    | HandleKind.MethodDefinition ->
        let d = reader.GetMethodDefinition(MethodDefinitionHandle.op_Explicit handle)

        typeHandleName reader (TypeDefinitionHandle.op_Implicit (d.GetDeclaringType())),
        reader.GetString d.Name
    | _ -> ("", ""), ""

/// Reads the call targets from metadata, because reflection cannot resolve some instantiations without context.
let callTargetNames (method: MethodBase) : ((string * string) * string) list =
    match method.GetMethodBody() with
    | null -> []
    | body ->
        let il = nonNull (body.GetILAsByteArray())
        let reader = metadataOf method.Module

        let rec decode offset names =
            if
                offset
                >= il.Length
            then
                List.rev names
            else
                let code, operand =
                    if il[offset] = 0xFEuy then
                        0xFE00us
                        ||| uint16 il[offset + 1],
                        offset + 2
                    else
                        uint16 il[offset], offset + 1

                let op = opCodes[code]

                let names =
                    if
                        op = OpCodes.Call
                        || op = OpCodes.Callvirt
                    then
                        let token = BitConverter.ToInt32(il, operand)

                        methodTargetName reader (MetadataTokens.EntityHandle token)
                        :: names
                    else
                        names

                decode
                    (operand
                     + operandSize op il operand)
                    names

        decode 0 []

let callsSuspension (method: MethodBase) =
    callTargetNames method
    |> List.exists (fun ((ns, typeName), name) ->
        ns = "System.Runtime.CompilerServices"
        && typeName = "AsyncHelpers"
        && match name with
           | "Await"
           | "AwaitAwaiter"
           | "UnsafeAwaitAwaiter" -> true
           | _ -> false
    )

let suspensionCallers (types: Type seq) = [
    for t in types do
        for method in methodsOf t do
            if callsSuspension method then
                method
]

let describe (method: MethodBase) =
    $"%s{(nonNull method.DeclaringType).FullName}::%s{method.Name}"

// F# emits the outer computation in the named function or its name$cont continuation (Release), or in a closure named after it (Debug).
let methodGroup (container: Type) (name: string) : MethodBase list = [
    yield!
        methodsOf container
        |> List.filter (fun method ->
            method.Name = name
            || method.Name.StartsWith(
                name
                + "$cont"
            )
        )
    for t in nestedTypes container do
        if t.Name.StartsWith(name + "@") then
            yield! methodsOf t
]

let runGated (start: Task<unit> -> Task<'T>) : Task<'T> =
    task {
        let gate = newGate ()
        let running = start gate.Task

        Expect.isFalse
            running.IsCompleted
            "The computation must suspend while its source is pending"

        gate.SetResult(())
        return! running.WaitAsync(deadline)
    }

type Probe = {
    Name: string
    Method: string
    Expected: int
    Start: Task<unit> -> Task<Result<int, string>>
}

type Log() =
    let entries = ResizeArray<string>()

    member _.Add(entry: string) =
        lock entries (fun () -> entries.Add entry)

    member _.Entries = lock entries (fun () -> List.ofSeq entries)

let disposable (log: Log) name =
    { new IDisposable with
        member _.Dispose() = log.Add $"dispose %s{name}"
    }

let trackedSeq (log: Log) (items: 'T list) : seq<'T> =
    { new IEnumerable<'T> with
        member _.GetEnumerator() : IEnumerator<'T> =
            let inner = (items :> seq<'T>).GetEnumerator()

            { new IEnumerator<'T> with
                member _.Current = inner.Current
              interface Collections.IEnumerator with
                  member _.Current = box inner.Current
                  member _.MoveNext() = inner.MoveNext()
                  member _.Reset() = inner.Reset()
              interface IDisposable with
                  member _.Dispose() =
                      log.Add "dispose enumerator"
                      inner.Dispose()
            }
      interface Collections.IEnumerable with
          member this.GetEnumerator() =
              ((this :?> IEnumerable<'T>).GetEnumerator()) :> Collections.IEnumerator
    }

let gatedAsyncSeq (log: Log) (gate: Task<unit>) (items: 'T list) : IAsyncEnumerable<'T> =
    let arr = List.toArray items

    { new IAsyncEnumerable<'T> with
        member _.GetAsyncEnumerator(_) =
            let mutable index = -1

            { new IAsyncEnumerator<'T> with
                member _.MoveNextAsync() =
                    ValueTask<bool>(
                        gate.ContinueWith(
                            (fun (_: Task<unit>) ->
                                index <- index + 1
                                index < arr.Length
                            ),
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default
                        )
                    )

                member _.Current = arr.[index]

                member _.DisposeAsync() =
                    log.Add "dispose async enumerator"
                    ValueTask()
            }
    }

let observe (running: Task<'T>) : Task<Choice<'T, exn>> =
    task {
        try
            let! value = running.WaitAsync(deadline)
            return Choice1Of2 value
        with e ->
            return Choice2Of2 e
    }

let private referenceOperand (op: OpCode) =
    match op.OperandType with
    | OperandType.InlineMethod
    | OperandType.InlineField
    | OperandType.InlineType
    | OperandType.InlineTok -> true
    | _ -> false

let referencedMembers (method: MethodBase) : MemberInfo list =
    match method.GetMethodBody() with
    | null -> []
    | body ->
        let il = nonNull (body.GetILAsByteArray())

        let methodArguments: Type array | null =
            if method.IsGenericMethod then
                method.GetGenericArguments()
            else
                null

        let rec decode offset members =
            if
                offset
                >= il.Length
            then
                List.rev members
            else
                let code, operand =
                    if il[offset] = 0xFEuy then
                        0xFE00us
                        ||| uint16 il[offset + 1],
                        offset + 2
                    else
                        uint16 il[offset], offset + 1

                let op = opCodes[code]

                let members =
                    if referenceOperand op then
                        let token = BitConverter.ToInt32(il, operand)

                        resolveInContext method token methodArguments
                        :: members
                    else
                        members

                decode
                    (operand
                     + operandSize op il operand)
                    members

        decode 0 []

let rec private typeAndArguments (t: Type) : Type list = [
    if t.HasElementType then
        yield! typeAndArguments (nonNull (t.GetElementType()))
    else
        t

        if t.IsGenericType then
            for argument in t.GetGenericArguments() do
                yield! typeAndArguments argument
]

let touchedTypes (target: MemberInfo) : Type list = [
    match target with
    | :? Type as t -> yield! typeAndArguments t
    | :? FieldInfo as f ->
        yield! typeAndArguments (nonNull f.DeclaringType)
        yield! typeAndArguments f.FieldType
    | :? MethodBase as m ->
        yield! typeAndArguments (nonNull m.DeclaringType)

        for p in m.GetParameters() do
            yield! typeAndArguments p.ParameterType

        match m with
        | :? MethodInfo as mi ->
            yield! typeAndArguments mi.ReturnType

            if mi.IsGenericMethod then
                for argument in mi.GetGenericArguments() do
                    yield! typeAndArguments argument
        | _ -> ()
    | other ->
        match other.DeclaringType with
        | null -> ()
        | t -> yield! typeAndArguments t
]

let private legacyName =
    Text.RegularExpressions.Regex
        @"^(Background)?(Cancellable)?(Value)?Task(Result|Option|ValueOption|Validation|ResultOption)\w*(Builder|BuilderBase|StateMachineData)$"

let private legacyAssemblies =
    set [
        "FsToolkit.ErrorHandling"
        "FsToolkit.ErrorHandling.IcedTasks"
    ]

/// A legacy module can hold interop extensions for other builders, so only the builder and state types count.
let isLegacyBuilderType (t: Type) =
    let t = if t.IsGenericType then t.GetGenericTypeDefinition() else t

    legacyAssemblies.Contains(
        t.Assembly.GetName().Name
        |> nonNull
    )
    && (t.Namespace = "FsToolkit.ErrorHandling")
    && legacyName.IsMatch(t.Name.Split('`').[0])

let private fieldsOf (t: Type) =
    t.GetFields declaredMembers
    |> List.ofArray

/// A member in the given types that references a legacy builder type, with the method when the reference is in code.
type LegacyReference = {
    Owner: Type
    Method: MethodBase option
    Description: string
}

let legacyReferenceSites (types: Type seq) : LegacyReference list = [
    for t in types do
        for f in fieldsOf t do
            if
                typeAndArguments f.FieldType
                |> List.exists isLegacyBuilderType
            then
                {
                    Owner = t
                    Method = None
                    Description = $"%s{t.FullName}::%s{f.Name} : %s{f.FieldType.Name}"
                }

        for method in methodsOf t do
            let legacy =
                referencedMembers method
                |> List.filter (fun target ->
                    not (target :? Type)
                    && touchedTypes target
                       |> List.exists isLegacyBuilderType
                )

            for target in
                legacy
                |> List.distinct do
                let owner =
                    match target.DeclaringType with
                    | null -> ""
                    | d ->
                        d.Name
                        + "::"

                {
                    Owner = t
                    Method = Some method
                    Description = $"%s{describe method} -> %s{owner}%s{target.Name}"
                }
]

let legacyReferences (types: Type seq) : string list =
    legacyReferenceSites types
    |> List.map (fun site -> site.Description)

let private pdbReaders =
    Collections.Concurrent.ConcurrentDictionary<Assembly, MetadataReader>()

let private pdbOf (assembly: Assembly) =
    pdbReaders.GetOrAdd(
        assembly,
        fun assembly ->
            let stream = IO.File.OpenRead(IO.Path.ChangeExtension(assembly.Location, ".pdb"))
            MetadataReaderProvider.FromPortablePdbStream(stream).GetMetadataReader()
    )

/// The visible sequence points of a method, as document checksum and start line.
let sourceLines (method: MethodBase) : (string * int) list =
    let reader = pdbOf method.Module.Assembly

    let info =
        reader.GetMethodDebugInformation(MetadataTokens.MethodDefinitionHandle method.MetadataToken)

    [
        for point in info.GetSequencePoints() do
            if not point.IsHidden then
                let document = reader.GetDocument(point.Document)
                Convert.ToHexString(reader.GetBlobBytes(document.Hash)), point.StartLine
    ]
    |> List.distinct

let private closureLine = Text.RegularExpressions.Regex @"@(\d+)"

/// The source lines of a reference site. A generated closure without sequence points takes the line in its type name and the document of its module.
let siteLines (site: LegacyReference) : (string * int) list =
    let visible =
        match site.Method with
        | Some method -> sourceLines method
        | None -> []

    if not visible.IsEmpty then
        visible
    else
        let rec moduleOf (t: Type) =
            match t.DeclaringType with
            | null -> t
            | d when t.Name.Contains "@" -> moduleOf d
            | _ -> t

        let documents =
            methodsOf (moduleOf site.Owner)
            |> List.collect sourceLines
            |> List.map fst
            |> List.distinct

        match documents, closureLine.Match site.Owner.Name with
        | [ document ], m when m.Success -> [ document, int m.Groups[1].Value ]
        | _ -> []

let moduleTypes (moduleType: Type) =
    moduleType
    :: nestedTypes moduleType

let moduleOf (marker: Type) = nonNull marker.DeclaringType

type BuilderProbe = {
    Builder: string
    Value: obj
    Method: string
    Background: bool
    Expected: string
    Start: Task<unit> -> bool ref -> Task<string>
}

/// Keeps a canceled outcome canceled with its token, so tests can read the status of the rendered task.
let renderTask (running: Task<'T>) : Task<string> =
    running
        .ContinueWith(
            (fun (t: Task<'T>) ->
                try
                    Task.FromResult(sprintf "%A" (t.GetAwaiter().GetResult()))
                with
                | :? System.OperationCanceledException as e when
                    t.IsCanceled
                    && e.CancellationToken.IsCancellationRequested
                    ->
                    Task.FromCanceled<string> e.CancellationToken
                | e -> Task.FromException<string> e
            ),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        )
        .Unwrap()
