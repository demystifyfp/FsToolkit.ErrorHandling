# ValueTaskValueOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the voption. If the voption is `ValueSome`, the first function will be executed. If the voption is `ValueNone`, the second function will be executed.

```fsharp
(onValueSome : 'T -> ValueTask<'output>) 
	-> (onValueNone : unit -> ValueTask<'output>) 
	-> (input : ValueTask<'T voption>) 
	-> ValueTask<'output>
```

## Examples

### Example 1

```fsharp
ValueTaskValueOption.either (fun x -> valueTask { return x * 2 }) (fun () -> valueTask { return 0 }) (ValueTaskValueOption.valueSome 5)

// valueTask { 10 }
```

### Example 2

```fsharp
ValueTaskValueOption.either (fun x -> valueTask { return x * 2 }) (fun () -> valueTask { return 0 }) (ValueTask<_>(ValueNone))

// valueTask { 0 }
```
