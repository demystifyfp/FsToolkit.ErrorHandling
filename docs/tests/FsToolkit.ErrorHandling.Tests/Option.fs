module OptionTests

open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling


[<Tests>]
let traverseResultTests =
  testList "Option.traverseResult Tests" [
    testCase "traverseResult with Some of valid data" <| fun _ ->
      let (latitude, longitude) =
        (Some lat), (Some lng)
      
      latitude 
      |> Option.traverseResult Latitude.TryCreate
      |> Expect.hasOkValue (Some validLat)
      
      longitude 
      |> Option.traverseResult Longitude.TryCreate
      |> Expect.hasOkValue (Some validLng)
  ]