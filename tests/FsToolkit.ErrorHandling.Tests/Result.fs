module ResultTests 

open Expecto
open SampleDomain
open FsToolkit.ErrorHandling


let lat = 13.067439
let lng = 80.237617 

let validLat = Latitude.TryCreate lat
let validLng = Longitude.TryCreate lng
let validTweet = Tweet.TryCreate "Hello, World!"

[<Tests>]
let map2Tests =
  testList "Result.map2 Tests" [
    testCase "map2 with two ok parts" <| fun _ ->
      let location = Result.map2 location validLat validLng
      (fun location -> 
        location.Latitude.Value = lat && 
        location.Longitude.Value = lng)
      |> Expect.hasOkValuePredicate location 

    testCase "map2 with one Error and one Ok parts" <| fun _ ->
      let lat = Latitude.TryCreate 200.
      let lng = Longitude.TryCreate 80.237617
      let location = Result.map2 location lat lng
      Expect.hasErrorValue location "200.0 is a invalid latitude value"
    
    testCase "map2 with one Ok and one Error parts" <| fun _ ->
      let lat = Latitude.TryCreate 13.067439
      let lng = Longitude.TryCreate 200.
      let location = Result.map2 location lat lng
      Expect.hasErrorValue location "200.0 is a invalid longitude value"
  ]


[<Tests>]
let map3Tests =
  testList "Result.map3 Tests" [
    testCase "map3 with three ok parts" <| fun _ ->
      let lat = Latitude.TryCreate 13.067439
      let lng = Longitude.TryCreate 80.237617
      let tweet = Tweet.TryCreate "Hello, World!"
      let post = Result.map3 post lat lng tweet
      (fun post -> 
        post.Location.Latitude.Value = 13.067439 && 
        post.Location.Longitude.Value = 80.237617 && 
        post.Tweet.Value = "Hello, World!")
      |> Expect.hasOkValuePredicate post 

    testCase "map3 with (Error, Ok, Ok)" <| fun _ ->
      let lat = Latitude.TryCreate 200.
      let lng = Longitude.TryCreate 80.237617
      let tweet = Tweet.TryCreate "Hello, World!"
      let post = Result.map3 post lat lng tweet
      Expect.hasErrorValue post "200.0 is a invalid latitude value"
    
    testCase "map3 with (Ok, Error, Ok)" <| fun _ ->
      let lat = Latitude.TryCreate 13.067439
      let lng = Longitude.TryCreate 200.
      let tweet = Tweet.TryCreate "Hello, World!"
      let post = Result.map3 post lat lng tweet
      Expect.hasErrorValue post "200.0 is a invalid longitude value"


    testCase "map3 with (Ok, Ok, Error)" <| fun _ ->
      let lat = Latitude.TryCreate 13.067439
      let lng = Longitude.TryCreate 80.237617
      let tweet = Tweet.TryCreate ""
      let post = Result.map3 post lat lng tweet
      Expect.hasErrorValue post "Tweet shouldn't be empty"
  ]