open System
open System.Reflection

type Comparison =
    | Exact of string
    | Contains of string

let inline contains (value) (sequence: ^a) =
    let bool = (^a: (member Contains: 'b -> bool) (sequence, value))
    bool

let inline comparison (value) (comparision) =
    match comparision with
    | Exact compareValue -> value = compareValue
    | Contains compareValue ->
        value
        |> contains compareValue


let getMethodsByModuleName namesToFilter moduleName (types: Type seq) =
    let findWantedModules = Seq.filter (fun (t: Type) -> t.Name = moduleName)
    let getAllMethods = Seq.collect (fun (t: Type) -> t.GetMethods())

    let removeUnwantedMethods =
        Seq.filter (fun (m: MethodInfo) ->
            namesToFilter
            |> Seq.exists (comparison m.Name)
            |> not
        )

    types
    |> findWantedModules
    |> getAllMethods
    |> removeUnwantedMethods

let report moduleName (methods: MethodInfo seq) =
    printfn "---%s---" moduleName

    methods
    |> Seq.iter (fun m ->
        m.Name
        |> printfn "%s"
    )

/// FSharp.Core uses CompiledName attribute to change the case to more C# friendly names,
/// this undoes it
let normalize (name: string) : string =
    String(
        Char.ToLower(name.[0])
        |> Array.singleton
    )
    + name.Substring(1)

type Module = {
    ReflectionLookupName: string
    OutputName: string
} with

    static member Create r c = {
        ReflectionLookupName = r
        OutputName = c
    }

/// List of modules to get info for
let allModules: list<Module> = [
    Module.Create "OptionModule" "Option"
    Module.Create "Option" "Option"
    Module.Create "ValueOption" "ValueOption"
    Module.Create "Async" "Async"
    Module.Create "FSharpAsync" "Async"
    Module.Create "Job" "Job"
    Module.Create "ResultModule" "Result"
    Module.Create "Result" "Result"
    Module.Create "Validation" "Validation"
    Module.Create "AsyncResult" "AsyncResult"
    Module.Create "TaskResult" "TaskResult"
    Module.Create "JobResult" "JobResult"
]

let namespaceNormalizer = [
    Contains "FSharp.Core", "FSharp.Core"
    Contains "FsToolkit", "FsToolkit"
    Contains "Hopac", "Hopac"
    Contains "FSharpAsync", "FSharp.Core"
]

let findInNamespaceNormalizer namespaceNormalizer item =
    namespaceNormalizer
    |> List.tryFind (fun (comparer, _) -> comparison item (comparer))
    |> Option.map (fun (_, normalizedName) -> normalizedName)

let createTableRow method modules =
    let outputModules =
        allModules
        |> List.map (fun m -> m.OutputName)
        |> List.distinct

    let sparseSet =
        Array.create<string>
            (outputModules
             |> List.length)
            ""

    for ((moduleName: string), (mInfo: MethodInfo)) in modules do
        let index =
            outputModules
            |> List.findIndex (fun m -> m = moduleName)

        sparseSet.[index] <- mInfo.DeclaringType.FullName

    let data =
        sparseSet
        |> Array.map (fun b ->
            findInNamespaceNormalizer namespaceNormalizer b
            |> Option.defaultValue b
        )
        |> String.concat "|"

    sprintf "|%s|%s|" method data

let generateTableHeaderSeprator count =
    List.replicate count "---"
    |> String.concat "|"
    |> sprintf "|%s|"

let createMarkdownTable headers headersCount rows =
    printfn "%s" headers

    printfn "%s"
    <| generateTableHeaderSeprator headersCount

    for row in rows do
        printfn "%s" row

let namesToFilter = [
    Exact "GetType"
    Exact "ToString"
    Exact "Equals"
    Exact "GetHashCode"
    Contains "op_"
]

[<EntryPoint>]
let main argv =
    let types = [
        yield! Assembly.Load("FSharp.Core").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling.TaskResult").GetTypes()
        yield! Assembly.Load("Hopac").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling.JobResult").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling.AsyncSeq").GetTypes()
    ]

    let getMethodsByModuleName name =
        getMethodsByModuleName namesToFilter name types
    // types |> Seq.iter(fun f -> f.Name |> printfn "%s")

    let headers, headersCount =
        let methods = "Methods"

        let items =
            methods
            :: (allModules
                |> List.map (fun m -> m.OutputName)
                |> List.distinct)

        let tail =
            items
            |> String.concat "|"

        sprintf "|%s|" tail, items.Length

    allModules
    |> Seq.collect (fun name ->
        getMethodsByModuleName name.ReflectionLookupName
        |> Seq.map (fun m -> name.OutputName, m)
    )
    |> Seq.groupBy (fun (name, method) -> normalize method.Name)
    |> Seq.sortBy (fst)
    |> Seq.map (fun (method, group) ->
        // let moduleNames = group |> Seq.map(fst) |> String.concat ","
        // printfn "%s : %s" method moduleNames
        // let modules = group |> Seq.map(fst)
        createTableRow method (group)
    )
    |> createMarkdownTable headers headersCount

    0
