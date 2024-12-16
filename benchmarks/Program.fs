open BenchmarkDotNet.Running
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Columns
open BenchmarkDotNet.Reports
open System.Reflection


[<EntryPoint>]
let main argv =

    let cfg =
        DefaultConfig.Instance
            .AddColumn(StatisticColumn.P80, StatisticColumn.P95)
            .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))

    // see here for console args: https://benchmarkdotnet.org/articles/guides/console-args.html
    BenchmarkRunner.Run(assembly = Assembly.GetExecutingAssembly(), config = cfg, args = argv)
    |> ignore

    0 // return an integer exit code
