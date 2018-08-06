module ResultTests 

open Expecto
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.ResultComputationExpression


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
      |> Expect.hasErrorValue emptyTweetErrMsg
    
    testCase "map3 with (Error, Error, Error)" <| fun _ ->
      Result.map3 post invalidLat invalidLng emptyInvalidTweet
      |> Expect.hasErrorValue invalidLatMsg
  ]

[<Tests>]
let applyTests =

  testList "Result.apply tests" [
    testCase "apply with Ok" <| fun _ ->
      Tweet.TryCreate "foobar"
      |> Result.apply (Ok remainingCharacters) 
      |> Expect.hasOkValue 274
    
    testCase "apply with Error" <| fun _ ->
      Result.apply (Ok remainingCharacters) emptyInvalidTweet
      |> Expect.hasErrorValue emptyTweetErrMsg
  ]

[<Tests>]
let foldTests =

  testList "Result.fold tests" [
    testCase "fold with Ok" <| fun _ ->
      let actual =
        Tweet.TryCreate "foobar"
        |> Result.fold (fun t -> t.Value) id
      Expect.equal actual "foobar" "fold to string should succeed"
    
    testCase "fold with Error" <| fun _ ->
      let actual =
        Tweet.TryCreate ""
        |> Result.fold (fun t -> t.Value) id
      Expect.equal actual emptyTweetErrMsg "fold to string should fail"
  ]


[<Tests>]
let ofChoiceTests =

  testList "Result.ofChoice tests" [
    testCase "ofChoice with Choice1Of2" <| fun _ ->
      Result.ofChoice (Choice1Of2 1)
      |> Expect.hasOkValue 1
    
    testCase "ofChoice with Choice2Of2" <| fun _ ->
      Result.ofChoice (Choice2Of2 1)
      |> Expect.hasErrorValue 1
  ]


[<Tests>]
let resultCETests =
  testList "result Computation Expression tests" [
    testCase "bind with all Ok" <| fun _ ->
      let post = result {
        let! lat = validLat 
        let! lng = validLng 
        let! tweet = validTweet
        return post lat lng tweet
      }
      ((fun post -> 
        post.Location.Latitude.Value = lat && 
        post.Location.Longitude.Value = lng && 
        post.Tweet.Value = "Hello, World!"), post)
      ||> Expect.hasOkValuePredicate 

    testCase "bind with Error" <| fun _ -> 
      let post = result {
        let! lat = invalidLat
        Tests.failtestf "this should not get executed!"
        let! lng = validLng 
        let! tweet = validTweet
        return post lat lng tweet
      }
      post
      |> Expect.hasErrorValue invalidLatMsg
  ]