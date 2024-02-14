module SeqTests

#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open SampleDomain
open TestData
open FsToolkit.ErrorHandling

let sequenceResultMTests =
    testList "Seq.sequenceResultM Tests" [
        testCase "sequenceResult with an empty sequence"
        <| fun _ ->
            let tweets = Seq.empty
            let expected = Ok [||]

            let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "Should have an empty list of valid tweets"

        testCase "sequenceResult with a sequence of valid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected = Ok [| for x in tweets -> tweet x |]

            let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "sequenceResult with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    ""
                    "Hello"
                    aLongerInvalidTweet
                }

            let expected = Error emptyTweetErrMsg

            let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "traverse the sequence and return the first error"

        testCase "sequenceResult stops after first invalid data"
        <| fun _ ->
            let mutable counter = 0

            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                    aLongerInvalidTweet

                    counter <-
                        counter
                        + 1
                }

            let expected = Error longerTweetErrMsg

            let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "traverse the sequence and return the first error"

            Expect.equal counter 0 "evaluation of the sequence stops at the first error"
    ]

let allTests = testList "Seq Tests" [ sequenceResultMTests ]
