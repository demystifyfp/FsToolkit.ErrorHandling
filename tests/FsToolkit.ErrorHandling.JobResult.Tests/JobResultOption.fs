module JobResultOptionTests

open Expecto
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.JobResultOption
open System
open Hopac
open Expects.JobResult

let getUserById x =
    getUserById x
    |> Job.fromAsync

let getPostById x =
    getPostById x
    |> Job.fromAsync


[<Tests>]
let mapTests =
    testList "JobResultOption.map tests" [
        testCase "map with Job(Ok Some(x))"
        <| fun _ ->
            getUserById sampleUserId
            |> JobResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasJobOkValueSync (Some "someone")

        testCase "map with Job(Ok None)"
        <| fun _ ->
            getUserById (UserId(Guid.NewGuid()))
            |> JobResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasJobOkValueSync None

        testCase "map with Job(Error x)"
        <| fun _ ->
            getUserById (UserId(Guid.Empty))
            |> JobResultOption.map (fun user -> user.Name.Value)
            |> Expect.hasJobErrorValueSync "invalid user id"
    ]


type UserTweet = { Name: string; Tweet: string }

let userTweet (p: Post) (u: User) = {
    Name = u.Name.Value
    Tweet = p.Tweet.Value
}


[<Tests>]
let bindTests =
    testList "JobResultOption.bind tests" [
        testCase "bind with Job(Ok Some(x)) Job(Ok Some(x))"
        <| fun _ ->
            getPostById samplePostId
            |> JobResultOption.bind (fun post -> getUserById post.UserId)
            |> Expect.hasJobOkValueSync (Some sampleUser)

        testCase "bind with Job(Ok None) Job(Ok Some(x))"
        <| fun _ ->
            getPostById (PostId(Guid.NewGuid()))
            |> JobResultOption.bind (fun post ->
                failwith "this shouldn't be called!!"
                getUserById post.UserId
            )
            |> Expect.hasJobOkValueSync None

        testCase "bind with Job(Ok Some(x)) Job(Ok None)"
        <| fun _ ->
            getPostById samplePostId
            |> JobResultOption.bind (fun _ -> getUserById (UserId(Guid.NewGuid())))
            |> Expect.hasJobOkValueSync None

        testCase "bind with Job(Ok None) Job(Ok None)"
        <| fun _ ->
            getPostById (PostId(Guid.NewGuid()))
            |> JobResultOption.bind (fun _ ->
                failwith "this shouldn't be called!!"
                getUserById (UserId(Guid.NewGuid()))
            )
            |> Expect.hasJobOkValueSync None

        testCase "bind with Job(Error x) Job(Ok (Some y))"
        <| fun _ ->
            getPostById (PostId Guid.Empty)
            |> JobResultOption.bind (fun _ ->
                failwith "this shouldn't be called!!"
                getUserById (UserId(Guid.NewGuid()))
            )
            |> Expect.hasJobErrorValueSync "invalid post id"
    ]

[<Tests>]
let map2Tests =
    testList "JobResultOption.map2 tests" [
        testCase "map2 with Job(Ok Some(x)) Job(Ok Some(x))"
        <| fun _ ->
            let postARO = getPostById samplePostId
            let userARO = getUserById sampleUserId

            JobResultOption.map2 userTweet postARO userARO
            |> Expect.hasJobOkValueSync (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            )

        testCase "map2 with Job(Ok Some(x)) Job(Ok None))"
        <| fun _ ->
            let postARO = getPostById samplePostId
            let userARO = getUserById (UserId(Guid.NewGuid()))

            JobResultOption.map2 userTweet postARO userARO
            |> Expect.hasJobOkValueSync None

        testCase "map2 with Job(Ok Some(x)) Job(Ok None)"
        <| fun _ ->
            let postARO = getPostById (PostId(Guid.NewGuid()))
            let userARO = getUserById sampleUserId

            JobResultOption.map2 userTweet postARO userARO
            |> Expect.hasJobOkValueSync None

        testCase "map2 with Job(Ok None) Job(Ok None)"
        <| fun _ ->
            let postARO = getPostById (PostId(Guid.NewGuid()))
            let userARO = getUserById (UserId(Guid.NewGuid()))

            JobResultOption.map2 userTweet postARO userARO
            |> Expect.hasJobOkValueSync None

        testCase "map2 with Job(Error x) Job(Ok None)"
        <| fun _ ->
            let postARO = getPostById (PostId Guid.Empty)
            let userARO = getUserById (UserId(Guid.NewGuid()))

            JobResultOption.map2 userTweet postARO userARO
            |> Expect.hasJobErrorValueSync "invalid post id"

        testCase "map2 with Job(Error x) Job(Error y)"
        <| fun _ ->
            let postARO = getPostById (PostId Guid.Empty)
            let userARO = getUserById (UserId Guid.Empty)

            JobResultOption.map2 userTweet postARO userARO
            |> Expect.hasJobErrorValueSync "invalid post id"
    ]

[<Tests>]
let ignoreTests =
    testList "JobResultOption.ignore tests" [
        testCase "ignore with Job(Ok Some(x))"
        <| fun _ ->
            getUserById sampleUserId
            |> JobResultOption.ignore
            |> Expect.hasJobOkValueSync (Some())

        testCase "ignore with Job(Ok None)"
        <| fun _ ->
            getUserById (UserId(Guid.NewGuid()))
            |> JobResultOption.ignore
            |> Expect.hasJobOkValueSync None

        testCase "ignore with Job(Error x)"
        <| fun _ ->
            getUserById (UserId(Guid.Empty))
            |> JobResultOption.ignore
            |> Expect.hasJobErrorValueSync "invalid user id"

        testCase "can call ignore without type parameters"
        <| fun _ -> ignore JobResultOption.ignore

        testCase "can call ignore with type parameters"
        <| fun _ ->
            ignore<Job<Result<int option, string>> -> Job<Result<unit option, string>>>
                JobResultOption.ignore<int, string>
    ]

[<Tests>]
let computationExpressionTests =
    testList "jobResultOption CE tests" [
        testCase "CE with Job(Ok Some(x)) Job(Ok Some(x))"
        <| fun _ ->
            jobResultOption {
                let! post = getPostById samplePostId
                let! user = getUserById post.UserId
                return userTweet post user
            }
            |> Expect.hasJobOkValueSync (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            )

        testCase "CE with Job(Ok None) Job(Ok Some(x))"
        <| fun _ ->
            jobResultOption {
                let! post = getPostById (PostId(Guid.NewGuid()))
                let! user = getUserById post.UserId
                return userTweet post user
            }
            |> Expect.hasJobOkValueSync None

        testCase "CE with Job(Ok Some(x)) Job(Ok None)"
        <| fun _ ->
            jobResultOption {
                let! post = getPostById samplePostId
                let! user = getUserById (UserId(Guid.NewGuid()))
                return userTweet post user
            }
            |> Expect.hasJobOkValueSync None

        testCase "CE with Job(Error x) Job(Ok None)"
        <| fun _ ->
            jobResultOption {
                let! post = getPostById (PostId Guid.Empty)
                let! user = getUserById post.UserId
                return userTweet post user
            }
            |> Expect.hasJobErrorValueSync "invalid post id"
    ]

[<Tests>]
let operatorTests =
    testList "JobResultOption Operators Tests" [
        testCase "map & apply operators"
        <| fun _ ->
            let getPostResult = getPostById samplePostId
            let getUserResult = getUserById sampleUserId

            userTweet
            <!> getPostResult
            <*> getUserResult
            |> Expect.hasJobOkValueSync (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            )

        testCase "bind & map operator"
        <| fun _ ->
            getPostById samplePostId
            >>= (fun post ->
                (fun user -> userTweet post user)
                <!> getUserById post.UserId
            )
            |> Expect.hasJobOkValueSync (
                Some {
                    Name = "someone"
                    Tweet = "Hello, World!"
                }
            )
    ]

[<Tests>]
let ``JobResultOptionCE inference checks`` =
    testList "JobResultOptionCE inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = jobResultOption { return! res }

            f (JobResult.singleton ())
            |> ignore
    ]
