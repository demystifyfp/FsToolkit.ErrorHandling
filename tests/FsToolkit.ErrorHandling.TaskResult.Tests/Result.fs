module Result

open FSharp.Control.Tasks.V2.ContextInsensitive
open Expecto
open FsToolkit.ErrorHandling


[<Tests>]
let sequenceTaskTests =
  testList "sequenceTask Tests" [
    testCase "sequenceTask returns the task value if Ok" <| fun _ ->
      let resTask = task { return "foo" } |> Ok
      let value = (resTask |> Result.sequenceTask).Result
      Expect.equal value (Ok "foo") ""

    testCase "sequenceTask returns the error value if Error" <| fun _ ->
      let resTask = Error "foo"
      let value = (resTask |> Result.sequenceTask).Result
      Expect.equal value (Error "foo") ""
  ]
