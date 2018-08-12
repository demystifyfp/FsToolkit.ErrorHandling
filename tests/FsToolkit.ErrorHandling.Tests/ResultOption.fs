module ResultOptionTests

open Expecto
open TestData
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.ResultOptionComputationExpression


[<Tests>]
let map2Tests =
  testList "ResultOption.map2 Tests" [

    testCase "map2 with Ok(Some x) Ok(Some y)" <| fun _ ->
      ResultOption.map2 (+) (Ok (Some 1)) (Ok (Some 2))
      |> Expect.hasOkValue (Some 3)

    testCase "map2 with Ok(Some x) Ok(None)" <| fun _ ->
      ResultOption.map2 (+) (Ok (Some 1)) (Ok None)
      |> Expect.hasOkValue None

    testCase "map2 with Ok(None) Ok(None)" <| fun _ ->
      ResultOption.map2 (+) (Ok None) (Ok None)
      |> Expect.hasOkValue None

    testCase "map2 with Error(Some x) Error(Some x)" <| fun _ ->
      ResultOption.map2 (+) (Error (Some 1)) (Error (Some 2))
      |> Expect.hasErrorValue (Some 1)
    
    testCase "map2 with Ok(Some x) Error(Some x)" <| fun _ ->
      ResultOption.map2 (+) (Ok (Some 1)) (Error (Some 2))
      |> Expect.hasErrorValue (Some 2)
  ]