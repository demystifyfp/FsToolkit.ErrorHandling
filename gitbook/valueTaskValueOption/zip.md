# ValueTaskValueOption.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two voptions and returns a tuple of the pair or ValueNone if either are ValueNone

## Function Signature

```fsharp
ValueTask<'left voption> -> ValueTask<'right voption> -> ValueTask<('left * 'right) voption>
```

## Examples

### Example 1

```fsharp
let left = ValueTaskValueOption.some 123
let right = ValueTaskValueOption.some "abc"

ValueTaskValueOption.zip left right
// valueTask { ValueSome (123, "abc") }
```

### Example 2

```fsharp
let left = ValueTaskValueOption.some 123
let right = ValueTaskValueOption.none

ValueTaskValueOption.zip left right
// valueTask { ValueNone }
```
