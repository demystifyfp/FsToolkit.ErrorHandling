## Result.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c) -> Result<'a, 'd> -> Result<'b, 'd>
  -> Result<'c, 'd>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `result` computation expression](../result/ce.md).

### Example 1

Let's assume that we have an `add` function that adds two numbers:

```fsharp
// int -> int -> int
let add a b = a + b
```

And an another function that converts a string to an integer:

```fsharp
// string -> Result<int, string>
let tryParseInt str =
  match System.Int32.TryParse str with
  | true, x -> Ok x
  | false, _ ->
    Error (sprintf "unable to parse '%s' to integer" str)
```

With the help of `Result.map2` function, we can now do the following:

```fsharp
let okResult =
  Result.map2 add (tryParseInt "40") (tryParseInt "2")
  // Ok 42

let errorResult =
  Result.map2 add (tryParseInt "40") (tryParseInt "foobar")
  // Error "unable to parse 'foobar' to integer"
```

### Example 2

Let's assume that we have the following types:

#### Latitude

```fsharp
type Latitude = private Latitude of float with
  // float
  member this.Value = let (Latitude lat) = this in lat
  // float -> Result<Latitude, string>
  static member TryCreate (lat : float) =
    if lat > -180. && lat <= 180. then
      Ok (Latitude lat)
    else
      sprintf "%A is a invalid latitude value" lat |> Error
```

#### Longitude

```fsharp
type Longitude = private Longitude of float with
  // float
  member this.Value = let (Longitude lng) = this in lng
  // float -> Result<Longitude, string>
  static member TryCreate (lng : float) =
    if lng >= -90. && lng <= 90. then
      Ok (Longitude lng)
    else
      sprintf "%A is a invalid longitude value" lng |> Error
```

#### Location

```fsharp
type Location =
  { Latitude : Latitude
    Longitude : Longitude }
  static member Create lat lng =
    { Latitude = lat; Longitude = lng }
```

Then, we can use the `Result.map2` function as below to create the location with validation:

```fsharp
let validLatR = Latitude.TryCreate 13.067439
let validLngR = Longitude.TryCreate 80.237617

open FsToolkit.ErrorHandling

let result =
  Result.map2 Location.Create validLatR validLngR
(* Ok {Latitude = Latitude {Value = 13.067439;};
       Longitude = Longitude {Value = 80.237617;};} *)
```

When we try with an invalid latitude value, we'll get the following result:

```fsharp
let invalidLatR = Latitude.TryCreate 200.
let result =
  Result.map2 Location.Create invalidLatR validLngR
  // Error "200.0 is a invalid latitude value"
```