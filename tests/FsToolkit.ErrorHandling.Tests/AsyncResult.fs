module AsyncResultTests


open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.CE.AsyncResult
open FsToolkit.ErrorHandling.Operator.AsyncResult
open System

[<Tests>]
let mapTests =
  testList "AsyncResult.map tests" [
    testCase "map with Async(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> AsyncResult.map (fun (PostId id) -> {Id = id})
      |> Expect.hasAsyncOkValue {Id = newPostId}

    testCase "map with Async(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> AsyncResult.mapError (fun ex -> {Message = ex.Message})
      |> Expect.hasAsyncErrorValue {Message = "something went wrong!"}
  ]

[<Tests>]
let map2Tests =
  testList "AyncResult.map2 tests" [
    testCase "map2 with Async(Ok x) Async(Ok y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersSuccess (UserId userId)
      let createPostResult = createPostSuccess validCreatePostRequest
      AsyncResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasAsyncOkValue {NewPostId = PostId newPostId; UserIds = followerIds}
    
    testCase "map2 with Async(Error x) Async(Ok y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersFailure (UserId userId)
      let createPostResult = createPostSuccess validCreatePostRequest
      AsyncResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasAsyncErrorValue getFollowersEx
    
    testCase "map2 with Async(Ok x) Async(Error y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersSuccess (UserId userId)
      let createPostResult = createPostFailure validCreatePostRequest
      AsyncResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasAsyncErrorValue commonEx

    testCase "map2 with Async(Error x) Async(Error y)" <| fun _ ->
      let userId = Guid.NewGuid()
      let getFollowersResult = getFollowersFailure (UserId userId)
      let createPostResult = createPostFailure validCreatePostRequest
      AsyncResult.map2 newPostRequest getFollowersResult createPostResult 
      |> Expect.hasAsyncErrorValue getFollowersEx
  ]


[<Tests>]
let foldResultTests =

  testList "AsyncResult.foldResult tests" [
    testCase "foldResult with Async(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> AsyncResult.foldResult (fun (PostId id) -> id.ToString()) string
      |> Async.RunSynchronously
      |> Expect.same (newPostId.ToString())

    testCase "foldResult with Async(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> AsyncResult.foldResult string (fun ex -> ex.Message) 
      |> Async.RunSynchronously
      |> Expect.same (commonEx.Message)
  ]


[<Tests>]
let mapErrorTests =
  testList "AsyncResult.mapError tests" [
    testCase "mapError with Async(Ok x)" <| fun _ ->
      createPostSuccess validCreatePostRequest
      |> AsyncResult.mapError id
      |> Async.RunSynchronously
      |> flip Expect.isOk "mapError should not map Ok"
    
    testCase "mapError with Async(Error x)" <| fun _ ->
      createPostFailure validCreatePostRequest
      |> AsyncResult.mapError (fun ex -> ex.Message)
      |> Expect.hasAsyncErrorValue (commonEx.Message)
  ]

[<Tests>]
let bindTests =
  testList "AsyncResult.bind tests" [
    testCase "bind with Async(Ok x)" <| fun _ ->
      allowedToPost sampleUserId
      |> AsyncResult.bind (fun isAllowed -> async { 
         if isAllowed then 
          return! createPostSuccess validCreatePostRequest
         else
          return (Error (Exception "not allowed to post"))})
      |> Expect.hasAsyncOkValue (PostId newPostId)

    testCase "bind with Async(Error x)" <| fun _ ->
      allowedToPost (UserId (Guid.NewGuid()))
      |> AsyncResult.bind (fun isAllowed -> async {return Ok isAllowed})
      |> Expect.hasAsyncErrorValue commonEx

    testCase "bind with Async(Ok x) that returns Async (Error x)" <| fun _ ->
      let ex = Exception "not allowed to post"
      allowedToPost sampleUserId
      |> AsyncResult.bind (fun _ -> async { 
          return (Error ex)
        })
      |> Expect.hasAsyncErrorValue ex
  ]

let err = "foobar"
let toAsync x = async {return x}

[<Tests>]
let requireTrueTests =
  testList "AsyncResult.requireTrue Tests" [
    testCase "requireTrue happy path" <| fun _ ->
      toAsync true
      |> AsyncResult.requireTrue err 
      |> Expect.hasAsyncOkValue ()

    testCase "requireTrue error path" <| fun _ ->
      toAsync false
      |> AsyncResult.requireTrue err 
      |> Expect.hasAsyncErrorValue err
  ]

[<Tests>]
let requireFalseTests =
  testList "AsyncResult.requireFalse Tests" [
    testCase "requireFalse happy path" <| fun _ ->
      toAsync false
      |> AsyncResult.requireFalse err  
      |> Expect.hasAsyncOkValue ()

    testCase "requireFalse error path" <| fun _ ->
      toAsync true
      |> AsyncResult.requireFalse err 
      |> Expect.hasAsyncErrorValue err
  ]

[<Tests>]
let requireSomeTests =
  testList "AsyncResult.requireSome Tests" [
    testCase "requireSome happy path" <| fun _ ->
      toAsync (Some 42) 
      |> AsyncResult.requireSome err 
      |> Expect.hasAsyncOkValue 42

    testCase "requireSome error path" <| fun _ ->
      toAsync None
      |> AsyncResult.requireSome err  
      |> Expect.hasAsyncErrorValue err
  ]

[<Tests>]
let requireNoneTests =
  testList "AsyncResult.requireNone Tests" [
    testCase "requireNone happy path" <| fun _ ->
      toAsync None 
      |> AsyncResult.requireNone err 
      |> Expect.hasAsyncOkValue ()

    testCase "requireNone error path" <| fun _ ->
      toAsync (Some 42)
      |> AsyncResult.requireNone err  
      |> Expect.hasAsyncErrorValue err
  ]

[<Tests>]
let requireEqualsTests =
  testList "AsyncResult.requireEquals Tests" [
    testCase "requireEquals happy path" <| fun _ ->
      toAsync 42
      |> AsyncResult.requireEquals 42 err 
      |> Expect.hasAsyncOkValue ()

    testCase "requireEquals error path" <| fun _ ->
      toAsync 43
      |> AsyncResult.requireEquals 42 err
      |> Expect.hasAsyncErrorValue err
  ]

[<Tests>]
let requireEmptyTests =
  testList "AsyncResult.requireEmpty Tests" [
    testCase "requireEmpty happy path" <| fun _ ->
      toAsync []
      |> AsyncResult.requireEmpty err
      |> Expect.hasAsyncOkValue ()

    testCase "requireEmpty error path" <| fun _ ->
      toAsync [42]
      |> AsyncResult.requireEmpty err
      |> Expect.hasAsyncErrorValue err
  ]

[<Tests>]
let requireNotEmptyTests =
  testList "AsyncResult.requireNotEmpty Tests" [
    testCase "requireNotEmpty happy path" <| fun _ ->
      toAsync [42]
      |> AsyncResult.requireNotEmpty err
      |> Expect.hasAsyncOkValue ()

    testCase "requireNotEmpty error path" <| fun _ ->
      toAsync []
      |> AsyncResult.requireNotEmpty err
      |> Expect.hasAsyncErrorValue err
  ]

[<Tests>]
let requireHeadTests =
  testList "AsyncResult.requireHead Tests" [
    testCase "requireHead happy path" <| fun _ ->
      toAsync [42]
      |> AsyncResult.requireHead err
      |> Expect.hasAsyncOkValue 42

    testCase "requireHead error path" <| fun _ ->
      toAsync []
      |> AsyncResult.requireHead err
      |> Expect.hasAsyncErrorValue err
  ]

[<Tests>]
let setErrorTests =
  testList "AsyncResult.setError Tests" [
    testCase "setError replaces a any error value with a custom error value" <| fun _ ->
      toAsync (Error "foo")
      |> AsyncResult.setError err 
      |> Expect.hasAsyncErrorValue err

    testCase "setError does not change an ok value" <| fun _ ->
      toAsync (Ok 42)
      |> AsyncResult.setError err 
      |> Expect.hasAsyncOkValue 42
  ]

[<Tests>]
let withErrorTests =
  testList "AsyncResult.withError Tests" [
    testCase "withError replaces a any error value with a custom error value" <| fun _ ->
      toAsync (Error ())
      |> AsyncResult.withError err 
      |> Expect.hasAsyncErrorValue err

    testCase "withError does not change an ok value" <| fun _ ->
      toAsync (Ok 42)
      |> AsyncResult.withError err 
      |> Expect.hasAsyncOkValue 42
  ]

[<Tests>]
let defaultValueTests = 
  testList "AsyncResult.defaultValue Tests" [
    testCase "defaultValue returns the ok value" <| fun _ ->
      let v = AsyncResult.defaultValue 43 (toAsync (Ok 42))
      Expect.hasAsyncValue 42 v

    testCase "defaultValue returns the given value for Error" <| fun _ ->
      let v = AsyncResult.defaultValue 43 (toAsync (Error err))
      Expect.hasAsyncValue 43 v
  ]

[<Tests>]
let defaultWithTests =
  testList "AsyncResult.defaultWith Tests" [
    testCase "defaultWith returns the ok value" <| fun _ ->
      let v = AsyncResult.defaultWith (fun () -> 43) (toAsync (Ok 42))
      Expect.hasAsyncValue 42 v

    testCase "defaultValue invoks the given thunk for Error" <| fun _ ->
      let v = AsyncResult.defaultWith (fun () -> 42) (toAsync (Error err))
      Expect.hasAsyncValue 42 v
  ]

[<Tests>]
let ignoreErrorTests =
  testList "AsyncResult.ignoreError Tests" [
    testCase "ignoreError returns the unit for ok" <| fun _ ->
      Expect.hasAsyncValue () (AsyncResult.ignoreError (toAsync(Ok ())))

    testCase "ignoreError returns the unit for Error" <| fun _ ->
      Expect.hasAsyncValue () (AsyncResult.ignoreError (toAsync(Error err)))
  ]

[<Tests>]
let teeTests =

  testList "AsyncResult.tee Tests" [
    testCase "tee executes the function for ok" <| fun _ ->
      let foo = ref "foo"
      let input = ref 0
      let bar x = 
        input := x
        foo := "bar"
      let result = AsyncResult.tee bar (toAsync (Ok 42))
      Expect.hasAsyncOkValue 42 result
      Expect.equal !foo "bar" ""
      Expect.equal !input 42 ""

    testCase "tee ignores the function for Error" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = AsyncResult.tee bar (toAsync (Error err))
      Expect.hasAsyncErrorValue err result
      Expect.equal !foo "foo" ""
  ]

let returnTrue _ = true
let returnFalse _ = false

[<Tests>]
let teeIfTests =
  testList "AsyncResult.teeIf Tests" [
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
      let result = AsyncResult.teeIf returnTrue bar (toAsync (Ok 42))
      Expect.hasAsyncOkValue 42 result
      Expect.equal !foo "bar" ""
      Expect.equal !input 42 ""
      Expect.equal !pInput 42 ""

    testCase "teeIf ignores the function for Ok and false predicate" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = AsyncResult.teeIf returnFalse bar (toAsync (Ok 42))
      Expect.hasAsyncOkValue 42 result
      Expect.equal !foo "foo" ""

    testCase "teeIf ignores the function for Error" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = AsyncResult.teeIf returnTrue bar (toAsync (Error err))
      Expect.hasAsyncErrorValue err result
      Expect.equal !foo "foo" ""
  ]

[<Tests>]
let teeErrorTests =

  testList "AsyncResult.teeError Tests" [
    testCase "teeError executes the function for Error" <| fun _ ->
      let foo = ref "foo"
      let input = ref ""
      let bar x = 
        input := x
        foo := "bar"
      let result = AsyncResult.teeError bar (toAsync (Error err))
      Expect.hasAsyncErrorValue err result
      Expect.equal !foo "bar" ""
      Expect.equal !input err ""

    testCase "teeError ignores the function for Ok" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = AsyncResult.teeError bar (toAsync (Ok 42))
      Expect.hasAsyncOkValue 42 result
      Expect.equal !foo "foo" ""
  ]


[<Tests>]
let teeErrorIfTests =
  testList "AsyncResult.teeErrorIf Tests" [
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
      let result = AsyncResult.teeErrorIf returnTrue bar (toAsync (Error err))
      Expect.hasAsyncErrorValue err result
      Expect.equal !foo "bar" ""
      Expect.equal !input err ""
      Expect.equal !pInput err ""

    testCase "teeErrorIf ignores the function for Error and false predicate" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = AsyncResult.teeErrorIf returnFalse bar (toAsync (Error err))
      Expect.hasAsyncErrorValue err result
      Expect.equal !foo "foo" ""

    testCase "teeErrorIf ignores the function for Ok" <| fun _ ->
      let foo = ref "foo"
      let bar _ = 
        foo := "bar"
      let result = AsyncResult.teeErrorIf returnTrue bar (toAsync (Ok 42))
      Expect.hasAsyncOkValue 42 result
      Expect.equal !foo "foo" ""
  ]

type CreatePostResult =
| PostSuccess of NotifyNewPostRequest
| NotAllowedToPost

[<Tests>]
let asyncResultCETests =
  let createPost userId = asyncResult {
    let! isAllowed = allowedToPost userId
    if isAllowed then
      let! postId = createPostSuccess validCreatePostRequest
      let! followerIds = getFollowersSuccess sampleUserId
      return PostSuccess {NewPostId = postId; UserIds = followerIds}
    else
      return NotAllowedToPost
  }
  testList "asyncResult Computation Expression tests" [
    testCase "bind with all Ok" <| fun _ ->
      createPost sampleUserId
      |> Expect.hasAsyncOkValue (PostSuccess {NewPostId = PostId newPostId; UserIds = followerIds})

    testCase "bind with an Error" <| fun _ ->
      createPost (UserId (System.Guid.NewGuid()))
      |> Expect.hasAsyncErrorValue commonEx
  ]

[<Tests>]
let asyncResultOperatorTests =
  testList "AsyncResult Operators Tests" [
    testCase "map & apply operators" <| fun _ ->
      let getFollowersResult = getFollowersSuccess sampleUserId
      let createPostResult = createPostSuccess validCreatePostRequest
      newPostRequest <!> getFollowersResult <*> createPostResult
      |> Expect.hasAsyncOkValue {NewPostId = PostId newPostId; UserIds = followerIds}
    
    testCase "bind operator" <| fun _ ->
      allowedToPost sampleUserId
      >>= (fun isAllowed -> 
            if isAllowed then 
              createPostSuccess validCreatePostRequest
            else
              AsyncResult.returnError (Exception ""))
      |> Expect.hasAsyncOkValue (PostId newPostId)
  ]