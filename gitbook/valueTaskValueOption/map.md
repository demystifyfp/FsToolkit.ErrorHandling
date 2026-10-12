# ValueTaskValueOption.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the value of a value task voption if it is `ValueSome`. If the voption is `ValueNone`, return `ValueNone`.

## Function Signature

```fsharp
('input -> 'output) -> ValueTask<'input voption> -> ValueTask<'output voption>
```

## Examples

### Example 1

```fsharp
ValueTaskValueOption.map (fun x -> x + 1) (ValueTaskValueOption.some 1)

// valueTask { ValueSome 2 }
```

### Example 2

```fsharp
ValueTaskValueOption.map (fun x -> x + 1) ValueTaskValueOption.none

// valueTask { ValueNone }
```
