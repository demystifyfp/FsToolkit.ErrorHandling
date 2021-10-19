``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Big Sur 11.5.2 (20G95) [Darwin 20.6.0]
Intel Core i9-9980HK CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=5.0.103
  [Host]     : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT DEBUG
  DefaultJob : .NET 5.0.6 (5.0.621.22011), X64 RyuJIT


```
|               Method |       Mean |     Error |    StdDev |     Median |
|--------------------- |-----------:|----------:|----------:|-----------:|
|           ResultMap2 | 7,417.8 ns | 147.85 ns | 376.34 ns | 7,273.3 ns |
|        ResultAltMap2 | 2,270.1 ns |  29.87 ns |  27.94 ns | 2,275.7 ns |
|    ResultInlinedMap2 | 3,553.9 ns |  39.19 ns |  36.66 ns | 3,559.3 ns |
| ResultAltInlinedMap2 |   874.0 ns |  11.02 ns |   9.77 ns |   873.7 ns |
