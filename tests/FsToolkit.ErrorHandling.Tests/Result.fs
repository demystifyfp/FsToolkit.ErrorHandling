module ResultTests 

open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.ComputationExpression.Result
open FsToolkit.ErrorHandling.Operator.Result


[<Tests>]
let map2Tests =
  testList "Result.map2 Tests" [
    testCase "map2 with two ok parts" <| fun _ ->
      Result.map2 location validLatR validLngR
      |> Expect.hasOkValue validLocation 

    testCase "map2 with one Error and one Ok parts" <| fun _ -> 
      Result.map2 location invalidLatR validLngR
      |> Expect.hasErrorValue invalidLatMsg
    
    testCase "map2 with one Ok and one Error parts" <| fun _ ->
      Result.map2 location validLatR invalidLngR
      |> Expect.hasErrorValue invalidLngMsg

    testCase "map2 with two Error parts" <| fun _ -> 
      Result.map2 location invalidLatR invalidLngR
      |> Expect.hasErrorValue invalidLatMsg
  ]


[<Tests>]
let map3Tests =
  testList "Result.map3 Tests" [
    testCase "map3 with three ok parts" <| fun _ ->
      
      Result.map3 createPostRequest validLatR validLngR validTweetR
      |> Expect.hasOkValue validCreatePostRequest

    testCase "map3 with (Error, Ok, Ok)" <| fun _ ->
      Result.map3 createPostRequest invalidLatR validLngR validTweetR
      |> Expect.hasErrorValue  invalidLatMsg
    
    testCase "map3 with (Ok, Error, Ok)" <| fun _ ->
      Result.map3 createPostRequest validLatR invalidLngR  validTweetR
      |> Expect.hasErrorValue invalidLngMsg


    testCase "map3 with (Ok, Ok, Error)" <| fun _ ->
      Result.map3 createPostRequest validLatR validLngR emptyInvalidTweetR
      |> Expect.hasErrorValue emptyTweetErrMsg
    
    testCase "map3 with (Error, Error, Error)" <| fun _ ->
      Result.map3 createPostRequest invalidLatR invalidLngR  emptyInvalidTweetR
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
      Result.apply (Ok remainingCharacters) emptyInvalidTweetR
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
      let createPostRequest = result {
        let! lat = validLatR 
        let! lng = validLngR 
        let! tweet = validTweetR
        return createPostRequest lat lng tweet
      }
      Expect.hasOkValue validCreatePostRequest createPostRequest 

    testCase "bind with Error" <| fun _ -> 
      let post = result {
        let! lat = invalidLatR
        Tests.failtestf "this should not get executed!"
        let! lng = validLngR 
        let! tweet = validTweetR
        return createPostRequest lat lng tweet
      }
      post
      |> Expect.hasErrorValue invalidLatMsg
  ]


[<Tests>]
let resultOperatorsTests =

  testList "Result Operators Tests" [
    testCase "map & apply operators" <| fun _ ->
      createPostRequest <!> validLatR <*> validLngR <*> validTweetR
      |> Expect.hasOkValue validCreatePostRequest
    
    testCase "bind operator" <| fun _ ->
      validLatR
      >>= (fun lat -> 
            validLngR
            >>= (fun lng ->
                  validTweetR
                  >>= (fun tweet ->
                        Ok (createPostRequest lat lng tweet))))
      |> Expect.hasOkValue validCreatePostRequest
  ]
