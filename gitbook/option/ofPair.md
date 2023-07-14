# Option.ofPair

Namespace: `FsToolkit.ErrorHandling`

Transforms a `bool * 'T` value to `'T Option`.

## Function Signature

```fsharp
bool * 'T -> 'T Option
```

## Examples

### Example 1

```fsharp
let opt = Option.ofPair (true, 1) 
// Some 1
```

### Example 2

```fsharp
let opt = Option.ofPair (false, 1)
// None
```
