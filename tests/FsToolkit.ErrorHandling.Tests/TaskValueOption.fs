module TaskValueOptionTests

open Expecto

open System
open System.Threading.Tasks

open TestData
open TestHelpers
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskValueOption

let runTaskSync (task: Task<_>) = task.Result

let createPostValueSome =
    createPostSome
    >> Async.map Option.toValueOption
    >> Async.StartImmediateAsTask

let getFollowersValueSome =
    getFollowersSome
    >> Async.map Option.toValueOption
    >> Async.StartImmediateAsTask

let allowedToPostOptional =
    allowedToPostOptional
    >> Async.map Option.toValueOption
    >> Async.StartImmediateAsTask


let mapTests =
    testList "TaskValueOption.map Tests" [
        testCase "map with Task(ValueSome x)"
        <| fun _ ->
            Task.singleton (ValueSome validTweet)
            |> TaskValueOption.map remainingCharacters
            |> Expect.hasTaskValueSomeValue 267

        testCase "map with Task(ValueNone)"
        <| fun _ ->
            Task.singleton ValueNone
            |> TaskValueOption.map remainingCharacters
            |> Expect.hasTaskValueNoneValue
    ]

let bindTests =
    testList "TaskValueOption.bind tests" [
        testCase "bind with Task(ValueSome x)"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            |> TaskValueOption.bind (fun isAllowed ->
                task {
                    if isAllowed then
                        return! createPostValueSome validCreatePostRequest
                    else
                        return ValueNone
                }
            )
            |> Expect.hasTaskValueSomeValue (PostId newPostId)

        testCase "bind with Task(ValueNone)"
        <| fun _ ->
            allowedToPostOptional (UserId(Guid.NewGuid()))
            |> TaskValueOption.bind (fun isAllowed -> task { return ValueSome isAllowed })
            |> Expect.hasTaskValueNoneValue

        testCase "bind with Task(Ok x) that returns Task (None)"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            |> TaskValueOption.bind (fun _ -> task { return ValueNone })
            |> Expect.hasTaskValueNoneValue
    ]

let applyTests =
    testList "TaskValueOption.apply Tests" [
        testCase "apply with Task(ValueSome x)"
        <| fun _ ->
            Task.singleton (ValueSome validTweet)
            |> TaskValueOption.apply (Task.singleton (ValueSome remainingCharacters))
            |> Expect.hasTaskValueSomeValue (267)

        testCase "apply with Task(ValueNone)"
        <| fun _ ->
            Task.singleton ValueNone
            |> TaskValueOption.apply (Task.singleton (ValueSome remainingCharacters))
            |> Expect.hasTaskValueNoneValue
    ]

let valueSomeTests =
    testList "TaskValueOption.valueSome Tests" [
        testCase "valueSome with x"
        <| fun _ ->
            TaskValueOption.valueSome 267
            |> Expect.hasTaskValueSomeValue 267
    ]

let taskValueOptionOperatorTests =
    testList "TaskValueOption Operators Tests" [
        testCase "map & apply operators"
        <| fun _ ->
            let getFollowersResult = getFollowersValueSome sampleUserId
            let createPostResult = createPostValueSome validCreatePostRequest

            newPostRequest
            <!> getFollowersResult
            <*> createPostResult
            |> Expect.hasTaskValueSomeValue {
                NewPostId = PostId newPostId
                UserIds = followerIds
            }

        testCase "bind operator"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            >>= (fun isAllowed ->
                if isAllowed then
                    createPostValueSome validCreatePostRequest
                else
                    Task.singleton ValueNone
            )
            |> Expect.hasTaskValueSomeValue (PostId newPostId)
    ]


let eitherTests =
    testList "TaskValueOption.either Tests" [
        testCaseTask "ValueSome"
        <| fun () ->
            task {
                let value1 = TaskValueOption.valueSome 5
                let f () = Task.FromResult 42
                let add2 x = task { return x + 2 }
                let! result = (TaskValueOption.either add2 f value1)
                Expect.equal result 7 ""
            }
        testCaseTask "ValueNone"
        <| fun () ->
            task {
                let value1 = Task.FromResult ValueNone
                let f () = Task.FromResult 42
                let add2 x = task { return x + 2 }
                let! result = (TaskValueOption.either add2 f value1)
                Expect.equal result 42 ""
            }
    ]

let defaultValueTests =
    testList "TaskValueOption.defaultValue Tests" [
        testCaseTask "ValueSome"
        <| fun () ->
            task {
                let defaultValue = 10
                let expectedValue = 5

                let taskValueOption = TaskValueOption.valueSome expectedValue
                let! result = TaskValueOption.defaultValue defaultValue taskValueOption
                Expect.equal result expectedValue ""
            }

        testCaseTask "ValueNone"
        <| fun () ->
            task {
                let expectedValue = 10
                let taskValueOption = Task.singleton ValueNone
                let! result = TaskValueOption.defaultValue expectedValue taskValueOption
                Expect.equal result expectedValue ""
            }
    ]

let defaultWithTests =
    testList "TaskValueOption.defaultWith Tests" [
        testCaseTask "ValueSome"
        <| fun () ->
            task {
                let defaultValue = 10
                let expectedValue = 5

                let taskValueOption = TaskValueOption.valueSome expectedValue
                let! result = TaskValueOption.defaultWith (fun () -> defaultValue) taskValueOption
                Expect.equal result expectedValue ""
            }

        testCaseTask "ValueNone"
        <| fun () ->
            task {
                let expectedValue = 10
                let taskValueOption = Task.singleton ValueNone
                let! result = TaskValueOption.defaultWith (fun () -> expectedValue) taskValueOption
                Expect.equal result expectedValue ""
            }
    ]

let allTests =
    testList "Task ValueOption Tests" [
        mapTests
        bindTests
        applyTests
        valueSomeTests
        taskValueOptionOperatorTests
        eitherTests
        defaultValueTests
        defaultWithTests
    ]
