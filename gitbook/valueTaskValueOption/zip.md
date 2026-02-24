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
let left = ValueTaskValueOption.valueSome 123
let right = ValueTaskValueOption.valueSome "abc"

ValueTaskValueOption.zip left right
// valueTask { ValueSome (123, "abc") }
```

### Example 2

```fsharp
let left = ValueTaskValueOption.valueSome 123
let right = ValueTask<_>(ValueNone)

ValueTaskValueOption.zip left right
// valueTask { ValueNone }
```
