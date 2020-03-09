module SeqTests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open SampleDomain
open TestData
open TestHelpers
open System
open FsToolkit.ErrorHandling

open Fable.Core
let traverseResultTests =
  testList "Seq.traverseResultM Tests" [
    testCase "traverseResultM with a seq of valid data" <| fun _ ->
      let tweets = seq [ yield "Hi"; yield "Hello"; yield "Hola" ]
      let expected = Seq.map tweet tweets |> List.ofSeq
      let actual = Seq.traverseResultM Tweet.TryCreate tweets |> Result.defaultWith (fun _ -> failwith "") |> List.ofSeq
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "traverseResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet] |> Seq.ofList
      let actual = Seq.traverseResultM Tweet.TryCreate tweets
      Expect.equal actual (Error emptyTweetErrMsg) "traverse the seq and return the first error"
  ]

let sequenceResultMTests =
  testList "Seq.sequenceResultM Tests" [
    testCase "traverseResultM with a seq of valid data" <| fun _ ->
      let tweets = [ "Hi"; "Hello"; "Hola" ] |> List.toSeq
      let expected = Seq.map tweet tweets |> Seq.toList
      let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets)  |> Result.defaultWith(fun _ -> failwith "") |> Seq.toList
      Expect.equal actual expected "Should have a seq of valid tweets"

    testCase "sequenceResultM with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet]
      let actual = Seq.sequenceResultM (Seq.map Tweet.TryCreate tweets) 
      Expect.equal actual (Error emptyTweetErrMsg) "traverse the list and return the first error"
  ]

let traverseResultATests =
  testList "Seq.traverseResultA Tests" [
    testCase "traverseResultA with a seq of valid data" <| fun _ ->
      let tweets = seq [ yield "Hi"; yield "Hello"; yield "Hola" ]

      let expected = Seq.map tweet tweets |> Seq.toList |> Ok
      let actual = Seq.traverseResultA Tweet.TryCreate tweets |> Result.map(Seq.toList)
      Expect.equal actual expected (sprintf "Should have a list of valid tweets. actual was %A" actual)

    testCase "traverseResultA with few invalid data" <| fun _ ->
      let tweets = [ ""; "Hello"; aLongerInvalidTweet ] |> List.toSeq
      let actual = Seq.traverseResultA Tweet.TryCreate tweets
      Expect.equal actual (Error [emptyTweetErrMsg;longerTweetErrMsg]) "traverse the list and return all the errors"
  ]


let sequenceResultATests =
  testList "Seq.sequenceResultA Tests" [
    testCase "traverseResultA with a seq of valid data" <| fun _ ->
      let tweets = ["Hi"; "Hello"; "Hola"] |> List.toSeq
      let expected = Seq.map tweet tweets |> List.ofSeq
      let actual =
          Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets)
          |> Result.defaultWith(fun _ -> failwith "")
          |> Seq.toList
      Expect.equal actual expected "Should have a list of valid tweets"

    testCase "sequenceResultA with few invalid data" <| fun _ ->
      let tweets = [""; "Hello"; aLongerInvalidTweet] |> Seq.ofList
      let actual = Seq.sequenceResultA (Seq.map Tweet.TryCreate tweets) 
      Expect.equal actual (Error [emptyTweetErrMsg;longerTweetErrMsg]) "traverse the list and return all the errors"
  ]

let traverseValidationATests =
  testList "Seq.traverseValidationA Tests" [
    testCase "traverseValidationA with a seq of valid data" <| fun _ ->
      let tweets = seq [yield "Hi"; yield "Hello"; yield "Hola"]
      let expected = Seq.map tweet tweets |> Ok
      let actual = Seq.traverseValidationA (Tweet.TryCreate >> (Result.mapError List.singleton)) tweets
      Expect.equal (Result.map(List.ofSeq) actual) (Result.map (List.ofSeq) expected) "Should have a list of valid tweets"

    testCase "traverseValidationA with few invalid data" <| fun _ ->
      let tweets = seq [yield ""; yield "Hello"; yield aLongerInvalidTweet]
      let actual = Seq.traverseValidationA (Tweet.TryCreate >> (Result.mapError List.singleton)) tweets
      Expect.equal actual (Error [longerTweetErrMsg;emptyTweetErrMsg]) "traverse the list and return all the errors"
  ]

let sequenceValidationATests =
  let tryCreateTweet = Tweet.TryCreate >> (Result.mapError List.singleton)
  testList "Seq.sequenceValidationA Tests" [
    testCase "traverseValidation with a list of valid data" <| fun _ ->
      let tweets = seq [yield "Hi"; yield "Hello"; yield "Hola"]
      let expected = Seq.map tweet tweets |> Ok
      let actual = Seq.sequenceValidationA (Seq.map tryCreateTweet tweets) 
      Expect.equal (Result.map(List.ofSeq) actual) (Result.map (List.ofSeq) expected) "Should have a list of valid tweets"

    testCase "sequenceValidationM with few invalid data" <| fun _ ->
      let tweets = seq [ yield ""; yield "Hello"; yield aLongerInvalidTweet]
      let actual = Seq.sequenceValidationA (Seq.map tryCreateTweet tweets) 
      Expect.equal actual (Error [longerTweetErrMsg;emptyTweetErrMsg]) "traverse the list and return all the errors"
  ]


let userId1 = Guid.NewGuid()
let userId2 = Guid.NewGuid()
let userId3 = Guid.NewGuid()
let userId4 = Guid.NewGuid()

let traverseAsyncResultMTests =
  
  let userIds = Seq.map UserId [userId1;userId2;userId3]

  testList "Seq.traverseAsyncResultM Tests" [
    testCaseAsync "traverseAsyncResultM with a list of valid data" <| async {
      let expected = [();();()]
      let actual = 
        Seq.traverseAsyncResultM (notifyNewPostSuccess (PostId newPostId)) userIds
      do! Expect.hasAsyncOkValue (expected) (AsyncResult.map (Seq.toList) actual)
    }

    testCaseAsync "traverseResultA with few invalid data" <| async {
      let expected = sprintf "error: %s" (userId1.ToString())
      let actual = 
        Seq.traverseAsyncResultM (notifyNewPostFailure (PostId newPostId)) userIds
      do! Expect.hasAsyncErrorValue expected actual
    }
  ]

let notifyFailure (PostId _) (UserId uId) = async {
  if (uId = userId1 || uId = userId3) then
    return sprintf "error: %s" (uId.ToString()) |> Error
  else
    return Ok ()
}


let traverseAsyncResultATests =
  let userIds = Seq.map UserId [userId1;userId2;userId3;userId4]
  testList "Seq.traverseAsyncResultA Tests" [
    testCaseAsync "traverseAsyncResultA with a list of valid data" <| async {
      let expected = [();();();()]
      let actual = 
        Seq.traverseAsyncResultA (notifyNewPostSuccess (PostId newPostId)) userIds
      do! Expect.hasAsyncOkValue expected (AsyncResult.map (Seq.toList) actual)
    }

    testCaseAsync "traverseResultA with few invalid data" <| async {
      let expected = [sprintf "error: %s" (userId1.ToString())
                      sprintf "error: %s" (userId3.ToString())]
      let actual = 
        Seq.traverseAsyncResultA (notifyFailure (PostId newPostId)) userIds
      do! Expect.hasAsyncErrorValue expected actual
    }
  ]


let sequenceAsyncResultMTests =
  let userIds = Seq.map UserId [userId1;userId2;userId3;userId4]
  testList "Seq.sequenceAsyncResultM Tests" [
    testCaseAsync "sequenceAsyncResultM with a list of valid data" <| async {
      let expected = [();();();()]
      let actual = 
        Seq.map (notifyNewPostSuccess (PostId newPostId)) userIds
        |> Seq.sequenceAsyncResultM
      do! Expect.hasAsyncOkValue expected (AsyncResult.map (Seq.toList) actual)
    }
    
    testCaseAsync "sequenceAsyncResultM with few invalid data" <| async {
      let expected = sprintf "error: %s" (userId1.ToString())
      let actual = 
        Seq.map (notifyFailure (PostId newPostId)) userIds
        |> Seq.sequenceAsyncResultM
      do! Expect.hasAsyncErrorValue expected actual
    }
  ]


let sequenceAsyncResultATests =
  let userIds = Seq.map UserId [userId1;userId2;userId3;userId4]
  testList "Seq.sequenceAsyncResultA Tests" [
    testCaseAsync "sequenceAsyncResultA with a list of valid data" <| async {
      let expected = [();();();()]
      let actual = 
        Seq.map (notifyNewPostSuccess (PostId newPostId)) userIds
        |> Seq.sequenceAsyncResultA
      do! Expect.hasAsyncOkValue expected (AsyncResult.map (Seq.toList) actual)
    }
    
    testCaseAsync "sequenceAsyncResultA with few invalid data" <| async {
      let expected = [sprintf "error: %s" (userId1.ToString())
                      sprintf "error: %s" (userId3.ToString())]
      let actual = 
        Seq.map (notifyFailure (PostId newPostId)) userIds
        |> Seq.sequenceAsyncResultA
      do! Expect.hasAsyncErrorValue expected actual
    }
  ]


let allTests = testList "Seq Tests" [
  traverseResultTests
  sequenceResultMTests
  traverseResultATests
  sequenceResultATests
  traverseValidationATests
  sequenceValidationATests
  traverseAsyncResultMTests
  traverseAsyncResultATests
  sequenceAsyncResultMTests
  sequenceAsyncResultATests
]
