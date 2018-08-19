module ListTests

open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling


[<Tests>]
let traverseResultATests =
  testList "List.traverseResult Tests" [
    testCase "traverseResult with a list of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"]
      let expected = List.map tweet tweets |> Ok
      let actual = List.traverseResult Tweet.TryCreate tweets
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "traverseResult with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = List.traverseResult Tweet.TryCreate tweets
      Expect.equal actual (Error emptyTweetErrMsg) "traverse the list and return the first error"
  ]