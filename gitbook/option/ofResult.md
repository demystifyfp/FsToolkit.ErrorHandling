# Option.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` value to `'T Option`.

## Function Signature

```fsharp
Result<'T, 'Error> -> 'T Option
```

## Examples

### Example 1

```fsharp
let opt = Option.ofResult (Ok 1)
// Some 1
```

### Example 2

```fsharp
let opt = Option.ofResult (Error "error")
// None
```

