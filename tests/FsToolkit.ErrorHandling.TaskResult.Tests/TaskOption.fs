module TaskOptionTests


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open TestData
open TestHelpers
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskOption

let runTaskSync (task : Task<_>) =
    task.Result
let createPostSome = createPostSome >> Async.StartAsTask
let getFollowersSome = getFollowersSome >> Async.StartAsTask
let allowedToPostOptional = allowedToPostOptional >> Async.StartAsTask

let mapTests =
    testList "TaskOption.map Tests" [
      testCase"map with Task(Some x)" <| fun _ ->
        Task.singleton (Some validTweet)
        |> TaskOption.map remainingCharacters
        |> Expect.hasTaskSomeValue 267
      
      testCase "map with Task(None)"  <| fun _ ->
        Task.singleton (None)
         |> TaskOption.map remainingCharacters
         |> Expect.hasTaskNoneValue
    ]

let bindTests =
  testList "TaskOption.bind tests" [
    testCase "bind with Task(Some x)" <| fun _ ->
      allowedToPostOptional sampleUserId
       |> TaskOption.bind (fun isAllowed -> task { 
          if isAllowed then 
           return! createPostSome validCreatePostRequest
          else
           return None })
       |> Expect.hasTaskSomeValue (PostId newPostId)

    testCase "bind with Task(None)" <| fun _ ->
      allowedToPostOptional (UserId (Guid.NewGuid()))
       |> TaskOption.bind (fun isAllowed -> task {return Some isAllowed})
       |> Expect.hasTaskNoneValue

    testCase "bind with Task(Ok x) that returns Task (None)" <| fun _ ->
      allowedToPostOptional sampleUserId
       |> TaskOption.bind (fun _ -> task { 
           return None
         })
       |> Expect.hasTaskNoneValue
  ]

let applyTests = 
  testList "TaskOption.apply Tests" [
    testCase "apply with Task(Some x)" <| fun _ ->
      Task.singleton (Some validTweet)
      |> TaskOption.apply (Task.singleton (Some remainingCharacters)) 
      |> Expect.hasTaskSomeValue (267)
    
    testCase "apply with Task(None)" <| fun _ ->
      Task.singleton None
      |> TaskOption.apply (Task.singleton (Some remainingCharacters)) 
      |> Expect.hasTaskNoneValue
  ]

let retnTests = 
   testList "TaskOption.retn Tests" [
      testCase "retn with x" <| fun _ ->
        TaskOption.retn 267
        |> Expect.hasTaskSomeValue (267)
   ]

let taskOptionOperatorTests =
  testList "TaskOption Operators Tests" [
    testCase "map & apply operators" <| fun _ -> 
      let getFollowersResult = getFollowersSome sampleUserId
      let createPostResult = createPostSome validCreatePostRequest
      newPostRequest <!> getFollowersResult <*> createPostResult
      |> Expect.hasTaskSomeValue {NewPostId = PostId newPostId; UserIds = followerIds}
    
    testCase "bind operator" <| fun _ -> 
      allowedToPostOptional sampleUserId
      >>= (fun isAllowed -> 
              if isAllowed then 
                createPostSome validCreatePostRequest
              else
                Task.singleton None)
      |> Expect.hasTaskSomeValue (PostId newPostId)
  ]

let allTests = testList "Task Option Tests" [
  mapTests
  bindTests
  applyTests
  retnTests
  taskOptionOperatorTests
]