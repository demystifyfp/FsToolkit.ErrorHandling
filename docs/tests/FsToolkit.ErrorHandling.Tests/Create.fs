module CreateTests

open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling.Create


[<Tests>]
let tryCreateTests =
  testList "tryCreate Tests" [
    testCase "tryCreate happy path" <| fun _ ->
      tryCreate lat
      |> Expect.hasOkValue validLat
    
    testCase "tryCreate error path" <| fun _ ->
      let r : Result<Latitude, string> = tryCreate 200.
      Expect.hasErrorValue invalidLatMsg r
  ]

[<Tests>]
let tryCreate2Tests =
  testList "tryCreate2 Tests" [
    testCase "tryCreate2 happy path" <| fun _ ->
      tryCreate2 "lat" lat
      |> Expect.hasOkValue validLat
    
    testCase "tryCreate error path" <| fun _ ->
      let r : Result<Latitude, (string * string)> = tryCreate2 "lat" 200.
      Expect.hasErrorValue ("lat", invalidLatMsg) r
  ]