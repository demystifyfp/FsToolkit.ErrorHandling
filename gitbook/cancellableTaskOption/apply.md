# CancellableTaskOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableTask<('a -> 'b) option> -> CancellableTask<'a option> -> CancellableTask<'b option>
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
    CancellableTaskOption.some "foo" // CancellableTask<string option>
    |> CancellableTaskOption.apply (CancellableTaskOption.some characterCount) // CancellableTask<int option>

// cancellableTask { Some 3 }
```

### Example 2

```fsharp
let result =
    CancellableTask.singleton None // CancellableTask<string option>
    |> CancellableTaskOption.apply (CancellableTaskOption.some characterCount) // CancellableTask<int option>

// cancellableTask { None }
```

### Example 3

```fsharp
let result : CancellableTask<int option> =
    CancellableTaskOption.some "foo" // CancellableTask<string option>
    |> CancellableTaskOption.apply (CancellableTask.singleton None) // CancellableTask<int option>

// cancellableTask { None }
```
