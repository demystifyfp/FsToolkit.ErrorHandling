## Option Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

```fsharp
// Option<int>
let addResult = option {
  let! x = tryParseInt "35"
  let! y = tryParseInt "5"
  let! z = tryParseInt "2"
  return add x y z
}
```
