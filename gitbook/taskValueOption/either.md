# TaskValueOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the voption. If the voption is `ValueSome`, the first function will be executed. If the voption is `ValueNone`, the second function will be executed.

```fsharp
(onSome : 'input -> Task<'output>) 
	-> (onNone : 'output>) 
	-> (input : 'input voption) 
	-> Task<'output>
```

## Examples

### Example 1

```fsharp
TaskValueOption.valueSome 5 |> TaskValueOption.either (fun x -> x * 2) (fun () -> 0) 
// task { 10 }
```

### Example 2

```fsharp
Task.singleton ValueNone |> TaskValueOption.either (fun x -> x * 2) (fun () -> 0) 
// task { 0 }
```

