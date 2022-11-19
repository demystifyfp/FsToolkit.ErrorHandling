module TaskResultTests


open Expecto
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskResult
open System
open System.Threading.Tasks

#if NETSTANDARD2_0 || NET6_0
open FSharp.Control.Tasks
#endif

let runTaskSync (task: Task<_>) = task.Result

let createPostSuccess =
    createPostSuccess
    >> Async.StartAsTask

let createPostFailure =
    createPostFailure
    >> Async.StartAsTask

let getFollowersSuccess =
    getFollowersSuccess
    >> Async.StartAsTask

let getFollowersFailure =
    getFollowersFailure
    >> Async.StartAsTask

let allowedToPost =
    allowedToPost
    >> Async.StartAsTask

[<Tests>]
let mapTests =
    testList "TaskResult.map tests" [
        testCase "map with Task(Ok x)"
        <| fun _ ->
            createPostSuccess validCreatePostRequest
            |> TaskResult.map (fun (PostId id) -> { Id = id })
            |> Expect.hasTaskOkValueSync { Id = newPostId }

        testCase "map with Task(Error x)"
        <| fun _ ->
            createPostFailure validCreatePostRequest
            |> TaskResult.mapError (fun ex -> { Message = ex.Message })
            |> Expect.hasTaskErrorValueSync { Message = "something went wrong!" }
    ]

[<Tests>]
let map2Tests =
    testList "TaskResult.map2 tests" [
        testCase "map2 with Task(Ok x) Task(Ok y)"
        <| fun _ ->
            let userId = Guid.NewGuid()
            let getFollowersResult = getFollowersSuccess (UserId userId)
            let createPostResult = createPostSuccess validCreatePostRequest

            TaskResult.map2 newPostRequest getFollowersResult createPostResult
            |> Expect.hasTaskOkValueSync
                {
                    NewPostId = PostId newPostId
                    UserIds = followerIds
                }

        testCase "map2 with Task(Error x) Task(Ok y)"
        <| fun _ ->
            let userId = Guid.NewGuid()
            let getFollowersResult = getFollowersFailure (UserId userId)
            let createPostResult = createPostSuccess validCreatePostRequest

            TaskResult.map2 newPostRequest getFollowersResult createPostResult
            |> Expect.hasTaskErrorValueSync getFollowersEx

        testCase "map2 with Task(Ok x) Task(Error y)"
        <| fun _ ->
            let userId = Guid.NewGuid()
            let getFollowersResult = getFollowersSuccess (UserId userId)
            let createPostResult = createPostFailure validCreatePostRequest

            TaskResult.map2 newPostRequest getFollowersResult createPostResult
            |> Expect.hasTaskErrorValueSync commonEx

        testCase "map2 with Task(Error x) Task(Error y)"
        <| fun _ ->
            let userId = Guid.NewGuid()
            let getFollowersResult = getFollowersFailure (UserId userId)
            let createPostResult = createPostFailure validCreatePostRequest

            TaskResult.map2 newPostRequest getFollowersResult createPostResult
            |> Expect.hasTaskErrorValueSync getFollowersEx
    ]

[<Tests>]
let mapErrorTests =
    testList "TaskResult.mapError tests" [
        testCase "mapError with Task(Ok x)"
        <| fun _ ->
            createPostSuccess validCreatePostRequest
            |> TaskResult.mapError id
            |> runTaskSync
            |> flip Expect.isOk "mapError should not map Ok"

        testCase "mapError with Task(Error x)"
        <| fun _ ->
            createPostFailure validCreatePostRequest
            |> TaskResult.mapError (fun ex -> ex.Message)
            |> Expect.hasTaskErrorValueSync (commonEx.Message)
    ]

[<Tests>]
let bindTests =
    testList "TaskResult.bind tests" [
        testCase "bind with Task(Ok x)"
        <| fun _ ->
            allowedToPost sampleUserId
            |> TaskResult.bind (fun isAllowed -> task {
                if isAllowed then
                    return! createPostSuccess validCreatePostRequest
                else
                    return (Error(Exception "not allowed to post"))
            }
            )
            |> Expect.hasTaskOkValueSync (PostId newPostId)

        testCase "bind with Task(Error x)"
        <| fun _ ->
            allowedToPost (UserId(Guid.NewGuid()))
            |> TaskResult.bind (fun isAllowed -> task { return Ok isAllowed })
            |> Expect.hasTaskErrorValueSync commonEx

        testCase "bind with Task(Ok x) that returns Task (Error x)"
        <| fun _ ->
            let ex = Exception "not allowed to post"

            allowedToPost sampleUserId
            |> TaskResult.bind (fun _ -> task { return (Error ex) })
            |> Expect.hasTaskErrorValueSync ex
    ]


[<Tests>]
let orElseTests =
    testList "TaskResult.orElseWith Tests" [
        testCaseTask "Ok Ok takes first Ok"
        <| fun () -> task {
            do!
                TaskResult.ok "First"
                |> TaskResult.orElse (TaskResult.ok "Second")
                |> Expect.hasTaskOkValue "First"
           }
        testCaseTask "Ok Error takes first Ok"
        <| fun () -> task {
            return!
                TaskResult.ok "First"
                |> TaskResult.orElse (TaskResult.error "Second")
                |> Expect.hasTaskOkValue "First"
           }
        testCaseTask "Error Ok takes second Ok"
        <| fun () -> task {
            return!
                TaskResult.error "First"
                |> TaskResult.orElse (TaskResult.ok "Second")
                |> Expect.hasTaskOkValue "Second"
           }
        testCaseTask "Error Error takes second error"
        <| fun () -> task {
            return!
                TaskResult.error "First"
                |> TaskResult.orElse (TaskResult.error "Second")
                |> Expect.hasTaskErrorValue "Second"
           }
    ]

[<Tests>]
let orElseWithTests =
    testList "TaskResult.orElse Tests" [
        testCaseTask "Ok Ok takes first Ok"
        <| fun () -> task {
            return!
                TaskResult.ok "First"
                |> TaskResult.orElseWith (fun _ -> TaskResult.ok "Second")
                |> Expect.hasTaskOkValue "First"
           }
        testCaseTask "Ok Error takes first Ok"
        <| fun () -> task {
            return!
                TaskResult.ok "First"
                |> TaskResult.orElseWith (fun _ -> TaskResult.error "Second")
                |> Expect.hasTaskOkValue "First"
           }
        testCaseTask "Error Ok takes second Ok"
        <| fun () -> task {
            return!
                TaskResult.error "First"
                |> TaskResult.orElseWith (fun _ -> TaskResult.ok "Second")
                |> Expect.hasTaskOkValue "Second"
           }
        testCaseTask "Error Error takes second error"
        <| fun () -> task {
            return!
                TaskResult.error "First"
                |> TaskResult.orElseWith (fun _ -> TaskResult.error "Second")
                |> Expect.hasTaskErrorValue "Second"
           }
    ]

[<Tests>]
let ignoreTests =
    testList "TaskResult.ignore tests" [
        testCase "ignore with Task(Ok x)"
        <| fun _ ->
            createPostSuccess validCreatePostRequest
            |> TaskResult.ignore
            |> Expect.hasTaskOkValueSync ()

        testCase "ignore with Task(Error x)"
        <| fun _ ->
            createPostFailure validCreatePostRequest
            |> TaskResult.ignore
            |> Expect.hasTaskErrorValueSync commonEx

        testCase "can call ignore without type parameters"
        <| fun _ -> ignore TaskResult.ignore

        testCase "can call ignore with type parameters"
        <| fun _ ->
            ignore<Task<Result<int, string>> -> Task<Result<unit, string>>>
                TaskResult.ignore<int, string>
    ]

let err = "foobar"
let toTask x = task { return x }

[<Tests>]
let requireTrueTests =
    testList "TaskResult.requireTrue Tests" [
        testCase "requireTrue happy path"
        <| fun _ ->
            toTask true
            |> TaskResult.requireTrue err
            |> Expect.hasTaskOkValueSync ()

        testCase "requireTrue error path"
        <| fun _ ->
            toTask false
            |> TaskResult.requireTrue err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let requireFalseTests =
    testList "TaskResult.requireFalse Tests" [
        testCase "requireFalse happy path"
        <| fun _ ->
            toTask false
            |> TaskResult.requireFalse err
            |> Expect.hasTaskOkValueSync ()

        testCase "requireFalse error path"
        <| fun _ ->
            toTask true
            |> TaskResult.requireFalse err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let requireSomeTests =
    testList "TaskResult.requireSome Tests" [
        testCase "requireSome happy path"
        <| fun _ ->
            toTask (Some 42)
            |> TaskResult.requireSome err
            |> Expect.hasTaskOkValueSync 42

        testCase "requireSome error path"
        <| fun _ ->
            toTask None
            |> TaskResult.requireSome err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let requireNoneTests =
    testList "TaskResult.requireNone Tests" [
        testCase "requireNone happy path"
        <| fun _ ->
            toTask None
            |> TaskResult.requireNone err
            |> Expect.hasTaskOkValueSync ()

        testCase "requireNone error path"
        <| fun _ ->
            toTask (Some 42)
            |> TaskResult.requireNone err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let requireEqualToTests =
    testList "TaskResult.requireEqualTo Tests" [
        testCase "requireEqualTo happy path"
        <| fun _ ->
            toTask 42
            |> TaskResult.requireEqualTo 42 err
            |> Expect.hasTaskOkValueSync ()

        testCase "requireEqualTo error path"
        <| fun _ ->
            toTask 43
            |> TaskResult.requireEqualTo 42 err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let requireEqualTests =
    testList "TaskResult.requireEqual Tests" [
        testCase "requireEqual happy path"
        <| fun _ ->
            TaskResult.requireEqual 42 (toTask 42) err
            |> Expect.hasTaskOkValueSync ()

        testCase "requireEqual error path"
        <| fun _ ->
            TaskResult.requireEqual 42 (toTask 43) err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let requireEmptyTests =
    testList "TaskResult.requireEmpty Tests" [
        testCase "requireEmpty happy path"
        <| fun _ ->
            toTask []
            |> TaskResult.requireEmpty err
            |> Expect.hasTaskOkValueSync ()

        testCase "requireEmpty error path"
        <| fun _ ->
            toTask [ 42 ]
            |> TaskResult.requireEmpty err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let requireNotEmptyTests =
    testList "TaskResult.requireNotEmpty Tests" [
        testCase "requireNotEmpty happy path"
        <| fun _ ->
            toTask [ 42 ]
            |> TaskResult.requireNotEmpty err
            |> Expect.hasTaskOkValueSync ()

        testCase "requireNotEmpty error path"
        <| fun _ ->
            toTask []
            |> TaskResult.requireNotEmpty err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let requireHeadTests =
    testList "TaskResult.requireHead Tests" [
        testCase "requireHead happy path"
        <| fun _ ->
            toTask [ 42 ]
            |> TaskResult.requireHead err
            |> Expect.hasTaskOkValueSync 42

        testCase "requireHead error path"
        <| fun _ ->
            toTask []
            |> TaskResult.requireHead err
            |> Expect.hasTaskErrorValueSync err
    ]

[<Tests>]
let setErrorTests =
    testList "TaskResult.setError Tests" [
        testCase "setError replaces a any error value with a custom error value"
        <| fun _ ->
            toTask (Error "foo")
            |> TaskResult.setError err
            |> Expect.hasTaskErrorValueSync err

        testCase "setError does not change an ok value"
        <| fun _ ->
            toTask (Ok 42)
            |> TaskResult.setError err
            |> Expect.hasTaskOkValueSync 42
    ]

[<Tests>]
let withErrorTests =
    testList "TaskResult.withError Tests" [
        testCase "withError replaces a any error value with a custom error value"
        <| fun _ ->
            toTask (Error())
            |> TaskResult.withError err
            |> Expect.hasTaskErrorValueSync err

        testCase "withError does not change an ok value"
        <| fun _ ->
            toTask (Ok 42)
            |> TaskResult.withError err
            |> Expect.hasTaskOkValueSync 42
    ]

[<Tests>]
let defaultValueTests =
    testList "TaskResult.defaultValue Tests" [
        testCase "defaultValue returns the ok value"
        <| fun _ ->
            let v = TaskResult.defaultValue 43 (toTask (Ok 42))

            Expect.hasTaskValue 42 v

        testCase "defaultValue returns the given value for Error"
        <| fun _ ->
            let v = TaskResult.defaultValue 43 (toTask (Error err))

            Expect.hasTaskValue 43 v
    ]

[<Tests>]
let defaultErrorTests =
    testList "TaskResult.defaultError Tests" [
        testCase "defaultError returns the error value"
        <| fun _ ->
            let v = TaskResult.defaultError 43 (toTask (Error 42))

            Expect.hasTaskValue 42 v

        testCase "defaultError returns the given value for Ok"
        <| fun _ ->
            let v = TaskResult.defaultError 43 (toTask (Ok 42))

            Expect.hasTaskValue 43 v
    ]

[<Tests>]
let defaultWithTests =
    testList "TaskResult.defaultWith Tests" [
        testCase "defaultWith returns the ok value"
        <| fun _ ->
            let v = TaskResult.defaultWith (fun () -> 43) (toTask (Ok 42))

            Expect.hasTaskValue 42 v

        testCase "defaultValue invoks the given thunk for Error"
        <| fun _ ->
            let v = TaskResult.defaultWith (fun () -> 42) (toTask (Error err))

            Expect.hasTaskValue 42 v
    ]

[<Tests>]
let ignoreErrorTests =
    testList "TaskResult.ignoreError Tests" [
        testCase "ignoreError returns the unit for ok"
        <| fun _ -> Expect.hasTaskValue () (TaskResult.ignoreError (toTask (Ok())))

        testCase "ignoreError returns the unit for Error"
        <| fun _ -> Expect.hasTaskValue () (TaskResult.ignoreError (toTask (Error err)))

        testCase "can call ignoreError without type parameter"
        <| fun _ -> ignore TaskResult.ignoreError

        testCase "can call ignoreError with type parameter"
        <| fun _ -> ignore<Task<Result<unit, string>> -> Task<unit>> TaskResult.ignoreError<string>
    ]

[<Tests>]
let teeTests =

    testList "TaskResult.tee Tests" [
        testCase "tee executes the function for ok"
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = 0

            let bar x =
                input <- x

                foo <- "bar"

            let result = TaskResult.tee bar (toTask (Ok 42))
            Expect.hasTaskOkValueSync 42 result
            Expect.equal foo "bar" ""
            Expect.equal input 42 ""

        testCase "tee ignores the function for Error"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = TaskResult.tee bar (toTask (Error err))
            Expect.hasTaskErrorValueSync err result
            Expect.equal foo "foo" ""
    ]

let returnTrue _ = true
let returnFalse _ = false

[<Tests>]
let teeIfTests =
    testList "TaskResult.teeIf Tests" [
        testCase "teeIf executes the function for ok and true predicate "
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = 0
            let mutable pInput = 0

            let returnTrue x =
                pInput <- x

                true

            let bar x =
                input <- x

                foo <- "bar"

            let result = TaskResult.teeIf returnTrue bar (toTask (Ok 42))

            Expect.hasTaskOkValueSync 42 result
            Expect.equal foo "bar" ""
            Expect.equal input 42 ""
            Expect.equal pInput 42 ""

        testCase "teeIf ignores the function for Ok and false predicate"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = TaskResult.teeIf returnFalse bar (toTask (Ok 42))

            Expect.hasTaskOkValueSync 42 result
            Expect.equal foo "foo" ""

        testCase "teeIf ignores the function for Error"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = TaskResult.teeIf returnTrue bar (toTask (Error err))

            Expect.hasTaskErrorValueSync err result
            Expect.equal foo "foo" ""
    ]

[<Tests>]
let teeErrorTests =

    testList "TaskResult.teeError Tests" [
        testCase "teeError executes the function for Error"
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = ""

            let bar x =
                input <- x

                foo <- "bar"

            let result = TaskResult.teeError bar (toTask (Error err))

            Expect.hasTaskErrorValueSync err result
            Expect.equal foo "bar" ""
            Expect.equal input err ""

        testCase "teeError ignores the function for Ok"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = TaskResult.teeError bar (toTask (Ok 42))
            Expect.hasTaskOkValueSync 42 result
            Expect.equal foo "foo" ""
    ]


[<Tests>]
let teeErrorIfTests =
    testList "TaskResult.teeErrorIf Tests" [
        testCase "teeErrorIf executes the function for Error and true predicate "
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = ""
            let mutable pInput = ""

            let returnTrue x =
                pInput <- x

                true

            let bar x =
                input <- x

                foo <- "bar"

            let result = TaskResult.teeErrorIf returnTrue bar (toTask (Error err))

            Expect.hasTaskErrorValueSync err result
            Expect.equal foo "bar" ""
            Expect.equal input err ""
            Expect.equal pInput err ""

        testCase "teeErrorIf ignores the function for Error and false predicate"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = TaskResult.teeErrorIf returnFalse bar (toTask (Error err))

            Expect.hasTaskErrorValueSync err result
            Expect.equal foo "foo" ""

        testCase "teeErrorIf ignores the function for Ok"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = TaskResult.teeErrorIf returnTrue bar (toTask (Ok 42))

            Expect.hasTaskOkValueSync 42 result
            Expect.equal foo "foo" ""
    ]

[<Tests>]
let catchTests =
    let f (e: exn) = e.Message

    let taskThrow () = task {
        failwith err
        return Error ""
    }

    testList "TaskResult.catch tests" [
        testCase "catch returns success for Ok"
        <| fun _ -> Expect.hasTaskOkValueSync 42 (TaskResult.catch f (toTask (Ok 42)))

        testCase "catch returns mapped Error for exception"
        <| fun _ -> Expect.hasTaskErrorValueSync err (TaskResult.catch f (taskThrow ()))

        testCase "catch returns unmapped error without exception"
        <| fun _ ->
            Expect.hasTaskErrorValueSync "unmapped" (TaskResult.catch f (toTask (Error "unmapped")))
    ]

[<Tests>]
let zipTests =
    testList "TaskResult.zip tests" [
        testCase "Ok, Ok"
        <| fun _ ->
            let v = TaskResult.zip (toTask (Ok 1)) (toTask (Ok 2))

            Expect.hasTaskValue (Ok(1, 2)) v

        testCase "Ok, Error"
        <| fun _ ->
            let v = TaskResult.zip (toTask (Ok 1)) (toTask (Error "Bad"))

            Expect.hasTaskValue (Error("Bad")) v

        testCase "Error, Ok"
        <| fun _ ->
            let v = TaskResult.zip (toTask (Error "Bad")) (toTask (Ok 1))

            Expect.hasTaskValue (Error("Bad")) v

        testCase "Error, Error"
        <| fun _ ->
            let v = TaskResult.zip (toTask (Error "Bad1")) (toTask (Error "Bad2"))

            Expect.hasTaskValue (Error("Bad1")) v
    ]

[<Tests>]
let zipErrorTests =
    testList "TaskResult.zipError tests" [
        testCase "Ok, Ok"
        <| fun _ ->
            let v = TaskResult.zipError (toTask (Ok 1)) (toTask (Ok 2))

            Expect.hasTaskValue (Ok(1)) v

        testCase "Ok, Error"
        <| fun _ ->
            let v = TaskResult.zipError (toTask (Ok 1)) (toTask (Error "Bad"))

            Expect.hasTaskValue (Ok 1) v

        testCase "Error, Ok"
        <| fun _ ->
            let v = TaskResult.zipError (toTask (Error "Bad")) (toTask (Ok 1))

            Expect.hasTaskValue (Ok 1) v

        testCase "Error, Error"
        <| fun _ ->
            let v = TaskResult.zipError (toTask (Error "Bad1")) (toTask (Error "Bad2"))

            Expect.hasTaskValue (Error("Bad1", "Bad2")) v
    ]

type CreatePostResult =
    | PostSuccess of NotifyNewPostRequest
    | NotAllowedToPost

[<Tests>]
let TaskResultCETests =
    let createPost userId = taskResult {
        let! isAllowed = allowedToPost userId

        if isAllowed then
            let! postId = createPostSuccess validCreatePostRequest
            let! followerIds = getFollowersSuccess sampleUserId

            return
                PostSuccess
                    {
                        NewPostId = postId
                        UserIds = followerIds
                    }
        else
            return NotAllowedToPost
    }

    testList "TaskResult Computation Expression tests" [
        testCase "bind with all Ok"
        <| fun _ ->
            createPost sampleUserId
            |> Expect.hasTaskOkValueSync (
                PostSuccess
                    {
                        NewPostId = PostId newPostId
                        UserIds = followerIds
                    }
            )

        testCase "bind with an Error"
        <| fun _ ->
            createPost (UserId(System.Guid.NewGuid()))
            |> Expect.hasTaskErrorValueSync commonEx
    ]

[<Tests>]
let TaskResultOperatorTests =
    testList "TaskResult Operators Tests" [
        testCase "map & apply operators"
        <| fun _ ->
            let getFollowersResult = getFollowersSuccess sampleUserId
            let createPostResult = createPostSuccess validCreatePostRequest

            newPostRequest
            <!> getFollowersResult
            <*> createPostResult
            |> Expect.hasTaskOkValueSync
                {
                    NewPostId = PostId newPostId
                    UserIds = followerIds
                }

        testCase "bind operator"
        <| fun _ ->
            allowedToPost sampleUserId
            >>= (fun isAllowed ->
                if isAllowed then
                    createPostSuccess validCreatePostRequest
                else
                    TaskResult.returnError (Exception "")
            )
            |> Expect.hasTaskOkValueSync (PostId newPostId)
    ]

[<Tests>]
let TaskResultBindRequireTests =
    testList "TaskResult Bind + Require Tests" [
        testCaseTask "bindRequireNone"
        <| fun _ -> task {
            do!
                Some "john_doe"
                |> TaskResult.ok
                |> TaskResult.bindRequireNone "User exists"
                |> Expect.hasTaskErrorValue "User exists"
           }

        testCaseTask "bindRequireSome"
        <| fun _ -> task {
            do!
                Some "john_doe"
                |> TaskResult.ok
                |> TaskResult.bindRequireSome "User doesn't exist"
                |> Expect.hasTaskOkValue "john_doe"
           }
    ]
