module ArrayTests


#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open SampleDomain
open TestData
open TestHelpers
open System
open FsToolkit.ErrorHandling


let traverseResultMTests =
    testList "Array.traverseResultM Tests" [
        testCase "traverseResult with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected =
                Array.map tweet tweets
                |> Ok

            let actual = Array.traverseResultM Tweet.TryCreate tweets

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "traverseResultM with few invalid data"
        <| fun _ ->
            let tweets = [|
                ""
                "Hello"
                aLongerInvalidTweet
            |]

            let actual = Array.traverseResultM Tweet.TryCreate tweets

            Expect.equal
                actual
                (Error emptyTweetErrMsg)
                "traverse the array and return the first error"
    ]

let traverseOptionMTests =
    testList "Array.traverseOptionM Tests" [
        let tryTweetOption x =
            match x with
            | x when String.IsNullOrEmpty x -> None
            | _ -> Some x

        testCase "traverseOption with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected = Some tweets
            let actual = Array.traverseOptionM tryTweetOption tweets

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "traverseOption with few invalid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                String.Empty
            |]

            let expected = None
            let actual = Array.traverseOptionM tryTweetOption tweets

            Expect.equal actual expected "traverse the array and return none"
    ]

let sequenceResultMTests =
    testList "Array.sequenceResultM Tests" [
        testCase "traverseResult with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected =
                Array.map tweet tweets
                |> Ok

            let actual = Array.sequenceResultM (Array.map Tweet.TryCreate tweets)

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "sequenceResultM with few invalid data"
        <| fun _ ->
            let tweets = [|
                ""
                "Hello"
                aLongerInvalidTweet
            |]

            let actual = Array.sequenceResultM (Array.map Tweet.TryCreate tweets)

            Expect.equal
                actual
                (Error emptyTweetErrMsg)
                "traverse the array and return the first error"
    ]

let sequenceOptionMTests =
    testList "Array.sequenceOptionM Tests" [
        let tryTweetOption x =
            match x with
            | x when String.IsNullOrEmpty x -> None
            | _ -> Some x

        testCase "traverseOption with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected = Some tweets
            let actual = Array.sequenceOptionM (Array.map tryTweetOption tweets)

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "sequenceOptionM with few invalid data"
        <| fun _ ->
            let tweets = [|
                String.Empty
                "Hello"
                String.Empty
            |]

            let actual = Array.sequenceOptionM (Array.map tryTweetOption tweets)

            Expect.equal actual None "traverse the array and return none"
    ]

let traverseResultATests =
    testList "Array.traverseResultA Tests" [
        testCase "traverseResultA with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected =
                Array.map tweet tweets
                |> Ok

            let actual = Array.traverseResultA Tweet.TryCreate tweets

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "traverseResultA with few invalid data"
        <| fun _ ->
            let tweets = [|
                ""
                "Hello"
                aLongerInvalidTweet
            |]

            let actual = Array.traverseResultA Tweet.TryCreate tweets

            Expect.equal
                actual
                (Error [|
                    emptyTweetErrMsg
                    longerTweetErrMsg
                |])
                "traverse the array and return all the errors"
    ]


let sequenceResultATests =
    testList "Array.sequenceResultA Tests" [
        testCase "traverseResult with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected =
                Array.map tweet tweets
                |> Ok

            let actual = Array.sequenceResultA (Array.map Tweet.TryCreate tweets)

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "sequenceResultM with few invalid data"
        <| fun _ ->
            let tweets = [|
                ""
                "Hello"
                aLongerInvalidTweet
            |]

            let actual = Array.sequenceResultA (Array.map Tweet.TryCreate tweets)

            Expect.equal
                actual
                (Error [|
                    emptyTweetErrMsg
                    longerTweetErrMsg
                |])
                "traverse the array and return all the errors"
    ]


let traverseValidationATests =
    testList "Array.traverseValidationA Tests" [
        testCase "traverseValidationA with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected =
                Array.map tweet tweets
                |> Ok

            let actual =
                Array.traverseValidationA
                    (Tweet.TryCreate
                     >> (Result.mapError Array.singleton))
                    tweets

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "traverseValidationA with few invalid data"
        <| fun _ ->
            let tweets = [|
                ""
                "Hello"
                aLongerInvalidTweet
            |]

            let actual =
                Array.traverseValidationA
                    (Tweet.TryCreate
                     >> (Result.mapError Array.singleton))
                    tweets

            Expect.equal
                actual
                (Error [|
                    emptyTweetErrMsg
                    longerTweetErrMsg
                |])
                "traverse the array and return all the errors"
    ]


let sequenceValidationATests =
    let tryCreateTweet =
        Tweet.TryCreate
        >> (Result.mapError Array.singleton)

    testList "Array.sequenceValidationA Tests" [
        testCase "traverseValidation with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected =
                Array.map tweet tweets
                |> Ok

            let actual = Array.sequenceValidationA (Array.map tryCreateTweet tweets)

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "sequenceValidationM with few invalid data"
        <| fun _ ->
            let tweets = [|
                ""
                "Hello"
                aLongerInvalidTweet
            |]

            let actual = Array.sequenceValidationA (Array.map tryCreateTweet tweets)

            Expect.equal
                actual
                (Error [|
                    emptyTweetErrMsg
                    longerTweetErrMsg
                |])
                "traverse the array and return all the errors"
    ]

let userId1 = Guid.NewGuid()
let userId2 = Guid.NewGuid()
let userId3 = Guid.NewGuid()
let userId4 = Guid.NewGuid()


let traverseAsyncResultMTests =

    let userIds =
        Array.map UserId [|
            userId1
            userId2
            userId3
        |]

    testList "Array.traverseAsyncResultM Tests" [
        testCaseAsync "traverseAsyncResultM with an array of valid data"
        <| async {
            let expected =
                userIds
                |> Array.map (fun (UserId user) -> (newPostId, user))

            let actual =
                Array.traverseAsyncResultM (notifyNewPostSuccess (PostId newPostId)) userIds

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "traverseResultA with few invalid data"
        <| async {
            let expected = sprintf "error: %s" (userId1.ToString())

            let actual =
                Array.traverseAsyncResultM (notifyNewPostFailure (PostId newPostId)) userIds

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

let traverseAsyncOptionMTests =

    let userIds = [|
        userId1
        userId2
        userId3
    |]

    testList "Array.traverseAsyncOptionM Tests" [
        testCaseAsync "traverseAsyncOptionM with an array of valid data"
        <| async {
            let expected = Some userIds
            let f x = async { return Some x }
            let actual = Array.traverseAsyncOptionM f userIds

            match expected with
            | Some e -> do! Expect.hasAsyncSomeValue e actual
            | None -> failwith "Error in the test case code"
        }

        testCaseAsync "traverseOptionA with few invalid data"
        <| async {
            let expected = None
            let f _ = async { return None }
            let actual = Array.traverseAsyncOptionM f userIds

            match expected with
            | Some _ -> failwith "Error in the test case code"
            | None -> do! Expect.hasAsyncNoneValue actual
        }
    ]

let notifyFailure (PostId _) (UserId uId) =
    async {
        if
            (uId = userId1
             || uId = userId3)
        then
            return
                sprintf "error: %s" (uId.ToString())
                |> Error
        else
            return Ok()
    }


let traverseAsyncResultATests =
    let userIds =
        Array.map UserId [|
            userId1
            userId2
            userId3
            userId4
        |]

    testList "Array.traverseAsyncResultA Tests" [
        testCaseAsync "traverseAsyncResultA with an array of valid data"
        <| async {
            let expected =
                userIds
                |> Array.map (fun (UserId user) -> (newPostId, user))

            let actual =
                Array.traverseAsyncResultA (notifyNewPostSuccess (PostId newPostId)) userIds

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "traverseResultA with few invalid data"
        <| async {
            let expected = [|
                sprintf "error: %s" (userId1.ToString())
                sprintf "error: %s" (userId3.ToString())
            |]

            let actual = Array.traverseAsyncResultA (notifyFailure (PostId newPostId)) userIds

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]


let sequenceAsyncResultMTests =
    let userIds =
        Array.map UserId [|
            userId1
            userId2
            userId3
            userId4
        |]

    testList "Array.sequenceAsyncResultM Tests" [
        testCaseAsync "sequenceAsyncResultM with an array of valid data"
        <| async {
            let expected =
                userIds
                |> Array.map (fun (UserId user) -> (newPostId, user))

            let actual =
                Array.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> Array.sequenceAsyncResultM

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "sequenceAsyncResultM with few invalid data"
        <| async {
            let expected = sprintf "error: %s" (userId1.ToString())

            let actual =
                Array.map (notifyFailure (PostId newPostId)) userIds
                |> Array.sequenceAsyncResultM

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

let sequenceAsyncOptionMTests =

    let userIds = [|
        userId1
        userId2
        userId3
    |]

    testList "Array.sequenceAsyncOptionM Tests" [
        testCaseAsync "sequenceAsyncOptionM with an array of valid data"
        <| async {
            let expected = Some userIds
            let f x = async { return Some x }

            let actual =
                Array.map f userIds
                |> Array.sequenceAsyncOptionM

            match expected with
            | Some e -> do! Expect.hasAsyncSomeValue e actual
            | None -> failwith "Error in the test case code"
        }

        testCaseAsync "sequenceOptionA with few invalid data"
        <| async {
            let expected = None
            let f _ = async { return None }

            let actual =
                Array.map f userIds
                |> Array.sequenceAsyncOptionM

            match expected with
            | Some _ -> failwith "Error in the test case code"
            | None -> do! Expect.hasAsyncNoneValue actual
        }
    ]

let sequenceAsyncResultATests =
    let userIds =
        Array.map UserId [|
            userId1
            userId2
            userId3
            userId4
        |]

    testList "Array.sequenceAsyncResultA Tests" [
        testCaseAsync "sequenceAsyncResultA with an array of valid data"
        <| async {
            let expected =
                userIds
                |> Array.map (fun (UserId user) -> (newPostId, user))

            let actual =
                Array.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> Array.sequenceAsyncResultA

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "sequenceAsyncResultA with few invalid data"
        <| async {
            let expected = [|
                sprintf "error: %s" (userId1.ToString())
                sprintf "error: %s" (userId3.ToString())
            |]

            let actual =
                Array.map (notifyFailure (PostId newPostId)) userIds
                |> Array.sequenceAsyncResultA

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

#if !FABLE_COMPILER
let traverseVOptionMTests =
    testList "Array.traverseVOptionM Tests" [
        let tryTweetVOption x =
            match x with
            | x when String.IsNullOrEmpty x -> ValueNone
            | _ -> ValueSome x

        testCase "traverseVOption with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected = ValueSome tweets
            let actual = Array.traverseVOptionM tryTweetVOption tweets

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "traverseVOption with few invalid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                String.Empty
            |]

            let actual = Array.traverseVOptionM tryTweetVOption tweets
            Expect.equal actual ValueNone "traverse the array and return value none"
    ]

let sequenceVOptionMTests =
    testList "Array.sequenceVOptionM Tests" [
        let tryTweetOption x =
            match x with
            | x when String.IsNullOrEmpty x -> ValueNone
            | _ -> ValueSome x

        testCase "traverseVOption with an array of valid data"
        <| fun _ ->
            let tweets = [|
                "Hi"
                "Hello"
                "Hola"
            |]

            let expected = ValueSome tweets
            let actual = Array.sequenceVOptionM (Array.map tryTweetOption tweets)

            Expect.equal actual expected "Should have an array of valid tweets"

        testCase "sequenceVOptionM with few invalid data"
        <| fun _ ->
            let tweets = [|
                String.Empty
                "Hello"
                String.Empty
            |]

            let actual = Array.sequenceVOptionM (Array.map tryTweetOption tweets)
            Expect.equal actual ValueNone "traverse the array and return value none"
    ]

#endif

let allTests =
    testList "Array Tests" [
        traverseResultMTests
        traverseOptionMTests
        sequenceResultMTests
        sequenceOptionMTests
        traverseResultATests
        sequenceResultATests
        traverseValidationATests
        sequenceValidationATests
        traverseAsyncResultMTests
        traverseAsyncOptionMTests
        traverseAsyncResultATests
        sequenceAsyncResultMTests
        sequenceAsyncOptionMTests
        sequenceAsyncResultATests
#if !FABLE_COMPILER
        traverseVOptionMTests
        sequenceVOptionMTests
#endif
    ]
