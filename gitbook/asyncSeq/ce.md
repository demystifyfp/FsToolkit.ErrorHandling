## AsyncSeq Computation Expression Extensions

Namespace: `FsToolkit.ErrorHandling`

Package: `FsToolkit.ErrorHandling.AsyncSeq`

This module extends the `asyncResult` computation expression with support for iterating over [`AsyncSeq`](https://fsprojects.github.io/FSharp.Control.AsyncSeq/) sequences directly inside `asyncResult { }` blocks.

Without this extension, consuming an `AsyncSeq` inside `asyncResult` would require manual collection or conversion. With it, you can use `for` loop syntax naturally.

## Extended Members

The following members are added to `AsyncResultBuilder`:

- `For(xs: AsyncSeq<Result<'T, 'TError>>, binder)` — iterates over an `AsyncSeq` of `Result` values; short-circuits on the first `Error`.
- `For(xs: AsyncSeq<'T>, binder)` — iterates over an `AsyncSeq` of plain values inside `asyncResult`; each item is treated as `Ok`.
- `While(guard, computation)` — supports `while` loops driven by an `AsyncSeq` enumerator.

## Examples

### Example 1

Processing each item from an `AsyncSeq` of plain values inside `asyncResult`:

```fsharp
open FsToolkit.ErrorHandling
open FSharp.Control

let processItems (items: AsyncSeq<Item>) : Async<Result<unit, string>> =
    asyncResult {
        for item in items do
            do! processItem item  // processItem : Item -> Async<Result<unit, string>>
    }
// Iterates until all items are processed, or stops on the first Error
```

### Example 2

Iterating over an `AsyncSeq<Result<'T, 'TError>>`, short-circuiting on the first error:

```fsharp
open FsToolkit.ErrorHandling
open FSharp.Control

let validateAll (inputs: AsyncSeq<Result<Input, string>>) : Async<Result<unit, string>> =
    asyncResult {
        for input in inputs do
            do! validate input  // validate : Input -> Async<Result<unit, string>>
    }
// If any element in `inputs` is Error, iteration stops immediately
```

### Example 3

Reading lines from a stream as an `AsyncSeq` and writing them to a database:

```fsharp
open FsToolkit.ErrorHandling
open FSharp.Control

let importLines (stream: System.IO.Stream) : Async<Result<unit, string>> =
    asyncResult {
        let lines = asyncSeq {
            use reader = new System.IO.StreamReader(stream)
            while not reader.EndOfStream do
                yield reader.ReadLine()
        }

        for line in lines do
            do! persistLine line  // persistLine : string -> Async<Result<unit, string>>
    }
```
