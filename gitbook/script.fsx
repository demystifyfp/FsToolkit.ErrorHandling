#I @"./../src/FsToolkit.ErrorHandling"
#load "Create.fs"
#load "Result.fs"
#load "ResultCE.fs"
#load "ResultOp.fs"
#load "Validation.fs"
#load "ValidationOp.fs"

open System
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open FsToolkit.ErrorHandling.CE.Result
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

let t =  (createPostRequest userId) <!> validLatR <*> validLngR <*> validTweetR


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


let opResult =
  add3
  <!> tryParseInt "35"
  <*> tryParseInt "5"
  <*> tryParseInt "2"

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

let tryParseEvenInt2 str =
  tryParseInt str
  |> Result.bind evenInt