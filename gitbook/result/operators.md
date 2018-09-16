## Result Infix Operators

FsToolkit.ErrorHandling provides the standard infix operators for the map (`<!>`), apply (`<*>`) & bind (`>>=`) functions.

Namespace: `FsToolkit.ErrorHandling.Operator.Result`

## Examples:

### Example 1

The example that we saw in the [Result.map3](../result/map3.md#example-1) can be solved using the `map` & `apply` operators as below

```fsharp
open FsToolkit.ErrorHandling.Operator.Result

let opResult =
  add
  <!> tryParseInt "35"
  <*> tryParseInt "5"
  <*> tryParseInt "2"
```

Let's assume that we want to implement a function, `tryParseEvenInt`, which parses a string to an integer only if the string contains a even integer. We can implement it using the `bind` operator, as below

```fsharp
// int -> Result<int, string>
let evenInt x =
  if (x % 2 = 0) then
    Ok x 
  else 
    Error (sprintf "%d is not a even integer" x)

// string -> Result<int,string>
let tryParseEvenInt str =
  tryParseInt str
  >>= evenInt

tryParseEvenInt "42" // returns - Ok 42
tryParseEvenInt "41" // returns - Error "41 is not a even integer"
tryParseEvenItn "foo" // returns - Error "Unable to parse 'foo' to integer"
```

> Note: The `tryParseEvenInt` can be return using the `bind` function as follow

```fsharp
let tryParseEvenInt str =
  tryParseInt str
  |> Result.bind evenInt
```