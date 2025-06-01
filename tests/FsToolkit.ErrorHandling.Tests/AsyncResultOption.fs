module AsyncResultOptionTests
#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open SampleDomain
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResultOption
open System


let mapTests =
    testList "AsyncResultOption.map tests" [
        testCaseAsync "map with Async(Ok Some(x))"
        <| (getUserById sampleUserId
            |> AsyncResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasAsyncOkValue (Some "someone"))

        testCaseAsync "map with Async(Ok None)"
        <| (getUserById (UserId(Guid.NewGuid()))
            |> AsyncResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasAsyncOkValue None)

        testCaseAsync "map with Async(Error x)"
        <| (getUserById (UserId(Guid.Empty))
            |> AsyncResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasAsyncErrorValue "invalid user id")
    ]


type UserTweet = { Name: string; Tweet: string }

let userTweet (p: Post) (u: User) = {
    Name = u.Name.Value
    Tweet = p.Tweet.Value
}


let bindTests =
    testList "AsyncResultOption.bind tests" [
        testCaseAsync "bind with Async(Ok Some(x)) Async(Ok Some(x))"
        <| (getPostById samplePostId
            |> AsyncResultOption.bind (fun post -> getUserById post.UserId)
            |> Expect.hasAsyncOkValue (Some sampleUser))

        testCaseAsync "bind with Async(Ok None) Async(Ok Some(x))"
        <| (getPostById (PostId(Guid.NewGuid()))
            |> AsyncResultOption.bind (fun post ->
                failwith "this shouldn't be called!!"
                getUserById post.UserId
            )
            |> Expect.hasAsyncOkValue None)

        testCaseAsync "bind with Async(Ok Some(x)) Async(Ok None)"
        <| (getPostById samplePostId
            |> AsyncResultOption.bind (fun _ -> getUserById (UserId(Guid.NewGuid())))
            |> Expect.hasAsyncOkValue None)

        testCaseAsync "bind with Async(Ok None) Async(Ok None)"
        <| (getPostById (PostId(Guid.NewGuid()))
            |> AsyncResultOption.bind (fun _ ->
                failwith "this shouldn't be called!!"
                getUserById (UserId(Guid.NewGuid()))
            )
            |> Expect.hasAsyncOkValue None)

        testCaseAsync "bind with Async(Error x) Async(Ok (Some y))"
        <| (getPostById (PostId Guid.Empty)
            |> AsyncResultOption.bind (fun _ ->
                failwith "this shouldn't be called!!"
                getUserById (UserId(Guid.NewGuid()))
            )
            |> Expect.hasAsyncErrorValue "invalid post id")
    ]


let map2Tests =
    testList "AsyncResultOption.map2 tests" [
        testCaseAsync "map2 with Async(Ok Some(x)) Async(Ok Some(x))"
        <| (let postARO = getPostById samplePostId
            let userARO = getUserById sampleUserId

            AsyncResultOption.map2 userTweet postARO userARO
            |> Expect.hasAsyncOkValue (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            ))

        testCaseAsync "map2 with Async(Ok Some(x)) Async(Ok None))"
        <| (let postARO = getPostById samplePostId
            let userARO = getUserById (UserId(Guid.NewGuid()))

            AsyncResultOption.map2 userTweet postARO userARO
            |> Expect.hasAsyncOkValue None)

        testCaseAsync "map2 with Async(Ok Some(x)) Async(Ok None)"
        <| (let postARO = getPostById (PostId(Guid.NewGuid()))
            let userARO = getUserById sampleUserId

            AsyncResultOption.map2 userTweet postARO userARO
            |> Expect.hasAsyncOkValue None)

        testCaseAsync "map2 with Async(Ok None) Async(Ok None)"
        <| (let postARO = getPostById (PostId(Guid.NewGuid()))
            let userARO = getUserById (UserId(Guid.NewGuid()))

            AsyncResultOption.map2 userTweet postARO userARO
            |> Expect.hasAsyncOkValue None)

        testCaseAsync "map2 with Async(Error x) Async(Ok None)"
        <| (let postARO = getPostById (PostId Guid.Empty)
            let userARO = getUserById (UserId(Guid.NewGuid()))

            AsyncResultOption.map2 userTweet postARO userARO
            |> Expect.hasAsyncErrorValue "invalid post id")

        testCaseAsync "map2 with Async(Error x) Async(Error y)"
        <| (let postARO = getPostById (PostId Guid.Empty)
            let userARO = getUserById (UserId Guid.Empty)

            AsyncResultOption.map2 userTweet postARO userARO
            |> Expect.hasAsyncErrorValue "invalid post id")
    ]

#if !FABLE_COMPILER_PYTHON
// https://github.com/fable-compiler/Fable/issues/4125
let ignoreTests =
    testList "AsyncResultOption.ignore tests" [
        testCaseAsync "ignore with Async(Ok Some(x))"
        <| (getUserById sampleUserId
            |> AsyncResultOption.ignore
            |> Expect.hasAsyncOkValue (Some()))

        testCaseAsync "ignore with Async(Ok None)"
        <| (getUserById (UserId(Guid.NewGuid()))
            |> AsyncResultOption.ignore
            |> Expect.hasAsyncOkValue None)

        testCaseAsync "ignore with Async(Error x)"
        <| (getUserById (UserId(Guid.Empty))
            |> AsyncResultOption.ignore
            |> Expect.hasAsyncErrorValue "invalid user id")

        testCase "can call ignore without type parameters"
        <| fun () -> ignore AsyncResultOption.ignore

        testCase "can call ignore with type parameters"
        <| fun () ->
            ignore<Async<Result<int option, string>> -> Async<Result<unit option, string>>>
                AsyncResultOption.ignore<int, string>
    ]

#else
let ignoreTests = testList "AsyncResultOption.ignore tests" []
#endif

let operatorTests =
    testList "AsyncResultOption Operators Tests" [
        testCaseAsync "map & apply operators"
        <| (let getPostResult = getPostById samplePostId
            let getUserResult = getUserById sampleUserId

            userTweet
            <!> getPostResult
            <*> getUserResult
            |> Expect.hasAsyncOkValue (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            ))

        testCaseAsync "bind & map operator"
        <| (getPostById samplePostId
            >>= (fun post ->
                (fun user -> userTweet post user)
                <!> getUserById post.UserId
            )
            |> Expect.hasAsyncOkValue (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            ))
    ]

let allTests =
    testList "AsyncResultOptionTests" [
        mapTests
        bindTests
        map2Tests
        ignoreTests
        operatorTests
    ]
