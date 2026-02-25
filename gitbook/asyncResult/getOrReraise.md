## AsyncResult.getOrReraise

Namespace: `FsToolkit.ErrorHandling`

Unwraps the `Ok` value from an async result. If the result is `Error`, the contained exception is re-raised, preserving the original stack trace (on .NET; on Fable it is raised directly).

This function is only applicable when the error type is `exn`.

## Function Signature:

```fsharp
Async<Result<'ok, exn>> -> Async<'ok>
```

## Examples

### Example 1

Unwrapping a successful result:

```fsharp
let result : Async<int> =
  AsyncResult.ok 42
  |> AsyncResult.getOrReraise
// evaluates to 42
```

### Example 2

Re-raising an exception from a failing async computation:

```fsharp
let value : Async<string> =
  AsyncResult.ofTask (System.Threading.Tasks.Task.FromException<string>(System.Exception("oops")))
  |> AsyncResult.getOrReraise
// raises System.Exception "oops"
```

### Example 3

Using `catch` together with `getOrReraise` to control which exceptions become errors:

```fsharp
let run () : Async<int> =
  riskyOperation ()
  |> AsyncResult.catch id
  |> AsyncResult.getOrReraise
```
