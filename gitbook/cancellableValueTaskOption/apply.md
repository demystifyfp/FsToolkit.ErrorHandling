# CancellableValueTaskOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableValueTask<('a -> 'b) option> -> CancellableValueTask<'a option> -> CancellableValueTask<'b option>
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
    CancellableValueTaskOption.some "foo" // CancellableValueTask<string option>
    |> CancellableValueTaskOption.apply (CancellableValueTaskOption.some characterCount) // CancellableValueTask<int option>

// cancellableValueTask { Some 3 }
```

### Example 2

```fsharp
let result =
    CancellableValueTask.singleton None // CancellableValueTask<string option>
    |> CancellableValueTaskOption.apply (CancellableValueTaskOption.some characterCount) // CancellableValueTask<int option>

// cancellableValueTask { None }
```

### Example 3

```fsharp
let result : CancellableValueTask<int option> =
    CancellableValueTaskOption.some "foo" // CancellableValueTask<string option>
    |> CancellableValueTaskOption.apply (CancellableValueTask.singleton None) // CancellableValueTask<int option>

// cancellableValueTask { None }
```
