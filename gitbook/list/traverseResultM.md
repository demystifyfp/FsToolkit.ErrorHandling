## List.traverseResultM

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Result<'b,'c>) -> 'a list -> Result<'b list, 'c>
```

## Examples

### Example 1

```fsharp
// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> 
    Error (sprintf "unable to parse '%s' to integer" str)

// returns - Ok [1;2;3]
["1";"2";"3"]
|> List.traverseResultM tryParseInt 

// returns - Error "unable to parse 'foo' to integer"
["1";"foo";"3";"bar"]
|> List.traverseResultM tryParseInt 
```