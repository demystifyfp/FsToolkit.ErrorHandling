## List.sequenceResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Result<'a, 'b> list -> Result<'a list, 'b list>
```

Note that `sequence` is the same as `traverse id`. See also [List.traverseResultA](traverseResultA.md).

This is applicative, collecting all errors. Compare the example below with [sequenceResultM](sequenceResultM.md).

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
|> List.map tryParseInt
|> List.sequenceResultA
// Ok [1; 2; 3]

["1"; "foo"; "3"; "bar"]
|> List.map tryParseInt
|> List.sequenceResultA
// Error ["unable to parse 'foo' to integer"; 
//        "unable to parse 'bar' to integer"]
```