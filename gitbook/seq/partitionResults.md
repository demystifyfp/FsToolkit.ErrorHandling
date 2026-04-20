# Seq.partitionResults

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
seq<Result<'ok, 'error>> -> 'ok[] * 'error[]
```

Separates a sequence of `Result` values into a tuple of the `Ok` values and the `Error` values as arrays, preserving order within each group.

See also [Array.partitionResults](../array/partitionResults.md).

## Examples

### Example 1

```fsharp
let results = seq { Ok 1; Error "bad"; Ok 2; Ok 3; Error "worse" }

results |> Seq.partitionResults
// ([| 1; 2; 3 |], [| "bad"; "worse" |])
```

### Example 2

```fsharp
// string -> Result<int, string>
let tryParseInt str =
  match System.Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> Error $"unable to parse '{str}' to integer"

["1"; "foo"; "3"; "bar"]
|> Seq.map tryParseInt
|> Seq.partitionResults
// ([| 1; 3 |], [| "unable to parse 'foo' to integer"; "unable to parse 'bar' to integer" |])
```

### Example 3

All Ok values:

```fsharp
seq { Ok 1; Ok 2; Ok 3 } |> Seq.partitionResults
// ([| 1; 2; 3 |], [||])
```

All Error values:

```fsharp
seq { Error "a"; Error "b" } |> Seq.partitionResults
// ([||], [| "a"; "b" |])
```
