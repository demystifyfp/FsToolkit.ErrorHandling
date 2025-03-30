## Option.traverseAsyncResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Async<Result<'b,'c>>) -> 'a option -> Async<Result<'b option, 'c>>
```

Note that `traverse` is the same as `map >> sequence`. See also [Option.sequenceAsyncResult](sequenceAsyncResult.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

Say we have a function to get a number from a database (asynchronously), and multiply our input by that number if it's found:

```fsharp
let tryMultiplyWithDatabaseValue: float -> Async<Result<float, string>> = // ...
```

If we start with an optional vaue, then we could map this funciton using `Option.traverseAsyncResult` as follows:

```fsharp
let input = Some 1.234

input // float option
|> Option.traverseAsyncResult tryMultiplyWithDatabaseValue // Async<Result<float option, string>>
```

If we combine this with the [AsyncResult computation expression](../asyncResult/ce.md), we could directly `let!` the output:

```fsharp
asyncResult {
    let input = Some 1.234

    let! output = // float option
        input // float option
        |> Option.traverseAsyncResult tryMultiplyWithDatabaseValue // Async<Result<float option, string>>
}
```
