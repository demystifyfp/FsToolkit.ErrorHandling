```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                        | Mean      | Error     | StdDev    | P80       | P95       | Ratio        | RatioSD | Allocated | Alloc Ratio |
|------------------------------ |----------:|----------:|----------:|----------:|----------:|-------------:|--------:|----------:|------------:|
| Result_Normal_Bind            | 0.0247 ns | 0.0110 ns | 0.0102 ns | 0.0344 ns | 0.0414 ns |     baseline |         |         - |          NA |
| Result_Alt_InlinedLambda_Bind | 0.0324 ns | 0.0130 ns | 0.0122 ns | 0.0441 ns | 0.0487 ns | 1.57x slower |   0.93x |         - |          NA |
