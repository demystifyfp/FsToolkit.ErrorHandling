open System
open BenchmarkDotNet.Running
open benchmarks
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Columns
open BenchmarkDotNet.Environments
open BenchmarkDotNet.Reports
open FsToolkit.ErrorHandling.Benchmarks
open ApplicativeTests

[<EntryPoint>]
let main argv =

    let cfg =
        DefaultConfig.Instance
            // .AddJob(Job.Default.WithRuntime(CoreRuntime.Core50))
            .AddJob(Job.Default.WithRuntime(CoreRuntime.Core70))
            .AddColumn(StatisticColumn.P80, StatisticColumn.P95)
            .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
    // BenchmarkRunner.Run<EitherMapBenchmarks>() |> ignore
    // BenchmarkRunner.Run<TaskResult_BindCEBenchmarks>(cfg) |> ignore
    // BenchmarkRunner.Run<BindSameBenchmarks>() |> ignore

    BenchmarkRunner.Run<SeqTests.SeqBenchmarks>(cfg, argv)
    |> ignore

    0 // return an integer exit code
