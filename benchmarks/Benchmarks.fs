module benchmarks

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open Hopac

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
        (result: Result<'T, 'TError>, binder: 'T -> Result<'U, 'TError>)
        : Result<'U, 'TError> =
        Result.bind binder result


type ResultBuilderInlined() =
    member inline _.Return(value: 'T) : Result<'T, 'TError> = Ok value

    // member inline _.ReturnFrom(result: Result<'T, 'TError>) : Result<'T, 'TError> = result

    member inline this.Zero() : Result<unit, 'TError> = this.Return()

    member inline _.Bind
        (result: Result<'T, 'TError>, binder: 'T -> Result<'U, 'TError>)
        : Result<'U, 'TError> =
        Result.Inlined.bind binder result

type ResultBuilderInlinedLambda() =
    member inline _.Return(value: 'T) : Result<'T, 'TError> = Ok value

    member inline this.Zero() : Result<unit, 'TError> = this.Return()

    member inline _.Bind
        (result: Result<'T, 'TError>, [<InlineIfLambda>] binder: 'T -> Result<'U, 'TError>)
        : Result<'U, 'TError> =
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

module TaskCandidates =

    let inline directMap2 ([<InlineIfLambda>] f) (x: Task<_>) (y: Task<_>) =
        task {
            let! x' = x
            let! y' = y
            return f x' y'
        }

    let inline directMap3 ([<InlineIfLambda>] f) (x: Task<_>) (y: Task<_>) (z: Task<_>) =
        task {
            let! x' = x
            let! y' = y
            let! z' = z
            return f x' y' z'
        }

[<MemoryDiagnoser>]
type TaskMap2Benchmarks() =
    let x = Task.FromResult 1
    let y = Task.FromResult 2

    [<Benchmark(Baseline = true)>]
    member _.Task_Current_Map2() =
        FsToolkit.ErrorHandling.Task.map2 (fun x y -> x + y) x y

    [<Benchmark>]
    member _.Task_Direct_Map2() =
        TaskCandidates.directMap2 (fun x y -> x + y) x y

[<MemoryDiagnoser>]
type TaskMap3Benchmarks() =
    let x = Task.FromResult 1
    let y = Task.FromResult 2
    let z = Task.FromResult 3

    [<Benchmark(Baseline = true)>]
    member _.Task_Current_Map3() =
        FsToolkit.ErrorHandling.Task.map3 (fun x y z -> x + y + z) x y z

    [<Benchmark>]
    member _.Task_Direct_Map3() =
        TaskCandidates.directMap3 (fun x y z -> x + y + z) x y z

module ArrayCandidates =

    let inline traverseResultM ([<InlineIfLambda>] f: 'a -> Result<'b, 'e>) (xs: 'a[]) =
        let results = ResizeArray<'b>(xs.Length)
        let mutable index = 0
        let mutable error = Unchecked.defaultof<'e>
        let mutable ok = true

        while ok
              && index < xs.Length do
            match f xs[index] with
            | Ok value ->
                results.Add value
                index <- index + 1
            | Error e ->
                error <- e
                ok <- false

        if ok then Ok(results.ToArray()) else Error error

    let inline traverseResultA ([<InlineIfLambda>] f: 'a -> Result<'b, 'e>) (xs: 'a[]) =
        let results = ResizeArray<'b>(xs.Length)
        let errors = ResizeArray<'e>()
        let mutable ok = true

        for x in xs do
            match f x with
            | Ok value when ok -> results.Add value
            | Ok _ -> ()
            | Error e ->
                errors.Add e
                ok <- false

        if ok then
            Ok(results.ToArray())
        else
            Error(errors.ToArray())

    let inline traverseOptionM ([<InlineIfLambda>] f: 'a -> 'b option) (xs: 'a[]) =
        let results = ResizeArray<'b>(xs.Length)
        let mutable index = 0
        let mutable ok = true

        while ok
              && index < xs.Length do
            match f xs[index] with
            | Some value ->
                results.Add value
                index <- index + 1
            | None -> ok <- false

        if ok then Some(results.ToArray()) else None

    let inline traverseValidationA ([<InlineIfLambda>] f: 'a -> Result<'b, 'e[]>) (xs: 'a[]) =
        let results = ResizeArray<'b>(xs.Length)
        let errors = ResizeArray<'e>()
        let mutable ok = true

        for x in xs do
            match f x with
            | Ok value when ok -> results.Add value
            | Ok _ -> ()
            | Error errs ->
                errors.AddRange errs
                ok <- false

        if ok then
            Ok(results.ToArray())
        else
            Error(errors.ToArray())

    let inline traverseVOptionM ([<InlineIfLambda>] f: 'a -> 'b voption) (xs: 'a[]) =
        let results = ResizeArray<'b>(xs.Length)
        let mutable index = 0
        let mutable ok = true

        while ok
              && index < xs.Length do
            match f xs[index] with
            | ValueSome value ->
                results.Add value
                index <- index + 1
            | ValueNone -> ok <- false

        if ok then ValueSome(results.ToArray()) else ValueNone

module ArrayOriginal =

    let rec private traverseResultM' (state: Result<_, _>) (f: _ -> Result<_, _>) xs =
        match xs with
        | [||] ->
            state
            |> Result.map Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr

            let res =
                result {
                    let! y = f x
                    let! ys = state
                    return Array.append [| y |] ys
                }

            match res with
            | Ok _ -> traverseResultM' res f xs
            | Error _ -> res

    let traverseResultM f xs = traverseResultM' (Ok [||]) f xs

    let rec private traverseResultA' state f xs =
        match xs with
        | [||] ->
            state
            |> Result.eitherMap Array.rev Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr

            match state, f x with
            | Ok ys, Ok y -> traverseResultA' (Ok(Array.append [| y |] ys)) f xs
            | Error errs, Error e -> traverseResultA' (Error(Array.append [| e |] errs)) f xs
            | Ok _, Error e -> traverseResultA' (Error [| e |]) f xs
            | Error e, Ok _ -> traverseResultA' (Error e) f xs

    let traverseResultA f xs = traverseResultA' (Ok [||]) f xs

    let rec private traverseOptionM' (state: _ option) (f: _ -> _ option) xs =
        match xs with
        | [||] ->
            state
            |> Option.map Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr

            let r =
                option {
                    let! y = f x
                    let! ys = state
                    return Array.append [| y |] ys
                }

            match r with
            | Some _ -> traverseOptionM' r f xs
            | None -> r

    let traverseOptionM f xs = traverseOptionM' (Some [||]) f xs

    let rec private traverseValidationA' state f xs =
        match xs with
        | [||] ->
            state
            |> Result.eitherMap Array.rev Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr
            let fR = f x

            match state, fR with
            | Ok ys, Ok y -> traverseValidationA' (Ok(Array.append [| y |] ys)) f xs
            | Error errs1, Error errs2 ->
                let errs = Array.append errs2 errs1
                traverseValidationA' (Error errs) f xs
            | Ok _, Error errs
            | Error errs, Ok _ -> traverseValidationA' (Error errs) f xs

    let traverseValidationA f xs = traverseValidationA' (Ok [||]) f xs

    let rec private traverseVOptionM' (state: voption<_>) (f: _ -> voption<_>) xs =
        match xs with
        | [||] ->
            state
            |> ValueOption.map Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr

            let r =
                voption {
                    let! y = f x
                    let! ys = state
                    return Array.append [| y |] ys
                }

            match r with
            | ValueSome _ -> traverseVOptionM' r f xs
            | ValueNone -> r

    let traverseVOptionM f xs = traverseVOptionM' (ValueSome [||]) f xs

    let rec private traverseAsyncResultM'
        (state: Async<Result<_, _>>)
        (f: _ -> Async<Result<_, _>>)
        xs
        =
        match xs with
        | [||] ->
            state
            |> AsyncResult.map Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr

            async {
                let! r =
                    asyncResult {
                        let! ys = state
                        let! y = f x
                        return Array.append [| y |] ys
                    }

                match r with
                | Ok _ -> return! traverseAsyncResultM' (Async.singleton r) f xs
                | Error _ -> return r
            }

    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (AsyncResult.ok [||]) f xs

    let rec private traverseAsyncResultA' state f xs =
        match xs with
        | [||] ->
            state
            |> AsyncResult.eitherMap Array.rev Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr

            async {
                let! s = state
                let! fR = f x

                match s, fR with
                | Ok ys, Ok y ->
                    return! traverseAsyncResultA' (AsyncResult.ok (Array.append [| y |] ys)) f xs
                | Error errs, Error e ->
                    return!
                        traverseAsyncResultA' (AsyncResult.error (Array.append [| e |] errs)) f xs
                | Ok _, Error e -> return! traverseAsyncResultA' (AsyncResult.error [| e |]) f xs
                | Error e, Ok _ -> return! traverseAsyncResultA' (AsyncResult.error e) f xs
            }

    let traverseAsyncResultA f xs =
        traverseAsyncResultA' (AsyncResult.ok [||]) f xs

    let rec private traverseAsyncOptionM' (state: Async<_ option>) (f: _ -> Async<_ option>) xs =
        match xs with
        | [||] ->
            state
            |> AsyncOption.map Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr

            async {
                let! o =
                    asyncOption {
                        let! y = f x
                        let! ys = state
                        return Array.append [| y |] ys
                    }

                match o with
                | Some _ -> return! traverseAsyncOptionM' (Async.singleton o) f xs
                | None -> return o
            }

    let traverseAsyncOptionM f xs =
        traverseAsyncOptionM' (AsyncOption.some [||]) f xs

[<MemoryDiagnoser>]
type ArrayTraverseBenchmarks() =
    let allOk = Array.init 1000 id

    let halfError = Array.init 1000 (fun i -> if i = 500 then -1 else i)

    let toResult x = if x < 0 then Error x else Ok(x + 1)
    let toOption x = if x < 0 then None else Some(x + 1)

    let toValidation x =
        if x < 0 then Error [| x |] else Ok(x + 1)

    [<Benchmark(Baseline = true)>]
    member _.Array_Original_ResultM_AllOk() =
        ArrayOriginal.traverseResultM toResult allOk

    [<Benchmark>]
    member _.Array_Current_ResultM_AllOk() =
        FsToolkit.ErrorHandling.Array.traverseResultM toResult allOk

    [<Benchmark>]
    member _.Array_Candidate_ResultM_AllOk() =
        ArrayCandidates.traverseResultM toResult allOk

    [<Benchmark>]
    member _.Array_Original_ResultM_EarlyError() =
        ArrayOriginal.traverseResultM toResult halfError

    [<Benchmark>]
    member _.Array_Current_ResultM_EarlyError() =
        FsToolkit.ErrorHandling.Array.traverseResultM toResult halfError

    [<Benchmark>]
    member _.Array_Candidate_ResultM_EarlyError() =
        ArrayCandidates.traverseResultM toResult halfError

    [<Benchmark>]
    member _.Array_Original_ResultA_AllOk() =
        ArrayOriginal.traverseResultA toResult allOk

    [<Benchmark>]
    member _.Array_Current_ResultA_AllOk() =
        FsToolkit.ErrorHandling.Array.traverseResultA toResult allOk

    [<Benchmark>]
    member _.Array_Candidate_ResultA_AllOk() =
        ArrayCandidates.traverseResultA toResult allOk

    [<Benchmark>]
    member _.Array_Original_ResultA_Errors() =
        ArrayOriginal.traverseResultA toResult halfError

    [<Benchmark>]
    member _.Array_Current_ResultA_Errors() =
        FsToolkit.ErrorHandling.Array.traverseResultA toResult halfError

    [<Benchmark>]
    member _.Array_Candidate_ResultA_Errors() =
        ArrayCandidates.traverseResultA toResult halfError

    [<Benchmark>]
    member _.Array_Original_OptionM_AllSome() =
        ArrayOriginal.traverseOptionM toOption allOk

    [<Benchmark>]
    member _.Array_Current_OptionM_AllSome() =
        FsToolkit.ErrorHandling.Array.traverseOptionM toOption allOk

    [<Benchmark>]
    member _.Array_Candidate_OptionM_AllSome() =
        ArrayCandidates.traverseOptionM toOption allOk

    [<Benchmark>]
    member _.Array_Original_ValidationA_AllOk() =
        ArrayOriginal.traverseValidationA toValidation allOk

    [<Benchmark>]
    member _.Array_Current_ValidationA_AllOk() =
        FsToolkit.ErrorHandling.Array.traverseValidationA toValidation allOk

    [<Benchmark>]
    member _.Array_Candidate_ValidationA_AllOk() =
        ArrayCandidates.traverseValidationA toValidation allOk

    [<Benchmark>]
    member _.Array_Original_ValidationA_Errors() =
        ArrayOriginal.traverseValidationA toValidation halfError

    [<Benchmark>]
    member _.Array_Current_ValidationA_Errors() =
        FsToolkit.ErrorHandling.Array.traverseValidationA toValidation halfError

    [<Benchmark>]
    member _.Array_Candidate_ValidationA_Errors() =
        ArrayCandidates.traverseValidationA toValidation halfError

[<MemoryDiagnoser>]
type ArrayVOptionTraverseBenchmarks() =
    let allSome = Array.init 1000 id

    let halfNone = Array.init 1000 (fun i -> if i = 500 then -1 else i)

    let toVOption x =
        if x < 0 then ValueNone else ValueSome(x + 1)

    [<Benchmark(Baseline = true)>]
    member _.Array_Original_VOptionM_AllSome() =
        ArrayOriginal.traverseVOptionM toVOption allSome

    [<Benchmark>]
    member _.Array_Current_VOptionM_AllSome() =
        FsToolkit.ErrorHandling.Array.traverseVOptionM toVOption allSome

    [<Benchmark>]
    member _.Array_Candidate_VOptionM_AllSome() =
        ArrayCandidates.traverseVOptionM toVOption allSome

    [<Benchmark>]
    member _.Array_Original_VOptionM_EarlyNone() =
        ArrayOriginal.traverseVOptionM toVOption halfNone

    [<Benchmark>]
    member _.Array_Current_VOptionM_EarlyNone() =
        FsToolkit.ErrorHandling.Array.traverseVOptionM toVOption halfNone

    [<Benchmark>]
    member _.Array_Candidate_VOptionM_EarlyNone() =
        ArrayCandidates.traverseVOptionM toVOption halfNone

[<MemoryDiagnoser>]
type ArrayAsyncTraverseBenchmarks() =
    let allOk = Array.init 1000 id

    let halfError = Array.init 1000 (fun i -> if i = 500 then -1 else i)

    let toAsyncResult x =
        async { return if x < 0 then Error x else Ok(x + 1) }

    let toAsyncOption x =
        async { return if x < 0 then None else Some(x + 1) }

    [<Benchmark(Baseline = true)>]
    member _.Array_Original_AsyncResultM_AllOk() =
        ArrayOriginal.traverseAsyncResultM toAsyncResult allOk
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Current_AsyncResultM_AllOk() =
        FsToolkit.ErrorHandling.Array.traverseAsyncResultM toAsyncResult allOk
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Original_AsyncResultM_EarlyError() =
        ArrayOriginal.traverseAsyncResultM toAsyncResult halfError
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Current_AsyncResultM_EarlyError() =
        FsToolkit.ErrorHandling.Array.traverseAsyncResultM toAsyncResult halfError
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Original_AsyncResultA_AllOk() =
        ArrayOriginal.traverseAsyncResultA toAsyncResult allOk
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Current_AsyncResultA_AllOk() =
        FsToolkit.ErrorHandling.Array.traverseAsyncResultA toAsyncResult allOk
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Original_AsyncResultA_Errors() =
        ArrayOriginal.traverseAsyncResultA toAsyncResult halfError
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Current_AsyncResultA_Errors() =
        FsToolkit.ErrorHandling.Array.traverseAsyncResultA toAsyncResult halfError
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Original_AsyncOptionM_AllSome() =
        ArrayOriginal.traverseAsyncOptionM toAsyncOption allOk
        |> Async.RunSynchronously

    [<Benchmark>]
    member _.Array_Current_AsyncOptionM_AllSome() =
        FsToolkit.ErrorHandling.Array.traverseAsyncOptionM toAsyncOption allOk
        |> Async.RunSynchronously

module TaskOptionOriginal =

    let inline bind ([<InlineIfLambda>] f) (ar: Task<_>) =
        task {
            let! opt = ar

            let t =
                match opt with
                | Some x -> f x
                | None -> task { return None }

            return! t
        }

    let inline some x = task { return Some x }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> some (f' x')) x) f

module TaskValueOptionOriginal =

    let inline bind ([<InlineIfLambda>] f) (ar: Task<_>) =
        task {
            let! opt = ar

            let t =
                match opt with
                | ValueSome x -> f x
                | ValueNone -> task { return ValueNone }

            return! t
        }

    let inline valueSome x = task { return ValueSome x }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> valueSome (f' x')) x) f

[<MemoryDiagnoser>]
type TaskOptionApplyBenchmarks() =
    let someF = Task.FromResult(Some(fun x -> x + 1))
    let someX = Task.FromResult(Some 1)
    let noneF: Task<(int -> int) option> = Task.FromResult None
    let noneX: Task<int option> = Task.FromResult None

    [<Benchmark(Baseline = true)>]
    member _.TaskOption_Original_Apply_SomeSome() = TaskOptionOriginal.apply someF someX

    [<Benchmark>]
    member _.TaskOption_Current_Apply_SomeSome() =
        FsToolkit.ErrorHandling.TaskOption.apply someF someX

    [<Benchmark>]
    member _.TaskOption_Original_Apply_NoneFunction() = TaskOptionOriginal.apply noneF someX

    [<Benchmark>]
    member _.TaskOption_Current_Apply_NoneFunction() =
        FsToolkit.ErrorHandling.TaskOption.apply noneF someX

    [<Benchmark>]
    member _.TaskOption_Original_Apply_NoneValue() = TaskOptionOriginal.apply someF noneX

    [<Benchmark>]
    member _.TaskOption_Current_Apply_NoneValue() =
        FsToolkit.ErrorHandling.TaskOption.apply someF noneX

    [<Benchmark>]
    member _.TaskOption_Original_Some() = TaskOptionOriginal.some 1

    [<Benchmark>]
    member _.TaskOption_Current_Some() =
        FsToolkit.ErrorHandling.TaskOption.some 1

    [<Benchmark>]
    member _.TaskOption_Original_Bind_Some() =
        TaskOptionOriginal.bind (fun x -> Task.FromResult(Some(x + 1))) someX

    [<Benchmark>]
    member _.TaskOption_Current_Bind_Some() =
        FsToolkit.ErrorHandling.TaskOption.bind (fun x -> Task.FromResult(Some(x + 1))) someX

    [<Benchmark>]
    member _.TaskOption_Original_Bind_None() =
        TaskOptionOriginal.bind (fun x -> Task.FromResult(Some(x + 1))) noneX

    [<Benchmark>]
    member _.TaskOption_Current_Bind_None() =
        FsToolkit.ErrorHandling.TaskOption.bind (fun x -> Task.FromResult(Some(x + 1))) noneX

[<MemoryDiagnoser>]
type TaskValueOptionApplyBenchmarks() =
    let someF = Task.FromResult(ValueSome(fun x -> x + 1))
    let someX = Task.FromResult(ValueSome 1)
    let noneF: Task<(int -> int) voption> = Task.FromResult ValueNone
    let noneX: Task<int voption> = Task.FromResult ValueNone

    [<Benchmark(Baseline = true)>]
    member _.TaskValueOption_Original_Apply_ValueSomeValueSome() =
        TaskValueOptionOriginal.apply someF someX

    [<Benchmark>]
    member _.TaskValueOption_Current_Apply_ValueSomeValueSome() =
        FsToolkit.ErrorHandling.TaskValueOption.apply someF someX

    [<Benchmark>]
    member _.TaskValueOption_Original_Apply_ValueNoneFunction() =
        TaskValueOptionOriginal.apply noneF someX

    [<Benchmark>]
    member _.TaskValueOption_Current_Apply_ValueNoneFunction() =
        FsToolkit.ErrorHandling.TaskValueOption.apply noneF someX

    [<Benchmark>]
    member _.TaskValueOption_Original_Apply_ValueNoneValue() =
        TaskValueOptionOriginal.apply someF noneX

    [<Benchmark>]
    member _.TaskValueOption_Current_Apply_ValueNoneValue() =
        FsToolkit.ErrorHandling.TaskValueOption.apply someF noneX

    [<Benchmark>]
    member _.TaskValueOption_Original_ValueSome() = TaskValueOptionOriginal.valueSome 1

    [<Benchmark>]
    member _.TaskValueOption_Current_ValueSome() =
        FsToolkit.ErrorHandling.TaskValueOption.valueSome 1

    [<Benchmark>]
    member _.TaskValueOption_Original_Bind_ValueSome() =
        TaskValueOptionOriginal.bind (fun x -> Task.FromResult(ValueSome(x + 1))) someX

    [<Benchmark>]
    member _.TaskValueOption_Current_Bind_ValueSome() =
        FsToolkit.ErrorHandling.TaskValueOption.bind
            (fun x -> Task.FromResult(ValueSome(x + 1)))
            someX

    [<Benchmark>]
    member _.TaskValueOption_Original_Bind_ValueNone() =
        TaskValueOptionOriginal.bind (fun x -> Task.FromResult(ValueSome(x + 1))) noneX

    [<Benchmark>]
    member _.TaskValueOption_Current_Bind_ValueNone() =
        FsToolkit.ErrorHandling.TaskValueOption.bind
            (fun x -> Task.FromResult(ValueSome(x + 1)))
            noneX

module JobOptionOriginal =

    let inline bind ([<InlineIfLambda>] f) (ar: Job<_>) =
        job {
            let! opt = ar

            let t =
                match opt with
                | Some x -> f x
                | None -> job { return None }

            return! t
        }

    let inline singleton x = job { return Some x }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> singleton (f' x')) x) f

[<MemoryDiagnoser>]
type JobOptionApplyBenchmarks() =
    let someF = Job.result (Some(fun x -> x + 1))
    let someX = Job.result (Some 1)
    let noneF: Job<(int -> int) option> = Job.result None
    let noneX: Job<int option> = Job.result None

    [<Benchmark(Baseline = true)>]
    member _.JobOption_Original_Apply_SomeSome() =
        JobOptionOriginal.apply someF someX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Current_Apply_SomeSome() =
        FsToolkit.ErrorHandling.JobOption.apply someF someX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Original_Apply_NoneFunction() =
        JobOptionOriginal.apply noneF someX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Current_Apply_NoneFunction() =
        FsToolkit.ErrorHandling.JobOption.apply noneF someX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Original_Apply_NoneValue() =
        JobOptionOriginal.apply someF noneX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Current_Apply_NoneValue() =
        FsToolkit.ErrorHandling.JobOption.apply someF noneX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Original_Singleton() =
        JobOptionOriginal.singleton 1
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Current_Singleton() =
        FsToolkit.ErrorHandling.JobOption.singleton 1
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Original_Bind_Some() =
        JobOptionOriginal.bind (fun x -> Job.result (Some(x + 1))) someX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Current_Bind_Some() =
        FsToolkit.ErrorHandling.JobOption.bind (fun x -> Job.result (Some(x + 1))) someX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Original_Bind_None() =
        JobOptionOriginal.bind (fun x -> Job.result (Some(x + 1))) noneX
        |> Hopac.run

    [<Benchmark>]
    member _.JobOption_Current_Bind_None() =
        FsToolkit.ErrorHandling.JobOption.bind (fun x -> Job.result (Some(x + 1))) noneX
        |> Hopac.run

module ListOriginal =

    let private traverseTaskResultM' (f: 'c -> Task<Result<'a, 'b>>) (xs: 'c list) =
        let mutable state = Ok []
        let mutable index = 0

        let xs =
            xs
            |> List.toArray

        task {
            while state
                  |> Result.isOk
                  && index < xs.Length do
                let! r =
                    xs
                    |> Array.item index
                    |> f

                index <- index + 1

                match (r, state) with
                | Ok y, Ok ys -> state <- Ok(y :: ys)
                | Error e, _ -> state <- Error e
                | _, _ -> ()

            return
                state
                |> Result.map List.rev
        }

    let traverseTaskResultM f xs = traverseTaskResultM' f xs

[<MemoryDiagnoser>]
type ListTaskResultTraverseBenchmarks() =
    let allOk = List.init 1000 id
    let halfError = List.init 1000 (fun i -> if i = 500 then -1 else i)

    let toTaskResult x =
        Task.FromResult(if x < 0 then Error x else Ok(x + 1))

    [<Benchmark(Baseline = true)>]
    member _.List_Original_TaskResultM_AllOk() =
        ListOriginal.traverseTaskResultM toTaskResult allOk

    [<Benchmark>]
    member _.List_Current_TaskResultM_AllOk() =
        FsToolkit.ErrorHandling.List.traverseTaskResultM toTaskResult allOk

    [<Benchmark>]
    member _.List_Original_TaskResultM_EarlyError() =
        ListOriginal.traverseTaskResultM toTaskResult halfError

    [<Benchmark>]
    member _.List_Current_TaskResultM_EarlyError() =
        FsToolkit.ErrorHandling.List.traverseTaskResultM toTaskResult halfError
