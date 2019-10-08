module ResultOptionTests


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open TestData
open TestHelpers
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.ResultOption



let map2Tests =
  testList "ResultOption.map2 Tests" [

    testCase "map2 with Ok(Some x) Ok(Some y)" <| fun _ ->
      ResultOption.map2 location (Ok (Some validLat)) (Ok (Some validLng))
      |> Expect.hasOkValue (Some validLocation)

    testCase "map2 with Ok(Some x) Ok(None)" <| fun _ ->
      ResultOption.map2 location (Ok (Some validLat)) (Ok None)
      |> Expect.hasOkValue None

    testCase "map2 with Ok(None) Ok(None)" <| fun _ ->
      ResultOption.map2 location (Ok None) (Ok None)
      |> Expect.hasOkValue None

    testCase "map2 with Error(Some x) Error(Some x)" <| fun _ ->
      ResultOption.map2 location (Error (Some validLat)) (Error (Some validLat2))
      |> Expect.hasErrorValue (Some validLat)
    
    testCase "map2 with Ok(Some x) Error(Some x)" <| fun _ ->
      ResultOption.map2 location (Ok (Some validLat)) (Error (Some validLng))
      |> Expect.hasErrorValue (Some validLng)
  ]


let map3Tests =
  testList "ResultOption.map3 Tests" [

    testCase "map3 with Ok(Some x) Ok(Some y) Ok (Some z)" <| fun _ ->
      ResultOption.map3 createPostRequest (Ok (Some validLat)) (Ok (Some validLng)) (Ok (Some validTweet))
      |> Expect.hasOkValue (Some validCreatePostRequest)

    testCase "map3 with Ok(Some x) Ok(None) Ok(None)" <| fun _ ->
      ResultOption.map3 createPostRequest (Ok (Some validLat)) (Ok None) (Ok None)
      |> Expect.hasOkValue None

    testCase "map3 with Ok(None) Ok(None) (Ok None)" <| fun _ ->
      ResultOption.map3 createPostRequest (Ok None) (Ok None) (Ok None)
      |> Expect.hasOkValue None

    testCase "map3 with Error(Some x) Error(Some x) (Ok None)" <| fun _ ->
      ResultOption.map3 createPostRequest (Error (Some validLat)) (Error (Some validLat2)) (Ok None)
      |> Expect.hasErrorValue (Some validLat)
    
    testCase "map3 with Ok(Some x) Error(Some x) (Ok None)" <| fun _ ->
      ResultOption.map3 createPostRequest (Ok (Some validLat)) (Error (Some validLng)) (Ok None)
      |> Expect.hasErrorValue (Some validLng)
  ]



let applyTests = 
  testList "ResultOption.apply Tests" [
    testCase "apply with Ok(Some x)" <| fun _ ->
      Ok (Some validTweet)
      |> ResultOption.apply (Ok (Some remainingCharacters)) 
      |> Expect.hasOkValue (Some 267)
    
    testCase "apply with Ok(None)" <| fun _ ->
      Ok None
      |> ResultOption.apply (Ok (Some remainingCharacters)) 
      |> Expect.hasOkValue None
  ]


let mapTests =
  testList "ResultOption.map Tests" [
    testCase "map with Ok(Some x)" <| fun _ ->
      Ok (Some validTweet)
      |> ResultOption.map remainingCharacters
      |> Expect.hasOkValue (Some 267)
    
    testCase "map with Ok(None)" <| fun _ ->
      Ok None
      |> ResultOption.map remainingCharacters
      |> Expect.hasOkValue None
  ]


let bindTests =
  testList "ResultOption.bind Tests" [
    testCase "bind with Ok(Some x)" <| fun _ ->
      Ok (Some validTweet2)
      |> ResultOption.bind firstURLInTweet
      |> Expect.hasOkValue (Some validURL)
  ]


let ignoreTests =
  testList "ResultOption.ignore Tests" [
    testCase "ignore with Ok(Some x)" <| fun _ ->
      Ok (Some validTweet)
      |> ResultOption.ignore
      |> Expect.hasOkValue (Some ())
    
    testCase "ignore with Ok(None)" <| fun _ ->
      Ok None
      |> ResultOption.ignore
      |> Expect.hasOkValue None
  ]


let resultOptionCETests =
  testList "resultOption Computation Expression tests" [
    testCase "bind with all Ok" <| fun _ ->
      let createPostRequest = resultOption {
        let! lat = Ok (Some validLat) 
        let! lng = Ok (Some validLng)
        let! tweet = validTweetR |> Result.map Some
        return createPostRequest lat lng tweet
      }
      Expect.hasOkValue (Some validCreatePostRequest) createPostRequest 

    testCase "bind with Error" <| fun _ -> 
      let post = resultOption {
        let! lat = invalidLatR |> Result.map Some
        Tests.failtestf "this should not get executed!"
        let! lng = Ok (Some validLng)
        let! tweet = validTweetR |> Result.map Some
        return createPostRequest lat lng tweet
      }
      post
      |> Expect.hasErrorValue invalidLatMsg
  ]


let resultOptionOperatorsTests =

  testList "ResultOption Operators Tests" [
    testCase "map & apply operators" <| fun _ ->
      createPostRequest <!> Ok (Some validLat) <*> Ok (Some validLng) <*^> Ok validTweet
      |> Expect.hasOkValue (Some validCreatePostRequest)
    
    testCase "bind operator" <| fun _ ->
      Ok (Some validTweet2)
      >>= firstURLInTweet
      |> Expect.hasOkValue (Some validURL)
  ]

let allTests = testList "Result Option Tests" [
  map2Tests
  map3Tests
  applyTests
  mapTests
  bindTests
  ignoreTests
  resultOptionCETests
  resultOptionOperatorsTests
]