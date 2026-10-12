# CancellableValueTaskOption.eitherBind

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
(onSome : 'input -> 'output) 
	-> (onNone : unit -> 'output) 
	-> (input : CancellableValueTask<'input option>) 
	-> CancellableValueTask<'output>
```

## Examples

### Example 1

```fsharp
CancellableValueTaskOption.some 5
|> CancellableValueTaskOption.eitherBind (fun x -> x * 2) (fun () -> 0) 

// cancellableValueTask { 10 }
```

### Example 2

```fsharp
CancellableValueTaskOption.none
|> CancellableValueTaskOption.eitherBind (fun x -> x * 2) (fun () -> 0) 

// cancellableValueTask { 0 }
```
