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
    testCase "traverseResult with a list of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"] |> Seq.ofList
      let expected = Seq.map tweet tweets
      let actual = Seq.traverseResultM Tweet.TryCreate tweets |> Result.defaultWith (fun _ -> failwith "")
      Expect.sequenceEqual actual expected "Should have a list of valid tweets"

    testCase "traverseResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet] |> Seq.ofList
      let actual = Seq.traverseResultM Tweet.TryCreate tweets
      Expect.equal actual (Error emptyTweetErrMsg) "traverse the list and return the first error"
  ]

let allTests = ftestList "Seq Tests" [
  traverseResultTests
]