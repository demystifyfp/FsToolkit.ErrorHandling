module SeqTests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif
open SampleDomain
open TestData
open TestHelpers
open System
open FsToolkit.ErrorHandling

let traverseResultTests =
  testList "Seq.traverseResultM Tests" [
    testCase "traverseResult with a seq of valid data" <| fun _ ->
      let tweets = [ "Hi"; "Hello"; "Hola" ] |> List.toSeq
      let expected = Seq.map tweet tweets
      let actual = Seq.traverseResultM Tweet.TryCreate tweets |> Result.defaultWith (fun _ -> failwith "")
      Expect.sequenceEqual actual expected "Should have a list of valid tweets"

    testCase "traverseResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet] |> Seq.ofList
      let actual = Seq.traverseResultM Tweet.TryCreate tweets
      Expect.equal actual (Error emptyTweetErrMsg) "traverse the seq and return the first error"
  ]

let sequenceResultMTests =
  testList "Seq.sequenceResultM Tests" [
    testCase "traverseResult with a seq of valid data" <| fun _ ->
      let tweets = [ "Hi"; "Hello"; "Hola" ] |> List.toSeq
      let expected = Seq.map tweet tweets
      let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)  |> Result.defaultWith(fun _ -> failwith "")
      Expect.sequenceEqual actual expected "Should have a seq of valid tweets"

    testCase "sequenceResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets) 
      Expect.equal actual (Error emptyTweetErrMsg) "traverse the list and return the first error"
  ]

let traverseResultATests =
  testList "Seq.traverseResultA Tests" [
    testCase "traverseResultA with a seq of valid data" <| fun _ ->
      let tweets = [ "Hi"; "Hello"; "Hola" ] |> List.toSeq
      let expected = Seq.map tweet tweets
      let actual = Seq.traverseResultA Tweet.TryCreate tweets |> Result.defaultWith (fun _ -> failwith "")
      Expect.sequenceEqual actual expected "Should have a list of valid tweets"

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let tweets = [ ""; "Hello"; aLongerInvalidTweet ] |> List.toSeq
      let actual = Seq.traverseResultA Tweet.TryCreate tweets
      Expect.equal actual (Error [emptyTweetErrMsg;longerTweetErrMsg]) "traverse the list and return all the errors"
  ]


let sequenceResultATests =
  testList "Seq.sequenceResultA Tests" [
    testCase "traverseResult with a seq of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"] |> List.toSeq
      let expected = Seq.map tweet tweets
      let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)  |> Result.defaultWith(fun _ -> failwith "")
      Expect.sequenceEqual actual expected "Should have a list of valid tweets"

    testCase "sequenceResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet] |> Seq.ofList
      let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets) 
      Expect.equal actual (Error [emptyTweetErrMsg;longerTweetErrMsg]) "traverse the list and return all the errors"
  ]

let allTests = testList "Seq Tests" [
  traverseResultTests
  sequenceResultMTests
  traverseResultATests
  sequenceResultATests
]