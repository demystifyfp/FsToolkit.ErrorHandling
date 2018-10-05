## List.traverseResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Result<'b,'c>) -> 'a list -> Result<'b list, 'c list>
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
|> List.traverseResultA tryParseInt 

// returns - 
//  Error ["unable to parse 'foo' to integer"; 
//          "unable to parse 'bar' to integer"]
["1";"foo";"3";"bar"]
|> List.traverseResultA tryParseInt 
```