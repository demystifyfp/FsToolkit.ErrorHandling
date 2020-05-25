## Validation Computation Expression

Namespace: `FsToolkit.ErrorHandling`

The `Validation` type is defined as:

```fsharp
type Validation<'a,'err> = Result<'a, 'err list>


This CE can take advantage of the [and! operator](https://github.com/fsharp/fslang-suggestions/issues/579) to join multiple error results into a list.


## Examples:

### Example 1

The example from [Validation.map3](../validation/map3.md#example-1) can be solved using the `validation` computation expression as below:

```fsharp
// Validation<int, string>
let addResult = validation {
  let! x = tryParseInt "35"
  and! y = tryParseInt "5"
  and! z = tryParseInt "2"
  return add x y z
}
```

