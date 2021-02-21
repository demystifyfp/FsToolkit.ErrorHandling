
open System
open System.Reflection


let namesToFilter = [
    "GetType"
    "ToString"
    "Equals"
    "GetHashCode"
]

let getMethodsByModuleName moduleName (types : Type seq) =
    types 
    |> Seq.filter(fun t -> t.Name = moduleName)
    |> Seq.collect(fun t -> t.GetMethods())
    |> Seq.filter(fun m -> namesToFilter |> Seq.contains m.Name |> not )


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

[<EntryPoint>]
let main argv =
    let types = [
        yield! Assembly.Load("FSharp.Core").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling.TaskResult").GetTypes()
        yield! Assembly.Load("FsToolkit.ErrorHandling.JobResult").GetTypes()
    ]
    let getMethodsByModuleName name = getMethodsByModuleName name types
    // types |> Seq.iter(fun f -> f.Name |> printfn "%s")
    [
        "ResultModule" //Fsharp.Core 
        "OptionModule" //Fsharp.Core 
        "Option"
        "Result" 
        "Validation" 
        "Async"
        "AsyncResult"
        "TaskResult" 
        "JobResult" 
    ]
    |> Seq.collect(fun name -> getMethodsByModuleName name |> Seq.map(fun m -> name, m))
    |> Seq.groupBy(fun (name, method) -> normalize method.Name)
    |> Seq.sortBy(fst)
    |> Seq.iter(fun (method, group) ->
        let moduleNames = group |> Seq.map(fst) |> String.concat ","
        printfn "%s : %s" method moduleNames
    )
    

    0 