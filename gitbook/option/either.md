# Option.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

Provide two functions to execute depending on the value of the option. If the option is `Some`, the first function will be executed. If the option is `None`, the second function will be executed.

```fsharp
('T-> 'output) -> (unit -> 'output) -> 'T option -> 'output
```

## Examples

### Example 1

```fsharp
Option.either (fun x -> x * 2) (fun () -> 0) (Some 5)

// 10
```

### Example 2

```fsharp
Option.either (fun x -> x * 2) (fun () -> 0) None

// 0
```

