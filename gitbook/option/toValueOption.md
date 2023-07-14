# Option.toValueOption

Namespace: `FsToolkit.ErrorHandling`

Transforms a `'T Option` value to `'T voption`.

## Function Signature

```fsharp
'T Option -> 'T voption
```

## Examples

### Example 1

```fsharp
let opt = Option.toValueOption (Some 1)
// Some 1
```

### Example 2

```fsharp
let opt = Option.toValueOption None
// None
```

