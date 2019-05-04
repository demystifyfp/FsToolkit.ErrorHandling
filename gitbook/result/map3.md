## Result.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd) -> Result<'a, 'e> -> Result<'b, 'e>
  -> Result<'c, 'e> -> Result<'d, 'e>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `result` computation expression](../result/ce.md).

### Example 1

Let's assume that we have an `add` function that adds three numbers:

```fsharp
// int -> int -> int -> int
let add a b c = a + b + c
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

With the help of `Result.map3` function, we can now do the following:

```fsharp
let okResult =
  Result.map3 add (tryParseInt "35") (tryParseInt "5") (tryParseInt "2")
  // Ok 42

let errorResult =
  Result.map3 add (tryParseInt "40") (tryParseInt "foobar") (tryParseInt "2")
  // Error "unable to parse 'foobar' to integer"
```

### Example 2

Let's assume that we have the following types in addition to the types that we saw in the [map2](../result/map2.md#a-example-2) example to model a request for posting a tweet:

#### UserId

```fsharp
type UserId = UserId of Guid
```

#### Tweet

```fsharp
type Tweet = private Tweet of string with
  member this.Value = let (Tweet tweet) = this in tweet

  static member TryCreate (tweet : string) =
    if String.IsNullOrEmpty(tweet) then
      Error "Tweet shouldn't be empty"
    elif tweet.Length > 280 then
      Error "Tweet shouldn't contain more than 280 characters"
    else Ok (Tweet x)
```

#### CreatePostRequest

```fsharp
type CreatePostRequest =
  { UserId : UserId
    Tweet : Tweet
    Location : Location option }
  static member Create userId lat long tweet =
    { Tweet = tweet
      Location = Some (location lat long)
      UserId = userId }
```

Then, we can use the `Result.map3` function as below to create the `CreatePostRequest` with validation:

```fsharp
let validLatR = Latitude.TryCreate 13.067439
let validLngR = Longitude.TryCreate 80.237617
let validTweetR = Tweet.TryCreate "Hello, World!"
let userId  = UserId (Guid.NewGuid())

let result =
  Result.map3 (CreatePostRequest.Create userId) validLatR validLngR validTweetR
(* Ok {UserId = UserId 6c0fb13f-ff91-46c7-a486-201257f18877;
       Tweet = Tweet "Hello, World!";
       Location = Some {Latitude = Latitude 13.067439;
                        Longitude = Longitude 80.237617;};} *)
```

When we try with an invalid latitude value, we'll get the following result:

```fsharp
let invalidLatR = Latitude.TryCreate 200.
let result =
  Result.map3 (CreatePostRequest.Create userId) invalidLatR validLngR validTweetR
  // Error "200.0 is a invalid latitude value"
```
