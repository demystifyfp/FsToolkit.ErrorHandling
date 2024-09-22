module SeqTests

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Order
open BenchmarkDotNet.Mathematics
open BenchmarkDotNet.Configs
open System.Threading
open System

module sequenceResultMTests =

    module v1 =

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

    module v2 =

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

    module v3 =

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

    module v4 =

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

    module v5 =

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

    module v6 =

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


[<MemoryDiagnoser>]
[<Orderer(SummaryOrderPolicy.FastestToSlowest)>]
[<RankColumn(NumeralSystem.Stars)>]
[<MinColumn; MaxColumn; MedianColumn; MeanColumn; CategoriesColumn>]
[<GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)>]
type SeqBenchmarks() =

    member _.GetPartialOkSeq size =
        seq {
            for i in 1u .. size do
                Thread.Sleep(TimeSpan.FromMicroseconds(1.0))
                if i = size / 2u then Error "error" else Ok i
        }

    member _.SmallSize = 100u

    member _.LargeSize = 5_000u

    [<Benchmark(Baseline = true, Description = "original")>]
    [<BenchmarkCategory("Small")>]
    member this.original() =
        sequenceResultMTests.v1.sequenceResultM (this.GetPartialOkSeq this.SmallSize)
        |> ignore

    [<Benchmark(Description = "Seq.fold")>]
    [<BenchmarkCategory("Small")>]
    member this.seqFoldSmall() =
        sequenceResultMTests.v2.sequenceResultM (this.GetPartialOkSeq this.SmallSize)
        |> ignore

    [<Benchmark(Description = "inlined Seq.fold")>]
    [<BenchmarkCategory("Small")>]
    member this.inlineSeqFoldSmall() =
        sequenceResultMTests.v3.sequenceResultM (this.GetPartialOkSeq this.SmallSize)
        |> ignore

    [<Benchmark(Description = "Seq.unfold")>]
    [<BenchmarkCategory("Small")>]
    member this.seqUnfoldSmall() =
        sequenceResultMTests.v4.sequenceResultM (this.GetPartialOkSeq this.SmallSize)
        |> ignore

    [<Benchmark(Description = "GetEnumerator w/ mutability")>]
    [<BenchmarkCategory("Small")>]
    member this.getEnumeratorSmall() =
        sequenceResultMTests.v5.sequenceResultM (this.GetPartialOkSeq this.SmallSize)
        |> ignore

    [<Benchmark(Description = "inlined GetEnumerator w/ mutability")>]
    [<BenchmarkCategory("Small")>]
    member this.inlineGetEnumeratorSmall() =
        sequenceResultMTests.v6.sequenceResultM (this.GetPartialOkSeq this.SmallSize)
        |> ignore

    // made this baseline for this category since unfold and original were so unperformant for this size of data
    [<Benchmark(Baseline = true, Description = "Seq.fold")>]
    [<BenchmarkCategory("Large")>]
    member this.seqFoldLarge() =
        sequenceResultMTests.v2.sequenceResultM (this.GetPartialOkSeq this.LargeSize)
        |> ignore

    [<Benchmark(Description = "inlined Seq.fold")>]
    [<BenchmarkCategory("Large")>]
    member this.inlineSeqFoldLarge() =
        sequenceResultMTests.v3.sequenceResultM (this.GetPartialOkSeq this.LargeSize)
        |> ignore

    [<Benchmark(Description = "GetEnumerator w/ mutability")>]
    [<BenchmarkCategory("Large")>]
    member this.getEnumeratorLarge() =
        sequenceResultMTests.v5.sequenceResultM (this.GetPartialOkSeq this.LargeSize)
        |> ignore

    [<Benchmark(Description = "inlined GetEnumerator w/ mutability")>]
    [<BenchmarkCategory("Large")>]
    member this.inlineGetEnumeratorLarge() =
        sequenceResultMTests.v6.sequenceResultM (this.GetPartialOkSeq this.LargeSize)
        |> ignore
