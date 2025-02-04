# AsyncOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
(onSome : 'T -> Async<'output>) -> (onNone : Async<'output>) -> (input : Async<'T option>) -> Async<'output>
```

## Examples

### Example 1

```fsharp
AsyncOption.either (fun x -> async { x * 2 }) (async { 0 }) (AsyncOption.some 5)

// async { 10 }
```

### Example 2

```fsharp
AsyncOption.either (fun x -> x * 2) (async { 0 }) None

// async { 0 }
```

