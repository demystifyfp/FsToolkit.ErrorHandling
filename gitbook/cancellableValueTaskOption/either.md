# CancellableValueTaskOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
(onSome : 'T -> CancellableValueTask<'output>) 
	-> (onNone : unit -> CancellableValueTask<'output>) 
	-> (input : CancellableValueTask<'T option>) 
	-> CancellableValueTask<'output>
```

## Examples

### Example 1

```fsharp
CancellableValueTaskOption.either (fun x -> cancellableValueTask { return x * 2 }) (fun () -> cancellableValueTask { return 0 }) (CancellableValueTaskOption.some 5)

// cancellableValueTask { 10 }
```

### Example 2

```fsharp
CancellableValueTaskOption.either (fun x -> cancellableValueTask { return x * 2 }) (fun () -> cancellableValueTask { return 0 }) (CancellableValueTask.singleton None)

// cancellableValueTask { 0 }
```
