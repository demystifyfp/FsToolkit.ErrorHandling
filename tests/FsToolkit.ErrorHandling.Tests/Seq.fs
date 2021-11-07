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



let sequenceResultMTests =
    testList
        "Seq.sequenceResultM Tests"
        [ testCase "traverseResult with an empty sequence"
          <| fun _ ->
              let tweets = []
              let expected = Ok []

              let actual =
                  Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

              Expect.equal actual expected "Should have an empty list of valid tweets"

          testCase "traverseResult with a sequence of valid data"
          <| fun _ ->
              let tweets = [ "Hi"; "Hello"; "Hola" ]
              let expected = List.map tweet tweets |> Ok

              let actual =
                  Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

              Expect.equal actual expected "Should have a list of valid tweets"

          testCase "sequenceResultM with few invalid data"
          <| fun _ ->
              let tweets =
                  [ ""; "Hello"; aLongerInvalidTweet ] :> seq<_>

              let actual =
                  Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

              Expect.equal actual (Error emptyTweetErrMsg) "traverse the sequence and return the first error"

          testCase "sequenceResultM stops after first invalid data"
          <| fun _ ->
              let mutable counter = 0

              let tweets =
                  seq {
                      "Hi"
                      "Hello"
                      "Hola"
                      aLongerInvalidTweet
                      counter <- counter + 1
                  }

              let actual =
                  Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

              Expect.equal actual (Error longerTweetErrMsg) "traverse the sequence and return the first error"
              Expect.equal counter 0 "evaluation of the sequence stops at the first error" ]

let allTests =
    testList "Seq Tests" [ sequenceResultMTests ]
