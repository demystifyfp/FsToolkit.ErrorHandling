
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

/// List of modules to get info for
let allModules =
    [
        "ResultModule" //Fsharp.Core 
        "OptionModule" //Fsharp.Core
        "Option"
        "ValueOption" 
        "Result" 
        "Validation" 
        "Async"
        "Job"
        "AsyncResult"
        "TaskResult" 
        "JobResult" 
    ]

let createCsvRow method modules =
    let sparseSet = Array.create<string> (allModules.Length) ""
    for (moduleName, (mInfo : MethodInfo)) in modules do
        let index = allModules |> List.findIndex (fun m -> m = moduleName)
        sparseSet.[index] <- mInfo.DeclaringType.FullName
    let data = 
        sparseSet 
        |> Array.map (fun b ->
            if b |> contains "FSharp.Core" then "FSharp.Core"
            elif b |> contains "FsToolkit" then "FsToolkit"
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
        // yield! Assembly.Load("Hopac").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling.JobResult").GetTypes()
    ]
    let getMethodsByModuleName name = getMethodsByModuleName name types
    // types |> Seq.iter(fun f -> f.Name |> printfn "%s")

    let headers = 
        let methods = "Methods"
        let tail = allModules |> String.concat ","
        sprintf "%s,%s" methods tail

    allModules
    |> Seq.collect(fun name -> getMethodsByModuleName name |> Seq.map(fun m -> name, m))
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