## List.sequenceResultM

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Result<'a, 'b> list -> Result<'a list, 'b>
```

Note that `sequence` is the same as `traverse id`. See also [List.traverseResultM](traverseResultM.md).

This is monadic, stopping on the first error. Compare the example below with [sequenceResultA](sequenceResultA.md).

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
|> List.sequenceResultM 
// Ok [1; 2; 3]

["1"; "foo"; "3"; "bar"]
|> List.map tryParseInt
|> List.sequenceResultM  
// Error "unable to parse 'foo' to integer"
```