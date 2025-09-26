```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                     | Mean     | Error    | StdDev   | P80      | P95      | Ratio        | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------:|---------:|---------:|---------:|---------:|-------------:|--------:|-------:|----------:|------------:|
| afib                       | 893.7 ns | 12.33 ns | 10.93 ns | 903.8 ns | 906.5 ns |     baseline |         | 0.2298 |   3.52 KB |             |
| Result_Normal_Bind_CE      | 933.7 ns | 18.71 ns | 31.26 ns | 951.1 ns | 997.8 ns | 1.04x slower |   0.04x | 0.2451 |   3.77 KB |  1.07x more |
| Result_Alt_Inlined_Bind_CE | 914.7 ns | 15.16 ns | 14.18 ns | 925.6 ns | 934.6 ns | 1.02x slower |   0.02x | 0.2451 |   3.77 KB |  1.07x more |
