# CancellableTaskOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
(onSome : 'input -> 'output) 
	-> (onNone : unit -> 'output) 
	-> (input : 'input option) 
	-> CancellableTask<'output>
```

## Examples

### Example 1

```fsharp
CancellableTaskOption.some 5
|> CancellableTaskOption.either (fun x -> x * 2) (fun () -> 0) 

// cancellableTask { 10 }
```

### Example 2

```fsharp
CancellableTask.singleton None
|> CancellableTaskOption.either (fun x -> x * 2) (fun () -> 0) 

// cancellableTask { 0 }
```

