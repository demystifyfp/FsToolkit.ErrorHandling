module OptionTests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling


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

let allTests = testList "Option Tests" [
  traverseResultTests
]