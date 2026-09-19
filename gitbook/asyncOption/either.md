# AsyncOption.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
(onSome : 'input -> 'output) -> (onNone : unit -> 'output) -> (input : Async<'input option>) -> Async<'output>
```

## Examples

### Example 1

```fsharp
AsyncOption.some 5 |> AsyncOption.either (fun x -> x * 2) (fun () -> 0)

// async { 10 }
```

### Example 2

```fsharp
async { return None } |> AsyncOption.either (fun x -> x * 2) (fun () -> 0) 

// async { 0 }
```
