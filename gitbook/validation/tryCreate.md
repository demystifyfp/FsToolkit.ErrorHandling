## Result.tryCreate 

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
string -> 'a -> Result<^b, (string * 'c)>
```

`^b` is a [statically resolved parameter](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/generics/statically-resolved-type-parameters) with the below constraint

```fsharp
^b : (static member TryCreate : 'a -> Result< ^b, 'c>)
```

This can be useful when constructing types for collecting construction validation errors associated with passed-in parameter names, as the example below demonstrate.

## Examples

### Example 1

[Making illegal states unrepresentable](https://fsharpforfunandprofit.com/posts/designing-with-types-making-illegal-states-unrepresentable/) is a common practice in F#. A common way to do it is to have a type, say `MyType`, with a private constructor and a `TryCreate` member that returns a `Result<MyType, 'a>`, like shown below:

```fsharp
type Longitude = private Longitude of float with
  member this.Value = let (Longitude lng) = this in lng

  // float -> Result<Longitude, string>
  static member TryCreate (lng : float) =
    if lng >= -90. && lng <= 90. then
      Ok (Longitude lng)
    else
      sprintf "%A is a invalid longitude value" lng |> Error 
```

Let's assume that we have few more similar types as below

```fsharp
type Longitude = private Longitude of float with
  member this.Value =
    let (Longitude lng) = this
    lng
  static member TryCreate (lng : float) =
    if lng > -90. && lng < 90. then
      Ok (Longitude lng)
    else
      sprintf "%A is a invalid longitude value" lng |> Error 

type Tweet = private Tweet of string with
  member this.Value =
    let (Tweet tweet) = this in tweet

  static member TryCreate (tweet : string) =
    if String.IsNullOrEmpty tweet then
      Error "Tweet shouldn't be empty"
    elif tweet.Length > 280 then
      Error "Tweet shouldn't contain more than 280 characters"
    else Ok (Tweet tweet)
```

Assume furthermore that the types above are used in the following types:

```fsharp
type Location = {
  Latitude : Latitude
  Longitude : Longitude
}

type CreatePostRequest = {
  Tweet : Tweet
  Location : Location
}
```

And that we have the following functions to create these composed types:

```fsharp
let location lat lng =
  {Latitude = lat; Longitude = lng}

let createPostRequest lat long tweet =
  {Tweet = tweet; Location = location lat long}
```

And the following DTO types:

```fsharp
type LocationDto = {
  Latitude : float
  Longitude : float
}

type CreatePostRequestDto = {
  Tweet : string
  Location : LocationDto
}
```

We can then do validation using `Result.tryResult` and the [`Validation` infix operators](../validation/operators.md) as below:

```fsharp
open FsToolkit.ErrorHandling.Operator.Validation 

// CreatePostRequestDto -> Result<CreatePostRequest, (string * string) list>
let validateCreatePostRequest (dto : CreatePostRequestDto) =
  createPostRequest
  <!^> Result.tryCreate "latitude" dto.Location.Latitude
  <*^> Result.tryCreate "longitude" dto.Location.Longitude
  <*^> Result.tryCreate "tweet" dto.Tweet
```

Here the types of the `Result.tryCreate` lines are inferred, and the types' `TryCreate` member is used to construct them.

```fsharp
validateCreatePostRequest
  {Tweet = ""; Location = {Latitude = 300.; Longitude = 400.}};;
// Error
//   [("latitude", "300.0 is a invalid latitude value")
//    ("longitude", "400.0 is a invalid longitude value")
//    ("tweet", "Tweet shouldn't be empty")]
```

These errors can then for example be returned in an API response:

```fsharp
validateCreatePostRequest dto
|> Result.mapError Map.ofList
// Map<string, string>
```

When serialized:

```json
{
  "latitude": "300.0 is a invalid latitude value",
  "longitude": "400.0 is a invalid longitude value",
  "tweet": "Tweet shouldn't be empty"
}
```

### Example 2

In Example 1, we collected all the error messages. But what if we wanted to stop on the first error? One way to do this is to make use of the `result` computation expression instead of using infix operators from `Validation` module.

```fsharp
// CreatePostRequestDto -> Result<CreatePostRequest, string>
let validateCreatePostRequest (dto : CreatePostRequestDto) = result {
  let! t = Result.tryCreate "tweet" dto.Tweet
  let! lat = Result.tryCreate "latitude" dto.Location.Latitude
  let! lng = Result.tryCreate "longitude" dto.Location.Longitude
  return createPostRequest lat lng t
}
```

### Example 3

In the examples above, we assume that a location is always required for creating a post. Let's assume that the requirement is changed and now the location is optional:

```fsharp
type CreatePostRequest = {
  Tweet : Tweet
  Location : Location option
}

type CreatePostRequestDto = {
  Tweet : string
  Location : LocationDto option
}

let createPostRequest location tweet =
  {Tweet = tweet; Location = location}
```

Then `validateCreatePostRequest` can be rewritten using the [Option.traverseResult](../option/traverseResult.md) function as below:

```fsharp
let validateLocation (dto : LocationDto) =
  location
  <!^> Result.tryCreate "latitude" dto.Latitude
  <*^> Result.tryCreate "longitude" dto.Longitude

let validateCreatePostRequest (dto : CreatePostRequestDto) =
  createPostRequest
  <!> Option.traverseResult validateLocation dto.Location
  <*^> Result.tryCreate "tweet" dto.Tweet
```

Note: We are using the `<!>` operator in the `validateCreatePostRequest` instead of `<!^>` operator as the right side result is returning a list of errors (`Result<Location option, (string * string) list>`). 

```fsharp
validateCreatePostRequest 
  {Tweet = ""; Location = Some {Latitude = 300.; Longitude = 400.}}
//  Error
//    [("latitude", "300.0 is a invalid latitude value")
//     ("longitude", "400.0 is a invalid longitude value")
//     ("tweet", "Tweet shouldn't be empty")]

validateCreatePostRequest {Tweet = ""; Location = None}
//  Error [("tweet", "Tweet shouldn't be empty")]
```