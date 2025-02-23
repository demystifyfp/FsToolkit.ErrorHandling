# TaskOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<('a -> 'b) option> -> Task<'a option> -> Task<'b option>
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
    TaskOption.some "foo" // Task<string option>
    |> TaskOption.apply (TaskOption.some characterCount) // Task<int option>

// task { Some 3 }
```

### Example 2

```fsharp
let result =
    Task.singleton None // Task<string option>
    |> TaskOption.apply (TaskOption.some characterCount) // Task<int option>

// task { None }
```

### Example 3

```fsharp
let result : Task<int option> =
    TaskOption.some "foo" // Task<string option>
    |> TaskOption.apply (Task.singleton None) // Task<int option>

// task { None }
```
