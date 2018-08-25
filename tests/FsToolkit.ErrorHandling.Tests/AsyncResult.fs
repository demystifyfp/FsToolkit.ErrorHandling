module AsyncResultTests


open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
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