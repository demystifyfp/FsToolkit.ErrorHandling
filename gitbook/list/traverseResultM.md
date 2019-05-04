## List.traverseResultM

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Result<'b,'c>) -> 'a list -> Result<'b list, 'c>
```

Note that `traverse` is the same as `map >> sequence`. See also [List.sequenceResultM](sequenceResultM.md).

This is monadic, stopping on the first error. Compare the example below with [traverseResultA](traverseResultA.md).

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
|> List.traverseResultM tryParseInt 
// Ok [1; 2; 3]

["1"; "foo"; "3"; "bar"]
|> List.traverseResultM tryParseInt 
// Error "unable to parse 'foo' to integer"
```
