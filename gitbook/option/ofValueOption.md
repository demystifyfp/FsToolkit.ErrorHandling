# Option.ofValueOption

Namespace: `FsToolkit.ErrorHandling`

Transforms a `'T voption` value to `'T Option`.

## Function Signature

```fsharp
'T voption -> 'T Option
```

## Examples

### Example 1

```fsharp
let x : int voption = Some 1
let opt = Option.ofValueOption x
// Some 1
```

### Example 2

```fsharp
let x : int voption = None
let opt = Option.ofValueOption x
// None
```

