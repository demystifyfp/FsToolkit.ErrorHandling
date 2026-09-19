# ValueTaskValueOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the voption. If the voption is `ValueSome`, the first function will be executed. If the voption is `ValueNone`, the second function will be executed.

```fsharp
(onSome : 'input -> 'output) 
	-> (onNone : unit -> 'output) 
	-> (input : ValueTask<'input voption>) 
	-> ValueTask<'output>
```

## Examples

### Example 1

```fsharp
ValueTaskValueOption.valueSome 5
|> ValueTaskValueOption.either (fun x -> x * 2) (fun () -> 0) 

// valueTask { 10 }
```

### Example 2

```fsharp
ValueTask.singleton None
|> ValueTaskValueOption.either (fun x -> x * 2) (fun () -> 0) 

// valueTask { 0 }
```
