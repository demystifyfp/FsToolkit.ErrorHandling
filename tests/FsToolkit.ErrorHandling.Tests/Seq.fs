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
                |> Seq.toArray

            let actual = Seq.traverseResultM Tweet.TryCreate tweets

            let actual = Expect.wantOk actual "Expected result to be Ok"

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

            let expected = Seq.toArray tweets
            let actual = Seq.traverseOptionM tryTweetOption tweets

            let actual = Expect.wantSome actual "Expected result to be Some"
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
                |> Seq.toArray

            let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)

            let actual = Expect.wantOk actual "Expected result to be Ok"

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

            Expect.equal
                actual
                (Error emptyTweetErrMsg)
                "traverse the sequence and return the first error"

        testCase "sequenceResultM with few invalid data should exit early"
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

            let expected = Seq.toArray tweets
            let actual = Seq.sequenceOptionM (Seq.map tryTweetOption tweets)

            let actual = Expect.wantSome actual "Expected result to be Some"

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

let traverseResultATests =
    testList "Seq.traverseResultA Tests" [
        testCase "traverseResultA with a sequence of valid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected =
                Seq.map tweet tweets
                |> Seq.toArray

            let actual = Seq.traverseResultA Tweet.TryCreate tweets

            let actual = Expect.wantOk actual "Expected result to be Ok"

            Expect.equal actual expected "Should have a sequence of valid tweets"

        testCase "traverseResultA with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    ""
                    "Hello"
                    aLongerInvalidTweet
                }

            let actual = Seq.traverseResultA Tweet.TryCreate tweets

            let actual = Expect.wantError actual "Expected result to be Error"

            let expected = [|
                emptyTweetErrMsg
                longerTweetErrMsg
            |]

            Expect.equal actual expected "traverse the sequence and return all the errors"
    ]

let sequenceResultATests =
    testList "Seq.sequenceResultA Tests" [
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
                |> Seq.toArray

            let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)

            let actual = Expect.wantOk actual "Expected result to be Ok"

            Expect.equal actual expected "Should have a sequence of valid tweets"

        testCase "sequenceResultM with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    ""
                    "Hello"
                    aLongerInvalidTweet
                }

            let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)

            let actual = Expect.wantError actual "Expected result to be Error"

            let expected = [|
                emptyTweetErrMsg
                longerTweetErrMsg
            |]

            Expect.equal actual expected "traverse the sequence and return all the errors"
    ]

let userId1 = Guid.NewGuid()
let userId2 = Guid.NewGuid()
let userId3 = Guid.NewGuid()
let userId4 = Guid.NewGuid()

let traverseAsyncResultMTests =

    let userIds =
        seq {
            userId1
            userId2
            userId3
        }
        |> Seq.map UserId

    testList "Seq.traverseAsyncResultM Tests" [
        testCaseAsync "traverseAsyncResultM with a sequence of valid data"
        <| async {
            let expected =
                userIds
                |> Seq.map (fun (UserId user) -> (newPostId, user))
                |> Seq.toArray

            let! actual = Seq.traverseAsyncResultM (notifyNewPostSuccess (PostId newPostId)) userIds

            let actual = Expect.wantOk actual "Expected result to be Ok"

            Expect.equal actual expected "Should have a sequence of valid data"
        }

        testCaseAsync "traverseResultA with few invalid data"
        <| async {
            let expected = sprintf "error: %s" (userId1.ToString())

            let actual =
                Seq.traverseAsyncResultM (notifyNewPostFailure (PostId newPostId)) userIds

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

let traverseTaskResultMTests =

    let notifyNewPostSuccess (PostId post) (UserId user) = TaskResult.ok (post, user)

    let notifyNewPostFailure (PostId _) (UserId uId) = TaskResult.error $"error: %O{uId}"

    let userIds =
        seq {
            userId1
            userId2
            userId3
        }
        |> Seq.map UserId

    testList "Seq.traverseTaskResultM Tests" [
        testCaseAsync "traverseTaskResultM with a sequence of valid data"
        <| async {
            let expected =
                userIds
                |> Seq.map (fun (UserId user) -> (newPostId, user))
                |> Seq.toArray

            let! actual =
                Seq.traverseTaskResultM (notifyNewPostSuccess (PostId newPostId)) userIds
                |> Async.AwaitTask

            let actual = Expect.wantOk actual "Expected result to be Ok"

            Expect.equal actual expected "Should have a sequence of valid data"
        }

        testCaseAsync "traverseResultA with few invalid data"
        <| async {
            let expected = $"error: %O{userId1}"

            let actual =
                Seq.traverseTaskResultM (notifyNewPostFailure (PostId newPostId)) userIds

            do!
                Expect.hasTaskErrorValue expected actual
                |> Async.AwaitTask
        }
    ]

let traverseAsyncOptionMTests =

    let userIds =
        seq {
            userId1
            userId2
            userId3
        }

    testList "Seq.traverseAsyncOptionM Tests" [
        testCaseAsync "traverseAsyncOptionM with a sequence of valid data"
        <| async {
            let expected =
                userIds
                |> Seq.toArray
                |> Some

            let f x = async { return Some x }

            let actual = Seq.traverseAsyncOptionM f userIds

            match expected with
            | Some e -> do! Expect.hasAsyncSomeValue e actual
            | None -> failwith "Error in the test case code"
        }

        testCaseAsync "traverseOptionA with few invalid data"
        <| async {
            let expected = None
            let f _ = async { return None }
            let actual = Seq.traverseAsyncOptionM f userIds

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
        seq {
            userId1
            userId2
            userId3
            userId4
        }
        |> Seq.map UserId

    testList "Seq.traverseAsyncResultA Tests" [
        testCaseAsync "traverseAsyncResultA with a sequence of valid data"
        <| async {
            let expected =
                userIds
                |> Seq.map (fun (UserId user) -> (newPostId, user))
                |> Seq.toArray

            let! actual = Seq.traverseAsyncResultA (notifyNewPostSuccess (PostId newPostId)) userIds

            let actual = Expect.wantOk actual "Expected result to be Ok"

            Expect.equal actual expected "Should have a sequence of valid data"
        }

        testCaseAsync "traverseResultA with few invalid data"
        <| async {
            let expected = [|
                sprintf "error: %s" (userId1.ToString())
                sprintf "error: %s" (userId3.ToString())
            |]

            let! actual = Seq.traverseAsyncResultA (notifyFailure (PostId newPostId)) userIds

            let actual = Expect.wantError actual "Expected result to be Error"

            Expect.equal actual expected "Should have a sequence of errors"
        }
    ]

let sequenceAsyncResultMTests =
    let userIds =
        seq {
            userId1
            userId2
            userId3
            userId4
        }
        |> Seq.map UserId

    testList "Seq.sequenceAsyncResultM Tests" [
        testCaseAsync "sequenceAsyncResultM with a sequence of valid data"
        <| async {
            let expected =
                userIds
                |> Seq.map (fun (UserId user) -> (newPostId, user))
                |> Seq.toArray

            let! actual =
                Seq.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> Seq.sequenceAsyncResultM

            let actual = Expect.wantOk actual "Expected result to be Ok"

            Expect.equal actual expected "Should have a sequence of valid data"
        }

        testCaseAsync "sequenceAsyncResultM with few invalid data"
        <| async {
            let expected = sprintf "error: %s" (userId1.ToString())

            let actual =
                Seq.map (notifyFailure (PostId newPostId)) userIds
                |> Seq.sequenceAsyncResultM

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

let sequenceTaskResultMTests =
    let notifyNewPostSuccess (PostId post) (UserId user) = TaskResult.ok (post, user)

    let notifyFailure (PostId _) (UserId uId) =
        if
            uId = userId1
            || uId = userId3
        then
            TaskResult.error $"error: %O{uId}"
        else
            TaskResult.ok ()

    let userIds =
        seq {
            userId1
            userId2
            userId3
            userId4
        }
        |> Seq.map UserId

    testList "Seq.sequenceTaskResultM Tests" [
        testCaseAsync "sequenceTaskResultM with a sequence of valid data"
        <| async {
            let expected =
                userIds
                |> Seq.map (fun (UserId user) -> (newPostId, user))
                |> Seq.toArray

            let! actual =
                Seq.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> Seq.sequenceTaskResultM
                |> Async.AwaitTask

            let actual = Expect.wantOk actual "Expected result to be Ok"

            Expect.equal actual expected "Should have a sequence of valid data"
        }

        testCaseAsync "sequenceTaskResultM with few invalid data"
        <| async {
            let expected = sprintf "error: %s" (userId1.ToString())

            let actual =
                userIds
                |> Seq.map (notifyFailure (PostId newPostId))
                |> Seq.sequenceTaskResultM
                |> Async.AwaitTask

            do! Expect.hasAsyncErrorValue expected actual
        }
    ]

let sequenceAsyncOptionMTests =

    let userIds =
        seq {
            userId1
            userId2
            userId3
        }

    testList "Seq.sequenceAsyncOptionM Tests" [
        testCaseAsync "sequenceAsyncOptionM with a sequence of valid data"
        <| async {
            let expected =
                Seq.toArray userIds
                |> Some

            let f x = async { return Some x }

            let actual =
                Seq.map f userIds
                |> Seq.sequenceAsyncOptionM

            match expected with
            | Some e -> do! Expect.hasAsyncSomeValue e actual
            | None -> failwith "Error in the test case code"
        }

        testCaseAsync "sequenceOptionA with few invalid data"
        <| async {
            let expected = None
            let f _ = async { return None }

            let actual =
                Seq.map f userIds
                |> Seq.sequenceAsyncOptionM

            match expected with
            | Some _ -> failwith "Error in the test case code"
            | None -> do! Expect.hasAsyncNoneValue actual
        }
    ]

let sequenceAsyncResultATests =
    let userIds =
        seq {
            userId1
            userId2
            userId3
            userId4
        }
        |> Seq.map UserId

    testList "Seq.sequenceAsyncResultA Tests" [
        testCaseAsync "sequenceAsyncResultA with a sequence of valid data"
        <| async {
            let expected =
                userIds
                |> Seq.map (fun (UserId user) -> (newPostId, user))
                |> Seq.toArray

            let actual =
                Seq.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> Seq.sequenceAsyncResultA

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "sequenceAsyncResultA with few invalid data"
        <| async {
            let expected = [|
                sprintf "error: %s" (userId1.ToString())
                sprintf "error: %s" (userId3.ToString())
            |]

            let! actual =
                Seq.map (notifyFailure (PostId newPostId)) userIds
                |> Seq.sequenceAsyncResultA

            let actual = Expect.wantError actual "Expected result to be Error"
            Expect.equal actual expected "Should have a sequence of errors"
        }
    ]

#if !FABLE_COMPILER
let traverseVOptionMTests =
    testList "Seq.traverseVOptionM Tests" [
        let tryTweetVOption x =
            match x with
            | x when String.IsNullOrEmpty x -> ValueNone
            | _ -> ValueSome x

        testCase "traverseVOption with a sequence of valid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected = Seq.toArray tweets

            let actual = Seq.traverseVOptionM tryTweetVOption tweets

            match actual with
            | ValueSome actual ->
                Expect.equal actual expected "Should have a sequence of valid tweets"
            | ValueNone -> failwith "Expected a value some"

        testCase "traverseVOption with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    String.Empty
                }

            let actual = Seq.traverseVOptionM tryTweetVOption tweets
            Expect.equal actual ValueNone "traverse the sequence and return value none"
    ]

let sequenceVOptionMTests =
    testList "Seq.sequenceVOptionM Tests" [
        let tryTweetOption x =
            match x with
            | x when String.IsNullOrEmpty x -> ValueNone
            | _ -> ValueSome x

        testCase "traverseVOption with a sequence of valid data"
        <| fun _ ->
            let tweets =
                seq {
                    "Hi"
                    "Hello"
                    "Hola"
                }

            let expected = Seq.toArray tweets

            let actual = Seq.sequenceVOptionM (Seq.map tryTweetOption tweets)

            match actual with
            | ValueSome actual ->
                Expect.equal actual expected "Should have a sequence of valid tweets"
            | ValueNone -> failwith "Expected a value some"

        testCase "sequenceVOptionM with few invalid data"
        <| fun _ ->
            let tweets =
                seq {
                    String.Empty
                    "Hello"
                    String.Empty
                }

            let actual = Seq.sequenceVOptionM (Seq.map tryTweetOption tweets)
            Expect.equal actual ValueNone "traverse the sequence and return value none"

        testCase "sequenceVOptionM with few invalid data should exit early"
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
                | x when String.IsNullOrEmpty x -> ValueNone
                | x -> ValueSome x

            let actual = Seq.sequenceVOptionM (Seq.map tryCreate tweets)

            match actual with
            | ValueNone -> ()
            | ValueSome _ -> failwith "Expected a value none"

            Expect.equal callCount 1 "Should have called the function only 1 time"
            Expect.equal lastValue null ""
    ]

#endif

let allTests =
    testList "List Tests" [
        traverseResultMTests
        traverseOptionMTests
        sequenceResultMTests
        sequenceOptionMTests
        traverseResultATests
        sequenceResultATests
        traverseAsyncResultMTests
        traverseTaskResultMTests
        traverseAsyncOptionMTests
        traverseAsyncResultATests
        sequenceAsyncResultMTests
        sequenceTaskResultMTests
        sequenceAsyncOptionMTests
        sequenceAsyncResultATests
#if !FABLE_COMPILER
        traverseVOptionMTests
        sequenceVOptionMTests
#endif
    ]
