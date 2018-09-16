#I @"./../src/FsToolkit.ErrorHandling"
#load "Create.fs"
#load "Result.fs"
#load "ResultCE.fs"
#load "ResultOp.fs"
#load "ResultOption.fs"
#load "ResultOptionCE.fs"
#load "ResultOptionOp.fs"
#load "Validation.fs"
#load "ValidationOp.fs"
#load "Option.fs"

open System
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.CE.Result
open FsToolkit.ErrorHandling.CE.ResultOption
let add a b = a + b
let add3 a b c = a + b + c
// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> Error (sprintf "unable to parse '%s' to integer" str)


Result.map2 add (tryParseInt "40") (tryParseInt "2")

type Latitude = private Latitude of double with
  // double
  member this.Value =
    let (Latitude lat) = this
    lat
  // double -> Result<Latitude, string>
  static member TryCreate (lat : double) =
    if lat > -180. && lat < 180. then
      Ok (Latitude lat)
    else
      sprintf "%A is a invalid latitude value" lat |> Error 

type Longitude = private Longitude of double with
  // double
  member this.Value =
    let (Longitude lng) = this
    lng
  // double -> Result<Longitude, string>
  static member TryCreate (lng : double) =
    if lng > -90. && lng < 90. then
      Ok (Longitude lng)
    else
      sprintf "%A is a invalid longitude value" lng |> Error 


type Location = {
  Latitude : Latitude
  Longitude : Longitude
}


let location lat lng =
  {Latitude = lat; Longitude = lng}

type UserId = UserId of Guid

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

type CreatePostRequest = {
  UserId : UserId
  Tweet : Tweet
  Location : Location option
}

let createPostRequest userId lat long tweet =
  {Tweet = tweet
   Location = Some(location lat long)
   UserId = userId}

let validTweetR = Tweet.TryCreate "Hello, World!"
let userId  = UserId (Guid.NewGuid())

let validLatR = Latitude.TryCreate 13.067439
let validLngR = Longitude.TryCreate 80.237617
let resultMap3 =
  Result.map3 (createPostRequest userId) validLatR validLngR validTweetR


let resultMap3' = result {
  let! lat = Latitude.TryCreate 13.067439
  let! lng = Longitude.TryCreate 80.237617
  let! tweet = Tweet.TryCreate "Hello, World!"
  return createPostRequest userId lat lng tweet
}

let invalidLatR = Latitude.TryCreate 200.

let resultMap3E =
  Result.map3 (createPostRequest userId) invalidLatR validLngR validTweetR

let remainingCharacters (tweet : Tweet) =
  280 - tweet.Value.Length

validLatR 
|> Result.map (createPostRequest userId)
|> (fun f -> Result.apply f validLngR)
|> (fun f -> Result.apply f validTweetR)

let tryParseIntOrDefault str =
  str
  |> tryParseInt
  |> Result.fold id (fun _ -> 0)

type HttpResponse<'a, 'b> =
  | OK of 'a
  | BadRequest of 'b


// Result<int, string>
let addResultOp = result {
  let! x = tryParseInt "35"
  let! y = tryParseInt "5"
  let! z = tryParseInt "2"
  return add3 x y z
} // returns - Ok 42


// int -> Result<int, string>
let evenInt x =
  if (x % 2 = 0) then
    Ok x 
  else 
    Error (sprintf "%d is not a even integer" x)


let tryParseEvenInt2 str =
  tryParseInt str
  |> Result.bind evenInt


Option.traverseResult tryParseInt (Some "42")

ResultOption.map2 add (Ok (Some 40)) (Ok (Some 2))

let addResult : Result<int option, string> = resultOption {
  let! x = Ok (Some 30)
  let! y = Ok (Some 10)
  let! z = Ok (Some 2)
  return add3 x y z
}

type CreatePostRequest2 = {
  Tweet : Tweet
  Location : Location option
}

let createPostRequest2 tweet location =
  {Tweet = tweet; Location = location}

type CreatePostRequestDto = {
  Tweet : string
  Latitude : double option
  Longitude : double option
}

let toCreatePostRequest (dto :CreatePostRequestDto) = 

  // Result<Latitude option, string>
  let latR = Option.traverseResult Latitude.TryCreate dto.Latitude

  // Result<Longitude option, string>
  let lngR = Option.traverseResult Longitude.TryCreate dto.Longitude

  // Result<Location option, string>
  let locationR =
    ResultOption.map2 location latR lngR

  // Result<Tweet, string>
  let tweetR = Tweet.TryCreate dto.Tweet

  // Result<CreatePostRequest, string>
  Result.map2 createPostRequest2 tweetR locationR


let toCreatePostRequest2 (dto :CreatePostRequestDto) = 

  // Result<Location option, string>
  let locationR = resultOption {
    let! lat = 
      dto.Latitude
      |> Option.traverseResult Latitude.TryCreate 
    let! lng = 
      dto.Longitude
      |> Option.traverseResult Longitude.TryCreate
    return location lat lng
  }

  // Result<Tweet, string>
  let tweetR = Tweet.TryCreate dto.Tweet

  // Result<CreatePostRequest, string>
  Result.map2 createPostRequest2 tweetR locationR

open FsToolkit.ErrorHandling.Operator.ResultOption

let opResult : Result<int option, string> =
  add3
  <!> (Ok (Some 30)) 
  <*> (Ok (Some 10)) 
  <*> (Ok (Some 2)) 

let toCreatePostRequest3 (dto :CreatePostRequestDto) = 

  // Result<Location option, string>
  let locationR = 
    location
    <!> Option.traverseResult Latitude.TryCreate dto.Latitude
    <*> Option.traverseResult Longitude.TryCreate dto.Longitude

  // Result<Tweet, string>
  let tweetR = Tweet.TryCreate dto.Tweet

  // Result<CreatePostRequest, string>
  Result.map2 createPostRequest2 tweetR locationR


let opResult2 : Result<int option, string> =
  add3
  <!> (Ok (Some 30)) 
  <*^> (Ok 10) 
  <*^> (Ok 2) 