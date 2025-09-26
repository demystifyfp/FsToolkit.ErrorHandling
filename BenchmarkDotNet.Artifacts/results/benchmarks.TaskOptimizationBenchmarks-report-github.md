```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method               | Mean     | Error    | StdDev   | P80      | P95      | Ratio        | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------- |---------:|---------:|---------:|---------:|---------:|-------------:|--------:|-------:|----------:|------------:|
| Task_Original_Map    | 12.68 ns | 0.279 ns | 0.391 ns | 12.96 ns | 13.20 ns |     baseline |         | 0.0092 |     144 B |             |
| Task_Optimized_Map   | 12.73 ns | 0.284 ns | 0.399 ns | 13.06 ns | 13.36 ns | 1.00x slower |   0.04x | 0.0092 |     144 B |  1.00x more |
| Task_Original_Apply  | 21.72 ns | 0.459 ns | 0.564 ns | 22.15 ns | 22.69 ns | 1.71x slower |   0.07x | 0.0138 |     216 B |  1.50x more |
| Task_Optimized_Apply | 21.99 ns | 0.469 ns | 0.577 ns | 22.48 ns | 23.11 ns | 1.74x slower |   0.07x | 0.0138 |     216 B |  1.50x more |
