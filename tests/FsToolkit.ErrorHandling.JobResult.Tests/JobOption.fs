module JobOptionTests


open Expecto
open Expects.JobOption
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.JobOption
open System
open Hopac

let runJobSync = run
let createPostSome = createPostSome >> Job.fromAsync
let getFollowersSome = getFollowersSome >> Job.fromAsync
let allowedToPostOptional = allowedToPostOptional >> Job.fromAsync

let mapTests =
    testList "JobOption.map Tests" [
      testCase "map with Job(Some x)" <| fun _ ->
        Job.singleton (Some validTweet)
        |> JobOption.map remainingCharacters
        |> Expect.hasJobSomeValue 267
      
      testCase "map with Job(None)"  <| fun _ ->
        Job.singleton (None)
         |> JobOption.map remainingCharacters
         |> Expect.hasJobNoneValue
    ]

let bindTests =
  testList "JobOption.bind tests" [
    testCase "bind with Job(Some x)" <| fun _ ->
      allowedToPostOptional sampleUserId
       |> JobOption.bind (fun isAllowed -> job { 
          if isAllowed then 
           return! createPostSome validCreatePostRequest
          else
           return None })
       |> Expect.hasJobSomeValue (PostId newPostId)

    testCase "bind with Job(None)" <| fun _ ->
      allowedToPostOptional (UserId (Guid.NewGuid()))
       |> JobOption.bind (fun isAllowed -> job {return Some isAllowed})
       |> Expect.hasJobNoneValue

    testCase "bind with Job(Ok x) that returns Job (None)" <| fun _ ->
      allowedToPostOptional sampleUserId
       |> JobOption.bind (fun _ -> job { 
           return None
         })
       |> Expect.hasJobNoneValue
  ]

let applyTests = 
  testList "JobOption.apply Tests" [
    testCase "apply with Job(Some x)" <| fun _ ->
      Job.singleton (Some validTweet)
        |> JobOption.apply (Job.singleton (Some remainingCharacters)) 
        |> Expect.hasJobSomeValue (267)
    
    testCase "apply with Job(None)" <| fun _ ->
      Job.singleton None
      |> JobOption.apply (Job.singleton (Some remainingCharacters)) 
      |> Expect.hasJobNoneValue
  ]

let retnTests = 
   testList "JobOption.retn Tests" [
      testCase "retn with x" <| fun _ ->
      JobOption.retn 267
        |> Expect.hasJobSomeValue (267)
   ]

let jobOptionOperatorTests =
  testList "JobOption Operators Tests" [
    testCase "map & apply operators" <| fun _ -> 
      let getFollowersResult = getFollowersSome sampleUserId
      let createPostResult = createPostSome validCreatePostRequest
      newPostRequest <!> getFollowersResult <*> createPostResult
      |> Expect.hasJobSomeValue {NewPostId = PostId newPostId; UserIds = followerIds}
    
    testCase "bind operator" <| fun _ -> 
      allowedToPostOptional sampleUserId
      >>= (fun isAllowed -> 
              if isAllowed then 
                createPostSome validCreatePostRequest
              else
                Job.singleton None)
      |> Expect.hasJobSomeValue (PostId newPostId)
  ]

let allTests = testList "Job Option Tests" [
  mapTests
  bindTests
  applyTests
  retnTests
  jobOptionOperatorTests
]