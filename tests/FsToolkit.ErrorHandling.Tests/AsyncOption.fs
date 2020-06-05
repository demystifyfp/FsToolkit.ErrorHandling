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
// open FsToolkit.ErrorHandling.Operator.AsyncOption

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

let allTests = testList "Async Option Tests" [
  mapTests
  bindTests
]