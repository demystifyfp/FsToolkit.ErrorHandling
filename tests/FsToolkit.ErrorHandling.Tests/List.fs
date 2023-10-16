module ListTests


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


let traverseResultMTests =
    testList "List.traverseResultM Tests" [
        testCase "traverseResult with a list of valid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                "Hola"
            ]

            let expected =
                List.map tweet tweets
                |> Ok

            let actual = List.traverseResultM Tweet.TryCreate tweets

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "traverseResultM with few invalid data"
        <| fun _ ->
            let tweets = [
                ""
                "Hello"
                aLongerInvalidTweet
            ]

            let actual = List.traverseResultM Tweet.TryCreate tweets

            Expect.equal
                actual
                (Error emptyTweetErrMsg)
                "traverse the list and return the first error"
    ]

let traverseOptionMTests =
    testList "List.traverseOptionM Tests" [
        let tryTweetOption x =
            match x with
            | x when String.IsNullOrEmpty x -> None
            | _ -> Some x

        testCase "traverseOption with a list of valid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                "Hola"
            ]

            let expected = Some tweets
            let actual = List.traverseOptionM tryTweetOption tweets

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "traverseOption with few invalid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                String.Empty
            ]

            let expected = None
            let actual = List.traverseOptionM tryTweetOption tweets

            Expect.equal actual expected "traverse the list and return none"
    ]

let sequenceResultMTests =
    testList "List.sequenceResultM Tests" [
        testCase "traverseResult with a list of valid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                "Hola"
            ]

            let expected =
                List.map tweet tweets
                |> Ok

            let actual = List.sequenceResultM (List.map Tweet.TryCreate tweets)

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "sequenceResultM with few invalid data"
        <| fun _ ->
            let tweets = [
                ""
                "Hello"
                aLongerInvalidTweet
            ]

            let actual = List.sequenceResultM (List.map Tweet.TryCreate tweets)

            Expect.equal
                actual
                (Error emptyTweetErrMsg)
                "traverse the list and return the first error"
    ]

let sequenceOptionMTests =
    testList "List.sequenceOptionM Tests" [
        let tryTweetOption x =
            match x with
            | x when String.IsNullOrEmpty x -> None
            | _ -> Some x

        testCase "traverseOption with a list of valid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                "Hola"
            ]

            let expected = Some tweets
            let actual = List.sequenceOptionM (List.map tryTweetOption tweets)

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "sequenceOptionM with few invalid data"
        <| fun _ ->
            let tweets = [
                String.Empty
                "Hello"
                String.Empty
            ]

            let actual = List.sequenceOptionM (List.map tryTweetOption tweets)

            Expect.equal actual None "traverse the list and return none"
    ]

let traverseResultATests =
    testList "List.traverseResultA Tests" [
        testCase "traverseResultA with a list of valid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                "Hola"
            ]

            let expected =
                List.map tweet tweets
                |> Ok

            let actual = List.traverseResultA Tweet.TryCreate tweets

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "traverseResultA with few invalid data"
        <| fun _ ->
            let tweets = [
                ""
                "Hello"
                aLongerInvalidTweet
            ]

            let actual = List.traverseResultA Tweet.TryCreate tweets

            Expect.equal
                actual
                (Error [
                    emptyTweetErrMsg
                    longerTweetErrMsg
                ])
                "traverse the list and return all the errors"
    ]


let sequenceResultATests =
    testList "List.sequenceResultA Tests" [
        testCase "traverseResult with a list of valid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                "Hola"
            ]

            let expected =
                List.map tweet tweets
                |> Ok

            let actual = List.sequenceResultA (List.map Tweet.TryCreate tweets)

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "sequenceResultM with few invalid data"
        <| fun _ ->
            let tweets = [
                ""
                "Hello"
                aLongerInvalidTweet
            ]

            let actual = List.sequenceResultA (List.map Tweet.TryCreate tweets)

            Expect.equal
                actual
                (Error [
                    emptyTweetErrMsg
                    longerTweetErrMsg
                ])
                "traverse the list and return all the errors"
    ]


let traverseValidationATests =
    testList "List.traverseValidationA Tests" [
        testCase "traverseValidationA with a list of valid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                "Hola"
            ]

            let expected =
                List.map tweet tweets
                |> Ok

            let actual =
                List.traverseValidationA
                    (Tweet.TryCreate
                     >> (Result.mapError List.singleton))
                    tweets

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "traverseValidationA with few invalid data"
        <| fun _ ->
            let tweets = [
                ""
                "Hello"
                aLongerInvalidTweet
            ]

            let actual =
                List.traverseValidationA
                    (Tweet.TryCreate
                     >> (Result.mapError List.singleton))
                    tweets

            Expect.equal
                actual
                (Error [
                    emptyTweetErrMsg
                    longerTweetErrMsg
                ])
                "traverse the list and return all the errors"
    ]


let sequenceValidationATests =
    let tryCreateTweet =
        Tweet.TryCreate
        >> (Result.mapError List.singleton)

    testList "List.sequenceValidationA Tests" [
        testCase "traverseValidation with a list of valid data"
        <| fun _ ->
            let tweets = [
                "Hi"
                "Hello"
                "Hola"
            ]

            let expected =
                List.map tweet tweets
                |> Ok

            let actual = List.sequenceValidationA (List.map tryCreateTweet tweets)

            Expect.equal actual expected "Should have a list of valid tweets"

        testCase "sequenceValidationM with few invalid data"
        <| fun _ ->
            let tweets = [
                ""
                "Hello"
                aLongerInvalidTweet
            ]

            let actual = List.sequenceValidationA (List.map tryCreateTweet tweets)

            Expect.equal
                actual
                (Error [
                    emptyTweetErrMsg
                    longerTweetErrMsg
                ])
                "traverse the list and return all the errors"
    ]

let userId1 = Guid.NewGuid()
let userId2 = Guid.NewGuid()
let userId3 = Guid.NewGuid()
let userId4 = Guid.NewGuid()


let traverseAsyncResultMTests =

    let userIds =
        List.map UserId [
            userId1
            userId2
            userId3
        ]

    testList "List.traverseAsyncResultM Tests" [
        testCaseAsync "traverseAsyncResultM with a list of valid data"
        <| async {
            let expected =
                userIds
                |> List.map (fun (UserId user) -> (newPostId, user))

            let actual =
                List.traverseAsyncResultM (notifyNewPostSuccess (PostId newPostId)) userIds

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "traverseResultA with few invalid data"
        <| async {
            let expected = sprintf "error: %s" (userId1.ToString())

            let actual =
                List.traverseAsyncResultM (notifyNewPostFailure (PostId newPostId)) userIds

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

let traverseAsyncOptionMTests =

    let userIds = [
        userId1
        userId2
        userId3
    ]

    testList "List.traverseAsyncOptionM Tests" [
        testCaseAsync "traverseAsyncOptionM with a list of valid data"
        <| async {
            let expected = Some userIds
            let f x = async { return Some x }
            let actual = List.traverseAsyncOptionM f userIds

            match expected with
            | Some e -> do! Expect.hasAsyncSomeValue e actual
            | None -> failwith "Error in the test case code"
        }

        testCaseAsync "traverseOptionA with few invalid data"
        <| async {
            let expected = None
            let f _ = async { return None }
            let actual = List.traverseAsyncOptionM f userIds

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
        List.map UserId [
            userId1
            userId2
            userId3
            userId4
        ]

    testList "List.traverseAsyncResultA Tests" [
        testCaseAsync "traverseAsyncResultA with a list of valid data"
        <| async {
            let expected =
                userIds
                |> List.map (fun (UserId user) -> (newPostId, user))

            let actual =
                List.traverseAsyncResultA (notifyNewPostSuccess (PostId newPostId)) userIds

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "traverseResultA with few invalid data"
        <| async {
            let expected = [
                sprintf "error: %s" (userId1.ToString())
                sprintf "error: %s" (userId3.ToString())
            ]

            let actual = List.traverseAsyncResultA (notifyFailure (PostId newPostId)) userIds

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]


let sequenceAsyncResultMTests =
    let userIds =
        List.map UserId [
            userId1
            userId2
            userId3
            userId4
        ]

    testList "List.sequenceAsyncResultM Tests" [
        testCaseAsync "sequenceAsyncResultM with a list of valid data"
        <| async {
            let expected =
                userIds
                |> List.map (fun (UserId user) -> (newPostId, user))

            let actual =
                List.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> List.sequenceAsyncResultM

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "sequenceAsyncResultM with few invalid data"
        <| async {
            let expected = sprintf "error: %s" (userId1.ToString())

            let actual =
                List.map (notifyFailure (PostId newPostId)) userIds
                |> List.sequenceAsyncResultM

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

let sequenceAsyncOptionMTests =

    let userIds = [
        userId1
        userId2
        userId3
    ]

    testList "List.sequenceAsyncOptionM Tests" [
        testCaseAsync "sequenceAsyncOptionM with a list of valid data"
        <| async {
            let expected = Some userIds
            let f x = async { return Some x }

            let actual =
                List.map f userIds
                |> List.sequenceAsyncOptionM

            match expected with
            | Some e -> do! Expect.hasAsyncSomeValue e actual
            | None -> failwith "Error in the test case code"
        }

        testCaseAsync "sequenceOptionA with few invalid data"
        <| async {
            let expected = None
            let f _ = async { return None }

            let actual =
                List.map f userIds
                |> List.sequenceAsyncOptionM

            match expected with
            | Some _ -> failwith "Error in the test case code"
            | None -> do! Expect.hasAsyncNoneValue actual
        }
    ]

let sequenceAsyncResultATests =
    let userIds =
        List.map UserId [
            userId1
            userId2
            userId3
            userId4
        ]

    testList "List.sequenceAsyncResultA Tests" [
        testCaseAsync "sequenceAsyncResultA with a list of valid data"
        <| async {
            let expected =
                userIds
                |> List.map (fun (UserId user) -> (newPostId, user))

            let actual =
                List.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> List.sequenceAsyncResultA

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "sequenceAsyncResultA with few invalid data"
        <| async {
            let expected = [
                sprintf "error: %s" (userId1.ToString())
                sprintf "error: %s" (userId3.ToString())
            ]

            let actual =
                List.map (notifyFailure (PostId newPostId)) userIds
                |> List.sequenceAsyncResultA

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

let allTests =
    testList "List Tests" [
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
    ]
