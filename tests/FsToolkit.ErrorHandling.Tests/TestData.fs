module TestData
open SampleDomain
let lat = 13.067439
let lng = 80.237617 

let validLat = Latitude.TryCreate lat
let invalidLat = Latitude.TryCreate 200.
let invalidLatMsg = "200.0 is a invalid latitude value"
let validLng = Longitude.TryCreate lng
let invalidLng = Longitude.TryCreate 200.
let invalidLngMsg = "200.0 is a invalid longitude value"
let validTweet = Tweet.TryCreate "Hello, World!"
let emptyInvalidTweet = Tweet.TryCreate ""
let emptyTweetErrMsg = "Tweet shouldn't be empty"