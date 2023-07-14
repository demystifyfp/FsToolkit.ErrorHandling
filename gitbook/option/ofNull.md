# Option.ofNull

Namespace: `FsToolkit.ErrorHandling`

Transforms a `nullableValue` value to `'nullableValue Option`.

## Function Signature

```fsharp
'nullableValue -> 'nullableValue Option
```

## Examples

### Example 1

```fsharp
let opt = Option.ofNull 1
// Some 1
```

### Example 2

```fsharp
let opt = Option.ofNull null
// None
```

