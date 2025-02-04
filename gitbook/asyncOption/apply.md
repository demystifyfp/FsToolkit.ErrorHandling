# AsyncOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Async<('a -> 'b) option> -> Async<'a option> 
  -> Async<'b option>
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
    AsyncOption.some "foo" // Async<string option>
    |> AsyncOption.apply (AsyncOption.some characterCount) // Async<int option>

// async { Some 3 }
```

### Example 2

```fsharp
let result =
    Async.singleton None // Async<string option>
    |> AsyncOption.apply (AsyncOption.some characterCount) // Async<int option>

// async { None }
```

### Example 3

```fsharp
let result : Async<int option> =
    AsyncOption.some "foo" // Async<string option>
    |> AsyncOption.apply (Async.singleton None) // Async<int option>

// async { None }
```
