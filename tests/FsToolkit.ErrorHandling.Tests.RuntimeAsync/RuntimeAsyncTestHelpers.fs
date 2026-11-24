module RuntimeAsyncTestHelpers

open System
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open System.Runtime.CompilerServices
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

                        method.Module.ResolveMethod(
                            token,
                            genericArguments method.DeclaringType,
                            methodArguments
                        )
                        |> nonNull
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

let callsSuspension (method: MethodBase) =
    directCalls method
    |> List.exists (fun target ->
        target.DeclaringType = typeof<AsyncHelpers>
        && match target.Name with
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

// F# emits the outer computation in the named function (Release) or in a closure named after it (Debug).
let methodGroup (container: Type) (name: string) : MethodBase list = [
    yield!
        methodsOf container
        |> List.filter (fun method -> method.Name = name)
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
