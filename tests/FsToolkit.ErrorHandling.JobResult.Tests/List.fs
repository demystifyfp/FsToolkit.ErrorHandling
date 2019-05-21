module ListTests

open Expecto
open SampleDomain
open TestData
open System
open FsToolkit.ErrorHandling
open Hopac
open Expects.JobResult


let userId1 = Guid.NewGuid()
let userId2 = Guid.NewGuid()
let userId3 = Guid.NewGuid()
let userId4 = Guid.NewGuid()
let notifyNewPostSuccess x = notifyNewPostSuccess x >> Job.fromAsync
let notifyNewPostFailure x = notifyNewPostFailure x >>Job.fromAsync

[<Tests>]
let traverseJobResultMTests =
  
  let userIds = List.map UserId [userId1;userId2;userId3]

  testList "List.traverseJobResultM Tests" [
    testCase "traverseJobResultM with a list of valid data" <| fun _ ->
      let expected = [();();()]
      let actual = 
        List.traverseJobResultM (notifyNewPostSuccess (PostId newPostId)) userIds
      Expect.hasJobOkValue expected actual

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let expected = sprintf "error: %s" (userId1.ToString())
      let actual = 
        List.traverseJobResultM (notifyNewPostFailure (PostId newPostId)) userIds
      Expect.hasJobErrorValue expected actual
  ]

let notifyFailure (PostId _) (UserId uId) = job {
  if (uId = userId1 || uId = userId3) then
    return sprintf "error: %s" (uId.ToString()) |> Error
  else
    return Ok ()
}


[<Tests>]
let traverseJobResultATests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.traverseJobResultA Tests" [
    testCase "traverseJobResultA with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.traverseJobResultA (notifyNewPostSuccess (PostId newPostId)) userIds
      Expect.hasJobOkValue expected actual

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let expected = [sprintf "error: %s" (userId1.ToString())
                      sprintf "error: %s" (userId3.ToString())]
      let actual = 
        List.traverseJobResultA (notifyFailure (PostId newPostId)) userIds
      Expect.hasJobErrorValue expected actual
  ]

[<Tests>]
let sequenceJobResultMTests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.sequenceJobResultM Tests" [
    testCase "sequenceJobResultM with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.map (notifyNewPostSuccess (PostId newPostId)) userIds
        |> List.sequenceJobResultM
      Expect.hasJobOkValue expected actual
    
    testCase "sequenceJobResultM with few invalid data" <| fun _ ->
      let expected = sprintf "error: %s" (userId1.ToString())
      let actual = 
        List.map (notifyFailure (PostId newPostId)) userIds
        |> List.sequenceJobResultM
      Expect.hasJobErrorValue expected actual
  ]

[<Tests>]
let sequenceJobResultATests =
  let userIds = List.map UserId [userId1;userId2;userId3;userId4]
  testList "List.sequenceJobResultA Tests" [
    testCase "sequenceJobResultA with a list of valid data" <| fun _ ->
      let expected = [();();();()]
      let actual = 
        List.map (notifyNewPostSuccess (PostId newPostId)) userIds
        |> List.sequenceJobResultA
      Expect.hasJobOkValue expected actual
    
    testCase "sequenceJobResultA with few invalid data" <| fun _ ->
      let expected = [sprintf "error: %s" (userId1.ToString())
                      sprintf "error: %s" (userId3.ToString())]
      let actual = 
        List.map (notifyFailure (PostId newPostId)) userIds
        |> List.sequenceJobResultA
      Expect.hasJobErrorValue expected actual
  ]