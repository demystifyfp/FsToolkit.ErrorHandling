module Result

open Hopac
open Expecto
open FsToolkit.ErrorHandling


[<Tests>]
let sequenceJobTests =
  testList "sequenceJob Tests" [
    testCase "sequenceJob returns the job value if Ok" <| fun _ ->
      let resJob = job { return "foo" } |> Ok
      let value = resJob |> Result.sequenceJob |> run
      Expect.equal value (Ok "foo") ""

    testCase "sequenceJob returns the error value if Error" <| fun _ ->
      let resJob = Error "foo"
      let value = resJob |> Result.sequenceJob |> run
      Expect.equal value (Error "foo") ""
  ]
