module ResultTests 

open Expecto
open SampleDomain
open FsToolkit.ErrorHandling


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


[<Tests>]
let map2Tests =
  testList "Result.map2 Tests" [
    testCase "map2 with two ok parts" <| fun _ ->
      let location = Result.map2 location validLat validLng
      ((fun location -> 
        location.Latitude.Value = lat && 
        location.Longitude.Value = lng), location)
      ||> Expect.hasOkValuePredicate 

    testCase "map2 with one Error and one Ok parts" <| fun _ -> 
      Result.map2 location invalidLat validLng
      |> Expect.hasErrorValue invalidLatMsg
    
    testCase "map2 with one Ok and one Error parts" <| fun _ ->
      Result.map2 location validLat invalidLng
      |> Expect.hasErrorValue invalidLngMsg

    testCase "map2 with two Error parts" <| fun _ -> 
      Result.map2 location invalidLat invalidLng
      |> Expect.hasErrorValue invalidLatMsg
  ]


[<Tests>]
let map3Tests =
  testList "Result.map3 Tests" [
    testCase "map3 with three ok parts" <| fun _ ->
      
      let post = Result.map3 post validLat validLng validTweet
      ((fun post -> 
        post.Location.Latitude.Value = lat && 
        post.Location.Longitude.Value = lng && 
        post.Tweet.Value = "Hello, World!"), post)
      ||> Expect.hasOkValuePredicate 

    testCase "map3 with (Error, Ok, Ok)" <| fun _ ->
      Result.map3 post invalidLat validLng validTweet
      |> Expect.hasErrorValue invalidLatMsg
    
    testCase "map3 with (Ok, Error, Ok)" <| fun _ ->
      Result.map3 post validLat invalidLng validTweet
      |> Expect.hasErrorValue invalidLngMsg


    testCase "map3 with (Ok, Ok, Error)" <| fun _ ->
      Result.map3 post validLat validLng emptyInvalidTweet
      |> Expect.hasErrorValue "Tweet shouldn't be empty"
    
    testCase "map3 with (Error, Error, Error)" <| fun _ ->
      Result.map3 post invalidLat invalidLng emptyInvalidTweet
      |> Expect.hasErrorValue invalidLatMsg
  ]

[<Tests>]
let applyTests =
  let inc a = a + 1

  testList "Result.apply tests" [
    testCase "apply with Ok" <| fun _ ->
      Result.apply (Ok inc) (Ok 1)
      |> Expect.hasOkValue 2
    
    testCase "apply with Error" <| fun _ ->
      Result.apply (Ok inc) (Error 1)
      |> Expect.hasErrorValue 1
  ]