module SeqTests

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Order
open BenchmarkDotNet.Mathematics
open BenchmarkDotNet.Configs
open System.Threading
open System
open FsToolkit.ErrorHandling

module sequenceResultMTests =

    // original used in v4.16.0
    module SeqCache =

        let sequenceResultM (xs: seq<Result<'t, 'e>>) : Result<'t seq, 'e> =
            let rec loop xs ts =
                match Seq.tryHead xs with
                | Some x ->
                    x
                    |> Result.bind (fun t -> loop (Seq.tail xs) (t :: ts))
                | None ->
                    Ok(
                        List.rev ts
                        |> List.toSeq
                    )

            // Seq.cache prevents double evaluation in Seq.tail
            loop (Seq.cache xs) []

    module SeqFold =

        let traverseResultM' state (f: 'okInput -> Result<'okOutput, 'error>) xs =
            let folder state x =
                match state, f x with
                | Error e, _ -> Error e
                | Ok oks, Ok ok ->
                    Seq.singleton ok
                    |> Seq.append oks
                    |> Ok
                | Ok _, Error e -> Error e

            Seq.fold folder state xs
            |> Result.map Seq.rev

        let traverseResultM (f: 'okInput -> Result<'okOutput, 'error>) xs =
            traverseResultM' (Ok Seq.empty) f xs

        let sequenceResultM xs = traverseResultM id xs

    module InlinedSeqFold =

        let inline traverseResultM'
            state
            ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
            xs
            =
            let folder state x =
                match state, f x with
                | Error e, _ -> Error e
                | Ok oks, Ok ok ->
                    Seq.singleton ok
                    |> Seq.append oks
                    |> Ok
                | Ok _, Error e -> Error e

            Seq.fold folder state xs
            |> Result.map Seq.rev

        let traverseResultM (f: 'okInput -> Result<'okOutput, 'error>) xs =
            traverseResultM' (Ok Seq.empty) f xs

        let sequenceResultM xs = traverseResultM id xs

    module SeqUnfold =

        let traverseResultM' initialState (f: 'okInput -> Result<'okOutput, 'error>) xs =
            (initialState, 0)
            |> Seq.unfold (fun (state, i) ->
                xs
                |> Seq.tryItem i
                |> Option.bind (fun x ->
                    match state, f x with
                    | Error _, _ -> None
                    | Ok oks, Ok ok ->
                        let newState =
                            Seq.singleton ok
                            |> Seq.append oks
                            |> Ok

                        Some(newState, (newState, i + 1))
                    | Ok _, Error e -> Some(Error e, (Error e, i + 1))
                )
            )
            |> Seq.last

        let traverseResultM f xs = traverseResultM' (Ok Seq.empty) f xs
        let sequenceResultM xs = traverseResultM id xs

    module GetEnumerator =

        let traverseResultM' state (f: 'okInput -> Result<'okOutput, 'error>) (xs: seq<'okInput>) =
            let mutable state = state

            let enumerator = xs.GetEnumerator()

            while Result.isOk state
                  && enumerator.MoveNext() do
                match state, f enumerator.Current with
                | Error _, _ -> ()
                | Ok oks, Ok ok -> state <- Ok(Seq.append oks (Seq.singleton ok))
                | Ok _, Error e -> state <- Error e

            state

        let traverseResultM f xs = traverseResultM' (Ok Seq.empty) f xs
        let sequenceResultM xs = traverseResultM id xs

    module InlinedGetEnumerator =

        // adds an early exit upon encountering an error
        let inline traverseResultM'
            state
            ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
            (xs: seq<'okInput>)
            =
            let mutable state = state
            let enumerator = xs.GetEnumerator()

            while Result.isOk state
                  && enumerator.MoveNext() do
                match state, f enumerator.Current with
                | Error e, _ -> state <- Error e
                | Ok oks, Ok ok ->
                    state <-
                        Seq.singleton ok
                        |> Seq.append oks
                        |> Ok
                | Ok _, Error e -> state <- Error e

            state

        let traverseResultM f xs = traverseResultM' (Ok Seq.empty) f xs
        let sequenceResultM xs = traverseResultM id xs

    module SeqExpr =

        let inline traverseResultM'
            state
            ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
            (xs: 'okInput seq)
            =
            match state with
            | Error _ -> state
            | Ok oks ->
                use enumerator = xs.GetEnumerator()

                let rec loop oks =
                    if enumerator.MoveNext() then
                        match f enumerator.Current with
                        | Ok ok ->
                            loop (
                                seq {
                                    yield ok
                                    yield! oks
                                }
                            )
                        | Error e -> Error e
                    else
                        Ok(Seq.rev oks)

                loop oks

        let traverseResultM f xs = traverseResultM' (Ok Seq.empty) f xs
        let sequenceResultM xs = traverseResultM id xs


[<MemoryDiagnoser>]
[<Orderer(SummaryOrderPolicy.FastestToSlowest)>]
[<RankColumn(NumeralSystem.Stars)>]
[<MinColumn; MaxColumn; MedianColumn; MeanColumn; CategoriesColumn>]
[<GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)>]
type SeqBenchmarks() =

    member _.thousandWithSleep = 1000u

    member _.fiveThousandWithSleep = 5_000u

    // for testing early escape performance
    member _.GetPartialOkSeqWithSleep size =
        seq {
            for i in 1u .. size do
                Thread.Sleep(TimeSpan.FromMicroseconds(1.0))
                if i = size / 2u then Error "error" else Ok i
        }

    member _.thousand = 1_000u

    member _.million = 1_000_000u

    member _.GetPartialOkSeq size =
        seq {
            for i in 1u .. size do
                if i = size / 2u then Error "error" else Ok i
        }

    [<Benchmark(Baseline = true, Description = "original")>]
    [<BenchmarkCategory("1000", "sleep")>]
    member this.seqCache() =
        sequenceResultMTests.SeqCache.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.thousandWithSleep
        )

    [<Benchmark(Description = "Seq.fold")>]
    [<BenchmarkCategory("1000", "sleep")>]
    member this.seqFoldSmall() =
        sequenceResultMTests.SeqFold.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.thousandWithSleep
        )

    [<Benchmark(Description = "inlined Seq.fold")>]
    [<BenchmarkCategory("1000", "sleep")>]
    member this.inlineSeqFoldSmall() =
        sequenceResultMTests.InlinedSeqFold.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.thousandWithSleep
        )

    [<Benchmark(Description = "Seq.unfold")>]
    [<BenchmarkCategory("1000", "sleep")>]
    member this.seqUnfoldSmall() =
        sequenceResultMTests.SeqUnfold.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.thousandWithSleep
        )

    [<Benchmark(Description = "GetEnumerator w/ mutability")>]
    [<BenchmarkCategory("1000", "sleep")>]
    member this.getEnumeratorSmall() =
        sequenceResultMTests.GetEnumerator.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.thousandWithSleep
        )

    [<Benchmark(Description = "inlined GetEnumerator w/ mutability")>]
    [<BenchmarkCategory("1000", "sleep")>]
    member this.inlineGetEnumeratorSmall() =
        sequenceResultMTests.InlinedGetEnumerator.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.thousandWithSleep
        )

    [<Benchmark(Description = "Seq expression")>]
    [<BenchmarkCategory("1000", "sleep")>]
    member this.seqExpressionSmall() =
        sequenceResultMTests.SeqExpr.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.thousandWithSleep
        )

    // made this baseline for this category since unfold and original were so unperformant for this size of data
    [<Benchmark(Baseline = true, Description = "Seq.fold")>]
    [<BenchmarkCategory("5000", "sleep")>]
    member this.seqFoldLarge() =
        sequenceResultMTests.SeqFold.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.fiveThousandWithSleep
        )

    [<Benchmark(Description = "inlined Seq.fold")>]
    [<BenchmarkCategory("5000", "sleep")>]
    member this.inlineSeqFoldLarge() =
        sequenceResultMTests.InlinedSeqFold.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.fiveThousandWithSleep
        )

    [<Benchmark(Description = "GetEnumerator w/ mutability")>]
    [<BenchmarkCategory("5000", "sleep")>]
    member this.getEnumeratorLarge() =
        sequenceResultMTests.GetEnumerator.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.fiveThousandWithSleep
        )

    [<Benchmark(Description = "inlined GetEnumerator w/ mutability")>]
    [<BenchmarkCategory("5000", "sleep")>]
    member this.inlineGetEnumeratorLarge() =
        sequenceResultMTests.InlinedGetEnumerator.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.fiveThousandWithSleep
        )

    [<Benchmark(Description = "Seq expression")>]
    [<BenchmarkCategory("5000", "sleep")>]
    member this.seqExpressionLarge() =
        sequenceResultMTests.SeqExpr.sequenceResultM (
            this.GetPartialOkSeqWithSleep this.fiveThousandWithSleep
        )

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("1,000", "func vs expr")>]
    member this.SeqFuncsSmall() =
        this.GetPartialOkSeq this.thousand
        |> sequenceResultMTests.InlinedGetEnumerator.sequenceResultM

    [<Benchmark>]
    [<BenchmarkCategory("1,000", "func vs expr")>]
    member this.SeqExprSmall() =
        this.GetPartialOkSeq this.thousand
        |> sequenceResultMTests.SeqExpr.sequenceResultM

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("1,000", "func vs expr", "iter")>]
    member this.SeqFuncsSmallIter() =
        this.GetPartialOkSeq this.thousand
        |> sequenceResultMTests.GetEnumerator.sequenceResultM
        |> Result.map (Seq.iter ignore)

    [<Benchmark>]
    [<BenchmarkCategory("1,000", "func vs expr", "iter")>]
    member this.SeqExprSmallIter() =
        this.GetPartialOkSeq this.thousand
        |> sequenceResultMTests.SeqExpr.sequenceResultM
        |> Result.map (Seq.iter ignore)

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("1,000,000", "func vs expr")>]
    member this.SeqFuncsLarge() =
        this.GetPartialOkSeq this.million
        |> sequenceResultMTests.InlinedGetEnumerator.sequenceResultM

    [<Benchmark>]
    [<BenchmarkCategory("1,000,000", "func vs expr")>]
    member this.SeqExprLarge() =
        this.GetPartialOkSeq this.million
        |> sequenceResultMTests.SeqExpr.sequenceResultM

    [<Benchmark(Baseline = true)>]
    [<BenchmarkCategory("1,000,000", "func vs expr", "iter")>]
    member this.SeqFuncsLargeIter() =
        this.GetPartialOkSeq this.million
        |> sequenceResultMTests.GetEnumerator.sequenceResultM
        |> Result.map (Seq.iter ignore)

    [<Benchmark>]
    [<BenchmarkCategory("1,000,000", "func vs expr", "iter")>]
    member this.SeqExprLargeIter() =
        this.GetPartialOkSeq this.million
        |> sequenceResultMTests.SeqExpr.sequenceResultM
        |> Result.map (Seq.iter ignore)
