module ListTraverse

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Configs
open FsToolkit.ErrorHandling
open Microsoft.FSharp.Core.CompilerServices

module Original =

    let rec private traverseResultA' state f xs =
        match xs with
        | [] ->
            state
            |> Result.eitherMap List.rev List.rev
        | x :: xs ->

            match state, f x with
            | Ok ys, Ok y -> traverseResultA' (Ok(y :: ys)) f xs
            | Error errs, Error e -> traverseResultA' (Error(e :: errs)) f xs
            | Ok _, Error e -> traverseResultA' (Error [ e ]) f xs
            | Error e, Ok _ -> traverseResultA' (Error e) f xs

    let traverseResultA f xs = traverseResultA' (Ok []) f xs

    let rec private traverseAsyncResultA' state f xs =
        match xs with
        | [] ->
            state
            |> AsyncResult.eitherMap List.rev List.rev

        | x :: xs ->
            async {
                let! s = state
                let! fR = f x

                match s, fR with
                | Ok ys, Ok y -> return! traverseAsyncResultA' (AsyncResult.ok (y :: ys)) f xs
                | Error errs, Error e ->
                    return! traverseAsyncResultA' (AsyncResult.error (e :: errs)) f xs
                | Ok _, Error e -> return! traverseAsyncResultA' (AsyncResult.error [ e ]) f xs
                | Error e, Ok _ -> return! traverseAsyncResultA' (AsyncResult.error e) f xs
            }

    let traverseAsyncResultA f xs =
        traverseAsyncResultA' (AsyncResult.ok []) f xs

    let rec private traverseAsyncResultM'
        (state: Async<Result<_, _>>)
        (f: _ -> Async<Result<_, _>>)
        xs
        =
        match xs with
        | [] ->
            state
            |> AsyncResult.map List.rev
        | x :: xs ->
            async {
                let! r =
                    asyncResult {
                        let! ys = state
                        let! y = f x
                        return y :: ys
                    }

                match r with
                | Ok _ -> return! traverseAsyncResultM' (async.Return r) f xs
                | Error _ -> return r
            }

    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (AsyncResult.ok []) f xs

module private Updated =

    let inline traverseResultA'1
        (state: Result<'a list, 'b list>)
        ([<InlineIfLambda>] f: 'c -> Result<'a, 'b>)
        (xs: 'c list)
        : Result<'a list, 'b list> =
        Seq.traverseResultA' state f xs
        |> Result.eitherMap List.ofArray List.ofArray

    let inline traverseResultA'2
        (state: Result<'ok list, 'error list>)
        ([<InlineIfLambda>] f: 't -> Result<'ok, 'error>)
        (xs: 't list)
        : Result<'ok list, 'error list> =
#if FABLE_COMPILER
    traverseResultA'1 state f xs
#else
        match xs with
        | [] -> state
        | xs ->
            match state with
            | Error failuresToDate ->
                let errs = ListCollector() in
                errs.AddMany failuresToDate

                for x in xs do
                    match f x with
                    | Error e -> errs.Add e
                    | Ok _ -> () // as the initial state was failure, oks are irrelevant

                Error(errs.Close())
            | Ok initialSuccesses ->

                match xs with
                | [ x ] ->
                    match f x with
                    | Ok r ->
                        let oks = ListCollector() in
                        oks.AddMany initialSuccesses
                        oks.Add r
                        Ok(oks.Close())
                    | Error e -> Error [ e ]
                | xs ->

                    let oks = ListCollector() in
                    oks.AddMany initialSuccesses
                    let errs = ListCollector()
                    let mutable ok = true

                    for x in xs do
                        match f x with
                        | Ok r when ok -> oks.Add r
                        | Ok _ -> () // no point saving results we won't use given the end result will be Error
                        | Error e ->
                            errs.Add e
                            ok <- false

                    if ok then Ok(oks.Close()) else Error(errs.Close())
#endif

    let inline traverseResultA1 f xs = traverseResultA'1 (Ok []) f xs
    let inline traverseResultA2 f xs = traverseResultA'2 (Ok []) f xs

    let inline traverseAsyncResultA'1
        (state: Async<Result<'a list, 'b list>>)
        ([<InlineIfLambda>] f: 'c -> Async<Result<'a, 'b>>)
        (xs: 'c list)
        : Async<Result<'a list, 'b list>> =
        Seq.traverseAsyncResultA' state f xs
        |> AsyncResult.eitherMap List.ofArray List.ofArray

    let inline traverseAsyncResultA'2
        (state: Async<Result<'a list, 'b list>>)
        ([<InlineIfLambda>] f: 'c -> Async<Result<'a, 'b>>)
        (xs: 'c list)
        : Async<Result<'a list, 'b list>> =
#if FABLE_COMPILER
    traverseAsyncResultA' state f xs
#else
        match xs with
        | [] -> state
        | xs ->

            async {
                match! state with
                | Error failuresToDate ->
                    let errs = ListCollector() in
                    errs.AddMany failuresToDate

                    for x in xs do
                        match! f x with
                        | Ok _ -> () // as the initial state was failure, oks are irrelevant
                        | Error e -> errs.Add e

                    return Error(errs.Close())
                | Ok initialSuccesses ->

                    let oks = ListCollector() in
                    oks.AddMany initialSuccesses
                    let errs = ListCollector()
                    let mutable ok = true

                    for x in xs do
                        match! f x with
                        | Ok r when ok -> oks.Add r
                        | Ok _ -> () // no point saving results we won't use given the end result will be Error
                        | Error e ->
                            ok <- false
                            errs.Add e

                    if ok then
                        return Ok(oks.Close())
                    else
                        return Error(errs.Close())
            }
#endif

    let traverseAsyncResultA1 f xs =
        traverseAsyncResultA'1 (async { return Ok List.empty }) f xs

    let traverseAsyncResultA2 f xs =
        traverseAsyncResultA'2 (async { return Ok List.empty }) f xs

    let inline traverseAsyncResultM'1
        (state: Async<Result<'a list, 'b>>)
        ([<InlineIfLambda>] f: 'c -> Async<Result<'a, 'b>>)
        (xs: 'c list)
        : Async<Result<'a list, 'b>> =
        Seq.traverseAsyncResultM' state f xs
        |> Async.map (Result.map List.ofArray)

    let inline traverseAsyncResultM'2
        (state: Async<Result<'ok list, 'error>>)
        ([<InlineIfLambda>] f: 't -> Async<Result<'ok, 'error>>)
        (xs: 't list)
        : Async<Result<'ok list, 'error>> =
        match xs with
        | [] -> state
        | xs ->
            async {
                match! state with
                | Error e -> return Error e
                | Ok initialSuccesses ->
#if FABLE_COMPILER
                    let oks = ResizeArray(initialSuccesses) in
#else
                    let oks = ListCollector() in
                    oks.AddMany initialSuccesses

                    match xs with
                    | [] -> return Ok(oks.Close())
                    | [ x ] ->
                        match! f x with
                        | Ok r ->
                            oks.Add r
                            return Ok(oks.Close())
                        | Error e -> return Error e
                    | xs ->
#endif
                    use e = (List.toSeq xs).GetEnumerator()

                    let mutable err = Unchecked.defaultof<'error>
                    let mutable ok = true

                    while ok
                          && e.MoveNext() do
                        match! f e.Current with
                        | Ok r -> oks.Add r
                        | Error e ->
                            ok <- false
                            err <- e

#if FABLE_COMPILER
                    return if ok then Ok(oks.ToArray()) else Error err
#else
                    return if ok then Ok(oks.Close()) else Error err
#endif
            }

    let traverseAsyncResultM1 f xs =
        traverseAsyncResultM'1 (async { return Ok List.empty }) f xs

    let traverseAsyncResultM2 f xs =
        traverseAsyncResultM'2 (async { return Ok List.empty }) f xs

[<MemoryDiagnoser>]
[<GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)>]
type ListTraverseBenchmarks() =
    let mutable inputs = []

    let toResult x = if x < 0 then Error x else Ok(x + 1)
    let toAsyncResult x = async { return toResult x }

    [<Params(0, 1, 5, 50)>]
    member val Size = 0 with get, set

    [<Params(false, true)>]
    member val HasErrors = false with get, set

    [<GlobalSetup>]
    member this.Setup() =
        inputs <-
            List.init
                this.Size
                (fun i ->
                    if
                        this.HasErrors
                        && i = this.Size
                               / 2
                    then
                        -1
                    else
                        i
                )

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA() =
        Original.traverseResultA toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA_1() =
        Updated.traverseResultA1 toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA_2() =
        Updated.traverseResultA2 toResult inputs

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("AsyncResultA")>]
    member _.TraverseAsyncResultA() =
        Original.traverseAsyncResultA toAsyncResult inputs
        |> Async.RunSynchronously

    [<Benchmark>]
    [<BenchmarkCategory("AsyncResultA")>]
    member _.TraverseAsyncResultA_1() =
        Updated.traverseAsyncResultA1 toAsyncResult inputs
        |> Async.RunSynchronously

    [<Benchmark>]
    [<BenchmarkCategory("AsyncResultA")>]
    member _.TraverseAsyncResultA_2() =
        Updated.traverseAsyncResultA2 toAsyncResult inputs
        |> Async.RunSynchronously

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("AsyncResultM")>]
    member _.TraverseAsyncResultM() =
        List.traverseAsyncResultM toAsyncResult inputs
        |> Async.RunSynchronously

    [<Benchmark>]
    [<BenchmarkCategory("AsyncResultM")>]
    member _.TraverseAsyncResultM_1() =
        Updated.traverseAsyncResultM1 toAsyncResult inputs
        |> Async.RunSynchronously

    [<Benchmark>]
    [<BenchmarkCategory("AsyncResultM")>]
    member _.TraverseAsyncResultM_2() =
        Updated.traverseAsyncResultM2 toAsyncResult inputs
        |> Async.RunSynchronously
