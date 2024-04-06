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
open TestHelpers
open System
open FsToolkit.ErrorHandling

let traverseResultMTests =
    testList "Seq.traverseResultM Tests" [
        testCase "traverseResult with a sequence of valid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected =
                Seq.map tweet tweets
                |> Seq.toList

            let actual = Seq.traverseResultM Tweet.TryCreate tweets

            let actual =
                Expect.wantOk actual "Expected result to be Ok"
                |> Seq.toList

            Expect.equal actual expected "Should have a sequence of valid tweets"

        testCase "traverseResultM with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    ""
                    "Hello"
                    aLongerInvalidTweet
                }

            let actual = Seq.traverseResultM Tweet.TryCreate tweets

            Expect.equal
                actual
                (Error emptyTweetErrMsg)
                "traverse the sequence and return the first error"
    ]

let traverseOptionMTests =
    testList "Seq.traverseOptionM Tests" [
        let tryTweetOption x =
            match x with
            | x when String.IsNullOrEmpty x -> None
            | _ -> Some x

        testCase "traverseOption with a sequence of valid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected = Seq.toList tweets
            let actual = Seq.traverseOptionM tryTweetOption tweets

            let actual =
                Expect.wantSome actual "Expected result to be Some"
                |> Seq.toList

            Expect.equal actual expected "Should have a sequence of valid tweets"

        testCase "traverseOption with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    String.Empty
                }

            let expected = None
            let actual = Seq.traverseOptionM tryTweetOption tweets

            Expect.equal actual expected "traverse the sequence and return none"
    ]

let sequenceResultMTests =
    testList "Seq.sequenceResultM Tests" [
        testCase "traverseResult with a sequence of valid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected =
                Seq.map tweet tweets
                |> Seq.toList

            let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

            let actual =
                Expect.wantOk actual "Expected result to be Ok"
                |> Seq.toList

            Expect.equal actual expected "Should have a sequence of valid tweets"

        testCase "sequenceResultM with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    ""
                    "Hello"
                    aLongerInvalidTweet
                }

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

            let mutable lastValue = null
            let mutable callCount = 0

            let tweets =
                seq {
                    ""
                    "Hello"
                    aLongerInvalidTweet
                }

            let tryCreate tweet =
                callCount <-
                    callCount
                    + 1

                match tweet with
                | x when String.IsNullOrEmpty x -> Error "Tweet shouldn't be empty"
                | x when x.Length > 280 -> Error "Tweet shouldn't contain more than 280 characters"
                | x -> Ok(x)

            let actual = Seq.sequenceResultM (Seq.map tryCreate tweets)

            Expect.equal callCount 1 "Should have called the function only 1 time"
            Expect.equal lastValue null ""

            Expect.equal
                actual
                (Error emptyTweetErrMsg)
                "traverse the sequence and return the first error"
    ]

let sequenceOptionMTests =
    testList "Seq.sequenceOptionM Tests" [
        let tryTweetOption x =
            match x with
            | x when String.IsNullOrEmpty x -> None
            | _ -> Some x

        testCase "traverseOption with a sequence of valid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected = Error longerTweetErrMsg

            let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

            Expect.equal actual expected "traverse the sequence and return the first error"

            Expect.equal actual expected "Should have a sequence of valid tweets"

        testCase "sequenceOptionM with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    String.Empty
                    "Hello"
                    String.Empty
                }

            let actual = Seq.sequenceOptionM (Seq.map tryTweetOption tweets)

            Expect.equal actual None "traverse the sequence and return none"

        testCase "sequenceOptionM with few invalid data should exit early"
        <| fun _ ->

            let mutable lastValue = null
            let mutable callCount = 0

            let tweets =
                seq {
                    ""
                    "Hello"
                    aLongerInvalidTweet
                }

            let tryCreate tweet =
                callCount <-
                    callCount
                    + 1

                match tweet with
                | x when String.IsNullOrEmpty x -> None
                | x -> Some x

            let actual = Seq.sequenceOptionM (Seq.map tryCreate tweets)

            Expect.equal callCount 1 "Should have called the function only 1 time"
            Expect.equal lastValue null ""

            Expect.equal actual None "traverse the sequence and return none"
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
