module benchmarks 

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes
// open FsToolkit.ErrorHandling

let okF x = x + 2
let errorF x = x - 4

let add x y = x + y

module Result =
    module Normal =

        let either okF errorF x =
            match x with
            | Ok x -> okF x
            | Error err -> errorF err

        let eitherMap okF errorF x =
            either (okF >> Result.Ok) (errorF >> Result.Error) x

        let apply f x =
            Result.bind (fun f' ->
            Result.bind (f' >> Ok) x) f

        let map2 f x y =
            (apply (apply (Ok f) x) y)


        module NoComposition =
            let either (okF) (errorF) x =
                match x with
                | Ok x -> okF x
                | Error err -> errorF err

            let eitherMap (okF) (errorF) x =
                either (fun x -> x |> okF |> Result.Ok) (fun y -> y |> errorF |> Result.Error) x

            let bind (f) x =
                match x with
                | Ok x -> f x
                | Error e -> Error e
            let apply (f) x =
                bind (fun f' ->
                    bind (fun x -> f' x |> Ok) x) f
            let map2 (f) x y =
                (apply (apply (Ok f) x) y)

    module Inlined =  

        let inline either okF errorF x =
            match x with
            | Ok x -> okF x
            | Error err -> errorF err

        let inline eitherMap okF errorF x =
            either (okF >> Result.Ok) (errorF >> Result.Error) x

        let inline bind f x =
            match x with
            | Ok x -> f x
            | Error e -> Error e
        let inline apply f x =
            bind (fun f' ->
                bind (f' >> Ok) x) f
        let inline map2 f x y =
            (apply (apply (Ok f) x) y)


        module NoComposition =
            let inline either (okF) (errorF) x =
                match x with
                | Ok x -> okF x
                | Error err -> errorF err

            let inline eitherMap (okF) (errorF) x =
                either (fun x -> x |> okF |> Result.Ok) (fun y -> y |> errorF |> Result.Error) x

            let inline bind (f) x =
                match x with
                | Ok x -> f x
                | Error e -> Error e
            let inline apply (f) x =
                bind (fun f' ->
                    bind (fun x -> f' x |> Ok) x) f
            let inline map2 (f) x y =
                (apply (apply (Ok f) x) y)

    module InlinedLambda =
        let inline either ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
            match x with
            | Ok x -> okF x
            | Error err -> errorF err

        let inline eitherMap ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
            either (okF >> Result.Ok) (errorF >> Result.Error) x

        let inline bind ([<InlineIfLambda>] f) x =
            match x with
            | Ok x -> f x
            | Error e -> Error e
        let inline apply (f) x =
            bind (fun f' ->
                bind (f' >> Ok) x) f
        let inline map2 ([<InlineIfLambda>] f) x y =
            (apply (apply (Ok f) x) y)

        module NoComposition =
            let inline either ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
                match x with
                | Ok x -> okF x
                | Error err -> errorF err

            let inline eitherMap ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
                either (fun x -> x |> okF |> Result.Ok) (fun y -> y |> errorF |> Result.Error) x

            let inline bind ([<InlineIfLambda>] f) x =
                match x with
                | Ok x -> f x
                | Error e -> Error e
            let inline apply (f) x =
                bind (fun f' ->
                    bind (fun x -> f' x |> Ok) x) f
            let inline map2 ([<InlineIfLambda>] f) x y =
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
        module InlinedLambda =

            let inline eitherMap ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF)  x =
                match x with
                | Ok x -> okF x |> Ok
                | Error e -> errorF e |> Error

            let inline apply f x =
                match f,x with
                | Ok f, Ok x -> f x |> Ok
                | Error e, _ -> Error e
                | _ , Error e -> Error e

            let inline map2 ([<InlineIfLambda>] f) x y =
                match x, y with
                | Ok x, Ok y -> f x y |> Ok
                | Error e as z, _ -> Error e
                | _, Error e -> Error e

[<MemoryDiagnoser>]
type EitherMapBenchmarks () =
    
    [<Benchmark(Baseline = true)>]
    member this.Result_Normal_EitherMap () =
        Ok 4
        |> Result.Normal.eitherMap okF errorF 
    [<Benchmark>]
    member this.Result_Normal_NoComposition_EitherMap () =
        Ok 4
        |> Result.Normal.NoComposition.eitherMap okF errorF 
        
    [<Benchmark>]
    member this.Result_Inlined_EitherMap () =
        Ok 4
        |> Result.Inlined.eitherMap okF errorF 
    [<Benchmark>]
    member this.Result_Inlined_NoComposition_EitherMap () =
        Ok 4
        |> Result.Inlined.NoComposition.eitherMap okF errorF 

    [<Benchmark>]
    member this.Result_InlinedLambda_EitherMap () =
        Ok 4
        |> Result.InlinedLambda.eitherMap okF errorF 
    [<Benchmark>]
    member this.Result_Normal_InlinedLambda_NoComposition_EitherMap () = 
        Ok 4
        |> Result.InlinedLambda.NoComposition.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_Alt_EitherMap () = 
        Ok 4
        |> Result.Alt.eitherMap okF errorF 
    [<Benchmark>]
    member this.Result_Alt_Inlined_EitherMap () = 
        Ok 4
        |> Result.Alt.Inlined.eitherMap okF errorF 
    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_EitherMap () = 
        Ok 4
        |> Result.Alt.InlinedLambda.eitherMap okF errorF 


[<MemoryDiagnoser>]
type Map2Benchmarks () =
    
    [<Benchmark(Baseline = true)>]
    member this.Result_Normal_Map2 ()  = 
        Result.Normal.map2 add (Ok 1) (Ok 2) : Result<int,int>
    [<Benchmark>]
    member this.Result_NoComposition_Map2 ()  = 
        Result.Normal.NoComposition.map2 add (Ok 1) (Ok 2) : Result<int,int>

    [<Benchmark>]
    member this.Result_Inlined_Map2 ()  = 
        Result.Inlined.map2 add (Ok 1) (Ok 2) : Result<int,int>
    [<Benchmark>]
    member this.Result_Inlined_NoComposition_Map2 ()  = 
        Result.Inlined.NoComposition.map2 add (Ok 1) (Ok 2) : Result<int,int>

    [<Benchmark>]
    member this.Result_InlinedLambda_Map2 ()  = 
        Result.InlinedLambda.map2 add (Ok 1) (Ok 2) : Result<int,int>
    [<Benchmark>]
    member this.Result_InlinedLambda_NoComposition_Map2 ()  = 
        Result.InlinedLambda.NoComposition.map2 add (Ok 1) (Ok 2) : Result<int,int>

    [<Benchmark>]
    member this.Result_Alt_Map2 () = 
        
        Result.Alt.map2 add (Ok 1) (Ok 2) : Result<int,int>
    [<Benchmark>]
    member this.Result_Alt_Inlined_Map2 () = 
        
        Result.Alt.Inlined.map2 add (Ok 1) (Ok 2) : Result<int,int>

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_Map2 () =         
        Result.Alt.InlinedLambda.map2 add (Ok 1) (Ok 2) : Result<int,int>
        
    [<Benchmark>]
    member this.Result_Alt_InlinedLambda2_Map2 () =         
        Result.Alt.InlinedLambda.map2 (fun x y -> x + y) (Ok 1) (Ok 2) : Result<int,int>
