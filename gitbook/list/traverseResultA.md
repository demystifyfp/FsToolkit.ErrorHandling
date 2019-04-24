## List.traverseResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Result<'b,'c>) -> 'a list -> Result<'b list, 'c list>
```

Note that `traverse` is the same as `map >> sequence`. See also [List.sequenceResultA](sequenceResultA.md).

This is applicative, collecting all errors. Compare the example below with [traverseResultM](traverseResultM.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> 
    Error (sprintf "unable to parse '%s' to integer" str)

["1"; "2"; "3"]
|> List.traverseResultA tryParseInt
// Ok [1; 2; 3]

["1"; "foo"; "3"; "bar"]
|> List.traverseResultA tryParseInt
//  Error ["unable to parse 'foo' to integer";
//         "unable to parse 'bar' to integer"]
```
