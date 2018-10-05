## Result.tryCreate 

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
string -> 'a -> Result<^b, (string * 'c)>
```

`^b` is a [statically resolved parameter](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/generics/statically-resolved-type-parameters) with the below constraint

```fsharp
^b : (static member TryCreate : 'a -> Result< ^b, 'c>)
```

## Examples

### Example 1

[Making illegal states unrepresentable](https://fsharpforfunandprofit.com/posts/designing-with-types-making-illegal-states-unrepresentable/) is one of the common practice in F# and I typically do it as below

```fsharp
type Longitude = private Longitude of double with
  member this.Value =
    let (Longitude lng) = this
    lng

  // double -> Result<Longitude, string>
  static member TryCreate (lng : double) =
    if lng > -90. && lng < 90. then
      Ok (Longitude lng)
    else
      sprintf "%A is a invalid longitude value" lng |> Error 
```

The type will have a private constructor and a static member `TryCreate` to create a value of underlying type with validaion. 

Let's assume that we have few more similar types as below

```fsharp
type Longitude = private Longitude of double with
  member this.Value =
    let (Longitude lng) = this
    lng
  static member TryCreate (lng : double) =
    if lng > -90. && lng < 90. then
      Ok (Longitude lng)
    else
      sprintf "%A is a invalid longitude value" lng |> Error 

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

Then the composition of these types

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

And some functions to create this composition type 

```fsharp
let location lat lng =
  {Latitude = lat; Longitude = lng}

let createPostRequest lat long tweet =
  {Tweet = tweet; Location = location lat long}
```

With these types in place, we can do validation using `Result.tryResult` and Validation [infix operators](../validation/operators.md#example-2) as below

```fsharp
type LocationDto = {
  Latitude : double
  Longitude : double
}

type CreatePostRequestDto = {
  Tweet : string
  Location : LocationDto
}
```

```fsharp
open FsToolkit.ErrorHandling.Operator.Validation 

// CreatePostRequestDto -> Result<CreatePostRequest, (string * string) list>
let validateCreatePostRequest (dto : CreatePostRequestDto) =
  createPostRequest
  <!^> Result.tryCreate "latitude" dto.Location.Latitude
  <*^> Result.tryCreate "longitude" dto.Location.Longitude
  <*^> Result.tryCreate "tweet" dto.Tweet
```

```
> validateCreatePostRequest {Tweet = ""; Location = {Latitude = 300.; Longitude = 400.}};;

Error
    [("latitude", "300.0 is a invalid latitude value")
     ("longitude", "400.0 is a invalid longitude value")
     ("tweet", "Tweet shouldn't be empty")]
```

We typically `map` this error to a F# Map data structure and communicate it back with the front end with these error messages JSON serialized

```fsharp
// Map<string, string>
validateCreatePostRequest dto
|> Result.mapError Map.ofList
```

```json
{
  "latitude": "300.0 is a invalid latitude value",
  "longitude": "400.0 is a invalid longitude value",
  "tweet": "Tweet shouldn't be empty"
}
```

### Example 2

In Example 1, we are interested in collecting all the error messages but what if we wanted to return on first error. To do it, we can make use of the Result's computation expression instead of using infix operators from Validation module

```fsharp
// CreatePostRequestDto -> Result<CreatePostRequest, string>
let validateCreatePostRequest (dto : CreatePostRequestDto) = result {
  let! t = Result.tryCreate "tweet" dto.Tweet
  let! lat = Result.tryCreate "latitude" dto.Location.Latitude
  let! lng = Result.tryCreate "longitude" dto.Location.Longitude
  return (createPostRequest lat lng t)
}
```

### Example 3

In the above examples, we assume that location is always required for creating a post. Let's assume that the requirement is changed and now the location is optional 

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

Then the `validateCreatePostRequest` can be rewritten using the [Option.traverseResult](../option/traverseResult.md) function as below

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

> Note: We are using `<!>` operator in the `validateCreatePostRequest` instead of `<!^>` operator as the right side result is returning a list type in the error (`Result<Locaiton option, (string * string) list>`). 

In Runtime, the `validateCreatePostRequest` responds like this

```
> validateCreatePostRequest {Tweet = ""; Location = Some {Latitude = 300.; Longitude = 400.}};;
  
  Error
    [("latitude", "300.0 is a invalid latitude value");
     ("longitude", "400.0 is a invalid longitude value");
     ("tweet", "Tweet shouldn't be empty")]

> validateCreatePostRequest {Tweet = ""; Location = None};;
  
  Error [("tweet", "Tweet shouldn't be empty")]
```