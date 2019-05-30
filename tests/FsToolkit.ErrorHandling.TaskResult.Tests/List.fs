module ListTests

open Expecto
open SampleDomain
open TestData
open TestHelpers
open System
open FsToolkit.ErrorHandling
open FSharp.Control.Tasks.V2.ContextInsensitive


let userId1 = Guid.NewGuid()
let userId2 = Guid.NewGuid()
let userId3 = Guid.NewGuid()
let userId4 = Guid.NewGuid()
let notifyNewPostSuccess x = notifyNewPostSuccess x >> Async.StartAsTask
let notifyNewPostFailure x = notifyNewPostFailure x >> Async.StartAsTask

[<Tests>]
let traverseTaskResultMTests =
  
  let userIds = List.map UserId [userId1;userId2;userId3]

  testList "List.traverseTaskResultM Tests" [
    testCase "traverseTaskResultM with a list of valid data" <| fun _ ->
      let expected = [();();()]
      let actual = 
        List.traverseTaskResultM (notifyNewPostSuccess (PostId newPostId)) userIds
      Expect.hasTaskOkValue expected actual

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let expected = sprintf "error: %s" (userId1.ToString())
      let actual = 
        List.traverseTaskResultM (notifyNewPostFailure (PostId newPostId)) userIds
      Expect.hasTaskErrorValue expected actual
  ]

let notifyFailure (PostId _) (UserId uId) = task {
  if (uId = userId1 || uId = userId3) then
    return sprintf "error: %s" (uId.ToString()) |> Error
  else
    return Ok ()
}


[<Tests>]
let traverseTaskResultATests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.traverseTaskResultA Tests" [
    testCase "traverseTaskResultA with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.traverseTaskResultA (notifyNewPostSuccess (PostId newPostId)) userIds
      Expect.hasTaskOkValue expected actual

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let expected = [sprintf "error: %s" (userId1.ToString())
                      sprintf "error: %s" (userId3.ToString())]
      let actual = 
        List.traverseTaskResultA (notifyFailure (PostId newPostId)) userIds
      Expect.hasTaskErrorValue expected actual
  ]

[<Tests>]
let sequenceTaskResultMTests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.sequenceTaskResultM Tests" [
    testCase "sequenceTaskResultM with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.map (notifyNewPostSuccess (PostId newPostId)) userIds
        |> List.sequenceTaskResultM
      Expect.hasTaskOkValue expected actual
    
    testCase "sequenceTaskResultM with few invalid data" <| fun _ ->
      let expected = sprintf "error: %s" (userId1.ToString())
      let actual = 
        List.map (notifyFailure (PostId newPostId)) userIds
        |> List.sequenceTaskResultM
      Expect.hasTaskErrorValue expected actual
  ]

[<Tests>]
let sequenceTaskResultATests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.sequenceTaskResultA Tests" [
    testCase "sequenceTaskResultA with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.map (notifyNewPostSuccess (PostId newPostId)) userIds
        |> List.sequenceTaskResultA
      Expect.hasTaskOkValue expected actual
    
    testCase "sequenceTaskResultA with few invalid data" <| fun _ ->
      let expected = [sprintf "error: %s" (userId1.ToString())
                      sprintf "error: %s" (userId3.ToString())]
      let actual = 
        List.map (notifyFailure (PostId newPostId)) userIds
        |> List.sequenceTaskResultA
      Expect.hasTaskErrorValue expected actual
  ]