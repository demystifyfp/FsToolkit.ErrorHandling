module ResultTests 

open Expecto
open SampleDomain
open FsToolkit.ErrorHandling


let lat = 13.067439
let lng = 80.237617 

let validLat = Latitude.TryCreate lat
let invalidLat = Latitude.TryCreate 200.
let validLng = Longitude.TryCreate lng
let invalidLng = Longitude.TryCreate 200.
let validTweet = Tweet.TryCreate "Hello, World!"

let emptyInvalidTweet = Tweet.TryCreate ""


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
      let location = Result.map2 location invalidLat validLng
      Expect.hasErrorValue location "200.0 is a invalid latitude value"
    
    testCase "map2 with one Ok and one Error parts" <| fun _ ->
      let location = Result.map2 location validLat invalidLng
      Expect.hasErrorValue location "200.0 is a invalid longitude value"

    testCase "map2 with two Error parts" <| fun _ -> 
      let location = Result.map2 location invalidLat invalidLng
      Expect.hasErrorValue location "200.0 is a invalid latitude value"
  ]


[<Tests>]
let map3Tests =
  testList "Result.map3 Tests" [
    testCase "map3 with three ok parts" <| fun _ ->
      
      let post = Result.map3 post validLat validLng validTweet
      (fun post -> 
        post.Location.Latitude.Value = lat && 
        post.Location.Longitude.Value = lng && 
        post.Tweet.Value = "Hello, World!")
      |> Expect.hasOkValuePredicate post 

    testCase "map3 with (Error, Ok, Ok)" <| fun _ ->
      let post = Result.map3 post invalidLat validLng validTweet
      Expect.hasErrorValue post "200.0 is a invalid latitude value"
    
    testCase "map3 with (Ok, Error, Ok)" <| fun _ ->
      let post = Result.map3 post validLat invalidLng validTweet
      Expect.hasErrorValue post "200.0 is a invalid longitude value"


    testCase "map3 with (Ok, Ok, Error)" <| fun _ ->
      let post = Result.map3 post validLat validLng emptyInvalidTweet
      Expect.hasErrorValue post "Tweet shouldn't be empty"
    
    testCase "map3 with (Error, Error, Error)" <| fun _ ->
      let post = Result.map3 post invalidLat invalidLng emptyInvalidTweet
      Expect.hasErrorValue post "200.0 is a invalid latitude value"
  ]