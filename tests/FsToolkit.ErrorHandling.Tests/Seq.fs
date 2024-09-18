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

            let expected = Seq.toList tweets
            let actual = Seq.sequenceOptionM (Seq.map tryTweetOption tweets)

            let actual =
                Expect.wantSome actual "Expected result to be Some"
                |> Seq.toList

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
                |> Seq.toList

            let actual = Seq.traverseResultA Tweet.TryCreate tweets

            let actual =
                Expect.wantOk actual "Expected result to be Ok"
                |> Seq.toList

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

            let actual =
                Expect.wantError actual "Expected result to be Error"
                |> Seq.toList

            let expected = [
                emptyTweetErrMsg
                longerTweetErrMsg
            ]

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
                |> Seq.toList

            let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)

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

            let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)

            let actual =
                Expect.wantError actual "Expected result to be Error"
                |> Seq.toList

            let expected = [
                emptyTweetErrMsg
                longerTweetErrMsg
            ]

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
                |> Seq.toList

            let! actual = Seq.traverseAsyncResultM (notifyNewPostSuccess (PostId newPostId)) userIds

            let actual =
                Expect.wantOk actual "Expected result to be Ok"
                |> Seq.toList

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
                |> Seq.toList
                |> Some

            let f x = async { return Some x }

            let actual =
                Seq.traverseAsyncOptionM f userIds
                |> AsyncOption.map Seq.toList

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
                |> Seq.toList

            let! actual = Seq.traverseAsyncResultA (notifyNewPostSuccess (PostId newPostId)) userIds

            let actual =
                Expect.wantOk actual "Expected result to be Ok"
                |> Seq.toList

            Expect.equal actual expected "Should have a sequence of valid data"
        }

        testCaseAsync "traverseResultA with few invalid data"
        <| async {
            let expected = [
                sprintf "error: %s" (userId1.ToString())
                sprintf "error: %s" (userId3.ToString())
            ]

            let! actual = Seq.traverseAsyncResultA (notifyFailure (PostId newPostId)) userIds

            let actual =
                Expect.wantError actual "Expected result to be Error"
                |> Seq.toList

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
                |> Seq.toList

            let! actual =
                Seq.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> Seq.sequenceAsyncResultM

            let actual =
                Expect.wantOk actual "Expected result to be Ok"
                |> Seq.toList

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
                Seq.toList userIds
                |> Some

            let f x = async { return Some x }

            let actual =
                Seq.map f userIds
                |> Seq.sequenceAsyncOptionM
                |> AsyncOption.map Seq.toList

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
                |> Seq.toList

            let actual =
                Seq.map (notifyNewPostSuccess (PostId newPostId)) userIds
                |> Seq.sequenceAsyncResultA
                |> AsyncResult.map Seq.toList

            do! Expect.hasAsyncOkValue expected actual
        }

        testCaseAsync "sequenceAsyncResultA with few invalid data"
        <| async {
            let expected = [
                sprintf "error: %s" (userId1.ToString())
                sprintf "error: %s" (userId3.ToString())
            ]

            let! actual =
                Seq.map (notifyFailure (PostId newPostId)) userIds
                |> Seq.sequenceAsyncResultA
                |> AsyncResult.mapError Seq.toList

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

            let expected = Seq.toList tweets

            let actual =
                Seq.traverseVOptionM tryTweetVOption tweets
                |> ValueOption.map Seq.toList

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

            let expected = Seq.toList tweets

            let actual =
                Seq.sequenceVOptionM (Seq.map tryTweetOption tweets)
                |> ValueOption.map Seq.toList

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
