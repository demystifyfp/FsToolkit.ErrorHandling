module AsyncOptionTests


#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
#endif

open System
open TestData
open TestHelpers
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncOption

let mapTests =
    testList "AsyncOption.map Tests" [
        testCaseAsync "map with Async(Some x)"
        <| (Async.singleton (Some validTweet)
            |> AsyncOption.map remainingCharacters
            |> Expect.hasAsyncSomeValue 267)

        testCaseAsync "map with Async(None)"
        <| (Async.singleton (None)
            |> AsyncOption.map remainingCharacters
            |> Expect.hasAsyncNoneValue)
    ]

let bindTests =
    testList "AsyncOption.bind tests" [
        testCaseAsync "bind with Async(Some x)"
        <| (allowedToPostOptional sampleUserId
            |> AsyncOption.bind (fun isAllowed ->
                async {
                    if isAllowed then
                        return! createPostSome validCreatePostRequest
                    else
                        return None
                }
            )
            |> Expect.hasAsyncSomeValue (PostId newPostId))

        testCaseAsync "bind with Async(None)"
        <| (allowedToPostOptional (UserId(Guid.NewGuid()))
            |> AsyncOption.bind (fun isAllowed -> async { return Some isAllowed })
            |> Expect.hasAsyncNoneValue)

        testCaseAsync "bind with Async(Ok x) that returns Async (None)"
        <| (allowedToPostOptional sampleUserId
            |> AsyncOption.bind (fun _ -> async { return None })
            |> Expect.hasAsyncNoneValue)
    ]

let applyTests =
    testList "AsyncOption.apply Tests" [
        testCaseAsync "apply with Async(Some x)"
        <| (Async.singleton (Some validTweet)
            |> AsyncOption.apply (Async.singleton (Some remainingCharacters))
            |> Expect.hasAsyncSomeValue (267))

        testCaseAsync "apply with Async(None)"
        <| (Async.singleton None
            |> AsyncOption.apply (Async.singleton (Some remainingCharacters))
            |> Expect.hasAsyncNoneValue)
    ]

let retnTests =
    testList "AsyncOption.retn Tests" [
        testCaseAsync "retn with x"
        <| (AsyncOption.some 267
            |> Expect.hasAsyncSomeValue (267))
    ]

let asyncOptionOperatorTests =
    testList "AsyncOption Operators Tests" [
        testCaseAsync "map & apply operators"
        <| async {
            let getFollowersResult = getFollowersSome sampleUserId
            let createPostResult = createPostSome validCreatePostRequest

            do!
                newPostRequest
                <!> getFollowersResult
                <*> createPostResult
                |> Expect.hasAsyncSomeValue {
                    NewPostId = PostId newPostId
                    UserIds = followerIds
                }
        }

        testCaseAsync "bind operator"
        <| async {
            do!
                allowedToPostOptional sampleUserId
                >>= (fun isAllowed ->
                    if isAllowed then
                        createPostSome validCreatePostRequest
                    else
                        Async.singleton None
                )
                |> Expect.hasAsyncSomeValue (PostId newPostId)
        }
    ]

let eitherTests =
    testList "AsyncOption.either Tests" [
        testCaseAsync "Some"
        <| async {
            let value1 = AsyncOption.some 5
            let f = async.Return 42
            let add2 x = async { return x + 2 }
            let! result = (AsyncOption.either add2 f value1)
            Expect.equal result 7 ""
        }
        testCaseAsync "None"
        <| async {
            let value1 = async.Return None
            let f = async.Return 42
            let add2 x = async { return x + 2 }
            let! result = (AsyncOption.either add2 f value1)
            Expect.equal result 42 ""
        }
    ]

let defaultValueTests =
    testList "TaskOption.defaultValue Tests" [
        testCaseAsync "Some"
        <| async {
            let defaultValue = 10
            let expectedValue = 5

            let asyncOption = AsyncOption.some expectedValue
            let! result = AsyncOption.defaultValue defaultValue asyncOption
            Expect.equal result expectedValue ""
        }

        testCaseAsync "None"
        <| async {
            let expectedValue = 10
            let asyncOption = Async.singleton None
            let! result = AsyncOption.defaultValue expectedValue asyncOption
            Expect.equal result expectedValue ""
        }
    ]

let defaultWithTests =
    testList "AsyncOption.defaultWith Tests" [
        testCaseAsync "Some"
        <| async {
            let defaultValue = 10
            let expectedValue = 5

            let asyncOption = AsyncOption.some expectedValue
            let! result = AsyncOption.defaultWith (fun () -> defaultValue) asyncOption
            Expect.equal result expectedValue ""
        }

        testCaseAsync "None"
        <| async {
            let expectedValue = 10
            let asyncOption = Async.singleton None
            let! result = AsyncOption.defaultWith (fun () -> expectedValue) asyncOption
            Expect.equal result expectedValue ""
        }
    ]

let allTests =
    testList "Async Option Tests" [
        mapTests
        bindTests
        applyTests
        retnTests
        asyncOptionOperatorTests
        eitherTests
        defaultValueTests
        defaultWithTests
    ]
