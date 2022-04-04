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

let runJobSync = run
let createPostSuccess = createPostSuccess >> Job.fromAsync
let createPostFailure = createPostFailure >> Job.fromAsync
let getFollowersSuccess = getFollowersSuccess >> Job.fromAsync
let getFollowersFailure = getFollowersFailure >> Job.fromAsync
let allowedToPost = allowedToPost >> Job.fromAsync

[<Tests>]
let mapTests =
    testList
        "JobResult.map tests"
        [ testCase "map with Task(Ok x)"
          <| fun _ ->
              createPostSuccess validCreatePostRequest
              |> JobResult.map (fun (PostId id) -> { Id = id })
              |> Expect.hasJobOkValueSync { Id = newPostId }

          testCase "map with Task(Error x)"
          <| fun _ ->
              createPostFailure validCreatePostRequest
              |> JobResult.mapError (fun ex -> { Message = ex.Message })
              |> Expect.hasJobErrorValueSync { Message = "something went wrong!" } ]

[<Tests>]
let map2Tests =
    testList
        "JobResult.map2 tests"
        [ testCase "map2 with Task(Ok x) Task(Ok y)"
          <| fun _ ->
              let userId = Guid.NewGuid()
              let getFollowersResult = getFollowersSuccess (UserId userId)
              let createPostResult = createPostSuccess validCreatePostRequest

              JobResult.map2 newPostRequest getFollowersResult createPostResult
              |> Expect.hasJobOkValueSync
                  { NewPostId = PostId newPostId
                    UserIds = followerIds }

          testCase "map2 with Task(Error x) Task(Ok y)"
          <| fun _ ->
              let userId = Guid.NewGuid()
              let getFollowersResult = getFollowersFailure (UserId userId)
              let createPostResult = createPostSuccess validCreatePostRequest

              JobResult.map2 newPostRequest getFollowersResult createPostResult
              |> Expect.hasJobErrorValueSync getFollowersEx

          testCase "map2 with Task(Ok x) Task(Error y)"
          <| fun _ ->
              let userId = Guid.NewGuid()
              let getFollowersResult = getFollowersSuccess (UserId userId)
              let createPostResult = createPostFailure validCreatePostRequest

              JobResult.map2 newPostRequest getFollowersResult createPostResult
              |> Expect.hasJobErrorValueSync commonEx

          testCase "map2 with Task(Error x) Task(Error y)"
          <| fun _ ->
              let userId = Guid.NewGuid()
              let getFollowersResult = getFollowersFailure (UserId userId)
              let createPostResult = createPostFailure validCreatePostRequest

              JobResult.map2 newPostRequest getFollowersResult createPostResult
              |> Expect.hasJobErrorValueSync getFollowersEx ]


[<Tests>]
let foldResultTests =

    testList
        "JobResult.foldResult tests"
        [ testCase "foldResult with Task(Ok x)"
          <| fun _ ->
              createPostSuccess validCreatePostRequest
              |> JobResult.foldResult (fun (PostId id) -> id.ToString()) string
              |> runJobSync
              |> Expect.same (newPostId.ToString())

          testCase "foldResult with Task(Error x)"
          <| fun _ ->
              createPostFailure validCreatePostRequest
              |> JobResult.foldResult string (fun ex -> ex.Message)
              |> runJobSync
              |> Expect.same (commonEx.Message) ]


[<Tests>]
let mapErrorTests =
    testList
        "JobResult.mapError tests"
        [ testCase "mapError with Task(Ok x)"
          <| fun _ ->
              createPostSuccess validCreatePostRequest
              |> JobResult.mapError id
              |> runJobSync
              |> flip Expect.isOk "mapError should not map Ok"

          testCase "mapError with Task(Error x)"
          <| fun _ ->
              createPostFailure validCreatePostRequest
              |> JobResult.mapError (fun ex -> ex.Message)
              |> Expect.hasJobErrorValueSync (commonEx.Message) ]

[<Tests>]
let bindTests =
    testList
        "JobResult.bind tests"
        [ testCase "bind with Task(Ok x)"
          <| fun _ ->
              allowedToPost sampleUserId
              |> JobResult.bind
                  (fun isAllowed ->
                      job {
                          if isAllowed then
                              return! createPostSuccess validCreatePostRequest
                          else
                              return (Error(Exception "not allowed to post"))
                      })
              |> Expect.hasJobOkValueSync (PostId newPostId)

          testCase "bind with Task(Error x)"
          <| fun _ ->
              allowedToPost (UserId(Guid.NewGuid()))
              |> JobResult.bind (fun isAllowed -> job { return Ok isAllowed })
              |> Expect.hasJobErrorValueSync commonEx

          testCase "bind with Task(Ok x) that returns Task (Error x)"
          <| fun _ ->
              let ex = Exception "not allowed to post"

              allowedToPost sampleUserId
              |> JobResult.bind (fun _ -> job { return (Error ex) })
              |> Expect.hasJobErrorValueSync ex ]


[<Tests>]
let orElseTests =
    testList
        "JobResult.orElseWith Tests"
        [ testCaseJob "Ok Ok takes first Ok"
          <| job {
              return!
                  JobResult.ok "First"
                  |> JobResult.orElse (JobResult.ok "Second")
                  |> Expect.hasJobOkValue "First"
             }
          testCaseJob "Ok Error takes first Ok"
          <| job {
              return!
                  JobResult.ok "First"
                  |> JobResult.orElse (JobResult.error "Second")
                  |> Expect.hasJobOkValue "First"
             }
          testCaseJob "Error Ok takes second Ok"
          <| job {
              return!
                  JobResult.error "First"
                  |> JobResult.orElse (JobResult.ok "Second")
                  |> Expect.hasJobOkValue "Second"
             }
          testCaseJob "Error Error takes second error"
          <| job {
              return!
                  JobResult.error "First"
                  |> JobResult.orElse (JobResult.error "Second")
                  |> Expect.hasJobErrorValue "Second"
             } ]

[<Tests>]
let orElseWithTests =
    testList
        "JobResult.orElse Tests"
        [ testCaseJob "Ok Ok takes first Ok"
          <| job {
              return!
                  JobResult.ok "First"
                  |> JobResult.orElseWith (fun _ -> JobResult.ok "Second")
                  |> Expect.hasJobOkValue "First"
             }
          testCaseJob "Ok Error takes first Ok"
          <| job {
              return!
                  JobResult.ok "First"
                  |> JobResult.orElseWith (fun _ -> JobResult.error "Second")
                  |> Expect.hasJobOkValue "First"
             }
          testCaseJob "Error Ok takes second Ok"
          <| job {
              return!
                  JobResult.error "First"
                  |> JobResult.orElseWith (fun _ -> JobResult.ok "Second")
                  |> Expect.hasJobOkValue "Second"
             }
          testCaseJob "Error Error takes second error"
          <| job {
              return!
                  JobResult.error "First"
                  |> JobResult.orElseWith (fun _ -> JobResult.error "Second")
                  |> Expect.hasJobErrorValue "Second"
             } ]



[<Tests>]
let ignoreTests =
    testList
        "JobResult.ignore tests"
        [ testCase "ignore with Task(Ok x)"
          <| fun _ ->
              createPostSuccess validCreatePostRequest
              |> JobResult.ignore
              |> Expect.hasJobOkValueSync ()

          testCase "ignore with Task(Error x)"
          <| fun _ ->
              createPostFailure validCreatePostRequest
              |> JobResult.ignore
              |> Expect.hasJobErrorValueSync commonEx

          testCase "can call ignore without type parameters"
          <| fun _ ->
              ignore JobResult.ignore

          testCase "can call ignore with type parameters"
          <| fun _ ->
              ignore<Job<Result<int, string>> -> Job<Result<unit, string>>> JobResult.ignore<int, string> ]

let err = "foobar"
let toJob = Job.singleton

[<Tests>]
let requireTrueTests =
    testList
        "JobResult.requireTrue Tests"
        [ testCase "requireTrue happy path"
          <| fun _ ->
              toJob true
              |> JobResult.requireTrue err
              |> Expect.hasJobOkValueSync ()

          testCase "requireTrue error path"
          <| fun _ ->
              toJob false
              |> JobResult.requireTrue err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let requireFalseTests =
    testList
        "JobResult.requireFalse Tests"
        [ testCase "requireFalse happy path"
          <| fun _ ->
              toJob false
              |> JobResult.requireFalse err
              |> Expect.hasJobOkValueSync ()

          testCase "requireFalse error path"
          <| fun _ ->
              toJob true
              |> JobResult.requireFalse err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let requireSomeTests =
    testList
        "JobResult.requireSome Tests"
        [ testCase "requireSome happy path"
          <| fun _ ->
              toJob (Some 42)
              |> JobResult.requireSome err
              |> Expect.hasJobOkValueSync 42

          testCase "requireSome error path"
          <| fun _ ->
              toJob None
              |> JobResult.requireSome err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let requireNoneTests =
    testList
        "JobResult.requireNone Tests"
        [ testCase "requireNone happy path"
          <| fun _ ->
              toJob None
              |> JobResult.requireNone err
              |> Expect.hasJobOkValueSync ()

          testCase "requireNone error path"
          <| fun _ ->
              toJob (Some 42)
              |> JobResult.requireNone err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let requireEqualToTests =
    testList
        "JobResult.requireEqualTo Tests"
        [ testCase "requireEqualTo happy path"
          <| fun _ ->
              toJob 42
              |> JobResult.requireEqualTo 42 err
              |> Expect.hasJobOkValueSync ()

          testCase "requireEqualTo error path"
          <| fun _ ->
              toJob 43
              |> JobResult.requireEqualTo 42 err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let requireEqualTests =
    testList
        "JobResult.requireEqual Tests"
        [ testCase "requireEqual happy path"
          <| fun _ ->
              JobResult.requireEqual 42 (toJob 42) err
              |> Expect.hasJobOkValueSync ()

          testCase "requireEqual error path"
          <| fun _ ->
              JobResult.requireEqual 42 (toJob 43) err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let requireEmptyTests =
    testList
        "JobResult.requireEmpty Tests"
        [ testCase "requireEmpty happy path"
          <| fun _ ->
              toJob []
              |> JobResult.requireEmpty err
              |> Expect.hasJobOkValueSync ()

          testCase "requireEmpty error path"
          <| fun _ ->
              toJob [ 42 ]
              |> JobResult.requireEmpty err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let requireNotEmptyTests =
    testList
        "JobResult.requireNotEmpty Tests"
        [ testCase "requireNotEmpty happy path"
          <| fun _ ->
              toJob [ 42 ]
              |> JobResult.requireNotEmpty err
              |> Expect.hasJobOkValueSync ()

          testCase "requireNotEmpty error path"
          <| fun _ ->
              toJob []
              |> JobResult.requireNotEmpty err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let requireHeadTests =
    testList
        "JobResult.requireHead Tests"
        [ testCase "requireHead happy path"
          <| fun _ ->
              toJob [ 42 ]
              |> JobResult.requireHead err
              |> Expect.hasJobOkValueSync 42

          testCase "requireHead error path"
          <| fun _ ->
              toJob []
              |> JobResult.requireHead err
              |> Expect.hasJobErrorValueSync err ]

[<Tests>]
let setErrorTests =
    testList
        "JobResult.setError Tests"
        [ testCase "setError replaces a any error value with a custom error value"
          <| fun _ ->
              toJob (Error "foo")
              |> JobResult.setError err
              |> Expect.hasJobErrorValueSync err

          testCase "setError does not change an ok value"
          <| fun _ ->
              toJob (Ok 42)
              |> JobResult.setError err
              |> Expect.hasJobOkValueSync 42 ]

[<Tests>]
let withErrorTests =
    testList
        "JobResult.withError Tests"
        [ testCase "withError replaces a any error value with a custom error value"
          <| fun _ ->
              toJob (Error())
              |> JobResult.withError err
              |> Expect.hasJobErrorValueSync err

          testCase "withError does not change an ok value"
          <| fun _ ->
              toJob (Ok 42)
              |> JobResult.withError err
              |> Expect.hasJobOkValueSync 42 ]

[<Tests>]
let defaultValueTests =
    testList
        "JobResult.defaultValue Tests"
        [ testCase "defaultValue returns the ok value"
          <| fun _ ->
              let v =
                  JobResult.defaultValue 43 (toJob (Ok 42))

              Expect.hasJobValue 42 v

          testCase "defaultValue returns the given value for Error"
          <| fun _ ->
              let v =
                  JobResult.defaultValue 43 (toJob (Error err))

              Expect.hasJobValue 43 v ]

[<Tests>]
let defaultErrorTests =
    testList
        "JobResult.defaultError Tests"
        [ testCase "defaultError returns the error value"
          <| fun _ ->
              let v =
                  JobResult.defaultError 43 (toJob (Error 42))

              Expect.hasJobValue 42 v

          testCase "defaultError returns the given value for Ok"
          <| fun _ ->
              let v =
                  JobResult.defaultError 43 (toJob (Ok 42))

              Expect.hasJobValue 43 v ]

[<Tests>]
let defaultWithTests =
    testList
        "JobResult.defaultWith Tests"
        [ testCase "defaultWith returns the ok value"
          <| fun _ ->
              let v =
                  JobResult.defaultWith (fun () -> 43) (toJob (Ok 42))

              Expect.hasJobValue 42 v

          testCase "defaultValue invoks the given thunk for Error"
          <| fun _ ->
              let v =
                  JobResult.defaultWith (fun () -> 42) (toJob (Error err))

              Expect.hasJobValue 42 v ]

[<Tests>]
let ignoreErrorTests =
    testList
        "JobResult.ignoreError Tests"
        [ testCase "ignoreError returns the unit for ok"
          <| fun _ -> Expect.hasJobValue () (JobResult.ignoreError (toJob (Ok())))

          testCase "ignoreError returns the unit for Error"
          <| fun _ -> Expect.hasJobValue () (JobResult.ignoreError (toJob (Error err)))

          testCase "Can call ignoreError without type parameter"
          <| fun _ -> ignore JobResult.ignoreError

          testCase "Can call ignoreError with type parameter"
          <| fun _ -> ignore<Job<Result<unit, string>> -> Job<unit>> JobResult.ignoreError<string> ]

[<Tests>]
let teeTests =

    testList
        "JobResult.tee Tests"
        [ testCase "tee executes the function for ok"
          <| fun _ ->
              let foo = ref "foo"
              let input = ref 0

              let bar x =
                  input := x
                  foo := "bar"

              let result = JobResult.tee bar (toJob (Ok 42))
              Expect.hasJobOkValueSync 42 result
              Expect.equal !foo "bar" ""
              Expect.equal !input 42 ""

          testCase "tee ignores the function for Error"
          <| fun _ ->
              let foo = ref "foo"
              let bar _ = foo := "bar"
              let result = JobResult.tee bar (toJob (Error err))
              Expect.hasJobErrorValueSync err result
              Expect.equal !foo "foo" "" ]

let returnTrue _ = true
let returnFalse _ = false

[<Tests>]
let teeIfTests =
    testList
        "JobResult.teeIf Tests"
        [ testCase "teeIf executes the function for ok and true predicate "
          <| fun _ ->
              let foo = ref "foo"
              let input = ref 0
              let pInput = ref 0

              let returnTrue x =
                  pInput := x
                  true

              let bar x =
                  input := x
                  foo := "bar"

              let result =
                  JobResult.teeIf returnTrue bar (toJob (Ok 42))

              Expect.hasJobOkValueSync 42 result
              Expect.equal !foo "bar" ""
              Expect.equal !input 42 ""
              Expect.equal !pInput 42 ""

          testCase "teeIf ignores the function for Ok and false predicate"
          <| fun _ ->
              let foo = ref "foo"
              let bar _ = foo := "bar"

              let result =
                  JobResult.teeIf returnFalse bar (toJob (Ok 42))

              Expect.hasJobOkValueSync 42 result
              Expect.equal !foo "foo" ""

          testCase "teeIf ignores the function for Error"
          <| fun _ ->
              let foo = ref "foo"
              let bar _ = foo := "bar"

              let result =
                  JobResult.teeIf returnTrue bar (toJob (Error err))

              Expect.hasJobErrorValueSync err result
              Expect.equal !foo "foo" "" ]

[<Tests>]
let teeErrorTests =

    testList
        "JobResult.teeError Tests"
        [ testCase "teeError executes the function for Error"
          <| fun _ ->
              let foo = ref "foo"
              let input = ref ""

              let bar x =
                  input := x
                  foo := "bar"

              let result =
                  JobResult.teeError bar (toJob (Error err))

              Expect.hasJobErrorValueSync err result
              Expect.equal !foo "bar" ""
              Expect.equal !input err ""

          testCase "teeError ignores the function for Ok"
          <| fun _ ->
              let foo = ref "foo"
              let bar _ = foo := "bar"
              let result = JobResult.teeError bar (toJob (Ok 42))
              Expect.hasJobOkValueSync 42 result
              Expect.equal !foo "foo" "" ]


[<Tests>]
let teeErrorIfTests =
    testList
        "JobResult.teeErrorIf Tests"
        [ testCase "teeErrorIf executes the function for Error and true predicate "
          <| fun _ ->
              let foo = ref "foo"
              let input = ref ""
              let pInput = ref ""

              let returnTrue x =
                  pInput := x
                  true

              let bar x =
                  input := x
                  foo := "bar"

              let result =
                  JobResult.teeErrorIf returnTrue bar (toJob (Error err))

              Expect.hasJobErrorValueSync err result
              Expect.equal !foo "bar" ""
              Expect.equal !input err ""
              Expect.equal !pInput err ""

          testCase "teeErrorIf ignores the function for Error and false predicate"
          <| fun _ ->
              let foo = ref "foo"
              let bar _ = foo := "bar"

              let result =
                  JobResult.teeErrorIf returnFalse bar (toJob (Error err))

              Expect.hasJobErrorValueSync err result
              Expect.equal !foo "foo" ""

          testCase "teeErrorIf ignores the function for Ok"
          <| fun _ ->
              let foo = ref "foo"
              let bar _ = foo := "bar"

              let result =
                  JobResult.teeErrorIf returnTrue bar (toJob (Ok 42))

              Expect.hasJobOkValueSync 42 result
              Expect.equal !foo "foo" "" ]

[<Tests>]
let catchTests =
    let f (e: exn) = e.Message

    let jobThrow () =
        job {
            failwith err
            return Error ""
        }

    testList
        "JobResult.catch tests"
        [ testCase "catch returns success for Ok"
          <| fun _ -> Expect.hasJobOkValueSync 42 (JobResult.catch f (toJob (Ok 42)))

          testCase "catch returns mapped Error for exception"
          <| fun _ -> Expect.hasJobErrorValueSync err (JobResult.catch f (jobThrow ()))

          testCase "catch returns unmapped error without exception"
          <| fun _ -> Expect.hasJobErrorValueSync "unmapped" (JobResult.catch f (toJob (Error "unmapped"))) ]


[<Tests>]
let zipTests =
    testList
        "JobResult.zip tests"
        [ testCase "Ok, Ok"
          <| fun _ ->
              let v =
                  JobResult.zip (toJob (Ok 1)) (toJob (Ok 2))

              Expect.hasJobValue (Ok(1, 2)) v

          testCase "Ok, Error"
          <| fun _ ->
              let v =
                  JobResult.zip (toJob (Ok 1)) (toJob (Error "Bad"))

              Expect.hasJobValue (Error("Bad")) v

          testCase "Error, Ok"
          <| fun _ ->
              let v =
                  JobResult.zip (toJob (Error "Bad")) (toJob (Ok 1))

              Expect.hasJobValue (Error("Bad")) v

          testCase "Error, Error"
          <| fun _ ->
              let v =
                  JobResult.zip (toJob (Error "Bad1")) (toJob (Error "Bad2"))

              Expect.hasJobValue (Error("Bad1")) v ]

[<Tests>]
let zipErrorTests =
    testList
        "JobResult.zipError tests"
        [ testCase "Ok, Ok"
          <| fun _ ->
              let v =
                  JobResult.zipError (toJob (Ok 1)) (toJob (Ok 2))

              Expect.hasJobValue (Ok(1)) v

          testCase "Ok, Error"
          <| fun _ ->
              let v =
                  JobResult.zipError (toJob (Ok 1)) (toJob (Error "Bad"))

              Expect.hasJobValue (Ok 1) v

          testCase "Error, Ok"
          <| fun _ ->
              let v =
                  JobResult.zipError (toJob (Error "Bad")) (toJob (Ok 1))

              Expect.hasJobValue (Ok 1) v

          testCase "Error, Error"
          <| fun _ ->
              let v =
                  JobResult.zipError (toJob (Error "Bad1")) (toJob (Error "Bad2"))

              Expect.hasJobValue (Error("Bad1", "Bad2")) v ]


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
//       |> Expect.hasJobOkValueSync (PostSuccess {NewPostId = PostId newPostId; UserIds = followerIds})

//     testCase "bind with an Error" <| fun _ ->
//       createPost (UserId (System.Guid.NewGuid()))
//       |> Expect.hasJobErrorValueSync commonEx
//   ]

// [<Tests>]
// let JobResultOperatorTests =
//   testList "JobResult Operators Tests" [
//     testCase "map & apply operators" <| fun _ ->
//       let getFollowersResult = getFollowersSuccess sampleUserId
//       let createPostResult = createPostSuccess validCreatePostRequest
//       newPostRequest <!> getFollowersResult <*> createPostResult
//       |> Expect.hasJobOkValueSync {NewPostId = PostId newPostId; UserIds = followerIds}

//     testCase "bind operator" <| fun _ ->
//       allowedToPost sampleUserId
//       >>= (fun isAllowed ->
//             if isAllowed then
//               createPostSuccess validCreatePostRequest
//             else
//               JobResult.returnError (Exception ""))
//       |> Expect.hasJobOkValueSync (PostId newPostId)
//   ]
