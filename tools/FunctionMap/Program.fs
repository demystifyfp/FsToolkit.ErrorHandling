
open System
open System.Reflection

type Comparison =
| Exact 
| Contains

let inline contains (value) (sequence : ^a)   =
    let bool = (^a : (member Contains : 'b -> bool) (sequence, value))
    bool

let inline comparison (value) (comparision, compareValue )  =
    match comparision with
    | Exact -> value = compareValue
    | Contains -> value |> contains compareValue      

let namesToFilter = [
    Exact, "GetType"
    Exact, "ToString"
    Exact, "Equals"
    Exact, "GetHashCode"
    Contains, "op_"
]

let getMethodsByModuleName moduleName (types : Type seq) =
    types 
    |> Seq.filter(fun t -> t.Name = moduleName)
    |> Seq.collect(fun t -> t.GetMethods())
    |> Seq.filter(fun m -> namesToFilter |> Seq.exists(comparison m.Name) |> not )


let report moduleName (methods : MethodInfo seq) = 
    printfn "---%s---" moduleName 
    methods
    |> Seq.iter(fun m ->
        m.Name |> printfn "%s"
    )

/// FSharp.Core uses CompiledName attribute to change the case to more C# friendly names,
/// this undoes it
let normalize (name : string) : string =
    String(Char.ToLower(name.[0]) |> Array.singleton) + name.Substring(1)

type Module = {
    ReflectionLookupName : string
    CSVOutputName : string
}
    with 
        static member Create r c = 
            {
                ReflectionLookupName = r
                CSVOutputName = c
            }

/// List of modules to get info for
let allModules : list<Module>=
    [
        Module.Create "OptionModule" "Option" 
        Module.Create "Option" "Option" 
        Module.Create "ValueOption" "ValueOption"
        Module.Create "Validation"  "Validation"
        Module.Create "Async" "Async"
        Module.Create "FSharpAsync" "Async"
        Module.Create "Job" "Job"
        Module.Create "ResultModule" "Result" 
        Module.Create "Result" "Result" 
        Module.Create "AsyncResult" "AsyncResult"
        Module.Create "TaskResult"  "TaskResult"
        Module.Create "JobResult"  "JobResult"
    ]

let createCsvRow method modules =
    let outputModules = allModules |> List.map(fun m -> m.CSVOutputName) |> List.distinct
    let sparseSet = Array.create<string> (outputModules |> List.length) ""
    for ((moduleName : string), (mInfo : MethodInfo)) in modules do
        let index = outputModules |> List.findIndex (fun m -> m = moduleName)
        sparseSet.[index] <- mInfo.DeclaringType.FullName
    let data = 
        sparseSet 
        |> Array.map (fun b ->
            if b |> contains "FSharp.Core" then "FSharp.Core"
            elif b |> contains "FsToolkit" then "FsToolkit"
            elif b |> contains "Hopac" then "Hopac"
            elif b |> contains "FSharpAsync" then "FSharp.Core"
            else b
        ) 
        |> String.concat ","
    sprintf "%s,%s" method data

let createCsv headers rows =
    printfn "%s" headers
    for row in rows do
        printfn "%s" row

[<EntryPoint>]
let main argv =
    let types = [
        yield! Assembly.Load("FSharp.Core").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling.TaskResult").GetTypes()
        yield! Assembly.Load("Hopac").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling.JobResult").GetTypes()
    ]
    let getMethodsByModuleName name = getMethodsByModuleName name types
    // types |> Seq.iter(fun f -> f.Name |> printfn "%s")

    let headers = 
        let methods = "Methods"
        let tail = allModules |> Seq.map(fun m -> m.CSVOutputName) |> Seq.distinct |> String.concat ","
        sprintf "%s,%s" methods tail

    allModules
    |> Seq.collect(fun name -> getMethodsByModuleName name.ReflectionLookupName |> Seq.map(fun m -> name.CSVOutputName, m))
    |> Seq.groupBy(fun (name, method) -> normalize method.Name)
    |> Seq.sortBy(fst)
    |> Seq.map(fun (method, group) ->
        // let moduleNames = group |> Seq.map(fst) |> String.concat ","
        // printfn "%s : %s" method moduleNames
        // let modules = group |> Seq.map(fst)
        createCsvRow method (group)
    )
    |> createCsv headers
    

    0 