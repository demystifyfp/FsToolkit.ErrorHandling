module SampleDomain

open FsToolkit.ErrorHandling
open System

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


type Location = {
  Latitude : Latitude
  Longitude : Longitude
}
let location lat lng =
  {Latitude = lat; Longitude = lng}

type Url = private Url of string with
  member this.Value =
    let (Url url) = this
    url
  
  static member TryCreate (url : string) =
    if Uri.IsWellFormedUriString(url, UriKind.Absolute) then 
      Ok (Url url)
    else
      sprintf "%s is a invalid URL" url |> Error


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

let remainingCharacters (tweet : Tweet) =
  280 - tweet.Value.Length

let firstURLInTweet (tweet : Tweet) =
  tweet.Value.Split([|' '|])
  |> Array.tryFind (fun s -> Uri.IsWellFormedUriString(s, UriKind.Absolute))
  |> Option.traverseResult Url.TryCreate

type UserId = UserId of Guid

type CreatePostRequest = {
  UserId : UserId
  Tweet : Tweet
  Location : Location option
}

let sampleUserId = UserId (System.Guid.NewGuid())

let createPostRequest lat long tweet =
  {Tweet = tweet; Location = Some(location lat long); UserId = sampleUserId}

let commonEx = new Exception("something went wrong!")
let getFollowersEx = new Exception("unable to fetch followers!")

let allowedToPost userId = async {
  if (userId = sampleUserId) then 
    return Ok true
  else
    return Error commonEx
}

let newPostId = Guid.NewGuid()

type PostId = PostId of Guid

let createPostSuccess (_ : CreatePostRequest) = async {
  return Ok (PostId newPostId)
}

let followerIds = [UserId (Guid.NewGuid()); UserId (Guid.NewGuid())]

let getFollowersSuccess (UserId _) = async {
  return Ok followerIds
}



let getFollowersFailure (UserId _) = async {
  return Error getFollowersEx
}

let createPostFailure (_ : CreatePostRequest) = async {
  return Error commonEx
}

type NotifyNewPostRequest = {
  UserIds : UserId list
  NewPostId : PostId
}

let newPostRequest userIds newPostsId =
  {UserIds = userIds; NewPostId = newPostsId}

type LocationDto = {
  Latitude : double
  Longitude : double
}

type CreatePostRequestDto = {
  Tweet : string
  Location : LocationDto option
}

type Response = {
  Id : Guid
}

type ErrorResponse = {
  Message : string
}