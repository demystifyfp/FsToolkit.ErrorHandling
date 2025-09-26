```

BenchmarkDotNet v0.15.0, Windows 11 (10.0.26100.4652/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900F 2.40GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 9.0.202
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2


```
| Method                       | Mean      | Error     | StdDev    | P80       | P95       | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| Result_Normal_Map            | 0.0191 ns | 0.0091 ns | 0.0080 ns | 0.0237 ns | 0.0286 ns |     ? |       ? |         - |           ? |
| Result_Alt_InlinedLambda_Map | 0.0326 ns | 0.0113 ns | 0.0105 ns | 0.0397 ns | 0.0510 ns |     ? |       ? |         - |           ? |
