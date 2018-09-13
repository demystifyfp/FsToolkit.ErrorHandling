module ValidationTests

open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation

let lift = Validation.ofResult

[<Tests>]
let map2Tests =
  testList "Validation.map2 Tests" [
    testCase "map2 with two ok parts" <| fun _ ->
      Validation.map2 location (lift validLatR) (lift validLngR)
      |> Expect.hasOkValue validLocation 

    testCase "map2 with one Error and one Ok parts" <| fun _ -> 
      Validation.map2 location (lift invalidLatR) (lift validLngR)
      |> Expect.hasErrorValue [invalidLatMsg]
    
    testCase "map2 with one Ok and one Error parts" <| fun _ ->
      Validation.map2 location (lift validLatR) (lift invalidLngR)
      |> Expect.hasErrorValue [invalidLngMsg]

    testCase "map2 with two Error parts" <| fun _ -> 
      Validation.map2 location (lift invalidLatR) (lift invalidLngR)
      |> Expect.hasErrorValue [invalidLatMsg; invalidLngMsg]
  ]

[<Tests>]
let map3Tests =
  testList "Validation.map3 Tests" [
    testCase "map3 with three ok parts" <| fun _ ->
      Validation.map3 createPostRequest (lift validLatR) (lift validLngR) (lift validTweetR)
      |> Expect.hasOkValue validCreatePostRequest

    testCase "map3 with (Error, Ok, Ok)" <| fun _ ->
      Validation.map3 createPostRequest (lift invalidLatR) (lift validLngR) (lift validTweetR)
      |> Expect.hasErrorValue [invalidLatMsg]
    
    testCase "map3 with (Ok, Error, Ok)" <| fun _ ->
      Validation.map3 createPostRequest (lift validLatR) (lift invalidLngR) (lift validTweetR)
      |> Expect.hasErrorValue [invalidLngMsg]


    testCase "map3 with (Ok, Ok, Error)" <| fun _ ->
      Validation.map3 createPostRequest (lift validLatR) (lift validLngR) (lift emptyInvalidTweetR)
      |> Expect.hasErrorValue [emptyTweetErrMsg]
    
    testCase "map3 with (Error, Error, Error)" <| fun _ ->
      Validation.map3 createPostRequest (lift invalidLatR) (lift invalidLngR) (lift emptyInvalidTweetR)
      |> Expect.hasErrorValue [invalidLatMsg;invalidLngMsg;emptyTweetErrMsg]
  ]

[<Tests>]
let applyTests =

  testList "Validation.apply tests" [
    testCase "apply with Ok" <| fun _ ->
      Tweet.TryCreate "foobar"
      |> lift
      |> Validation.apply (Ok remainingCharacters) 
      |> Expect.hasOkValue 274
    
    testCase "apply with Error" <| fun _ ->
      Validation.apply (Ok remainingCharacters) (lift emptyInvalidTweetR)
      |> Expect.hasErrorValue [emptyTweetErrMsg]
  ]

[<Tests>]
let operatorsTests =

  testList "Validation Operators Tests" [
    testCase "map & apply operators" <| fun _ ->
      createPostRequest <!> (lift validLatR) <*> (lift validLngR) <*> (lift validTweetR)
      |> Expect.hasOkValue validCreatePostRequest

    testCase "map^ & apply^ operators" <| fun _ ->
      createPostRequest <!^> validLatR <*^> validLngR <*^> validTweetR
      |> Expect.hasOkValue validCreatePostRequest
  ]