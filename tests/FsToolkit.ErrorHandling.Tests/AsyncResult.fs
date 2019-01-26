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