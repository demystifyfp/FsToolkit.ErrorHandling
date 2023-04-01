module benchmarks

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes
// open FsToolkit.ErrorHandling

let okF x = x + 2
let errorF x = x - 4
let add x y = x + y

module Result =

    //     let inline (|>>) ([<InlineIfLambda>] v : _ -> _) ([<InlineIfLambda>] f : _ -> _) = f v
    module Normal =

        let either okF errorF x =
            match x with
            | Ok x -> okF x
            | Error err -> errorF err

        let eitherMap okF errorF x =
            either
                (okF
                 >> Result.Ok)
                (errorF
                 >> Result.Error)
                x

        let apply f x =
            Result.bind (fun f' -> Result.bind (f' >> Ok) x) f

        let map2 f x y = (apply (apply (Ok f) x) y)


        module NoComposition =
            let either (okF) (errorF) x =
                match x with
                | Ok x -> okF x
                | Error err -> errorF err

            let eitherMap (okF) (errorF) x =
                either
                    (fun x ->
                        x
                        |> okF
                        |> Result.Ok
                    )
                    (fun y ->
                        y
                        |> errorF
                        |> Result.Error
                    )
                    x

            let apply (f) x =
                Result.bind (fun f' -> Result.bind (fun x -> f' x |> Ok) x) f

            let map2 (f) x y = (apply (apply (Ok f) x) y)

    module Inlined =

        let inline either okF errorF x =
            match x with
            | Ok x -> okF x
            | Error err -> errorF err

        let inline eitherMap okF errorF x =
            either
                (okF
                 >> Result.Ok)
                (errorF
                 >> Result.Error)
                x

        let inline bind f x =
            match x with
            | Ok x -> f x
            | Error e -> Error e

        let inline apply f x = bind (fun f' -> bind (f' >> Ok) x) f
        let inline map2 f x y = (apply (apply (Ok f) x) y)


        module NoComposition =
            let inline either (okF) (errorF) x =
                match x with
                | Ok x -> okF x
                | Error err -> errorF err

            let inline eitherMap (okF) (errorF) x =
                either
                    (fun x ->
                        x
                        |> okF
                        |> Result.Ok
                    )
                    (fun y ->
                        y
                        |> errorF
                        |> Result.Error
                    )
                    x

            let inline bind (f) x =
                match x with
                | Ok x -> f x
                | Error e -> Error e

            let inline apply (f) x =
                bind (fun f' -> bind (fun x -> f' x |> Ok) x) f

            let inline map2 (f) x y = (apply (apply (Ok f) x) y)

    module InlinedLambda =
        let inline either ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
            match x with
            | Ok x -> okF x
            | Error err -> errorF err

        let inline eitherMap ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
            either
                (okF
                 >> Result.Ok)
                (errorF
                 >> Result.Error)
                x

        let inline bind ([<InlineIfLambda>] f) x =
            match x with
            | Ok x -> f x
            | Error e -> Error e

        let inline apply (f) x = bind (fun f' -> bind (f' >> Ok) x) f
        let inline map2 ([<InlineIfLambda>] f) x y = (apply (apply (Ok f) x) y)

        module NoComposition =
            let inline either ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
                match x with
                | Ok x -> okF x
                | Error err -> errorF err

            let inline eitherMap ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
                either
                    (fun x ->
                        x
                        |> okF
                        |> Result.Ok
                    )
                    (fun y ->
                        y
                        |> errorF
                        |> Result.Error
                    )
                    x

            let inline bind ([<InlineIfLambda>] f) x =
                match x with
                | Ok x -> f x
                | Error e -> Error e

            let inline apply (f) x =
                bind (fun f' -> bind (fun x -> f' x |> Ok) x) f

            let inline map2 ([<InlineIfLambda>] f) x y = (apply (apply (Ok f) x) y)

    module Alt =
        let eitherMap okF errorF x =
            match x with
            | Ok x ->
                okF x
                |> Ok
            | Error e ->
                errorF e
                |> Error

        let apply f x =
            match f, x with
            | Ok f, Ok x -> f x |> Ok
            | Error e, _ -> Error e
            | _, Error e -> Error e

        let map2 f x y =
            match x, y with
            | Ok x, Ok y ->
                f x y
                |> Ok
            | Error e as z, _ -> Error e
            | _, Error e -> Error e

        module Inlined =
            let inline eitherMap okF errorF x =
                match x with
                | Ok x ->
                    okF x
                    |> Ok
                | Error e ->
                    errorF e
                    |> Error

            let inline apply f x =
                match f, x with
                | Ok f, Ok x -> f x |> Ok
                | Error e, _ -> Error e
                | _, Error e -> Error e

            let inline map2 f x y =
                match x, y with
                | Ok x, Ok y ->
                    f x y
                    |> Ok
                | Error e as z, _ -> Error e
                | _, Error e -> Error e

        module InlinedLambda =

            let inline eitherMap ([<InlineIfLambda>] okF) ([<InlineIfLambda>] errorF) x =
                match x with
                | Ok x ->
                    okF x
                    |> Ok
                | Error e ->
                    errorF e
                    |> Error

            let inline apply f x =
                match f, x with
                | Ok f, Ok x -> f x |> Ok
                | Error e, _ -> Error e
                | _, Error e -> Error e

            let inline map ([<InlineIfLambda>] mapper: 'ok -> 'ok2) value =
                match value with
                | Ok x -> Ok(mapper x)
                | Error e -> Error e

            let inline bind ([<InlineIfLambda>] binder: 'ok -> Result<'ok2, 'err>) value =
                match value with
                | Ok x -> binder x
                | Error e -> Error e

            let inline map2 ([<InlineIfLambda>] f: 'a -> 'b -> 'c) x y =
                match x, y with
                | Ok x, Ok y ->
                    f x y
                    |> Ok
                | Error e as z, _ -> Error e
                | _, Error e -> Error e

[<MemoryDiagnoser>]
type EitherMapBenchmarks() =

    [<Benchmark(Baseline = true)>]
    member this.Result_Normal_EitherMap() =
        Ok 4
        |> Result.Normal.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_Normal_NoComposition_EitherMap() =
        Ok 4
        |> Result.Normal.NoComposition.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_Inlined_EitherMap() =
        Ok 4
        |> Result.Inlined.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_Inlined_NoComposition_EitherMap() =
        Ok 4
        |> Result.Inlined.NoComposition.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_InlinedLambda_EitherMap() =
        Ok 4
        |> Result.InlinedLambda.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_Normal_InlinedLambda_NoComposition_EitherMap() =
        Ok 4
        |> Result.InlinedLambda.NoComposition.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_Alt_EitherMap() =
        Ok 4
        |> Result.Alt.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_Alt_Inlined_EitherMap() =
        Ok 4
        |> Result.Alt.Inlined.eitherMap okF errorF

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_EitherMap() =
        Ok 4
        |> Result.Alt.InlinedLambda.eitherMap okF errorF


type ResultBuilder() =
    member _.Return(value: 'T) : Result<'T, 'TError> = Ok value

    // member inline _.ReturnFrom(result: Result<'T, 'TError>) : Result<'T, 'TError> = result

    member this.Zero() : Result<unit, 'TError> = this.Return()

    member _.Bind
        (
            result: Result<'T, 'TError>,
            binder: 'T -> Result<'U, 'TError>
        ) : Result<'U, 'TError> =
        Result.bind binder result


type ResultBuilderInlined() =
    member inline _.Return(value: 'T) : Result<'T, 'TError> = Ok value

    // member inline _.ReturnFrom(result: Result<'T, 'TError>) : Result<'T, 'TError> = result

    member inline this.Zero() : Result<unit, 'TError> = this.Return()

    member inline _.Bind
        (
            result: Result<'T, 'TError>,
            binder: 'T -> Result<'U, 'TError>
        ) : Result<'U, 'TError> =
        Result.Inlined.bind binder result

type ResultBuilderInlinedLambda() =
    member inline _.Return(value: 'T) : Result<'T, 'TError> = Ok value

    member inline this.Zero() : Result<unit, 'TError> = this.Return()

    member inline _.Bind
        (
            result: Result<'T, 'TError>,
            [<InlineIfLambda>] binder: 'T -> Result<'U, 'TError>
        ) : Result<'U, 'TError> =
        Result.Alt.InlinedLambda.bind binder result


let result = ResultBuilder()
let resultInlined = ResultBuilderInlined()
let resultInlinedLambda = ResultBuilderInlinedLambda()

[<MemoryDiagnoser>]
type MapBenchmarks() =
    [<Benchmark(Baseline = true)>]
    member this.Result_Normal_Map() =
        Result.map (fun x -> x + 2) (Ok 1): Result<_, int>

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_Map() =
        Result.Alt.InlinedLambda.map (fun x -> x + 2) (Ok 1): Result<_, int>

let runTimes x action =
    let results = ResizeArray<_>()

    for i = 1 to x do
        action ()
        |> results.Add

    results

[<MemoryDiagnoser>]
type BindBenchmarks() =
    [<Benchmark(Baseline = true)>]
    member this.Result_Normal_Bind() =
        Result.bind (fun x -> Ok(x + 2)) (Ok 1): Result<int, int>

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_Bind() =
        Result.Alt.InlinedLambda.bind (fun x -> Ok(x + 2)) (Ok 1): Result<int, int>

// let inline divide x y =
//     if y = LanguagePrimitives.GenericZero then Error "Cannot divide by 0"
//     else Ok (x/y)
// match y with
// | LanguagePrimitives.GenericZero ->
// | _ -> Ok (x/y)
let divide x y =
    match y with
    | 0 -> Error "Cannot divide by 0"
    | y -> Ok(x / y)

[<MemoryDiagnoser>]
type BindCEBenchmarks() =
    [<Benchmark(Baseline = true)>]
    member this.Result_Normal_Bind_CE() =
        let action () : Result<int, string> =
            result {
                let! a = Ok 1
                let! b = Ok 3
                let! c = divide a b
                return c
            }

        action ()

    [<Benchmark>]
    member this.Result_Alt_Inlined_Bind_CE() =
        let action () : Result<int, string> =
            resultInlined {
                let! a = Ok 1
                let! b = Ok 3
                let! c = divide a b
                return c
            }

        action ()

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_Bind_Same_CE() =
        let action () : Result<int, string> =
            resultInlinedLambda {
                let! a = Ok 1
                let! b = Ok 3
                let! c = divide a b
                return c
            }

        action ()

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_Bind_CE() =
        let action () : Result<int, string> =
            resultInlinedLambda {
                let! a = Ok 1
                let! b = Ok 3.0
                let! c = divide a (int b)
                return c
            }

        action ()


    [<Benchmark>]
    member this.Result_Normal_Bind_CE_Error() =
        let action () : Result<int, string> =
            result {
                let! a = Ok 1
                let! b = Ok 0
                let! c = divide a b
                return c
            }

        action ()

    [<Benchmark>]
    member this.Result_Alt_Inlined_Bind_CE_Error() =
        let action () : Result<int, string> =
            resultInlined {
                let! a = Ok 1
                let! b = Ok 0
                let! c = divide a b
                return c
            }

        action ()

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_Bind_Same_CE_Error() =
        let action () : Result<int, string> =
            resultInlinedLambda {
                let! a = Ok 1
                let! b = Result<int, string>.Error ""
                let! c = divide a b
                return c
            }

        action ()

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_Bind_CE_Error() =
        let action () : Result<int, string> =
            resultInlinedLambda {
                let! a = Ok 1
                let! b = Result<float, string>.Error ""
                let! c = divide a (int b)
                return c
            }

        action ()

[<MemoryDiagnoser>]
type Map2Benchmarks() =

    [<Benchmark(Baseline = true)>]
    member this.Result_Normal_Map2() =
        Result.Normal.map2 add (Ok 1) (Ok 2): Result<int, int>

    [<Benchmark>]
    member this.Result_NoComposition_Map2() =
        Result.Normal.NoComposition.map2 add (Ok 1) (Ok 2): Result<int, int>

    [<Benchmark>]
    member this.Result_Inlined_Map2() =
        Result.Inlined.map2 add (Ok 1) (Ok 2): Result<int, int>

    [<Benchmark>]
    member this.Result_Inlined_NoComposition_Map2() =
        Result.Inlined.NoComposition.map2 add (Ok 1) (Ok 2): Result<int, int>

    [<Benchmark>]
    member this.Result_InlinedLambda_Map2() =
        Result.InlinedLambda.map2 add (Ok 1) (Ok 2): Result<int, int>

    [<Benchmark>]
    member this.Result_InlinedLambda_NoComposition_Map2() =
        Result.InlinedLambda.NoComposition.map2 add (Ok 1) (Ok 2): Result<int, int>

    [<Benchmark>]
    member this.Result_Alt_Map2() =
        Result.Alt.map2 add (Ok 1) (Ok 2): Result<int, int>

    [<Benchmark>]
    member this.Result_Alt_Inlined_Map2() =
        Result.Alt.Inlined.map2 add (Ok 1) (Ok 2): Result<int, int>

    [<Benchmark>]
    member this.Result_Alt_InlinedLambda_Map2() =
        Result.Alt.InlinedLambda.map2 add (Ok 1) (Ok 2): Result<int, int>
