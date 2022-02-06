open System
open BenchmarkDotNet.Running
open benchmarks

[<EntryPoint>]
let main argv =
    // BenchmarkRunner.Run<EitherMapBenchmarks>() |> ignore
    BenchmarkRunner.Run<Map2Benchmarks>() |> ignore
    
    0 // return an integer exit code