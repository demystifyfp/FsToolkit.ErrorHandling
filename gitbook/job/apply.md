# Job.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<('a -> 'b)> -> Job<'a> -> Job<'b>
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
    Job.singleton "foo" // Job<string>
    |> Job.apply (Job.singleton characterCount) // Job<int>

// job { return 3 }
```
