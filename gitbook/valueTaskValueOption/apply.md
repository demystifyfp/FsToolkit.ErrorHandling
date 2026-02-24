# ValueTaskValueOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
ValueTask<('a -> 'b) voption> -> ValueTask<'a voption> -> ValueTask<'b voption>
```

## Examples

Take the following function for example

```fsharp
// string -> int
let characterCount (s: string) = s.Length
```

### Example 1

```fsharp
let result =
    ValueTaskValueOption.valueSome "foo" // ValueTask<string voption>
    |> ValueTaskValueOption.apply (ValueTaskValueOption.valueSome characterCount) // ValueTask<int voption>

// valueTask { ValueSome 3 }
```

### Example 2

```fsharp
let result =
    ValueTask<_>(ValueNone) // ValueTask<string voption>
    |> ValueTaskValueOption.apply (ValueTaskValueOption.valueSome characterCount) // ValueTask<int voption>

// valueTask { ValueNone }
```

### Example 3

```fsharp
let result : ValueTask<int voption> =
    ValueTaskValueOption.valueSome "foo" // ValueTask<string voption>
    |> ValueTaskValueOption.apply (ValueTask<_>(ValueNone)) // ValueTask<int voption>

// valueTask { ValueNone }
```
