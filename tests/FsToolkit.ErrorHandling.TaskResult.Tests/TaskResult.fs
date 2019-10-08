module TaskResultTests


open Expecto
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskResult
open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive

let runTaskSync (task : Task<_>) =
    task.Result
let createPostSuccess = createPostSuccess >> Async.StartAsTask
let createPostFailure = createPostFailure >> Async.StartAsTask
let getFollowersSuccess = getFollowersSuccess >> Async.StartAsTask
let getFollowersFailure = getFollowersFailure >> Async.StartAsTask
let allowedToPost = allowedToPost >> Async.StartAsTask

[<Tests>]
let mapTests =
  testList "TaskResult.map tests" [
    testCase "map with Task(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> TaskResult.map (fun (PostId id) -> {Id = id})
      |> Expect.hasTaskOkValue {Id = newPostId}

    testCase "map with Task(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> TaskResult.mapError (fun ex -> {Message = ex.Message})
      |> Expect.hasTaskErrorValue {Message = "something went wrong!"}
  ]

[<Tests>]
let map2Tests =
  testList "TaskResult.map2 tests" [
    testCase "map2 with Task(Ok x) Task(Ok y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersSuccess (UserId userId) 
      let createPostResult = createPostSuccess validCreatePostRequest 
      TaskResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasTaskOkValue {NewPostId = PostId newPostId; UserIds = followerIds}
    
    testCase "map2 with Task(Error x) Task(Ok y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersFailure (UserId userId)
      let createPostResult = createPostSuccess validCreatePostRequest 
      TaskResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasTaskErrorValue getFollowersEx
    
    testCase "map2 with Task(Ok x) Task(Error y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersSuccess (UserId userId)
      let createPostResult = createPostFailure validCreatePostRequest
      TaskResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasTaskErrorValue commonEx

    testCase "map2 with Task(Error x) Task(Error y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersFailure (UserId userId) 
      let createPostResult = createPostFailure validCreatePostRequest 
      TaskResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasTaskErrorValue getFollowersEx
  ]


[<Tests>]
let foldResultTests =

  testList "TaskResult.foldResult tests" [
    testCase "foldResult with Task(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> TaskResult.foldResult (fun (PostId id) -> id.ToString()) string
      |> runTaskSync
      |> Expect.same (newPostId.ToString())

    testCase "foldResult with Task(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> TaskResult.foldResult string (fun ex -> ex.Message) 
      |> runTaskSync
      |> Expect.same (commonEx.Message)
  ]


[<Tests>]
let mapErrorTests =
  testList "TaskResult.mapError tests" [
    testCase "mapError with Task(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> TaskResult.mapError id
      |> runTaskSync
      |> flip Expect.isOk "mapError should not map Ok"
    
    testCase "mapError with Task(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> TaskResult.mapError (fun ex -> ex.Message)
      |> Expect.hasTaskErrorValue (commonEx.Message)
  ]

[<Tests>]
let bindTests =
  testList "TaskResult.bind tests" [
    testCase "bind with Task(Ok x)" <| fun _ ->
      allowedToPost sampleUserId
      |> TaskResult.bind (fun isAllowed -> task { 
         if isAllowed then 
          return! createPostSuccess validCreatePostRequest
         else
          return (Error (Exception "not allowed to post"))})
      |> Expect.hasTaskOkValue (PostId newPostId)

    testCase "bind with Task(Error x)" <| fun _ ->
      allowedToPost (UserId (Guid.NewGuid()))
      |> TaskResult.bind (fun isAllowed -> task {return Ok isAllowed})
      |> Expect.hasTaskErrorValue commonEx

    testCase "bind with Task(Ok x) that returns Task (Error x)" <| fun _ ->
      let ex = Exception "not allowed to post"
      allowedToPost sampleUserId
      |> TaskResult.bind (fun _ -> task { 
          return (Error ex)
        })
      |> Expect.hasTaskErrorValue ex
  ]

[<Tests>]
let ignoreTests =
  testList "TaskResult.ignore tests" [
    testCase "ignore with Task(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> TaskResult.ignore
      |> Expect.hasTaskOkValue ()

    testCase "ignore with Task(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> TaskResult.ignore
      |> Expect.hasTaskErrorValue commonEx
  ]

let err = "foobar"
let toTask x = task {return x}

[<Tests>]
let requireTrueTests =
  testList "TaskResult.requireTrue Tests" [
    testCase "requireTrue happy path" <| fun _ ->
      toTask true
      |> TaskResult.requireTrue err 
      |> Expect.hasTaskOkValue ()

    testCase "requireTrue error path" <| fun _ ->
      toTask false
      |> TaskResult.requireTrue err 
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let requireFalseTests =
  testList "TaskResult.requireFalse Tests" [
    testCase "requireFalse happy path" <| fun _ ->
      toTask false
      |> TaskResult.requireFalse err  
      |> Expect.hasTaskOkValue ()

    testCase "requireFalse error path" <| fun _ ->
      toTask true
      |> TaskResult.requireFalse err 
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let requireSomeTests =
  testList "TaskResult.requireSome Tests" [
    testCase "requireSome happy path" <| fun _ ->
      toTask (Some 42) 
      |> TaskResult.requireSome err 
      |> Expect.hasTaskOkValue 42

    testCase "requireSome error path" <| fun _ ->
      toTask None
      |> TaskResult.requireSome err  
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let requireNoneTests =
  testList "TaskResult.requireNone Tests" [
    testCase "requireNone happy path" <| fun _ ->
      toTask None 
      |> TaskResult.requireNone err 
      |> Expect.hasTaskOkValue ()

    testCase "requireNone error path" <| fun _ ->
      toTask (Some 42)
      |> TaskResult.requireNone err  
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let requireEqualToTests =
  testList "TaskResult.requireEqualTo Tests" [
    testCase "requireEqualTo happy path" <| fun _ ->
      toTask 42
      |> TaskResult.requireEqualTo 42 err 
      |> Expect.hasTaskOkValue ()

    testCase "requireEqualTo error path" <| fun _ ->
      toTask 43
      |> TaskResult.requireEqualTo 42 err
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let requireEqualTests =
  testList "TaskResult.requireEqual Tests" [
    testCase "requireEqual happy path" <| fun _ ->
      TaskResult.requireEqual 42 (toTask 42) err 
      |> Expect.hasTaskOkValue ()

    testCase "requireEqual error path" <| fun _ ->
      TaskResult.requireEqual 42 (toTask 43) err
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let requireEmptyTests =
  testList "TaskResult.requireEmpty Tests" [
    testCase "requireEmpty happy path" <| fun _ ->
      toTask []
      |> TaskResult.requireEmpty err
      |> Expect.hasTaskOkValue ()

    testCase "requireEmpty error path" <| fun _ ->
      toTask [42]
      |> TaskResult.requireEmpty err
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let requireNotEmptyTests =
  testList "TaskResult.requireNotEmpty Tests" [
    testCase "requireNotEmpty happy path" <| fun _ ->
      toTask [42]
      |> TaskResult.requireNotEmpty err
      |> Expect.hasTaskOkValue ()

    testCase "requireNotEmpty error path" <| fun _ ->
      toTask []
      |> TaskResult.requireNotEmpty err
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let requireHeadTests =
  testList "TaskResult.requireHead Tests" [
    testCase "requireHead happy path" <| fun _ ->
      toTask [42]
      |> TaskResult.requireHead err
      |> Expect.hasTaskOkValue 42

    testCase "requireHead error path" <| fun _ ->
      toTask []
      |> TaskResult.requireHead err
      |> Expect.hasTaskErrorValue err
  ]

[<Tests>]
let setErrorTests =
  testList "TaskResult.setError Tests" [
    testCase "setError replaces a any error value with a custom error value" <| fun _ ->
      toTask (Error "foo")
      |> TaskResult.setError err 
      |> Expect.hasTaskErrorValue err

    testCase "setError does not change an ok value" <| fun _ ->
      toTask (Ok 42)
      |> TaskResult.setError err 
      |> Expect.hasTaskOkValue 42
  ]

[<Tests>]
let withErrorTests =
  testList "TaskResult.withError Tests" [
    testCase "withError replaces a any error value with a custom error value" <| fun _ ->
      toTask (Error ())
      |> TaskResult.withError err 
      |> Expect.hasTaskErrorValue err

    testCase "withError does not change an ok value" <| fun _ ->
      toTask (Ok 42)
      |> TaskResult.withError err 
      |> Expect.hasTaskOkValue 42
  ]

[<Tests>]
let defaultValueTests = 
  testList "TaskResult.defaultValue Tests" [
    testCase "defaultValue returns the ok value" <| fun _ ->
      let v = TaskResult.defaultValue 43 (toTask (Ok 42))
      Expect.hasTaskValue 42 v

    testCase "defaultValue returns the given value for Error" <| fun _ ->
      let v = TaskResult.defaultValue 43 (toTask (Error err))
      Expect.hasTaskValue 43 v
  ]

[<Tests>]
let defaultWithTests =
  testList "TaskResult.defaultWith Tests" [
    testCase "defaultWith returns the ok value" <| fun _ ->
      let v = TaskResult.defaultWith (fun () -> 43) (toTask (Ok 42))
      Expect.hasTaskValue 42 v

    testCase "defaultValue invoks the given thunk for Error" <| fun _ ->
      let v = TaskResult.defaultWith (fun () -> 42) (toTask (Error err))
      Expect.hasTaskValue 42 v
  ]

[<Tests>]
let ignoreErrorTests =
  testList "TaskResult.ignoreError Tests" [
    testCase "ignoreError returns the unit for ok" <| fun _ ->
      Expect.hasTaskValue () (TaskResult.ignoreError (toTask(Ok ())))

    testCase "ignoreError returns the unit for Error" <| fun _ ->
      Expect.hasTaskValue () (TaskResult.ignoreError (toTask(Error err)))
  ]

[<Tests>]
let teeTests =

  testList "TaskResult.tee Tests" [
    testCase "tee executes the function for ok" <| fun _ ->
      let foo = ref "foo"
      let input = ref 0
      let bar x = 
        input := x
        foo := "bar"
      let result = TaskResult.tee bar (toTask (Ok 42))
      Expect.hasTaskOkValue 42 result
      Expect.equal !foo "bar" ""
      Expect.equal !input 42 ""

    testCase "tee ignores the function for Error" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = TaskResult.tee bar (toTask (Error err))
      Expect.hasTaskErrorValue err result
      Expect.equal !foo "foo" ""
  ]

let returnTrue _ = true
let returnFalse _ = false

[<Tests>]
let teeIfTests =
  testList "TaskResult.teeIf Tests" [
    testCase "teeIf executes the function for ok and true predicate " <| fun _ ->
      let foo = ref "foo"
      let input = ref 0
      let pInput = ref 0
      let returnTrue x = 
        pInput := x
        true
      let bar x = 
        input := x
        foo := "bar"
      let result = TaskResult.teeIf returnTrue bar (toTask (Ok 42))
      Expect.hasTaskOkValue 42 result
      Expect.equal !foo "bar" ""
      Expect.equal !input 42 ""
      Expect.equal !pInput 42 ""

    testCase "teeIf ignores the function for Ok and false predicate" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = TaskResult.teeIf returnFalse bar (toTask (Ok 42))
      Expect.hasTaskOkValue 42 result
      Expect.equal !foo "foo" ""

    testCase "teeIf ignores the function for Error" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = TaskResult.teeIf returnTrue bar (toTask (Error err))
      Expect.hasTaskErrorValue err result
      Expect.equal !foo "foo" ""
  ]

[<Tests>]
let teeErrorTests =

  testList "TaskResult.teeError Tests" [
    testCase "teeError executes the function for Error" <| fun _ ->
      let foo = ref "foo"
      let input = ref ""
      let bar x = 
        input := x
        foo := "bar"
      let result = TaskResult.teeError bar (toTask (Error err))
      Expect.hasTaskErrorValue err result
      Expect.equal !foo "bar" ""
      Expect.equal !input err ""

    testCase "teeError ignores the function for Ok" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = TaskResult.teeError bar (toTask (Ok 42))
      Expect.hasTaskOkValue 42 result
      Expect.equal !foo "foo" ""
  ]


[<Tests>]
let teeErrorIfTests =
  testList "TaskResult.teeErrorIf Tests" [
    testCase "teeErrorIf executes the function for Error and true predicate " <| fun _ ->
      let foo = ref "foo"
      let input = ref ""
      let pInput = ref ""
      let returnTrue x = 
        pInput := x
        true
      let bar x = 
        input := x
        foo := "bar"
      let result = TaskResult.teeErrorIf returnTrue bar (toTask (Error err))
      Expect.hasTaskErrorValue err result
      Expect.equal !foo "bar" ""
      Expect.equal !input err ""
      Expect.equal !pInput err ""

    testCase "teeErrorIf ignores the function for Error and false predicate" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = TaskResult.teeErrorIf returnFalse bar (toTask (Error err))
      Expect.hasTaskErrorValue err result
      Expect.equal !foo "foo" ""

    testCase "teeErrorIf ignores the function for Ok" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = TaskResult.teeErrorIf returnTrue bar (toTask (Ok 42))
      Expect.hasTaskOkValue 42 result
      Expect.equal !foo "foo" ""
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
      return PostSuccess {NewPostId = postId; UserIds = followerIds}
    else
      return NotAllowedToPost
  }
  testList "TaskResult Computation Expression tests" [
    testCase "bind with all Ok" <| fun _ ->
      createPost sampleUserId
      |> Expect.hasTaskOkValue (PostSuccess {NewPostId = PostId newPostId; UserIds = followerIds})

    testCase "bind with an Error" <| fun _ ->
      createPost (UserId (System.Guid.NewGuid()))
      |> Expect.hasTaskErrorValue commonEx
  ]

[<Tests>]
let TaskResultOperatorTests =
  testList "TaskResult Operators Tests" [
    testCase "map & apply operators" <| fun _ ->
      let getFollowersResult = getFollowersSuccess sampleUserId
      let createPostResult = createPostSuccess validCreatePostRequest
      newPostRequest <!> getFollowersResult <*> createPostResult
      |> Expect.hasTaskOkValue {NewPostId = PostId newPostId; UserIds = followerIds}
    
    testCase "bind operator" <| fun _ ->
      allowedToPost sampleUserId
      >>= (fun isAllowed -> 
            if isAllowed then 
              createPostSuccess validCreatePostRequest
            else
              TaskResult.returnError (Exception ""))
      |> Expect.hasTaskOkValue (PostId newPostId)
  ]