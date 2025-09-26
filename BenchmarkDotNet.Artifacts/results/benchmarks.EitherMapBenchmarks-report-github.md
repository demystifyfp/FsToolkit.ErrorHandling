```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                                              | Mean       | Error     | StdDev    | Median     | P80        | P95        | Ratio             | RatioSD   | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|------------------:|----------:|-------:|----------:|------------:|
| Result_Normal_EitherMap                             | 41.2824 ns | 0.4097 ns | 0.3632 ns | 41.2079 ns | 41.5505 ns | 41.8396 ns |          baseline |           | 0.0030 |      48 B |             |
| Result_Normal_NoComposition_EitherMap               | 42.5274 ns | 0.7575 ns | 0.7086 ns | 42.3763 ns | 43.3329 ns | 43.5685 ns |     1.030x slower |     0.02x | 0.0030 |      48 B |  1.00x more |
| Result_Inlined_EitherMap                            |  0.0267 ns | 0.0148 ns | 0.0138 ns |  0.0269 ns |  0.0396 ns |  0.0458 ns | 3,321.381x faster | 4,588.78x |      - |         - |          NA |
| Result_Inlined_NoComposition_EitherMap              |  0.0081 ns | 0.0064 ns | 0.0059 ns |  0.0091 ns |  0.0143 ns |  0.0151 ns |                NA |        NA |      - |         - |          NA |
| Result_InlinedLambda_EitherMap                      |  0.0139 ns | 0.0107 ns | 0.0100 ns |  0.0156 ns |  0.0190 ns |  0.0314 ns |                NA |        NA |      - |         - |          NA |
| Result_Normal_InlinedLambda_NoComposition_EitherMap |  0.0132 ns | 0.0161 ns | 0.0150 ns |  0.0068 ns |  0.0294 ns |  0.0394 ns |                NA |        NA |      - |         - |          NA |
| Result_Alt_EitherMap                                |  0.2275 ns | 0.0173 ns | 0.0153 ns |  0.2271 ns |  0.2372 ns |  0.2495 ns |   182.243x faster |    12.26x |      - |         - |          NA |
| Result_Alt_Inlined_EitherMap                        |  0.0209 ns | 0.0154 ns | 0.0144 ns |  0.0135 ns |  0.0337 ns |  0.0442 ns | 3,199.092x faster | 2,604.59x |      - |         - |          NA |
| Result_Alt_InlinedLambda_EitherMap                  |  0.0332 ns | 0.0104 ns | 0.0087 ns |  0.0374 ns |  0.0393 ns |  0.0408 ns | 1,367.892x faster |   506.50x |      - |         - |          NA |
