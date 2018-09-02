module AsyncResultOptionTests

open Expecto
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.ComputationExpression.AsyncResultOption
open FsToolkit.ErrorHandling.Operator.AsyncResultOption
open System

[<Tests>]
let mapTests =
  testList "AsyncResultOption.map tests" [
    testCase "map with Async(Ok Some(x))" <| fun _ ->
      getUserById sampleUserId
      |> AsyncResultOption.map (fun user -> user.Name.Value)
      |> Expect.hasAsyncOkValue (Some "someone")

    testCase "map with Async(Ok None)" <| fun _ ->
      getUserById (UserId (Guid.NewGuid()))
      |> AsyncResultOption.map (fun user -> user.Name.Value)
      |> Expect.hasAsyncOkValue None

    testCase "map with Async(Error x)" <| fun _ ->
      getUserById (UserId (Guid.Empty))
      |> AsyncResultOption.map (fun user -> user.Name.Value)
      |> Expect.hasAsyncErrorValue "invalid user id"
  ]
  

type UserTweet = {
  Name : string
  Tweet : string
}
let userTweet (p : Post) (u : User) =
  {Name = u.Name.Value; Tweet = p.Tweet.Value}


[<Tests>]
let bindTests =
  testList "AsyncResultOption.bind tests" [
    testCase "bind with Async(Ok Some(x)) Async(Ok Some(x))" <| fun _ ->
      getPostById samplePostId
      |> AsyncResultOption.bind (fun post -> getUserById post.UserId)
      |> Expect.hasAsyncOkValue (Some sampleUser)
    
    testCase "bind with Async(Ok None) Async(Ok Some(x))" <| fun _ ->
      getPostById (PostId (Guid.NewGuid()))
      |> AsyncResultOption.bind (fun post -> 
        failwith "this shouldn't be called!!"
        getUserById post.UserId)
      |> Expect.hasAsyncOkValue None
    
    testCase "bind with Async(Ok Some(x)) Async(Ok None)" <| fun _ ->
      getPostById samplePostId
      |> AsyncResultOption.bind (fun _ -> getUserById (UserId (Guid.NewGuid())))
      |> Expect.hasAsyncOkValue None
    
    testCase "bind with Async(Ok None) Async(Ok None)" <| fun _ ->
      getPostById (PostId (Guid.NewGuid()))
      |> AsyncResultOption.bind (fun _ -> 
        failwith "this shouldn't be called!!"
        getUserById (UserId (Guid.NewGuid())))
      |> Expect.hasAsyncOkValue None
    
    testCase "bind with Async(Error x) Async(Ok (Some y))" <| fun _ ->
      getPostById (PostId Guid.Empty)
      |> AsyncResultOption.bind (fun _ -> 
        failwith "this shouldn't be called!!"
        getUserById (UserId (Guid.NewGuid())))
      |> Expect.hasAsyncErrorValue "invalid post id"
  ]

[<Tests>]
let map2Tests =
  testList "AsyncResultOption.map2 tests" [
    testCase "map2 with Async(Ok Some(x)) Async(Ok Some(x))" <| fun _ ->
      let postARO= getPostById samplePostId
      let userARO = getUserById sampleUserId
      AsyncResultOption.map2 userTweet postARO userARO
      |> Expect.hasAsyncOkValue (Some {Name = "someone"; Tweet = "Hello, World!"})

    testCase "map2 with Async(Ok Some(x)) Async(Ok None))" <| fun _ ->
      let postARO= getPostById samplePostId
      let userARO = getUserById (UserId (Guid.NewGuid()))
      AsyncResultOption.map2 userTweet postARO userARO
      |> Expect.hasAsyncOkValue None
    
    testCase "map2 with Async(Ok Some(x)) Async(Ok None)" <| fun _ ->
      let postARO= getPostById (PostId (Guid.NewGuid()))
      let userARO = getUserById sampleUserId
      AsyncResultOption.map2 userTweet postARO userARO
      |> Expect.hasAsyncOkValue None
    
    testCase "map2 with Async(Ok None) Async(Ok None)" <| fun _ ->
      let postARO= getPostById (PostId (Guid.NewGuid()))
      let userARO = getUserById (UserId (Guid.NewGuid()))
      AsyncResultOption.map2 userTweet postARO userARO
      |> Expect.hasAsyncOkValue None
    
    testCase "map2 with Async(Error x) Async(Ok None)" <| fun _ ->
      let postARO= getPostById (PostId Guid.Empty)
      let userARO = getUserById (UserId (Guid.NewGuid()))
      AsyncResultOption.map2 userTweet postARO userARO
      |> Expect.hasAsyncErrorValue "invalid post id"

    testCase "map2 with Async(Error x) Async(Error y)" <| fun _ ->
      let postARO= getPostById (PostId Guid.Empty)
      let userARO = getUserById (UserId Guid.Empty)
      AsyncResultOption.map2 userTweet postARO userARO
      |> Expect.hasAsyncErrorValue "invalid post id"
  ]

[<Tests>]
let computationExpressionTests =
  testList "asyncResultOption CE tests" [
    testCase "CE with Async(Ok Some(x)) Async(Ok Some(x))" <| fun _ ->
      asyncResultOption {
        let! post = getPostById samplePostId
        let! user = getUserById post.UserId
        return userTweet post user
      } |> Expect.hasAsyncOkValue (Some {Name = "someone"; Tweet = "Hello, World!"})

    testCase "CE with Async(Ok None) Async(Ok Some(x))" <| fun _ ->
      asyncResultOption {
        let! post = getPostById (PostId (Guid.NewGuid()))
        let! user = getUserById post.UserId
        return userTweet post user
      } |> Expect.hasAsyncOkValue None
    
    testCase "CE with Async(Ok Some(x)) Async(Ok None)" <| fun _ ->
      asyncResultOption {
        let! post = getPostById samplePostId
        let! user = getUserById (UserId (Guid.NewGuid()))
        return userTweet post user
      } |> Expect.hasAsyncOkValue None
    
    testCase "CE with Async(Error x) Async(Ok None)" <| fun _ ->
      asyncResultOption {
        let! post = getPostById (PostId Guid.Empty)
        let! user = getUserById post.UserId
        return userTweet post user
      } |> Expect.hasAsyncErrorValue "invalid post id"
  ]

[<Tests>]
let operatorTests =
  testList "AsyncResultOption Operators Tests" [
    testCase "map & apply operators" <| fun _ ->
      let getPostResult = getPostById samplePostId
      let getUserResult = getUserById sampleUserId
      userTweet <!> getPostResult <*> getUserResult
      |> Expect.hasAsyncOkValue (Some {Name = "someone"; Tweet = "Hello, World!"})
    
    testCase "bind & map operator" <| fun _ ->
      getPostById samplePostId
      >>= (fun post -> 
        (fun user -> userTweet post user) <!> getUserById post.UserId      
      )
      |> Expect.hasAsyncOkValue (Some {Name = "someone"; Tweet = "Hello, World!"})
  ]