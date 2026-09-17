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

let createPostSome =
    createPostSome
    >> Job.fromAsync

let getFollowersSome =
    getFollowersSome
    >> Job.fromAsync

let allowedToPostOptional =
    allowedToPostOptional
    >> Job.fromAsync

let mapTests =
    testList "JobOption.map Tests" [
        testCase "map with Job(Some x)"
        <| fun _ ->
            Job.result (Some validTweet)
            |> JobOption.map remainingCharacters
            |> Expect.hasJobSomeValue 267

        testCase "map with Job(None)"
        <| fun _ ->
            Job.result None
            |> JobOption.map remainingCharacters
            |> Expect.hasJobNoneValue
    ]

let bindTests =
    testList "JobOption.bind tests" [
        testCase "bind with Job(Some x)"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            |> JobOption.bind (fun isAllowed ->
                job {
                    if isAllowed then
                        return! createPostSome validCreatePostRequest
                    else
                        return None
                }
            )
            |> Expect.hasJobSomeValue (PostId newPostId)

        testCase "bind with Job(None)"
        <| fun _ ->
            allowedToPostOptional (UserId(Guid.NewGuid()))
            |> JobOption.bind (fun isAllowed -> job { return Some isAllowed })
            |> Expect.hasJobNoneValue

        testCase "bind with Job(Ok x) that returns Job (None)"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            |> JobOption.bind (fun _ -> job { return None })
            |> Expect.hasJobNoneValue
    ]

let applyTests =
    testList "JobOption.apply Tests" [
        testCase "apply with Job(Some x)"
        <| fun _ ->
            Job.result (Some validTweet)
            |> JobOption.apply (Job.result (Some remainingCharacters))
            |> Expect.hasJobSomeValue (267)

        testCase "apply with Job(None)"
        <| fun _ ->
            Job.result None
            |> JobOption.apply (Job.result (Some remainingCharacters))
            |> Expect.hasJobNoneValue
    ]

let retnTests =
    testList "JobOption.retn Tests" [
        testCase "retn with x"
        <| fun _ ->
            JobOption.some 267
            |> Expect.hasJobSomeValue (267)
    ]

let jobOptionOperatorTests =
    testList "JobOption Operators Tests" [
        testCase "map & apply operators"
        <| fun _ ->
            let getFollowersResult = getFollowersSome sampleUserId
            let createPostResult = createPostSome validCreatePostRequest

            newPostRequest
            <!> getFollowersResult
            <*> createPostResult
            |> Expect.hasJobSomeValue {
                NewPostId = PostId newPostId
                UserIds = followerIds
            }

        testCase "bind operator"
        <| fun _ ->
            allowedToPostOptional sampleUserId
            >>= (fun isAllowed ->
                if isAllowed then
                    createPostSome validCreatePostRequest
                else
                    Job.result None
            )
            |> Expect.hasJobSomeValue (PostId newPostId)
    ]


let eitherTests =
    testList "JobOption.either Tests" [
        testCaseJob "Some"
        <| job {
            let value1 = JobOption.some 5
            let f = job.Return 42
            let add2 x = job { return x + 2 }
            let! result = (JobOption.either add2 f value1)
            Expect.equal result 7 ""
        }
        testCaseJob "None"
        <| job {
            let value1 = job.Return None
            let f = job.Return 42
            let add2 x = job { return x + 2 }
            let! result = (JobOption.either add2 f value1)
            Expect.equal result 42 ""
        }
    ]

let allTests =
    testList "Job Option Tests" [
        mapTests
        bindTests
        applyTests
        retnTests
        jobOptionOperatorTests
    ]
