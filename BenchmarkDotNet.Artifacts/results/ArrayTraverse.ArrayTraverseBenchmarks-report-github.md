```

BenchmarkDotNet v0.16.0-preview.2, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M3 Max, 1 CPU, 14 logical and 14 physical cores
Memory: 36 GB Total, 6.3 GB Available
.NET SDK 11.0.100
  [Host]   : .NET 11.0.0 (11.0.0, 11.0.26.53018), Arm64 RyuJIT armv8.0-a DEBUG
  ShortRun : .NET 11.0.0 (11.0.0, 11.0.26.53018), Arm64 RyuJIT armv8.0-a

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method           | Size | HasErrors | Mean          | Error       | StdDev     | P80           | P95           | Ratio        | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------- |----- |---------- |--------------:|------------:|-----------:|--------------:|--------------:|-------------:|--------:|-------:|-------:|----------:|------------:|
| **TraverseResultA**  | **0**    | **False**     |     **4.2430 ns** |   **1.3637 ns** |  **0.0747 ns** |     **4.2850 ns** |     **4.3152 ns** |     **baseline** |        **** | **0.0038** |      **-** |      **32 B** |            **** |
| TraverseResultA1 | 0    | False     |     1.6267 ns |   0.0960 ns |  0.0053 ns |     1.6295 ns |     1.6319 ns | 2.61x faster |   0.04x |      - |      - |         - |          NA |
| TraverseResultA2 | 0    | False     |     0.6674 ns |   0.0139 ns |  0.0008 ns |     0.6679 ns |     0.6679 ns | 6.36x faster |   0.10x |      - |      - |         - |          NA |
| TraverseResultA3 | 0    | False     |     0.6673 ns |   0.0047 ns |  0.0003 ns |     0.6674 ns |     0.6675 ns | 6.36x faster |   0.10x |      - |      - |         - |          NA |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **0**    | **True**      |     **4.1853 ns** |   **0.2270 ns** |  **0.0124 ns** |     **4.1926 ns** |     **4.1969 ns** |     **baseline** |        **** | **0.0038** |      **-** |      **32 B** |            **** |
| TraverseResultA1 | 0    | True      |     1.6511 ns |   0.2223 ns |  0.0122 ns |     1.6569 ns |     1.6631 ns | 2.53x faster |   0.02x |      - |      - |         - |          NA |
| TraverseResultA2 | 0    | True      |     0.6697 ns |   0.0767 ns |  0.0042 ns |     0.6720 ns |     0.6738 ns | 6.25x faster |   0.04x |      - |      - |         - |          NA |
| TraverseResultA3 | 0    | True      |     0.6664 ns |   0.0033 ns |  0.0002 ns |     0.6665 ns |     0.6665 ns | 6.28x faster |   0.02x |      - |      - |         - |          NA |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **1**    | **False**     |    **12.4251 ns** |   **0.8178 ns** |  **0.0448 ns** |    **12.4523 ns** |    **12.4583 ns** |     **baseline** |        **** | **0.0115** | **0.0000** |      **96 B** |            **** |
| TraverseResultA1 | 1    | False     |    12.3770 ns |   0.5907 ns |  0.0324 ns |    12.3962 ns |    12.4068 ns | 1.00x faster |   0.00x | 0.0115 | 0.0000 |      96 B |  1.00x more |
| TraverseResultA2 | 1    | False     |    12.9615 ns |   1.2456 ns |  0.0683 ns |    13.0032 ns |    13.0180 ns | 1.04x slower |   0.01x | 0.0115 | 0.0000 |      96 B |  1.00x more |
| TraverseResultA3 | 1    | False     |     7.5457 ns |   0.2449 ns |  0.0134 ns |     7.5537 ns |     7.5545 ns | 1.65x faster |   0.01x | 0.0038 |      - |      32 B |  3.00x less |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **1**    | **True**      |    **22.3204 ns** |   **0.3726 ns** |  **0.0204 ns** |    **22.3328 ns** |    **22.3363 ns** |     **baseline** |        **** | **0.0229** | **0.0000** |     **192 B** |            **** |
| TraverseResultA1 | 1    | True      |    18.2757 ns |   1.7880 ns |  0.0980 ns |    18.3269 ns |    18.3722 ns | 1.22x faster |   0.01x | 0.0191 | 0.0000 |     160 B |  1.20x less |
| TraverseResultA2 | 1    | True      |    18.5728 ns |   0.6691 ns |  0.0367 ns |    18.5915 ns |    18.6090 ns | 1.20x faster |   0.00x | 0.0191 | 0.0000 |     160 B |  1.20x less |
| TraverseResultA3 | 1    | True      |     7.2961 ns |   0.5155 ns |  0.0283 ns |     7.3114 ns |     7.3238 ns | 3.06x faster |   0.01x | 0.0038 | 0.0000 |      32 B |  6.00x less |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **2**    | **False**     |    **13.1568 ns** |   **2.9273 ns** |  **0.1605 ns** |    **13.2457 ns** |    **13.3125 ns** |     **baseline** |        **** | **0.0115** | **0.0000** |      **96 B** |            **** |
| TraverseResultA1 | 2    | False     |    13.6963 ns |   2.9501 ns |  0.1617 ns |    13.7939 ns |    13.8392 ns | 1.04x slower |   0.02x | 0.0115 | 0.0000 |      96 B |  1.00x more |
| TraverseResultA2 | 2    | False     |    14.2754 ns |   0.6785 ns |  0.0372 ns |    14.2980 ns |    14.3071 ns | 1.09x slower |   0.01x | 0.0115 | 0.0000 |      96 B |  1.00x more |
| TraverseResultA3 | 2    | False     |     8.7891 ns |   0.1060 ns |  0.0058 ns |     8.7926 ns |     8.7934 ns | 1.50x faster |   0.02x | 0.0038 |      - |      32 B |  3.00x less |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **2**    | **True**      |    **25.2166 ns** |   **0.6663 ns** |  **0.0365 ns** |    **25.2356 ns** |    **25.2526 ns** |     **baseline** |        **** | **0.0229** | **0.0000** |     **192 B** |            **** |
| TraverseResultA1 | 2    | True      |    19.8954 ns |   4.1104 ns |  0.2253 ns |    20.0280 ns |    20.0364 ns | 1.27x faster |   0.01x | 0.0191 | 0.0000 |     160 B |  1.20x less |
| TraverseResultA2 | 2    | True      |    19.9904 ns |   0.5481 ns |  0.0300 ns |    20.0086 ns |    20.0167 ns | 1.26x faster |   0.00x | 0.0191 | 0.0000 |     160 B |  1.20x less |
| TraverseResultA3 | 2    | True      |     8.5115 ns |   1.6390 ns |  0.0898 ns |     8.5639 ns |     8.5959 ns | 2.96x faster |   0.03x | 0.0038 |      - |      32 B |  6.00x less |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **3**    | **False**     |    **15.1275 ns** |   **2.0812 ns** |  **0.1141 ns** |    **15.1961 ns** |    **15.2062 ns** |     **baseline** |        **** | **0.0134** | **0.0000** |     **112 B** |            **** |
| TraverseResultA1 | 3    | False     |    15.5121 ns |   0.6768 ns |  0.0371 ns |    15.5339 ns |    15.5465 ns | 1.03x slower |   0.01x | 0.0134 | 0.0000 |     112 B |  1.00x more |
| TraverseResultA2 | 3    | False     |    15.5173 ns |   0.3550 ns |  0.0195 ns |    15.5280 ns |    15.5362 ns | 1.03x slower |   0.01x | 0.0134 | 0.0000 |     112 B |  1.00x more |
| TraverseResultA3 | 3    | False     |    17.8416 ns |   6.3570 ns |  0.3484 ns |    18.0066 ns |    18.1845 ns | 1.18x slower |   0.02x | 0.0134 | 0.0000 |     112 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **3**    | **True**      |    **25.0766 ns** |   **0.7493 ns** |  **0.0411 ns** |    **25.1014 ns** |    **25.1128 ns** |     **baseline** |        **** | **0.0239** | **0.0000** |     **200 B** |            **** |
| TraverseResultA1 | 3    | True      |    21.6809 ns |   0.5891 ns |  0.0323 ns |    21.7004 ns |    21.7094 ns | 1.16x faster |   0.00x | 0.0201 | 0.0000 |     168 B |  1.19x less |
| TraverseResultA2 | 3    | True      |    21.2373 ns |   0.5961 ns |  0.0327 ns |    21.2566 ns |    21.2581 ns | 1.18x faster |   0.00x | 0.0201 | 0.0000 |     168 B |  1.19x less |
| TraverseResultA3 | 3    | True      |    21.5993 ns |   0.4105 ns |  0.0225 ns |    21.6126 ns |    21.6136 ns | 1.16x faster |   0.00x | 0.0201 | 0.0000 |     168 B |  1.19x less |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **10**   | **False**     |    **30.6677 ns** |   **1.7189 ns** |  **0.0942 ns** |    **30.7196 ns** |    **30.7594 ns** |     **baseline** |        **** | **0.0191** | **0.0001** |     **160 B** |            **** |
| TraverseResultA1 | 10   | False     |    31.5495 ns |   0.9524 ns |  0.0522 ns |    31.5796 ns |    31.5991 ns | 1.03x slower |   0.00x | 0.0191 | 0.0001 |     160 B |  1.00x more |
| TraverseResultA2 | 10   | False     |    32.1371 ns |   0.2679 ns |  0.0147 ns |    32.1441 ns |    32.1516 ns | 1.05x slower |   0.00x | 0.0191 | 0.0001 |     160 B |  1.00x more |
| TraverseResultA3 | 10   | False     |    32.3641 ns |   0.4690 ns |  0.0257 ns |    32.3791 ns |    32.3883 ns | 1.06x slower |   0.00x | 0.0191 | 0.0001 |     160 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **10**   | **True**      |    **30.6171 ns** |   **0.1898 ns** |  **0.0104 ns** |    **30.6228 ns** |    **30.6272 ns** |     **baseline** |        **** | **0.0268** | **0.0001** |     **224 B** |            **** |
| TraverseResultA1 | 10   | True      |    26.6355 ns |   0.3478 ns |  0.0191 ns |    26.6449 ns |    26.6543 ns | 1.15x faster |   0.00x | 0.0249 | 0.0001 |     208 B |  1.08x less |
| TraverseResultA2 | 10   | True      |    27.2264 ns |   5.1476 ns |  0.2822 ns |    27.3986 ns |    27.4616 ns | 1.12x faster |   0.01x | 0.0249 | 0.0001 |     208 B |  1.08x less |
| TraverseResultA3 | 10   | True      |    27.2502 ns |   1.2687 ns |  0.0695 ns |    27.2887 ns |    27.3178 ns | 1.12x faster |   0.00x | 0.0249 | 0.0001 |     208 B |  1.08x less |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **100**  | **False**     |   **252.8141 ns** |   **3.9084 ns** |  **0.2142 ns** |   **252.9450 ns** |   **252.9862 ns** |     **baseline** |        **** | **0.1049** | **0.0024** |     **880 B** |            **** |
| TraverseResultA1 | 100  | False     |   234.0091 ns |  42.0949 ns |  2.3074 ns |   235.0775 ns |   236.2744 ns | 1.08x faster |   0.01x | 0.1051 | 0.0026 |     880 B |  1.00x more |
| TraverseResultA2 | 100  | False     |   240.5750 ns |  15.2267 ns |  0.8346 ns |   241.0708 ns |   241.3407 ns | 1.05x faster |   0.00x | 0.1049 | 0.0024 |     880 B |  1.00x more |
| TraverseResultA3 | 100  | False     |   236.0221 ns |  47.0658 ns |  2.5798 ns |   237.4469 ns |   238.5305 ns | 1.07x faster |   0.01x | 0.1049 | 0.0024 |     880 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **100**  | **True**      |   **145.5698 ns** |  **24.1864 ns** |  **1.3257 ns** |   **146.3759 ns** |   **146.5613 ns** |     **baseline** |        **** | **0.0696** |      **-** |     **584 B** |            **** |
| TraverseResultA1 | 100  | True      |   147.4793 ns |   5.7602 ns |  0.3157 ns |   147.6542 ns |   147.7860 ns | 1.01x slower |   0.01x | 0.0889 | 0.0002 |     744 B |  1.27x more |
| TraverseResultA2 | 100  | True      |   149.1087 ns |   7.3008 ns |  0.4002 ns |   149.3375 ns |   149.4911 ns | 1.02x slower |   0.01x | 0.0889 | 0.0002 |     744 B |  1.27x more |
| TraverseResultA3 | 100  | True      |   150.9591 ns |  69.3585 ns |  3.8018 ns |   152.8121 ns |   154.7075 ns | 1.04x slower |   0.02x | 0.0889 | 0.0002 |     744 B |  1.27x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **1000** | **False**     | **2,296.6312 ns** |  **95.0484 ns** |  **5.2099 ns** | **2,299.4980 ns** | **2,301.7035 ns** |     **baseline** |        **** | **0.9651** | **0.2403** |    **8080 B** |            **** |
| TraverseResultA1 | 1000 | False     | 2,298.9423 ns | 580.0278 ns | 31.7933 ns | 2,314.6098 ns | 2,330.3023 ns | 1.00x slower |   0.01x | 0.9651 | 0.2403 |    8080 B |  1.00x more |
| TraverseResultA2 | 1000 | False     | 2,314.4281 ns | 345.5410 ns | 18.9403 ns | 2,324.1892 ns | 2,333.0951 ns | 1.01x slower |   0.01x | 0.9651 | 0.2403 |    8080 B |  1.00x more |
| TraverseResultA3 | 1000 | False     | 2,339.9192 ns | 681.9394 ns | 37.3794 ns | 2,362.7581 ns | 2,370.2098 ns | 1.02x slower |   0.01x | 0.9651 | 0.2403 |    8080 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultA**  | **1000** | **True**      | **1,356.8740 ns** | **237.0940 ns** | **12.9959 ns** | **1,363.1309 ns** | **1,369.6788 ns** |     **baseline** |        **** | **0.4997** | **0.0076** |    **4184 B** |            **** |
| TraverseResultA1 | 1000 | True      | 1,397.6721 ns | 121.1795 ns |  6.6423 ns | 1,400.8778 ns | 1,404.2176 ns | 1.03x slower |   0.01x | 0.7343 | 0.0057 |    6144 B |  1.47x more |
| TraverseResultA2 | 1000 | True      | 1,391.4056 ns | 156.7828 ns |  8.5938 ns | 1,396.4352 ns | 1,396.6524 ns | 1.03x slower |   0.01x | 0.7343 | 0.0057 |    6144 B |  1.47x more |
| TraverseResultA3 | 1000 | True      | 1,385.8862 ns |  34.3901 ns |  1.8850 ns | 1,387.0213 ns | 1,387.2051 ns | 1.02x slower |   0.01x | 0.7343 | 0.0057 |    6144 B |  1.47x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **0**    | **False**     |     **4.0167 ns** |   **0.1022 ns** |  **0.0056 ns** |     **4.0200 ns** |     **4.0202 ns** |     **baseline** |        **** | **0.0038** |      **-** |      **32 B** |            **** |
| TraverseResultM1 | 0    | False     |     1.7548 ns |   0.0408 ns |  0.0022 ns |     1.7560 ns |     1.7569 ns | 2.29x faster |   0.00x |      - |      - |         - |          NA |
| TraverseResultM2 | 0    | False     |     2.2132 ns |   0.0265 ns |  0.0015 ns |     2.2139 ns |     2.2146 ns | 1.81x faster |   0.00x |      - |      - |         - |          NA |
| TraverseResultM3 | 0    | False     |     1.7577 ns |   0.0063 ns |  0.0003 ns |     1.7579 ns |     1.7579 ns | 2.29x faster |   0.00x |      - |      - |         - |          NA |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **0**    | **True**      |     **3.9457 ns** |   **0.0319 ns** |  **0.0017 ns** |     **3.9468 ns** |     **3.9471 ns** |     **baseline** |        **** | **0.0038** |      **-** |      **32 B** |            **** |
| TraverseResultM1 | 0    | True      |     1.7571 ns |   0.0110 ns |  0.0006 ns |     1.7575 ns |     1.7577 ns | 2.25x faster |   0.00x |      - |      - |         - |          NA |
| TraverseResultM2 | 0    | True      |     2.2179 ns |   0.0134 ns |  0.0007 ns |     2.2184 ns |     2.2185 ns | 1.78x faster |   0.00x |      - |      - |         - |          NA |
| TraverseResultM3 | 0    | True      |     1.7688 ns |   0.0140 ns |  0.0008 ns |     1.7693 ns |     1.7694 ns | 2.23x faster |   0.00x |      - |      - |         - |          NA |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **1**    | **False**     |    **11.9670 ns** |   **1.2540 ns** |  **0.0687 ns** |    **12.0028 ns** |    **12.0347 ns** |     **baseline** |        **** | **0.0115** |      **-** |      **96 B** |            **** |
| TraverseResultM1 | 1    | False     |    12.2962 ns |   0.5002 ns |  0.0274 ns |    12.3128 ns |    12.3203 ns | 1.03x slower |   0.01x | 0.0115 |      - |      96 B |  1.00x more |
| TraverseResultM2 | 1    | False     |     6.1405 ns |   0.3266 ns |  0.0179 ns |     6.1497 ns |     6.1582 ns | 1.95x faster |   0.01x | 0.0038 |      - |      32 B |  3.00x less |
| TraverseResultM3 | 1    | False     |    11.6728 ns |   2.7573 ns |  0.1511 ns |    11.7496 ns |    11.8219 ns | 1.03x faster |   0.01x | 0.0115 |      - |      96 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **1**    | **True**      |     **6.9398 ns** |   **0.0941 ns** |  **0.0052 ns** |     **6.9429 ns** |     **6.9444 ns** |     **baseline** |        **** | **0.0077** |      **-** |      **64 B** |            **** |
| TraverseResultM1 | 1    | True      |     6.9192 ns |   0.4077 ns |  0.0223 ns |     6.9323 ns |     6.9331 ns | 1.00x faster |   0.00x | 0.0077 |      - |      64 B |  1.00x more |
| TraverseResultM2 | 1    | True      |     3.8240 ns |   0.0438 ns |  0.0024 ns |     3.8255 ns |     3.8261 ns | 1.81x faster |   0.00x |      - |      - |         - |          NA |
| TraverseResultM3 | 1    | True      |     6.8813 ns |   0.6958 ns |  0.0381 ns |     6.9015 ns |     6.9188 ns | 1.01x faster |   0.00x | 0.0077 |      - |      64 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **2**    | **False**     |    **12.6134 ns** |   **0.5296 ns** |  **0.0290 ns** |    **12.6305 ns** |    **12.6405 ns** |     **baseline** |        **** | **0.0115** |      **-** |      **96 B** |            **** |
| TraverseResultM1 | 2    | False     |    12.1592 ns |   0.0529 ns |  0.0029 ns |    12.1605 ns |    12.1620 ns | 1.04x faster |   0.00x | 0.0115 |      - |      96 B |  1.00x more |
| TraverseResultM2 | 2    | False     |    12.7229 ns |   0.8901 ns |  0.0488 ns |    12.7524 ns |    12.7661 ns | 1.01x slower |   0.00x | 0.0115 |      - |      96 B |  1.00x more |
| TraverseResultM3 | 2    | False     |    12.4593 ns |   0.9053 ns |  0.0496 ns |    12.4885 ns |    12.5054 ns | 1.01x faster |   0.00x | 0.0115 |      - |      96 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **2**    | **True**      |     **8.5295 ns** |   **0.6554 ns** |  **0.0359 ns** |     **8.5479 ns** |     **8.5649 ns** |     **baseline** |        **** | **0.0076** |      **-** |      **64 B** |            **** |
| TraverseResultM1 | 2    | True      |     7.7096 ns |   0.3077 ns |  0.0169 ns |     7.7177 ns |     7.7262 ns | 1.11x faster |   0.00x | 0.0076 |      - |      64 B |  1.00x more |
| TraverseResultM2 | 2    | True      |     7.9749 ns |   0.4733 ns |  0.0259 ns |     7.9871 ns |     8.0004 ns | 1.07x faster |   0.00x | 0.0076 |      - |      64 B |  1.00x more |
| TraverseResultM3 | 2    | True      |     7.6945 ns |   0.6122 ns |  0.0336 ns |     7.7129 ns |     7.7272 ns | 1.11x faster |   0.01x | 0.0076 |      - |      64 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **3**    | **False**     |    **14.5328 ns** |   **0.2806 ns** |  **0.0154 ns** |    **14.5412 ns** |    **14.5479 ns** |     **baseline** |        **** | **0.0134** |      **-** |     **112 B** |            **** |
| TraverseResultM1 | 3    | False     |    13.4061 ns |   0.3997 ns |  0.0219 ns |    13.4174 ns |    13.4277 ns | 1.08x faster |   0.00x | 0.0134 |      - |     112 B |  1.00x more |
| TraverseResultM2 | 3    | False     |    13.9555 ns |   0.1658 ns |  0.0091 ns |    13.9609 ns |    13.9639 ns | 1.04x faster |   0.00x | 0.0134 |      - |     112 B |  1.00x more |
| TraverseResultM3 | 3    | False     |    13.5557 ns |   1.1237 ns |  0.0616 ns |    13.5933 ns |    13.6032 ns | 1.07x faster |   0.00x | 0.0134 |      - |     112 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **3**    | **True**      |     **8.9120 ns** |   **0.4004 ns** |  **0.0219 ns** |     **8.9250 ns** |     **8.9259 ns** |     **baseline** |        **** | **0.0086** |      **-** |      **72 B** |            **** |
| TraverseResultM1 | 3    | True      |     8.4867 ns |   0.1256 ns |  0.0069 ns |     8.4909 ns |     8.4925 ns | 1.05x faster |   0.00x | 0.0086 |      - |      72 B |  1.00x more |
| TraverseResultM2 | 3    | True      |     8.2242 ns |   0.2167 ns |  0.0119 ns |     8.2312 ns |     8.2316 ns | 1.08x faster |   0.00x | 0.0086 |      - |      72 B |  1.00x more |
| TraverseResultM3 | 3    | True      |     8.0194 ns |   2.0997 ns |  0.1151 ns |     8.0807 ns |     8.1324 ns | 1.11x faster |   0.01x | 0.0086 |      - |      72 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **10**   | **False**     |    **22.0611 ns** |   **5.0364 ns** |  **0.2761 ns** |    **22.2079 ns** |    **22.3322 ns** |     **baseline** |        **** | **0.0191** |      **-** |     **160 B** |            **** |
| TraverseResultM1 | 10   | False     |    21.0063 ns |   1.1115 ns |  0.0609 ns |    21.0428 ns |    21.0481 ns | 1.05x faster |   0.01x | 0.0191 |      - |     160 B |  1.00x more |
| TraverseResultM2 | 10   | False     |    21.4860 ns |   0.3472 ns |  0.0190 ns |    21.4976 ns |    21.5017 ns | 1.03x faster |   0.01x | 0.0191 |      - |     160 B |  1.00x more |
| TraverseResultM3 | 10   | False     |    20.9853 ns |   1.2742 ns |  0.0698 ns |    21.0225 ns |    21.0538 ns | 1.05x faster |   0.01x | 0.0191 |      - |     160 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **10**   | **True**      |    **13.0164 ns** |   **0.3642 ns** |  **0.0200 ns** |    **13.0284 ns** |    **13.0342 ns** |     **baseline** |        **** | **0.0115** |      **-** |      **96 B** |            **** |
| TraverseResultM1 | 10   | True      |    13.0677 ns |   0.6501 ns |  0.0356 ns |    13.0874 ns |    13.1023 ns | 1.00x slower |   0.00x | 0.0115 |      - |      96 B |  1.00x more |
| TraverseResultM2 | 10   | True      |    12.6898 ns |   0.7064 ns |  0.0387 ns |    12.7134 ns |    12.7193 ns | 1.03x faster |   0.00x | 0.0115 |      - |      96 B |  1.00x more |
| TraverseResultM3 | 10   | True      |    12.5020 ns |   1.2447 ns |  0.0682 ns |    12.5369 ns |    12.5693 ns | 1.04x faster |   0.01x | 0.0115 |      - |      96 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **100**  | **False**     |   **129.2796 ns** |  **10.1378 ns** |  **0.5557 ns** |   **129.5492 ns** |   **129.8274 ns** |     **baseline** |        **** | **0.1051** |      **-** |     **880 B** |            **** |
| TraverseResultM1 | 100  | False     |   121.8759 ns |   3.1856 ns |  0.1746 ns |   121.9701 ns |   122.0468 ns | 1.06x faster |   0.00x | 0.1051 |      - |     880 B |  1.00x more |
| TraverseResultM2 | 100  | False     |   123.2017 ns |   5.2050 ns |  0.2853 ns |   123.3659 ns |   123.4729 ns | 1.05x faster |   0.00x | 0.1051 |      - |     880 B |  1.00x more |
| TraverseResultM3 | 100  | False     |   121.5782 ns |   5.1735 ns |  0.2836 ns |   121.7279 ns |   121.8570 ns | 1.06x faster |   0.00x | 0.1051 |      - |     880 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **100**  | **True**      |    **55.1113 ns** |   **3.1054 ns** |  **0.1702 ns** |    **55.1930 ns** |    **55.2790 ns** |     **baseline** |        **** | **0.0545** |      **-** |     **456 B** |            **** |
| TraverseResultM1 | 100  | True      |    56.3395 ns |   2.0227 ns |  0.1109 ns |    56.4054 ns |    56.4413 ns | 1.02x slower |   0.00x | 0.0545 |      - |     456 B |  1.00x more |
| TraverseResultM2 | 100  | True      |    51.7404 ns |   1.1824 ns |  0.0648 ns |    51.7794 ns |    51.7858 ns | 1.07x faster |   0.00x | 0.0545 |      - |     456 B |  1.00x more |
| TraverseResultM3 | 100  | True      |    51.5151 ns |   1.9855 ns |  0.1088 ns |    51.5763 ns |    51.6201 ns | 1.07x faster |   0.00x | 0.0545 |      - |     456 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **1000** | **False**     | **1,118.3357 ns** |  **64.9718 ns** |  **3.5613 ns** | **1,120.1070 ns** | **1,121.8491 ns** |     **baseline** |        **** | **0.9651** | **0.0057** |    **8080 B** |            **** |
| TraverseResultM1 | 1000 | False     | 1,113.6045 ns |  42.6342 ns |  2.3369 ns | 1,114.9364 ns | 1,115.8422 ns | 1.00x faster |   0.00x | 0.9651 | 0.0057 |    8080 B |  1.00x more |
| TraverseResultM2 | 1000 | False     | 1,127.8421 ns |  92.8710 ns |  5.0906 ns | 1,130.2841 ns | 1,132.8565 ns | 1.01x slower |   0.00x | 0.9651 | 0.0057 |    8080 B |  1.00x more |
| TraverseResultM3 | 1000 | False     | 1,114.6618 ns |  10.5955 ns |  0.5808 ns | 1,114.9710 ns | 1,115.2319 ns | 1.00x faster |   0.00x | 0.9651 | 0.0057 |    8080 B |  1.00x more |
|                  |      |           |               |             |            |               |               |              |         |        |        |           |             |
| **TraverseResultM**  | **1000** | **True**      |   **499.4420 ns** |   **5.5439 ns** |  **0.3039 ns** |   **499.6159 ns** |   **499.7321 ns** |     **baseline** |        **** | **0.4845** |      **-** |    **4056 B** |            **** |
| TraverseResultM1 | 1000 | True      |   510.1690 ns |  29.4478 ns |  1.6141 ns |   510.9653 ns |   511.7611 ns | 1.02x slower |   0.00x | 0.4845 |      - |    4056 B |  1.00x more |
| TraverseResultM2 | 1000 | True      |   464.9951 ns |  10.9414 ns |  0.5997 ns |   465.3585 ns |   465.4313 ns | 1.07x faster |   0.00x | 0.4845 |      - |    4056 B |  1.00x more |
| TraverseResultM3 | 1000 | True      |   464.3532 ns |  11.6026 ns |  0.6360 ns |   464.7218 ns |   464.9543 ns | 1.08x faster |   0.00x | 0.4845 |      - |    4056 B |  1.00x more |
