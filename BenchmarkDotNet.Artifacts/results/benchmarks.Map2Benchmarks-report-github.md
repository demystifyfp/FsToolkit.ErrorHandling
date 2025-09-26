```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                                  | Mean      | Error     | StdDev    | P80       | P95       | Ratio         | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------- |----------:|----------:|----------:|----------:|----------:|--------------:|--------:|-------:|----------:|------------:|
| Result_Normal_Map2                      | 5.4319 ns | 0.1336 ns | 0.1250 ns | 5.5590 ns | 5.6074 ns |      baseline |         | 0.0020 |      32 B |             |
| Result_NoComposition_Map2               | 5.3819 ns | 0.1338 ns | 0.1374 ns | 5.4547 ns | 5.6319 ns | 1.010x faster |   0.03x | 0.0020 |      32 B |  1.00x more |
| Result_Inlined_Map2                     | 4.3685 ns | 0.0862 ns | 0.0720 ns | 4.4150 ns | 4.4692 ns | 1.244x faster |   0.03x | 0.0020 |      32 B |  1.00x more |
| Result_Inlined_NoComposition_Map2       | 4.4818 ns | 0.1175 ns | 0.1759 ns | 4.6492 ns | 4.8047 ns | 1.214x faster |   0.05x | 0.0020 |      32 B |  1.00x more |
| Result_InlinedLambda_Map2               | 4.3156 ns | 0.1104 ns | 0.0979 ns | 4.3967 ns | 4.4356 ns | 1.259x faster |   0.04x | 0.0020 |      32 B |  1.00x more |
| Result_InlinedLambda_NoComposition_Map2 | 4.3959 ns | 0.1135 ns | 0.1832 ns | 4.5370 ns | 4.7538 ns | 1.238x faster |   0.06x | 0.0020 |      32 B |  1.00x more |
| Result_Alt_Map2                         | 3.9517 ns | 0.0469 ns | 0.0439 ns | 3.9870 ns | 4.0045 ns | 1.375x faster |   0.03x |      - |         - |          NA |
| Result_Alt_Inlined_Map2                 | 0.0121 ns | 0.0123 ns | 0.0115 ns | 0.0204 ns | 0.0333 ns |            NA |      NA |      - |         - |          NA |
| Result_Alt_InlinedLambda_Map2           | 0.0170 ns | 0.0082 ns | 0.0077 ns | 0.0230 ns | 0.0242 ns |            NA |      NA |      - |         - |          NA |
