# List.partitionResults

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Result<'ok, 'error> list -> 'ok list * 'error list
```

Separates a list of `Result` values into a tuple of the `Ok` values and the `Error` values, preserving order within each group.

## Examples

### Example 1

```fsharp
let results = [Ok 1; Error "bad"; Ok 2; Ok 3; Error "worse"]

results |> List.partitionResults
// ([1; 2; 3], ["bad"; "worse"])
```

### Example 2

```fsharp
// string -> Result<int, string>
let tryParseInt str =
  match System.Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> Error (sprintf "unable to parse '%s' to integer" str)

["1"; "foo"; "3"; "bar"]
|> List.map tryParseInt
|> List.partitionResults
// ([1; 3], ["unable to parse 'foo' to integer"; "unable to parse 'bar' to integer"])
```

### Example 3

All Ok values:

```fsharp
[Ok 1; Ok 2; Ok 3] |> List.partitionResults
// ([1; 2; 3], [])
```

All Error values:

```fsharp
[Error "a"; Error "b"] |> List.partitionResults
// ([], ["a"; "b"])
```
