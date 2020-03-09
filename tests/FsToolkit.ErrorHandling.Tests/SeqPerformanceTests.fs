module SeqPerformanceTests

#if !FABLE_COMPILER
open Expecto
open Expecto.Tests
open Expecto.Performance
open FsToolkit.ErrorHandling
open SampleDomain

let inline repeat n a arg =
    [for _ in 0..(n - 1) -> a arg]

let inline repeat10 a arg =
    repeat 10 a arg

let traverseResultTests =
    testSequenced <| testList "Seq performance tests " [
        testCase "applying Seq.sequenceResultM against array must be faster than using List module" <| fun _ ->
            let arr = [| for i in 1..127 -> Ok (i)|]
            Expect.isFasterThan
                (fun () -> repeat10 (Seq.sequenceResultM >> Result.map(Seq.toArray)) arr )
                (fun () -> repeat10 (Array.toList >> List.sequenceResultM >> Result.map(List.toArray)) arr)
                ""
        testCase "applying Seq.sequenceResultA against array must be faster than using List module" <| fun _ ->
            let arr = [| for i in 1..127 -> Ok (i)|]
            Expect.isFasterThan
                (fun () -> repeat10 (Seq.sequenceResultA >> Result.map(Seq.toArray)) arr )
                (fun () -> repeat10 (Array.toList >> List.sequenceResultA >> Result.map(List.toArray)) arr)
                ""
        testCase "applying Seq.sequenceResultA against array must be faster than using List module (reference type)" <| fun _ ->
            let arr = [| for i in 1..127 -> Tweet.TryCreate(sprintf "%d" i)|]
            Expect.isFasterThan
                (fun () -> repeat10 (Seq.sequenceResultA >> Result.map(Seq.toArray)) arr )
                (fun () -> repeat10 (Array.toList >> List.sequenceResultA >> Result.map(List.toArray)) arr)
                ""
    ]

let allTests = testList "Seq performance tests" [
    traverseResultTests
]

#endif