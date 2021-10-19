module benchmarks 

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling

module Result =
    module Inlined =  
        let inline bind f x =
            match x with
            | Ok x -> f x
            | Error e -> Error e
        let inline apply f x =
            bind (fun f' ->
                bind (f' >> Ok) x) f
        let inline map2 f x y =
            (apply (apply (Ok f) x) y)

    module Alt = 
        let eitherMap okF errorF x =
            match x with
            | Ok x -> okF x |> Ok
            | Error e -> errorF e |> Error

        let apply f x =
            match f,x with
            | Ok f, Ok x -> f x |> Ok
            | Error e, _ -> Error e
            | _ , Error e -> Error e

        let map2 f x y =
            match x, y with
            | Ok x, Ok y -> f x y |> Ok
            | Error e as z, _ -> Error e
            | _, Error e -> Error e

        module Inlined =        
            let inline eitherMap okF errorF x =
                match x with
                | Ok x -> okF x |> Ok
                | Error e -> errorF e |> Error

            let inline apply f x =
                match f,x with
                | Ok f, Ok x -> f x |> Ok
                | Error e, _ -> Error e
                | _ , Error e -> Error e

            let inline map2 f x y =
                match x, y with
                | Ok x, Ok y -> f x y |> Ok
                | Error e as z, _ -> Error e
                | _, Error e -> Error e

type Benchmarks () =

    let okF x = x + 2
    let errorF x = x - 4

    let add x y = x + y

    let runXTimes (x : int) f =
        let results = ResizeArray<_>(x)
        for i=1 to x do
            f () |> results.Add
        results
            
    // [<Params(0, 1, 15, 100)>]
    // member val public sleepTime = 0 with get, set

    // [<GlobalSetup>]
    // member self.GlobalSetup() =
    //     printfn "%s" "Global Setup"

    // [<GlobalCleanup>]
    // member self.GlobalCleanup() =
    //     printfn "%s" "Global Cleanup"

    // [<IterationSetup>]
    // member self.IterationSetup() =
    //     printfn "%s" "Iteration Setup"
    
    // [<IterationCleanup>]
    // member self.IterationCleanup() =
    //     printfn "%s" "Iteration Cleanup"
    
    // [<Benchmark>]
    // member this.EitherMap () =
    //     Ok 4
    //     |> Result.eitherMap okF errorF 

    // [<Benchmark>]
    // member this.AltEitherMap () = 
    //     Ok 4
    //     |> Result.Alt.eitherMap okF errorF 
    
    // [<Benchmark>]
    // member this.ResultApply () : Result<int,int> =
    //     let okF = Ok okF
    //     Ok 4
    //     |> Result.apply okF  

    // [<Benchmark>]
    // member this.ResultAltApply () : Result<int,int>   = 
    //     let okF = Ok okF
    //     Ok 4
    //     |> Result.Alt.apply okF  

    
    [<Benchmark>]
    member this.ResultMap2 ()  = 
       fun () -> Result.map2 add (Ok 1) (Ok 2) : Result<int,int>
       |> runXTimes 100

    [<Benchmark>]
    member this.ResultAltMap2 () = 
        
       fun () -> Result.Alt.map2 add (Ok 1) (Ok 2) : Result<int,int>
       |> runXTimes 100
    
    [<Benchmark>]
    member this.ResultInlinedMap2 ()  = 
       fun () -> Result.Inlined.map2 add (Ok 1) (Ok 2) : Result<int,int>
       |> runXTimes 100

    [<Benchmark>]
    member this.ResultAltInlinedMap2 () = 
        
       fun () -> Result.Alt.Inlined.map2 add (Ok 1) (Ok 2) : Result<int,int>
       |> runXTimes 100
