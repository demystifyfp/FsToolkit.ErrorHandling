module SampleDomain

open FsToolkit.ErrorHandling
open System


let okOrFail = function
| Ok x -> x
| Error e -> failwithf "%A" e

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

let isWellFormedUrl (url : string) =

  #if FABLE_COMPILER
  let urlRegex = "^(?!mailto:)(?:(?:http|https|ftp)://)(?:\\S+(?::\\S*)?@)?(?:(?:(?:[1-9]\\d?|1\\d\\d|2[01]\\d|22[0-3])(?:\\.(?:1?\\d{1,2}|2[0-4]\\d|25[0-5])){2}(?:\\.(?:[0-9]\\d?|1\\d\\d|2[0-4]\\d|25[0-4]))|(?:(?:[a-z\\u00a1-\\uffff0-9]+-?)*[a-z\\u00a1-\\uffff0-9]+)(?:\\.(?:[a-z\\u00a1-\\uffff0-9]+-?)*[a-z\\u00a1-\\uffff0-9]+)*(?:\\.(?:[a-z\\u00a1-\\uffff]{2,})))|localhost)(?::\\d{2,5})?(?:(/|\\?|#)[^\\s]*)?$"
  Text.RegularExpressions.Regex.IsMatch(url, urlRegex)
  #else
  Uri.IsWellFormedUriString(url, UriKind.Absolute)
  #endif   

type Url = private Url of string with
  member this.Value =
    let (Url url) = this
    url
  
  static member TryCreate (url : string) =
    if isWellFormedUrl url then 
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
  |> Array.tryFind (isWellFormedUrl)
  |> Option.traverseResult Url.TryCreate

type UserId = UserId of Guid

type PersonName = private PersonName of string with
  member this.Value =
    let (PersonName name) = this
    name

  static member TryCreate (name : string) =
    match name with
    | x when String.IsNullOrEmpty x -> 
      Error "Name shouldn't be empty"
    | x when x.Length > 80 ->
      Error "Name shouldn't contain more than 80 characters"
    | x -> Ok (PersonName x)


type DateOfBirth = private DateOfBirth of DateTime with
    static member TryCreate (dateTime : DateTime) =
      match dateTime with
      | x when x > DateTime.Now ->
        Error "Date Of Birth Should not be in Future"
      | x when x = Unchecked.defaultof<DateTime> ->
        Error "Date Of Birth should not be empty"
      | _ ->
        Ok (DateOfBirth dateTime)

    member this.Value =
      let (DateOfBirth dob) = this
      dob

type CreateUserRequest = {
  Name : PersonName
  DateOfBirth : DateOfBirth
}

let createUserRequest name dob =
  {Name = name; DateOfBirth = dob}

type CreateUserRequestDto = {
  Name : string
  DateOfBirth : DateTime
}

let commonEx = new Exception("something went wrong!")

type User = {
  Id : UserId
  Name : PersonName
}

type UserDto = {
  Id : Guid
  Name : string
} with 
  static member ToUser (dto : UserDto) = result {
    let! name = PersonName.TryCreate dto.Name
    return {User.Id = UserId dto.Id; Name = name}
  }

let sampleUserGuid = System.Guid.NewGuid()
let sampleUserId = UserId sampleUserGuid

let sampleUserDto = {Id = sampleUserGuid; Name = "someone"}
let sampleUser = UserDto.ToUser {Id = sampleUserGuid; Name = "someone"} |> okOrFail
let getUserById (userId : UserId) = async {
  if userId = sampleUserId then 
    let user = Some sampleUserDto
    return Option.traverseResult UserDto.ToUser user
  elif userId = UserId Guid.Empty then
    return Error "invalid user id"
  else
    return Ok None
}


type CreatePostRequest = {
  UserId : UserId
  Tweet : Tweet
  Location : Location option
}



let createPostRequest lat long tweet =
  {Tweet = tweet; Location = Some(location lat long); UserId = sampleUserId}


let getFollowersEx = new Exception("unable to fetch followers!")

let allowedToPost userId = async {
  if (userId = sampleUserId) then 
    return Ok true
  else
    return Error commonEx
}

let newPostId = Guid.NewGuid()

type PostId = PostId of Guid
let samplePostId = PostId newPostId

type Post = {
  Id : PostId
  UserId : UserId
  Tweet : Tweet
  Location : Location option
}




let createPostSuccess (_ : CreatePostRequest) = async {
  return Ok samplePostId
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


let notifyNewPostSuccess (PostId _) (UserId _) = async {
  return Ok ()
}

let notifyNewPostFailure (PostId _) (UserId uId) = async {
  return sprintf "error: %s" (uId.ToString()) |> Error
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
} with
  static member ToLocation(dto: LocationDto) = result {
    let! lat = Latitude.TryCreate dto.Latitude
    let! lng = Longitude.TryCreate dto.Longitude
    return {Location.Latitude = lat; Longitude = lng}
  }

type CreatePostRequestDto = {
  Tweet : string
  Location : LocationDto option
}

type PostDto = {
  Id : Guid
  UserId : Guid
  Tweet : string
  Location : LocationDto option
} with 
  static member ToPost(dto : PostDto) = result {
    let! location =
      dto.Location
      |> Option.traverseResult LocationDto.ToLocation
    let! tweet = Tweet.TryCreate dto.Tweet
    return {
      Post.Id = PostId dto.Id
      UserId = UserId dto.UserId
      Tweet = tweet
      Location = location
    }
  }

let samplePostDto = {
  Id = newPostId
  UserId = sampleUserGuid
  Tweet = "Hello, World!"
  Location = None
}

let getPostById postId = async {
  if postId = samplePostId then
    return Option.traverseResult PostDto.ToPost (Some samplePostDto)
  elif postId = (PostId Guid.Empty) then
    return Error "invalid post id"
  else
    return Ok None
}
  

type Response = {
  Id : Guid
}

type ErrorResponse = {
  Message : string
}