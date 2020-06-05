module AsyncOptionTests


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open System
open TestData
open TestHelpers
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncOption

let mapTests =
    testList "AsyncOption.map Tests" [
      testCaseAsync "map with Async(Some x)" <| 
        (Async.singleton (Some validTweet)
         |> AsyncOption.map remainingCharacters
         |> Expect.hasAsyncSomeValue 267)
      
      testCaseAsync "map with Async(None)" <| 
        (Async.singleton (None)
         |> AsyncOption.map remainingCharacters
         |> Expect.hasAsyncNoneValue)
    ]

let bindTests =
  testList "AsyncOption.bind tests" [
    testCaseAsync "bind with Async(Some x)" <| 
      (allowedToPostOptional sampleUserId
       |> AsyncOption.bind (fun isAllowed -> async { 
          if isAllowed then 
           return! createPostSome validCreatePostRequest
          else
           return None })
       |> Expect.hasAsyncSomeValue (PostId newPostId))

    testCaseAsync "bind with Async(None)" <| 
      (allowedToPostOptional (UserId (Guid.NewGuid()))
       |> AsyncOption.bind (fun isAllowed -> async {return Some isAllowed})
       |> Expect.hasAsyncNoneValue )

    testCaseAsync "bind with Async(Ok x) that returns Async (None)" <|
      (allowedToPostOptional sampleUserId
       |> AsyncOption.bind (fun _ -> async { 
           return None
         })
       |> Expect.hasAsyncNoneValue)
  ]

let applyTests = 
  testList "AsyncOption.apply Tests" [
    testCaseAsync "apply with Async(Some x)" <| (
      Async.singleton (Some validTweet)
      |> AsyncOption.apply (Async.singleton (Some remainingCharacters)) 
      |> Expect.hasAsyncSomeValue (267))
    
    testCaseAsync "apply with Async(None)" <| (
      Async.singleton None
      |> AsyncOption.apply (Async.singleton (Some remainingCharacters)) 
      |> Expect.hasAsyncNoneValue)
  ]

let retnTests = 
   testList "AsyncOption.retn Tests" [
     testCaseAsync "retn with x" <| (
      AsyncOption.retn 267
      |> Expect.hasAsyncSomeValue (267))
   ]

let asyncOptionOperatorTests =
  testList "AsyncOption Operators Tests" [
    testCaseAsync "map & apply operators" <| async {
      let getFollowersResult = getFollowersSome sampleUserId
      let createPostResult = createPostSome validCreatePostRequest
      do!
        newPostRequest <!> getFollowersResult <*> createPostResult
        |> Expect.hasAsyncSomeValue {NewPostId = PostId newPostId; UserIds = followerIds}
    }
    
    testCaseAsync "bind operator" <| async {
      do!
        allowedToPostOptional sampleUserId
        >>= (fun isAllowed -> 
              if isAllowed then 
                createPostSome validCreatePostRequest
              else
                Async.singleton None)
        |> Expect.hasAsyncSomeValue (PostId newPostId)
    }
  ]

let allTests = testList "Async Option Tests" [
  mapTests
  bindTests
  applyTests
  retnTests
  asyncOptionOperatorTests
]