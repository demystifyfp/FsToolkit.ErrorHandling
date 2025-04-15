## Option.traverseTaskResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Task<Result<'b,'c>>) -> 'a option -> Task<Result<'b option, 'c>>
```

Note that `traverse` is the same as `map >> sequence`. See also [Option.sequenceTaskResult](sequenceTaskResult.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

Say we have a function to get a number from a database (asynchronously), and multiply our input by that number if it's found:

```fsharp
let tryMultiplyWithDatabaseValue: float -> Task<Result<float, string>> = // ...
```

If we start with an optional value, then we could map this function using `Option.traverseTaskResult` as follows:

```fsharp
let input = Some 1.234

input // float option
|> Option.traverseTaskResult tryMultiplyWithDatabaseValue // Task<Result<float option, string>>
```

If we combine this with the [TaskResult computation expression](../taskResult/ce.md), we could directly `let!` the output:

```fsharp
taskResult {
    let input = Some 1.234

    let! output = // float option
        input // float option
        |> Option.traverseTaskResult tryMultiplyWithDatabaseValue // Task<Result<float option, string>>
}
```
