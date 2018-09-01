module ListTests

open Expecto
open SampleDomain
open TestData
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
      Expect.equal actual (Error [longerTweetErrMsg;emptyTweetErrMsg]) "traverse the list and return all the errors"
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
      Expect.equal actual (Error [longerTweetErrMsg;emptyTweetErrMsg]) "traverse the list and return all the errors"
  ]


// [<Tests>]
// let traverseAsyncResult