# CancellableValueTaskOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
(onSome : 'unit -> 'output) 
	-> (onNone : unit -> unit -> 'output) 
	-> (input : 'input option) 
	-> CancellableValueTask<'output>
```

## Examples

### Example 1

```fsharp
CancellableValueTaskOption.some 5
|> CancellableValueTaskOption.either (fun x -> x * 2) (fun () -> 0) 

// cancellableValueTask { 10 }
```

### Example 2

```fsharp
CancellableValueTask.singleton None
|> CancellableValueTaskOption.either (fun x -> x * 2) (fun () -> 0) 

// cancellableValueTask { 0 }
```
