# Option.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two options and returns a tuple of the pair or None if either are None

## Function Signature

```fsharp
'left option -> 'right option -> ('left * 'right) option
```

## Examples

### Example 1

```fsharp
let left = Some 123
let right = Some "abc"

Option.zip left right
// Some (123, "abc")
```

### Example 2

```fsharp
let left = Some 123
let right = None

Option.zip left right
// None
```
