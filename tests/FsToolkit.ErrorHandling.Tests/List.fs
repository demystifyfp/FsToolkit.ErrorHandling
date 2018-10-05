module ListTests

open Expecto
open SampleDomain
open TestData
open System
open FsToolkit.ErrorHandling


[<Tests>]
let traverseResultMTests =
  testList "List.traverseResultM Tests" [
    testCase "traverseResult with a list of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"]
      let expected = List.map tweet tweets |> Ok
      let actual = List.traverseResultM Tweet.TryCreate tweets
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "traverseResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = List.traverseResultM Tweet.TryCreate tweets
      Expect.equal actual (Error emptyTweetErrMsg) "traverse the list and return the first error"
  ]


[<Tests>]
let sequenceResultMTests =
  testList "List.sequenceResultM Tests" [
    testCase "traverseResult with a list of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"]
      let expected = List.map tweet tweets |> Ok
      let actual = List.sequenceResultM (List.map Tweet.TryCreate tweets) 
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "sequenceResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = List.sequenceResultM (List.map Tweet.TryCreate tweets) 
      Expect.equal actual (Error emptyTweetErrMsg) "traverse the list and return the first error"
  ]


[<Tests>]
let traverseResultATests =
  testList "List.traverseResultA Tests" [
    testCase "traverseResultA with a list of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"]
      let expected = List.map tweet tweets |> Ok
      let actual = List.traverseResultA Tweet.TryCreate tweets
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = List.traverseResultA Tweet.TryCreate tweets
      Expect.equal actual (Error [emptyTweetErrMsg;longerTweetErrMsg]) "traverse the list and return all the errors"
  ]

[<Tests>]
let sequenceResultATests =
  testList "List.sequenceResultA Tests" [
    testCase "traverseResult with a list of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"]
      let expected = List.map tweet tweets |> Ok
      let actual = List.sequenceResultA (List.map Tweet.TryCreate tweets) 
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "sequenceResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = List.sequenceResultA (List.map Tweet.TryCreate tweets) 
      Expect.equal actual (Error [emptyTweetErrMsg;longerTweetErrMsg]) "traverse the list and return all the errors"
  ]

[<Tests>]
let traverseValidationATests =
  testList "List.traverseValidationA Tests" [
    testCase "traverseValidationA with a list of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"]
      let expected = List.map tweet tweets |> Ok
      let actual = List.traverseValidationA (Tweet.TryCreate >> (Result.mapError List.singleton)) tweets
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "traverseValidationA with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = List.traverseValidationA (Tweet.TryCreate >> (Result.mapError List.singleton)) tweets
      Expect.equal actual (Error [longerTweetErrMsg;emptyTweetErrMsg]) "traverse the list and return all the errors"
  ]

[<Tests>]
let sequenceValidationATests =
  let tryCreateTweet = Tweet.TryCreate >> (Result.mapError List.singleton)
  testList "List.sequenceValidationA Tests" [
    testCase "traverseValidation with a list of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"]
      let expected = List.map tweet tweets |> Ok
      let actual = List.sequenceValidationA (List.map tryCreateTweet tweets) 
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "sequenceValidationM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = List.sequenceValidationA (List.map tryCreateTweet tweets) 
      Expect.equal actual (Error [longerTweetErrMsg;emptyTweetErrMsg]) "traverse the list and return all the errors"
  ]

let userId1 = Guid.NewGuid()
let userId2 = Guid.NewGuid()
let userId3 = Guid.NewGuid()
let userId4 = Guid.NewGuid()

[<Tests>]
let traverseAsyncResultMTests =
  
  let userIds = List.map UserId [userId1;userId2;userId3]

  testList "List.traverseAsyncResultM Tests" [
    testCase "traverseAsyncResultM with a list of valid data" <| fun _ ->
      let expected = [();();()]
      let actual = 
        List.traverseAsyncResultM (notifyNewPostSuccess (PostId newPostId)) userIds
      Expect.hasAsyncOkValue expected actual

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let expected = sprintf "error: %s" (userId1.ToString())
      let actual = 
        List.traverseAsyncResultM (notifyNewPostFailure (PostId newPostId)) userIds
      Expect.hasAsyncErrorValue expected actual
  ]

let notifyFailure (PostId _) (UserId uId) = async {
  if (uId = userId1 || uId = userId3) then
    return sprintf "error: %s" (uId.ToString()) |> Error
  else
    return Ok ()
}

[<Tests>]
let traverseAsyncResultATests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.traverseAsyncResultA Tests" [
    testCase "traverseAsyncResultA with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.traverseAsyncResultA (notifyNewPostSuccess (PostId newPostId)) userIds
      Expect.hasAsyncOkValue expected actual

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let expected = [sprintf "error: %s" (userId1.ToString())
                      sprintf "error: %s" (userId3.ToString())]
      let actual = 
        List.traverseAsyncResultA (notifyFailure (PostId newPostId)) userIds
      Expect.hasAsyncErrorValue expected actual
  ]

[<Tests>]
let sequenceAsyncResultMTests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.sequenceAsyncResultM Tests" [
    testCase "sequenceAsyncResultM with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.map (notifyNewPostSuccess (PostId newPostId)) userIds
        |> List.sequenceAsyncResultM
      Expect.hasAsyncOkValue expected actual
    
    testCase "sequenceAsyncResultM with few invalid data" <| fun _ ->
      let expected = sprintf "error: %s" (userId1.ToString())
      let actual = 
        List.map (notifyFailure (PostId newPostId)) userIds
        |> List.sequenceAsyncResultM
      Expect.hasAsyncErrorValue expected actual
  ]

[<Tests>]
let sequenceAsyncResultATests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.sequenceAsyncResultA Tests" [
    testCase "sequenceAsyncResultA with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.map (notifyNewPostSuccess (PostId newPostId)) userIds
        |> List.sequenceAsyncResultA
      Expect.hasAsyncOkValue expected actual
    
    testCase "sequenceAsyncResultA with few invalid data" <| fun _ ->
      let expected = [sprintf "error: %s" (userId1.ToString())
                      sprintf "error: %s" (userId3.ToString())]
      let actual = 
        List.map (notifyFailure (PostId newPostId)) userIds
        |> List.sequenceAsyncResultA
      Expect.hasAsyncErrorValue expected actual
  ]