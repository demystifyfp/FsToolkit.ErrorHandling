```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                                      | Mean        | Error     | StdDev    | P80         | P95         | Ratio             | RatioSD   | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------- |------------:|----------:|----------:|------------:|------------:|------------------:|----------:|-------:|----------:|------------:|
| Result_Normal_Bind_CE                       | 114.1285 ns | 0.6707 ns | 0.6274 ns | 114.6632 ns | 114.7984 ns |          baseline |           | 0.0014 |      24 B |             |
| Result_Alt_Inlined_Bind_CE                  |   2.1060 ns | 0.0149 ns | 0.0139 ns |   2.1177 ns |   2.1299 ns |    54.195x faster |     0.45x |      - |         - |          NA |
| Result_Alt_InlinedLambda_Bind_Same_CE       |   0.8549 ns | 0.0160 ns | 0.0150 ns |   0.8701 ns |   0.8717 ns |   133.531x faster |     2.40x |      - |         - |          NA |
| Result_Alt_InlinedLambda_Bind_CE            |   1.0587 ns | 0.0181 ns | 0.0169 ns |   1.0720 ns |   1.0792 ns |   107.828x faster |     1.77x |      - |         - |          NA |
| Result_Normal_Bind_CE_Error                 | 112.8824 ns | 0.7177 ns | 0.6362 ns | 113.4376 ns | 113.8665 ns |     1.011x faster |     0.01x | 0.0014 |      24 B |  1.00x more |
| Result_Alt_Inlined_Bind_CE_Error            |   2.0847 ns | 0.0334 ns | 0.0312 ns |   2.1158 ns |   2.1321 ns |    54.757x faster |     0.84x |      - |         - |          NA |
| Result_Alt_InlinedLambda_Bind_Same_CE_Error |   0.0311 ns | 0.0087 ns | 0.0081 ns |   0.0359 ns |   0.0443 ns | 3,935.129x faster | 1,081.18x |      - |         - |          NA |
| Result_Alt_InlinedLambda_Bind_CE_Error      |   0.0250 ns | 0.0115 ns | 0.0107 ns |   0.0360 ns |   0.0390 ns | 5,572.306x faster | 2,541.88x |      - |         - |          NA |
