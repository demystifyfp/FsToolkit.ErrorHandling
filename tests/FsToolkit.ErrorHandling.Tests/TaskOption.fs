module TaskOptionTests

open Expecto

open System
open System.Threading.Tasks

open TestData
open TestHelpers
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskOption

let runTaskSync (task: Task<_>) = task.Result

let createPostSome =
    createPostSome
    >> Async.StartImmediateAsTask

let getFollowersSome =
    getFollowersSome
    >> Async.StartImmediateAsTask

let allowedToPostOptional =
    allowedToPostOptional
    >> Async.StartImmediateAsTask


let mapTests =
    testList "TaskOption.map Tests" [
        testCase "map with Task(Some x)"
        <| fun _ ->
            Task.singleton (Some validTweet)
            |> TaskOption.map remainingCharacters
            |> Expect.hasTaskSomeValue 267

        testCase "map with Task(None)"
        <| fun _ ->
            Task.singleton (None)
            |> TaskOption.map remainingCharacters
            |> Expect.hasTaskNoneValue
    ]

let bindTests =
    testList "TaskOption.bind tests" [
        testCase "bind with Task(Some x)"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            |> TaskOption.bind (fun isAllowed ->
                task {
                    if isAllowed then
                        return! createPostSome validCreatePostRequest
                    else
                        return None
                }
            )
            |> Expect.hasTaskSomeValue (PostId newPostId)

        testCase "bind with Task(None)"
        <| fun _ ->
            allowedToPostOptional (UserId(Guid.NewGuid()))
            |> TaskOption.bind (fun isAllowed -> task { return Some isAllowed })
            |> Expect.hasTaskNoneValue

        testCase "bind with Task(Ok x) that returns Task (None)"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            |> TaskOption.bind (fun _ -> task { return None })
            |> Expect.hasTaskNoneValue
    ]

let applyTests =
    testList "TaskOption.apply Tests" [
        testCase "apply with Task(Some x)"
        <| fun _ ->
            Task.singleton (Some validTweet)
            |> TaskOption.apply (Task.singleton (Some remainingCharacters))
            |> Expect.hasTaskSomeValue (267)

        testCase "apply with Task(None)"
        <| fun _ ->
            Task.singleton None
            |> TaskOption.apply (Task.singleton (Some remainingCharacters))
            |> Expect.hasTaskNoneValue
    ]

let retnTests =
    testList "TaskOption.retn Tests" [
        testCase "retn with x"
        <| fun _ ->
            TaskOption.some 267
            |> Expect.hasTaskSomeValue (267)
    ]

let taskOptionOperatorTests =
    testList "TaskOption Operators Tests" [
        testCase "map & apply operators"
        <| fun _ ->
            let getFollowersResult = getFollowersSome sampleUserId
            let createPostResult = createPostSome validCreatePostRequest

            newPostRequest
            <!> getFollowersResult
            <*> createPostResult
            |> Expect.hasTaskSomeValue {
                NewPostId = PostId newPostId
                UserIds = followerIds
            }

        testCase "bind operator"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            >>= (fun isAllowed ->
                if isAllowed then
                    createPostSome validCreatePostRequest
                else
                    Task.singleton None
            )
            |> Expect.hasTaskSomeValue (PostId newPostId)
    ]


let eitherTests =
    testList "TaskOption.either Tests" [
        testCaseTask "Some"
        <| fun () ->
            task {
                let value1 = TaskOption.some 5
                let f () = Task.FromResult 42
                let add2 x = task { return x + 2 }
                let! result = (TaskOption.either add2 f value1)
                Expect.equal result 7 ""
            }
        testCaseTask "None"
        <| fun () ->
            task {
                let value1 = Task.FromResult None
                let f () = Task.FromResult 42
                let add2 x = task { return x + 2 }
                let! result = (TaskOption.either add2 f value1)
                Expect.equal result 42 ""
            }
    ]

let defaultValueTests =
    testList "TaskOption.defaultValue Tests" [
        testCaseTask "Some"
        <| fun () ->
            task {
                let defaultValue = 10
                let expectedValue = 5

                let taskOption = TaskOption.some expectedValue
                let! result = TaskOption.defaultValue defaultValue taskOption
                Expect.equal result expectedValue ""
            }

        testCaseTask "None"
        <| fun () ->
            task {
                let expectedValue = 10
                let taskOption = Task.singleton None
                let! result = TaskOption.defaultValue expectedValue taskOption
                Expect.equal result expectedValue ""
            }
    ]

let defaultWithTests =
    testList "TaskOption.defaultWith Tests" [
        testCaseTask "Some"
        <| fun () ->
            task {
                let defaultValue = 10
                let expectedValue = 5

                let taskOption = TaskOption.some expectedValue
                let! result = TaskOption.defaultWith (fun () -> defaultValue) taskOption
                Expect.equal result expectedValue ""
            }

        testCaseTask "None"
        <| fun () ->
            task {
                let expectedValue = 10
                let taskOption = Task.singleton None
                let! result = TaskOption.defaultWith (fun () -> expectedValue) taskOption
                Expect.equal result expectedValue ""
            }
    ]

let allTests =
    testList "Task Option Tests" [
        mapTests
        bindTests
        applyTests
        retnTests
        taskOptionOperatorTests
        eitherTests
        defaultValueTests
        defaultWithTests
    ]
