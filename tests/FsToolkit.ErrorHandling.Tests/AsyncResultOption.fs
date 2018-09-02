module AsyncResultOptionTests

open Expecto
open SampleDomain
open TestData
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