## Result.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c -> 'd) -> Result<'a, 'e> -> Result<'b, 'e> 
  -> Result<'c, 'e> -> Result<'d, 'e>
```

## Examples:

### Basic Example

Let's assume that we have a `add` function which adds three numbers.

```fsharp
// int -> int -> int
let add a b c = a + b + c
```

And an another function that converts string to an integer

```fsharp
open System

// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> 
    Error (sprintf "unable to parse '%s' to integer" str)
```

With the help of `Result.map3` function, we can now do the following

```fsharp
open FsToolkit.ErrorHandling

let result =
  Result.map3 add (tryParseInt "35") (tryParseInt "5") (tryParseInt "2")
  // returns - Ok 42
```

```fsharp
open FsToolkit.ErrorHandling
let result =
  Result.map3 add (tryParseInt "40") (tryParseInt "foobar") (tryParseInt "2")
  // returns - Error "unable to parse 'foobar' to integer"
```

### A Real World Example

Let's assume that we have the following types in addition to the types that we saw in the [map2](gitbook/result/map2.md#a-real-world-example) example to model a request for posting a tweet.

#### UserId

```fsharp
type UserId = UserId of Guid
```

#### Tweet

```fsharp
type Tweet = private Tweet of string with
  member this.Value =
    let (Tweet tweet) = this
    tweet

  static member TryCreate (tweet : string) =
    match tweet with
    | x when String.IsNullOrEmpty x -> 
      Error "Tweet shouldn't be empty"
    | x when x.Length > 280 ->
      Error "Tweet shouldn't contain more than 280 characters"
    | x -> Ok (Tweet x)
```

#### CreatePostRequest

```fsharp
type CreatePostRequest = {
  UserId : UserId
  Tweet : Tweet
  Location : Location option
}
```

And also a function to create `CreatePostRequest`

```fsharp
let createPostRequest userId lat long tweet =
  {Tweet = tweet
   Location = Some(location lat long)
   UserId = userId}
```

Then, we can use the `Result.map3` function as below to create the `CreatePostRequest` with validation

```fsharp
let validLatR = Latitude.TryCreate 13.067439
let validLngR = Longitude.TryCreate 80.237617
let validTweetR = Tweet.TryCreate "Hello, World!" 

open FsToolkit.ErrorHandling
let userId  = UserId (Guid.NewGuid())

let result =
  Result.map3 (createPostRequest userId) validLatR validLngR validTweetR
(* returns - Ok {UserId = UserId 6c0fb13f-ff91-46c7-a486-201257f18877;
                  Tweet = Tweet "Hello, World!";
                  Location = Some {Latitude = Latitude 13.067439;
                                  Longitude = Longitude 80.237617;};} *)
```

When we try with an invalid latitude value, we'll get the following result

```fsharp
let invalidLatR = Latitude.TryCreate 200.
let result =
  Result.map3 (createPostRequest userId) invalidLatR validLngR validTweetR
  // returns - Error "200.0 is a invalid latitude value"
```