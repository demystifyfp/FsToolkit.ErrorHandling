module JobResultTests


open Expecto
open Expects.JobResult
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
// open FsToolkit.ErrorHandling.Operator.JobResult
open System
open Hopac

let runJobSync =
    run
let createPostSuccess = createPostSuccess >> Job.fromAsync
let createPostFailure = createPostFailure>> Job.fromAsync
let getFollowersSuccess = getFollowersSuccess>> Job.fromAsync
let getFollowersFailure = getFollowersFailure>> Job.fromAsync
let allowedToPost = allowedToPost>> Job.fromAsync

[<Tests>]
let mapTests =
  testList "JobResult.map tests" [
    testCase "map with Task(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> JobResult.map (fun (PostId id) -> {Id = id})
      |> Expect.hasJobOkValue {Id = newPostId}

    testCase "map with Task(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> JobResult.mapError (fun ex -> {Message = ex.Message})
      |> Expect.hasJobErrorValue {Message = "something went wrong!"}
  ]

[<Tests>]
let map2Tests =
  testList "JobResult.map2 tests" [
    testCase "map2 with Task(Ok x) Task(Ok y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersSuccess (UserId userId) 
      let createPostResult = createPostSuccess validCreatePostRequest 
      JobResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasJobOkValue {NewPostId = PostId newPostId; UserIds = followerIds}
    
    testCase "map2 with Task(Error x) Task(Ok y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersFailure (UserId userId)
      let createPostResult = createPostSuccess validCreatePostRequest 
      JobResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasJobErrorValue getFollowersEx
    
    testCase "map2 with Task(Ok x) Task(Error y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersSuccess (UserId userId)
      let createPostResult = createPostFailure validCreatePostRequest
      JobResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasJobErrorValue commonEx

    testCase "map2 with Task(Error x) Task(Error y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersFailure (UserId userId) 
      let createPostResult = createPostFailure validCreatePostRequest 
      JobResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasJobErrorValue getFollowersEx
  ]


[<Tests>]
let foldResultTests =

  testList "JobResult.foldResult tests" [
    testCase "foldResult with Task(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> JobResult.foldResult (fun (PostId id) -> id.ToString()) string
      |> runJobSync
      |> Expect.same (newPostId.ToString())

    testCase "foldResult with Task(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> JobResult.foldResult string (fun ex -> ex.Message) 
      |> runJobSync
      |> Expect.same (commonEx.Message)
  ]


[<Tests>]
let mapErrorTests =
  testList "JobResult.mapError tests" [
    testCase "mapError with Task(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> JobResult.mapError id
      |> runJobSync
      |> flip Expect.isOk "mapError should not map Ok"
    
    testCase "mapError with Task(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> JobResult.mapError (fun ex -> ex.Message)
      |> Expect.hasJobErrorValue (commonEx.Message)
  ]

[<Tests>]
let bindTests =
  testList "JobResult.bind tests" [
    testCase "bind with Task(Ok x)" <| fun _ ->
      allowedToPost sampleUserId
      |> JobResult.bind (fun isAllowed -> job { 
         if isAllowed then 
          return! createPostSuccess validCreatePostRequest
         else
          return (Error (Exception "not allowed to post"))})
      |> Expect.hasJobOkValue (PostId newPostId)

    testCase "bind with Task(Error x)" <| fun _ ->
      allowedToPost (UserId (Guid.NewGuid()))
      |> JobResult.bind (fun isAllowed -> job {return Ok isAllowed})
      |> Expect.hasJobErrorValue commonEx

    testCase "bind with Task(Ok x) that returns Task (Error x)" <| fun _ ->
      let ex = Exception "not allowed to post"
      allowedToPost sampleUserId
      |> JobResult.bind (fun _ -> job { 
          return (Error ex)
        })
      |> Expect.hasJobErrorValue ex
  ]

[<Tests>]
let ignoreTests =
  testList "JobResult.ignore tests" [
    testCase "ignore with Task(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> JobResult.ignore
      |> Expect.hasJobOkValue ()

    testCase "ignore with Task(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> JobResult.ignore
      |> Expect.hasJobErrorValue commonEx
  ]

let err = "foobar"
let toJob = Job.singleton

[<Tests>]
let requireTrueTests =
  testList "JobResult.requireTrue Tests" [
    testCase "requireTrue happy path" <| fun _ ->
      toJob true
      |> JobResult.requireTrue err 
      |> Expect.hasJobOkValue ()

    testCase "requireTrue error path" <| fun _ ->
      toJob false
      |> JobResult.requireTrue err 
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let requireFalseTests =
  testList "JobResult.requireFalse Tests" [
    testCase "requireFalse happy path" <| fun _ ->
      toJob false
      |> JobResult.requireFalse err  
      |> Expect.hasJobOkValue ()

    testCase "requireFalse error path" <| fun _ ->
      toJob true
      |> JobResult.requireFalse err 
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let requireSomeTests =
  testList "JobResult.requireSome Tests" [
    testCase "requireSome happy path" <| fun _ ->
      toJob (Some 42) 
      |> JobResult.requireSome err 
      |> Expect.hasJobOkValue 42

    testCase "requireSome error path" <| fun _ ->
      toJob None
      |> JobResult.requireSome err  
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let requireNoneTests =
  testList "JobResult.requireNone Tests" [
    testCase "requireNone happy path" <| fun _ ->
      toJob None 
      |> JobResult.requireNone err 
      |> Expect.hasJobOkValue ()

    testCase "requireNone error path" <| fun _ ->
      toJob (Some 42)
      |> JobResult.requireNone err  
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let requireEqualToTests =
  testList "JobResult.requireEqualTo Tests" [
    testCase "requireEqualTo happy path" <| fun _ ->
      toJob 42
      |> JobResult.requireEqualTo 42 err 
      |> Expect.hasJobOkValue ()

    testCase "requireEqualTo error path" <| fun _ ->
      toJob 43
      |> JobResult.requireEqualTo 42 err
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let requireEqualTests =
  testList "JobResult.requireEqual Tests" [
    testCase "requireEqual happy path" <| fun _ ->
      JobResult.requireEqual 42 (toJob 42) err 
      |> Expect.hasJobOkValue ()

    testCase "requireEqual error path" <| fun _ ->
      JobResult.requireEqual 42 (toJob 43) err
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let requireEmptyTests =
  testList "JobResult.requireEmpty Tests" [
    testCase "requireEmpty happy path" <| fun _ ->
      toJob []
      |> JobResult.requireEmpty err
      |> Expect.hasJobOkValue ()

    testCase "requireEmpty error path" <| fun _ ->
      toJob [42]
      |> JobResult.requireEmpty err
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let requireNotEmptyTests =
  testList "JobResult.requireNotEmpty Tests" [
    testCase "requireNotEmpty happy path" <| fun _ ->
      toJob [42]
      |> JobResult.requireNotEmpty err
      |> Expect.hasJobOkValue ()

    testCase "requireNotEmpty error path" <| fun _ ->
      toJob []
      |> JobResult.requireNotEmpty err
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let requireHeadTests =
  testList "JobResult.requireHead Tests" [
    testCase "requireHead happy path" <| fun _ ->
      toJob [42]
      |> JobResult.requireHead err
      |> Expect.hasJobOkValue 42

    testCase "requireHead error path" <| fun _ ->
      toJob []
      |> JobResult.requireHead err
      |> Expect.hasJobErrorValue err
  ]

[<Tests>]
let setErrorTests =
  testList "JobResult.setError Tests" [
    testCase "setError replaces a any error value with a custom error value" <| fun _ ->
      toJob (Error "foo")
      |> JobResult.setError err 
      |> Expect.hasJobErrorValue err

    testCase "setError does not change an ok value" <| fun _ ->
      toJob (Ok 42)
      |> JobResult.setError err 
      |> Expect.hasJobOkValue 42
  ]

[<Tests>]
let withErrorTests =
  testList "JobResult.withError Tests" [
    testCase "withError replaces a any error value with a custom error value" <| fun _ ->
      toJob (Error ())
      |> JobResult.withError err 
      |> Expect.hasJobErrorValue err

    testCase "withError does not change an ok value" <| fun _ ->
      toJob (Ok 42)
      |> JobResult.withError err 
      |> Expect.hasJobOkValue 42
  ]

[<Tests>]
let defaultValueTests = 
  testList "JobResult.defaultValue Tests" [
    testCase "defaultValue returns the ok value" <| fun _ ->
      let v = JobResult.defaultValue 43 (toJob (Ok 42))
      Expect.hasJobValue 42 v

    testCase "defaultValue returns the given value for Error" <| fun _ ->
      let v = JobResult.defaultValue 43 (toJob (Error err))
      Expect.hasJobValue 43 v
  ]

[<Tests>]
let defaultWithTests =
  testList "JobResult.defaultWith Tests" [
    testCase "defaultWith returns the ok value" <| fun _ ->
      let v = JobResult.defaultWith (fun () -> 43) (toJob (Ok 42))
      Expect.hasJobValue 42 v

    testCase "defaultValue invoks the given thunk for Error" <| fun _ ->
      let v = JobResult.defaultWith (fun () -> 42) (toJob (Error err))
      Expect.hasJobValue 42 v
  ]

[<Tests>]
let ignoreErrorTests =
  testList "JobResult.ignoreError Tests" [
    testCase "ignoreError returns the unit for ok" <| fun _ ->
      Expect.hasJobValue () (JobResult.ignoreError (toJob(Ok ())))

    testCase "ignoreError returns the unit for Error" <| fun _ ->
      Expect.hasJobValue () (JobResult.ignoreError (toJob(Error err)))
  ]

[<Tests>]
let teeTests =

  testList "JobResult.tee Tests" [
    testCase "tee executes the function for ok" <| fun _ ->
      let foo = ref "foo"
      let input = ref 0
      let bar x = 
        input := x
        foo := "bar"
      let result = JobResult.tee bar (toJob (Ok 42))
      Expect.hasJobOkValue 42 result
      Expect.equal !foo "bar" ""
      Expect.equal !input 42 ""

    testCase "tee ignores the function for Error" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = JobResult.tee bar (toJob (Error err))
      Expect.hasJobErrorValue err result
      Expect.equal !foo "foo" ""
  ]

let returnTrue _ = true
let returnFalse _ = false

[<Tests>]
let teeIfTests =
  testList "JobResult.teeIf Tests" [
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
      let result = JobResult.teeIf returnTrue bar (toJob (Ok 42))
      Expect.hasJobOkValue 42 result
      Expect.equal !foo "bar" ""
      Expect.equal !input 42 ""
      Expect.equal !pInput 42 ""

    testCase "teeIf ignores the function for Ok and false predicate" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = JobResult.teeIf returnFalse bar (toJob (Ok 42))
      Expect.hasJobOkValue 42 result
      Expect.equal !foo "foo" ""

    testCase "teeIf ignores the function for Error" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = JobResult.teeIf returnTrue bar (toJob (Error err))
      Expect.hasJobErrorValue err result
      Expect.equal !foo "foo" ""
  ]

[<Tests>]
let teeErrorTests =

  testList "JobResult.teeError Tests" [
    testCase "teeError executes the function for Error" <| fun _ ->
      let foo = ref "foo"
      let input = ref ""
      let bar x = 
        input := x
        foo := "bar"
      let result = JobResult.teeError bar (toJob (Error err))
      Expect.hasJobErrorValue err result
      Expect.equal !foo "bar" ""
      Expect.equal !input err ""

    testCase "teeError ignores the function for Ok" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = JobResult.teeError bar (toJob (Ok 42))
      Expect.hasJobOkValue 42 result
      Expect.equal !foo "foo" ""
  ]


[<Tests>]
let teeErrorIfTests =
  testList "JobResult.teeErrorIf Tests" [
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
      let result = JobResult.teeErrorIf returnTrue bar (toJob (Error err))
      Expect.hasJobErrorValue err result
      Expect.equal !foo "bar" ""
      Expect.equal !input err ""
      Expect.equal !pInput err ""

    testCase "teeErrorIf ignores the function for Error and false predicate" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = JobResult.teeErrorIf returnFalse bar (toJob (Error err))
      Expect.hasJobErrorValue err result
      Expect.equal !foo "foo" ""

    testCase "teeErrorIf ignores the function for Ok" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = JobResult.teeErrorIf returnTrue bar (toJob (Ok 42))
      Expect.hasJobOkValue 42 result
      Expect.equal !foo "foo" ""
  ]

type CreatePostResult =
| PostSuccess of NotifyNewPostRequest
| NotAllowedToPost

// [<Tests>]
// let JobResultCETests =
//   let createPost userId = taskResult {
//     let! isAllowed = allowedToPost userId
//     if isAllowed then
//       let! postId = createPostSuccess validCreatePostRequest
//       let! followerIds = getFollowersSuccess sampleUserId
//       return PostSuccess {NewPostId = postId; UserIds = followerIds}
//     else
//       return NotAllowedToPost
//   }
//   testList "JobResult Computation Expression tests" [
//     testCase "bind with all Ok" <| fun _ ->
//       createPost sampleUserId
//       |> Expect.hasJobOkValue (PostSuccess {NewPostId = PostId newPostId; UserIds = followerIds})

//     testCase "bind with an Error" <| fun _ ->
//       createPost (UserId (System.Guid.NewGuid()))
//       |> Expect.hasJobErrorValue commonEx
//   ]

// [<Tests>]
// let JobResultOperatorTests =
//   testList "JobResult Operators Tests" [
//     testCase "map & apply operators" <| fun _ ->
//       let getFollowersResult = getFollowersSuccess sampleUserId
//       let createPostResult = createPostSuccess validCreatePostRequest
//       newPostRequest <!> getFollowersResult <*> createPostResult
//       |> Expect.hasJobOkValue {NewPostId = PostId newPostId; UserIds = followerIds}
    
//     testCase "bind operator" <| fun _ ->
//       allowedToPost sampleUserId
//       >>= (fun isAllowed -> 
//             if isAllowed then 
//               createPostSuccess validCreatePostRequest
//             else
//               JobResult.returnError (Exception ""))
//       |> Expect.hasJobOkValue (PostId newPostId)
//   ]