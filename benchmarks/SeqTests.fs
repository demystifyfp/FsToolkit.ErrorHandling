module SeqTests

open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling
open System.Threading
open System
open BenchmarkDotNet.Attributes

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

[<MemoryDiagnoser>]
type SeqBenchmarks() =

    member _.GetOkSeq size =
        seq {
            for i in 1..size do
                yield Ok i
        }

    [<Params(1_000)>]
    member val Size = 0 with get, set

    [<Benchmark(Baseline = true, Description = "v1")>]
    member this.test1() =
        sequenceResultMTests.v1.sequenceResultM (this.GetOkSeq this.Size)

    [<Benchmark(Description = "v2")>]
    member this.test2() =
        sequenceResultMTests.v2.sequenceResultM (this.GetOkSeq this.Size)
