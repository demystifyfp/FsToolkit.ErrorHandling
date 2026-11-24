module ListTraverse

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Configs
open Microsoft.FSharp.Core.CompilerServices
open FsToolkit.ErrorHandling

module Original =

    let rec private traverseResultM' (state: Result<_, _>) (f: _ -> Result<_, _>) xs =
        match xs with
        | [] ->
            state
            |> Result.map List.rev
        | x :: xs ->
            let r =
                result {
                    let! y = f x
                    let! ys = state
                    return y :: ys
                }

            match r with
            | Ok _ -> traverseResultM' r f xs
            | Error _ -> r

    let traverseResultM f xs = traverseResultM' (Ok []) f xs

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

module private Candidates =

    let traverseResultM1' state f xs =
        Seq.traverseResultM' state f xs
        |> Result.map Array.toList

    let traverseResultM1 f xs = traverseResultM1' (Ok []) f xs

    let traverseResultM2'
        state
        (f: 'okInput -> Result<'okOutput, 'error>)
        xs
        : Result<'okOutput list, 'error> =
        match xs with
        | [] -> state
        | xs ->

            match state with
            | Error e -> Error e
            | Ok initialSuccesses ->

#if FABLE_COMPILER
                let oks = ResizeArray(initialSuccesses)
#else
                let mutable oks = ListCollector()
                oks.AddMany initialSuccesses
#endif
                let mutable err: 'error voption = ValueNone
                use e = (List.toSeq xs).GetEnumerator()

                while err.IsNone
                      && e.MoveNext() do
                    match f e.Current with
                    | Ok r -> oks.Add r
                    | Error e -> err <- ValueSome e

                match err with
                | ValueSome e -> Error e
#if FABLE_COMPILER
                | ValueNone -> Ok(List.ofSeq oks)
#else
                | ValueNone -> Ok(oks.Close())
#endif
    let traverseResultM2 f xs = traverseResultM2' (Ok []) f xs

    let inline traverseResultA1'
        (state: Result<'a list, 'b list>)
        ([<InlineIfLambda>] f: 'c -> Result<'a, 'b>)
        (xs: 'c list)
        : Result<'a list, 'b list> =
        match xs with
        | [] -> state
        | xs ->
            state
            |> fun state -> Seq.traverseResultA' state f xs
            |> Result.eitherMap List.ofArray List.ofArray

    let traverseResultA1 f xs = traverseResultA1' (Ok []) f xs

    let inline private traverseResultA2'
        (state: Result<'ok list, 'error list>)
        ([<InlineIfLambda>] f: 't -> Result<'ok, 'error>)
        (xs: 't list)
        : Result<'ok list, 'error list> =
        match xs with
        | [] -> state
        | xs ->
            match state with
            | Error failuresToDate ->
#if FABLE_COMPILER
                let errs = ResizeArray(failuresToDate)
#else
                let mutable errs = ListCollector()
                errs.AddMany failuresToDate
#endif

                for x in xs do
                    match f x with
                    | Error e -> errs.Add e
                    | Ok _ -> () // as the initial state was failure, oks are irrelevant

#if FABLE_COMPILER
                Error(List.ofSeq errs)
#else
                Error(errs.Close())
#endif
            | Ok initialSuccesses ->

                match xs with
                | [ x ] ->
                    match f x with
                    | Ok r ->
#if FABLE_COMPILER
                        let oks = ResizeArray(initialSuccesses)
#else
                        let mutable oks = ListCollector()
                        oks.AddMany initialSuccesses
#endif
                        oks.Add r
#if FABLE_COMPILER
                        Ok(Seq.toList oks)
#else
                        Ok(oks.Close())
#endif
                    | Error e -> Error [ e ]
                | xs ->

#if FABLE_COMPILER
                    let mutable oks = ResizeArray(initialSuccesses)
                    let mutable errs = ResizeArray()
#else
                    let mutable oks = ListCollector()
                    oks.AddMany initialSuccesses
                    let mutable errs = ListCollector()
#endif
                    let mutable ok = true

                    for x in xs do
                        match f x with
                        | Ok r when ok -> oks.Add r
                        | Ok _ -> () // no point saving results we won't use given the end result will be Error
                        | Error e ->
                            errs.Add e
                            ok <- false

#if FABLE_COMPILER
                    if ok then Ok(Seq.toList oks) else Error(Seq.toList errs)
#else
                    if ok then Ok(oks.Close()) else Error(errs.Close())
#endif
    let traverseResultA2 f xs = traverseResultA2' (Ok []) f xs

[<MemoryDiagnoser>]
[<GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)>]
type ListTraverseBenchmarks() =
    let mutable inputs = []

    let toResult x = if x < 0 then Error x else Ok(x + 1)
    let toAsyncResult x = async { return toResult x }

    [<Params(0, 1, 10, 100, 1000)>]
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
    [<BenchmarkCategory("ResultM")>]
    member _.TraverseResultM() =
        Original.traverseResultM toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultM")>]
    member _.TraverseResultM1() =
        Candidates.traverseResultM1 toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultM")>]
    member _.TraverseResultM2() =
        Candidates.traverseResultM2 toResult inputs

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA() =
        Original.traverseResultA toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA1() =
        Candidates.traverseResultA1 toResult inputs

    [<Benchmark>]
    [<BenchmarkCategory("ResultA")>]
    member _.TraverseResultA2() =
        Candidates.traverseResultA2 toResult inputs
