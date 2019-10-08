module TaskResultOptionTests

open Expecto
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskResultOption
open System
open TestHelpers

let getUserById x = getUserById x |> Async.StartAsTask
let getPostById x = getPostById x |> Async.StartAsTask


[<Tests>]
let mapTests =
  testList "TaskResultOption.map tests" [
    testCase "map with Task(Ok Some(x))" <| fun _ ->
      getUserById sampleUserId
      |> TaskResultOption.map (fun user -> user.Name.Value)
      |> Expect.hasTaskOkValue (Some "someone")

    testCase "map with Task(Ok None)" <| fun _ ->
      getUserById (UserId (Guid.NewGuid()))
      |> TaskResultOption.map (fun user -> user.Name.Value)
      |> Expect.hasTaskOkValue None

    testCase "map with Task(Error x)" <| fun _ ->
      getUserById (UserId (Guid.Empty))
      |> TaskResultOption.map (fun user -> user.Name.Value)
      |> Expect.hasTaskErrorValue "invalid user id"
  ]
  

type UserTweet = {
  Name : string
  Tweet : string
}
let userTweet (p : Post) (u : User) =
  {Name = u.Name.Value; Tweet = p.Tweet.Value}


[<Tests>]
let bindTests =
  testList "TaskResultOption.bind tests" [
    testCase "bind with Task(Ok Some(x)) Task(Ok Some(x))" <| fun _ ->
      getPostById samplePostId
      |> TaskResultOption.bind (fun post -> getUserById post.UserId)
      |> Expect.hasTaskOkValue (Some sampleUser)
    
    testCase "bind with Task(Ok None) Task(Ok Some(x))" <| fun _ ->
      getPostById (PostId (Guid.NewGuid()))
      |> TaskResultOption.bind (fun post -> 
        failwith "this shouldn't be called!!"
        getUserById post.UserId)
      |> Expect.hasTaskOkValue None
    
    testCase "bind with Task(Ok Some(x)) Task(Ok None)" <| fun _ ->
      getPostById samplePostId
      |> TaskResultOption.bind (fun _ -> getUserById (UserId (Guid.NewGuid())))
      |> Expect.hasTaskOkValue None
    
    testCase "bind with Task(Ok None) Task(Ok None)" <| fun _ ->
      getPostById (PostId (Guid.NewGuid()))
      |> TaskResultOption.bind (fun _ -> 
        failwith "this shouldn't be called!!"
        getUserById (UserId (Guid.NewGuid())))
      |> Expect.hasTaskOkValue None
    
    testCase "bind with Task(Error x) Task(Ok (Some y))" <| fun _ ->
      getPostById (PostId Guid.Empty)
      |> TaskResultOption.bind (fun _ -> 
        failwith "this shouldn't be called!!"
        getUserById (UserId (Guid.NewGuid())))
      |> Expect.hasTaskErrorValue "invalid post id"
  ]

[<Tests>]
let map2Tests =
  testList "TaskResultOption.map2 tests" [
    testCase "map2 with Task(Ok Some(x)) Task(Ok Some(x))" <| fun _ ->
      let postARO= getPostById samplePostId
      let userARO = getUserById sampleUserId
      TaskResultOption.map2 userTweet postARO userARO
      |> Expect.hasTaskOkValue (Some {Name = "someone"; Tweet = "Hello, World!"})

    testCase "map2 with Task(Ok Some(x)) Task(Ok None))" <| fun _ ->
      let postARO= getPostById samplePostId
      let userARO = getUserById (UserId (Guid.NewGuid()))
      TaskResultOption.map2 userTweet postARO userARO
      |> Expect.hasTaskOkValue None
    
    testCase "map2 with Task(Ok Some(x)) Task(Ok None)" <| fun _ ->
      let postARO= getPostById (PostId (Guid.NewGuid()))
      let userARO = getUserById sampleUserId
      TaskResultOption.map2 userTweet postARO userARO
      |> Expect.hasTaskOkValue None
    
    testCase "map2 with Task(Ok None) Task(Ok None)" <| fun _ ->
      let postARO= getPostById (PostId (Guid.NewGuid()))
      let userARO = getUserById (UserId (Guid.NewGuid()))
      TaskResultOption.map2 userTweet postARO userARO
      |> Expect.hasTaskOkValue None
    
    testCase "map2 with Task(Error x) Task(Ok None)" <| fun _ ->
      let postARO= getPostById (PostId Guid.Empty)
      let userARO = getUserById (UserId (Guid.NewGuid()))
      TaskResultOption.map2 userTweet postARO userARO
      |> Expect.hasTaskErrorValue "invalid post id"

    testCase "map2 with Task(Error x) Task(Error y)" <| fun _ ->
      let postARO= getPostById (PostId Guid.Empty)
      let userARO = getUserById (UserId Guid.Empty)
      TaskResultOption.map2 userTweet postARO userARO
      |> Expect.hasTaskErrorValue "invalid post id"
  ]

[<Tests>]
let ignoreTests =
  testList "TaskResultOption.ignore tests" [
    testCase "ignore with Task(Ok Some(x))" <| fun _ ->
      getUserById sampleUserId
      |> TaskResultOption.ignore
      |> Expect.hasTaskOkValue (Some ())

    testCase "ignore with Task(Ok None)" <| fun _ ->
      getUserById (UserId (Guid.NewGuid()))
      |> TaskResultOption.ignore
      |> Expect.hasTaskOkValue None

    testCase "ignore with Task(Error x)" <| fun _ ->
      getUserById (UserId (Guid.Empty))
      |> TaskResultOption.ignore
      |> Expect.hasTaskErrorValue "invalid user id"
  ]

[<Tests>]
let computationExpressionTests =
  testList "taskResultOption CE tests" [
    testCase "CE with Task(Ok Some(x)) Task(Ok Some(x))" <| fun _ ->
      taskResultOption {
        let! post = getPostById samplePostId
        let! user = getUserById post.UserId
        return userTweet post user
      } |> Expect.hasTaskOkValue (Some {Name = "someone"; Tweet = "Hello, World!"})

    testCase "CE with Task(Ok None) Task(Ok Some(x))" <| fun _ ->
      taskResultOption {
        let! post = getPostById (PostId (Guid.NewGuid()))
        let! user = getUserById post.UserId
        return userTweet post user
      } |> Expect.hasTaskOkValue None
    
    testCase "CE with Task(Ok Some(x)) Task(Ok None)" <| fun _ ->
      taskResultOption {
        let! post = getPostById samplePostId
        let! user = getUserById (UserId (Guid.NewGuid()))
        return userTweet post user
      } |> Expect.hasTaskOkValue None
    
    testCase "CE with Task(Error x) Task(Ok None)" <| fun _ ->
      taskResultOption {
        let! post = getPostById (PostId Guid.Empty)
        let! user = getUserById post.UserId
        return userTweet post user
      } |> Expect.hasTaskErrorValue "invalid post id"
  ]

[<Tests>]
let operatorTests =
  testList "TaskResultOption Operators Tests" [
    testCase "map & apply operators" <| fun _ ->
      let getPostResult = getPostById samplePostId
      let getUserResult = getUserById sampleUserId
      userTweet <!> getPostResult <*> getUserResult
      |> Expect.hasTaskOkValue (Some {Name = "someone"; Tweet = "Hello, World!"})
    
    testCase "bind & map operator" <| fun _ ->
      getPostById samplePostId
      >>= (fun post -> 
        (fun user -> userTweet post user) <!> getUserById post.UserId      
      )
      |> Expect.hasTaskOkValue (Some {Name = "someone"; Tweet = "Hello, World!"})
  ]