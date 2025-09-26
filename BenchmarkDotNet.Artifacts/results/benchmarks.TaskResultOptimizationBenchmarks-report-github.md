```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                    | Mean     | Error    | StdDev   | P80      | P95      | Ratio        | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |---------:|---------:|---------:|---------:|---------:|-------------:|--------:|-------:|----------:|------------:|
| TaskResult_Original_Bind  | 27.53 ns | 0.529 ns | 0.495 ns | 27.99 ns | 28.20 ns |     baseline |         | 0.0102 |     160 B |             |
| TaskResult_Optimized_Bind | 28.06 ns | 0.579 ns | 0.711 ns | 28.85 ns | 29.19 ns | 1.02x slower |   0.03x | 0.0102 |     160 B |  1.00x more |
| TaskResult_Original_Map2  | 16.80 ns | 0.358 ns | 0.413 ns | 17.21 ns | 17.43 ns | 1.64x faster |   0.05x | 0.0051 |      80 B |  2.00x less |
| TaskResult_Optimized_Map2 | 16.83 ns | 0.359 ns | 0.441 ns | 17.27 ns | 17.59 ns | 1.64x faster |   0.05x | 0.0051 |      80 B |  2.00x less |
