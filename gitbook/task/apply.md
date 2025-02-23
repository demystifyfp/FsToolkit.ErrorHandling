# Task.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<('a -> 'b)> -> Task<'a> -> Task<'b>
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
    Task.singleton "foo" // Task<string>
    |> Task.apply (Task.singleton characterCount) // Task<int>

// task { 3 }
```
