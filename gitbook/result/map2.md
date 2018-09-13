## Result.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:
```fsharp
('a -> 'b -> 'c) -> Result<'a, 'd> -> Result<'b, 'd> -> Result<'c, 'd>
```

### Examples:

Let's assume that we have a `add` function which adds two numbers.

```fsharp
// int -> int -> int
let add a b = a + b
```

And an another function that converts string to an integer

```fsharp
open System
// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> Error (sprintf "unable to parse '%s' to integer" str)
```

With the help of `Result.map2` function, we can now do the following

```bash
> open FsToolkit.ErrorHandling;;
> Result.map2 add (tryParseInt "40") (tryParseInt "2");;
val it : Result<int,string> = Ok 42
```

```bash
> open FsToolkit.ErrorHandling;;
> Result.map2 add (tryParseInt "40") (tryParseInt "foobar")
val it : Result<int,string> = Error "unable to parse 'foobar' to integer"
```