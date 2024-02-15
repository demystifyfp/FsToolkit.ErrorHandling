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
open FsToolkit.ErrorHandling
open SampleDomain
open TestData

let sequenceResultMTests =
    testList "Seq.sequenceResultM Tests" [
        testCase "empty sequence"
        <| fun _ ->
            let tweets = Seq.empty
            let expected = Ok [||]

            let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "Should have an empty list of valid tweets"

        testCase "valid data"
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

        testCase "valid and invalid data"
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

        testCase "stops after first invalid data"
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

let sequenceResultATests =
    testList "Seq.sequenceResultA Tests" [
        testCase "valid data only"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected = Ok [| for t in tweets -> tweet t |]

            let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "Should yield an array of valid tweets"

        testCase "valid and multiple invalid data"
        <| fun _ ->
            let tweets = [
                ""
                "Hello"
                aLongerInvalidTweet
            ]

            let expected =
                Error [|
                    emptyTweetErrMsg
                    longerTweetErrMsg
                |]

            let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "traverse the seq and return all the errors"

        testCase "iterates exacly once"
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

            let expected = Error [| longerTweetErrMsg |]

            let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "traverse the seq and return all the errors"

            Expect.equal counter 1 "evaluation of the sequence completes exactly once"
    ]

let allTests =
    testList "Seq Tests" [
        sequenceResultMTests
        sequenceResultATests
    ]
