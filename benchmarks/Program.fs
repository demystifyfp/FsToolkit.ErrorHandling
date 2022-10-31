open System
open BenchmarkDotNet.Running
open benchmarks
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Environments
open FsToolkit.ErrorHandling.Benchmarks
open ApplicativeTests

[<EntryPoint>]
let main argv =
    let cfg =
        DefaultConfig.Instance
            // .AddJob(Job.Default.WithRuntime(CoreRuntime.Core50))
            .AddJob(Job.Default.WithRuntime(CoreRuntime.Core60))
    // BenchmarkRunner.Run<EitherMapBenchmarks>() |> ignore
    // BenchmarkRunner.Run<TaskResult_BindCEBenchmarks>(cfg) |> ignore
    // BenchmarkRunner.Run<BindSameBenchmarks>() |> ignore

    BenchmarkRunner.Run<Result_BindvsAndCEBenchmarks>(cfg) |> ignore
//

    0 // return an integer exit code
