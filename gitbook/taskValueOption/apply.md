# TaskValueOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<('a -> 'b) voption> -> Task<'a voption> -> Task<'b voption>
```

## Examples

Take the following function for example

```fsharp
// string -> int
let characterCount (s: string) = s.Length
```

### Example 1

```fsharp
let result =
    TaskValueOption.valueSome "foo" // Task<string voption>
    |> TaskValueOption.apply (TaskValueOption.valueSome characterCount) // Task<int voption>

// task { ValueSome 3 }
```

### Example 2

```fsharp
let result =
    Task.singleton ValueNone // Task<string voption>
    |> TaskValueOption.apply (TaskValueOption.valueSome characterCount) // Task<int voption>

// task { ValueNone }
```

### Example 3

```fsharp
let result : Task<int voption> =
    TaskValueOption.valueSome "foo" // Task<string voption>
    |> TaskValueOption.apply (Task.singleton ValueNone) // Task<int voption>

// task { ValueNone }
```
