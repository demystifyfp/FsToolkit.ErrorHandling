## Result Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.Result`

FsToolkit.ErrorHandling provides the standard infix operators for the `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) functions of the `Result` type.

## Examples

### Example 1

The example that we saw in the [Result.map3](../result/map3.md#example-1) can be solved using the `map` and `apply` operators as below:

```fsharp
open FsToolkit.ErrorHandling.Operator.Result

let opResult =
  add
  <!> tryParseInt "35"
  <*> tryParseInt "5"
  <*> tryParseInt "2"
```

Let's assume that we want to implement a function, `tryParseEvenInt`, which parses a string to an integer only if the string represents an even integer. We can implement it using the `bind` operator as below:

```fsharp
// int -> Result<int, string>
let evenInt x =
  if x % 2 = 0 then
    Ok x 
  else
    Error (sprintf "%d is not a even integer" x)

// string -> Result<int,string>
let tryParseEvenInt str =
  tryParseInt str
  >>= evenInt

tryParseEvenInt "42" // Ok 42
tryParseEvenInt "41" // Error "41 is not a even integer"
tryParseEvenItn "foo" // Error "Unable to parse 'foo' to integer"
```

Note that `>>=` is just a shortcut for `|> Result.bind`:

```fsharp
let tryParseEvenInt str =
  tryParseInt str
  |> Result.bind evenInt
```