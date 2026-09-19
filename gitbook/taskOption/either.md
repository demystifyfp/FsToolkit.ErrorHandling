# TaskOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
(onSome : 'input -> 'output) 
	-> (onNone : unit -> 'output) 
	-> (input : 'input option) 
	-> Task<'output>
```

## Examples

### Example 1

```fsharp
TaskOption.some 5 |> TaskOption.either (fun x -> x * 2) (fun () -> 0) 

// task { 10 }
```

### Example 2

```fsharp
TaskResult.singleton None |> TaskOption.either (fun x -> x * 2) (fun () -> 0) 

// task { 0 }
```

