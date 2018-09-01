module TestData
open SampleDomain

let flip f x y = f y x
let lat = 13.067439
let lat2 = 14.067439
let lng = 80.237617 

let okOrFail = function
| Ok x -> x
| Error e -> failwithf "%A" e

let validLatR = Latitude.TryCreate lat
let validLatR2 = Latitude.TryCreate lat2
let validLat = okOrFail validLatR
let validLat2 = okOrFail validLatR2
let invalidLatR = Latitude.TryCreate 200.
let invalidLatMsg = "200.0 is a invalid latitude value"
let validLngR = Longitude.TryCreate lng
let validLng = okOrFail validLngR
let invalidLngR  = Longitude.TryCreate 200.
let invalidLngMsg = "200.0 is a invalid longitude value"

let validLocation : Location = 
  {Latitude = validLat; Longitude = validLng}
let validTweetR = Tweet.TryCreate "Hello, World!"
let validTweet2R = Tweet.TryCreate "A link http://bit.ly/test"
let validTweet = okOrFail validTweetR
let validTweet2 = okOrFail validTweet2R

let tweet twit  = Tweet.TryCreate twit |> okOrFail

let validURL = Url.TryCreate "http://bit.ly/test" |> okOrFail

let validCreatePostRequest : CreatePostRequest = 
  {Tweet = validTweet; Location = Some validLocation; UserId = sampleUserId}
let emptyInvalidTweetR = Tweet.TryCreate ""
let emptyTweetErrMsg = "Tweet shouldn't be empty"
let longerTweetErrMsg = "Tweet shouldn't contain more than 280 characters"

let aLongerInvalidTweet = [1..100] |> List.map string |> String.concat ","

