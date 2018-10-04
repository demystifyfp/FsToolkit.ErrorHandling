#load "Result.fs"
#load "ResultCE.fs"
#load "Validation.fs"
#load "ValidationOp.fs"

open System
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open FsToolkit.ErrorHandling.CE.Result

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


type Latitude = private Latitude of double with
  member this.Value =
    let (Latitude lat) = this
    lat
  static member TryCreate (lat : double) =
    if lat > -180. && lat < 180. then
      Ok (Latitude lat)
    else
      sprintf "%A is a invalid latitude value" lat |> Error 

type Longitude = private Longitude of double with
  member this.Value =
    let (Longitude lng) = this
    lng
  static member TryCreate (lng : double) =
    if lng > -90. && lng < 90. then
      Ok (Longitude lng)
    else
      sprintf "%A is a invalid longitude value" lng |> Error 


type CreatePostRequest = {
  Tweet : Tweet
  Latitude : Latitude
  Longitude : Longitude
}

let createPostRequest lat long tweet =
  {Tweet = tweet; Latitude = lat; Longitude = long}


let r = result {
  let! t = Result.tryCreate "tweet" "hello"
  let! lat = Result.tryCreate "lat" 82.0
  let! lng = Result.tryCreate "lng" 23.0
  return (createPostRequest lat lng t)
}

let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> Error (sprintf "unable to parse '%s' to integer" str)

let r2 =
  createPostRequest
  <!^> Result.tryCreate "lat" -360.
  <*^> Result.tryCreate "lng" -360.
  <*^> Result.tryCreate "tweet" ""
  |> Result.mapError Map.ofList

