## Option Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

The example from [Result CE](../result/ce.md#example-1) can be solved using the `option` computation expression as below:

```fsharp
// Option<int>
let addResult = option {
  let! x = tryParseInt "35"
  let! y = tryParseInt "5"
  let! z = tryParseInt "2"
  return add x y z
}
```