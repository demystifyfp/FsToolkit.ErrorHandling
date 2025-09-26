```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                     | Mean     | Error    | StdDev   | P80      | P95      | Ratio        | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |---------:|---------:|---------:|---------:|---------:|-------------:|--------:|-------:|-------:|----------:|------------:|
| afib                       | 12.06 μs | 0.301 μs | 0.879 μs | 12.85 μs | 13.71 μs |     baseline |         | 3.0670 | 0.1068 |   46.9 KB |             |
| Result_Normal_Bind_CE      | 12.42 μs | 0.328 μs | 0.961 μs | 13.21 μs | 14.10 μs | 1.04x slower |   0.11x | 3.1128 | 0.1068 |  47.52 KB |  1.01x more |
| Result_Alt_Inlined_Bind_CE | 12.50 μs | 0.319 μs | 0.937 μs | 13.30 μs | 13.96 μs | 1.04x slower |   0.11x | 3.1128 | 0.1221 |  47.55 KB |  1.01x more |
