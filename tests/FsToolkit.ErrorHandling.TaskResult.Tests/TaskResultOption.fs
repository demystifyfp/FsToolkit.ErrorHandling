module TaskResultOptionTests

open System.Threading.Tasks
open Expecto
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskResultOption
open System
open TestHelpers

let getUserById x =
    getUserById x
    |> Async.StartImmediateAsTask

let getPostById x =
    getPostById x
    |> Async.StartImmediateAsTask


[<Tests>]
let mapTests =
    testList "TaskResultOption.map tests" [
        testCase "map with Task(Ok Some(x))"
        <| fun _ ->
            getUserById sampleUserId
            |> TaskResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasTaskOkValueSync (Some "someone")

        testCase "map with Task(Ok None)"
        <| fun _ ->
            getUserById (UserId(Guid.NewGuid()))
            |> TaskResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasTaskOkValueSync None

        testCase "map with Task(Error x)"
        <| fun _ ->
            getUserById (UserId(Guid.Empty))
            |> TaskResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasTaskErrorValueSync "invalid user id"
    ]


type UserTweet = { Name: string; Tweet: string }

let userTweet (p: Post) (u: User) = {
    Name = u.Name.Value
    Tweet = p.Tweet.Value
}


[<Tests>]
let bindTests =
    testList "TaskResultOption.bind tests" [
        testCase "bind with Task(Ok Some(x)) Task(Ok Some(x))"
        <| fun _ ->
            getPostById samplePostId
            |> TaskResultOption.bind (fun post -> getUserById post.UserId)
            |> Expect.hasTaskOkValueSync (Some sampleUser)

        testCase "bind with Task(Ok None) Task(Ok Some(x))"
        <| fun _ ->
            getPostById (PostId(Guid.NewGuid()))
            |> TaskResultOption.bind (fun post ->
                failwith "this shouldn't be called!!"
                getUserById post.UserId
            )
            |> Expect.hasTaskOkValueSync None

        testCase "bind with Task(Ok Some(x)) Task(Ok None)"
        <| fun _ ->
            getPostById samplePostId
            |> TaskResultOption.bind (fun _ -> getUserById (UserId(Guid.NewGuid())))
            |> Expect.hasTaskOkValueSync None

        testCase "bind with Task(Ok None) Task(Ok None)"
        <| fun _ ->
            getPostById (PostId(Guid.NewGuid()))
            |> TaskResultOption.bind (fun _ ->
                failwith "this shouldn't be called!!"
                getUserById (UserId(Guid.NewGuid()))
            )
            |> Expect.hasTaskOkValueSync None

        testCase "bind with Task(Error x) Task(Ok (Some y))"
        <| fun _ ->
            getPostById (PostId Guid.Empty)
            |> TaskResultOption.bind (fun _ ->
                failwith "this shouldn't be called!!"
                getUserById (UserId(Guid.NewGuid()))
            )
            |> Expect.hasTaskErrorValueSync "invalid post id"
    ]

[<Tests>]
let map2Tests =
    testList "TaskResultOption.map2 tests" [
        testCase "map2 with Task(Ok Some(x)) Task(Ok Some(x))"
        <| fun _ ->
            let postARO = getPostById samplePostId
            let userARO = getUserById sampleUserId

            TaskResultOption.map2 userTweet postARO userARO
            |> Expect.hasTaskOkValueSync (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            )

        testCase "map2 with Task(Ok Some(x)) Task(Ok None))"
        <| fun _ ->
            let postARO = getPostById samplePostId
            let userARO = getUserById (UserId(Guid.NewGuid()))

            TaskResultOption.map2 userTweet postARO userARO
            |> Expect.hasTaskOkValueSync None

        testCase "map2 with Task(Ok Some(x)) Task(Ok None)"
        <| fun _ ->
            let postARO = getPostById (PostId(Guid.NewGuid()))
            let userARO = getUserById sampleUserId

            TaskResultOption.map2 userTweet postARO userARO
            |> Expect.hasTaskOkValueSync None

        testCase "map2 with Task(Ok None) Task(Ok None)"
        <| fun _ ->
            let postARO = getPostById (PostId(Guid.NewGuid()))
            let userARO = getUserById (UserId(Guid.NewGuid()))

            TaskResultOption.map2 userTweet postARO userARO
            |> Expect.hasTaskOkValueSync None

        testCase "map2 with Task(Error x) Task(Ok None)"
        <| fun _ ->
            let postARO = getPostById (PostId Guid.Empty)
            let userARO = getUserById (UserId(Guid.NewGuid()))

            TaskResultOption.map2 userTweet postARO userARO
            |> Expect.hasTaskErrorValueSync "invalid post id"

        testCase "map2 with Task(Error x) Task(Error y)"
        <| fun _ ->
            let postARO = getPostById (PostId Guid.Empty)
            let userARO = getUserById (UserId Guid.Empty)

            TaskResultOption.map2 userTweet postARO userARO
            |> Expect.hasTaskErrorValueSync "invalid post id"
    ]

[<Tests>]
let ignoreTests =
    testList "TaskResultOption.ignore tests" [
        testCase "ignore with Task(Ok Some(x))"
        <| fun _ ->
            getUserById sampleUserId
            |> TaskResultOption.ignore
            |> Expect.hasTaskOkValueSync (Some())

        testCase "ignore with Task(Ok None)"
        <| fun _ ->
            getUserById (UserId(Guid.NewGuid()))
            |> TaskResultOption.ignore
            |> Expect.hasTaskOkValueSync None

        testCase "ignore with Task(Error x)"
        <| fun _ ->
            getUserById (UserId(Guid.Empty))
            |> TaskResultOption.ignore
            |> Expect.hasTaskErrorValueSync "invalid user id"

        testCase "can call ignore without type parameters"
        <| fun _ -> ignore TaskResultOption.ignore

        testCase "can call ignore with type parameters"
        <| fun _ ->
            ignore<Task<Result<int option, string>> -> Task<Result<unit option, string>>>
                TaskResultOption.ignore<int, string>
    ]

[<Tests>]
let computationExpressionTests =
    testList "taskResultOption CE tests" [
        testCase "CE with Task(Ok Some(x)) Task(Ok Some(x))"
        <| fun _ ->
            taskResultOption {
                let! post = getPostById samplePostId
                let! user = getUserById post.UserId
                return userTweet post user
            }
            |> Expect.hasTaskOkValueSync (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            )

        testCase "CE with Task(Ok None) Task(Ok Some(x))"
        <| fun _ ->
            taskResultOption {
                let! post = getPostById (PostId(Guid.NewGuid()))
                let! user = getUserById post.UserId
                return userTweet post user
            }
            |> Expect.hasTaskOkValueSync None

        testCase "CE with Task(Ok Some(x)) Task(Ok None)"
        <| fun _ ->
            taskResultOption {
                let! post = getPostById samplePostId
                let! user = getUserById (UserId(Guid.NewGuid()))
                return userTweet post user
            }
            |> Expect.hasTaskOkValueSync None

        testCase "CE with Task(Error x) Task(Ok None)"
        <| fun _ ->
            taskResultOption {
                let! post = getPostById (PostId Guid.Empty)
                let! user = getUserById post.UserId
                return userTweet post user
            }
            |> Expect.hasTaskErrorValueSync "invalid post id"
    ]

[<Tests>]
let operatorTests =
    testList "TaskResultOption Operators Tests" [
        testCase "map & apply operators"
        <| fun _ ->
            let getPostResult = getPostById samplePostId
            let getUserResult = getUserById sampleUserId

            userTweet
            <!> getPostResult
            <*> getUserResult
            |> Expect.hasTaskOkValueSync (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            )

        testCase "bind & map operator"
        <| fun _ ->
            getPostById samplePostId
            >>= (fun post ->
                (fun user -> userTweet post user)
                <!> getUserById post.UserId
            )
            |> Expect.hasTaskOkValueSync (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            )
    ]


[<Tests>]
let ``TaskResultOptionCE inference checks`` =
    testList "TaskResultOption inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = taskResultOption { return! res }

            f (TaskResultOption.singleton ())
            |> ignore
    ]
