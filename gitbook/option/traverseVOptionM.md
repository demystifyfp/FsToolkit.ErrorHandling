# List.traverseVOptionM

Namespace: `FsToolkit.ErrorHandling`

Applies the given function to each element in the input list, and returns an option containing a list of the results. If any of the function applications return None, the entire result will be None.

## Function Signature

```fsharp
('a -> 'b voption) -> 'a list -> 'b list voption
```

## Examples

### Example 1

```fsharp
let tryParseInt (s: string) =
    match Int32.TryParse(s) with
    | true, i -> ValueSome i
    | false, _ -> ValueNone

let myList = ["123"; "456"; "789"]
  
List.traverseVOptionM tryParseInt myList
// Some [123; 456; 789]
```

### Example 2

```fsharp
let tryParseInt (s: string) =
    match Int32.TryParse(s) with
    | true, i -> ValueSome i
    | false, _ -> ValueNone

let myList = ["123"; "Not a number"; "789"]

List.traverseVOptionM tryParseInt myList
// None
```
