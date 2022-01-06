module OptionTests

open System
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
    testList
        "Option.traverseResult Tests"
        [ testCase "traverseResult with Some of valid data"
          <| fun _ ->
              let (latitude, longitude) = (Some lat), (Some lng)

              latitude
              |> Option.traverseResult Latitude.TryCreate
              |> Expect.hasOkValue (Some validLat)

              longitude
              |> Option.traverseResult Longitude.TryCreate
              |> Expect.hasOkValue (Some validLng) ]


let tryParseTests =
    testList
        "Option.tryParse"
        [
#if !FABLE_COMPILER
          testCase "Can Parse int"
          <| fun _ ->
              let expected = 3
              let actual = Option.tryParse<int> (string expected)
              Expect.equal actual (Some expected) "Should be parsed"

          testCase "Can Parse double"
          <| fun _ ->
              let expected: float = 3.0
              let actual = Option.tryParse<float> (string expected)
              Expect.equal actual (Some expected) "Should be parsed"

          testCase "Can Parse Guid"
          <| fun _ ->
              let expectedGuid = Guid.NewGuid()

              let parsedValue =
                  Option.tryParse<Guid> (string expectedGuid)

              Expect.equal parsedValue (Some expectedGuid) "Should be same guid"
#endif
        ]

let tryGetValueTests =
    testList
        "Option.tryGetValue"
        [
#if !FABLE_COMPILER
          testCase "Can Parse int"
          <| fun _ ->
              let expectedValue = 3
              let expectedKey = "someId"
              let dictToWorkOn = dict [ (expectedKey, expectedValue) ]

              let actual =
                  dictToWorkOn |> Option.tryGetValue expectedKey

              Expect.equal actual (Some expectedValue) "Should be some value"
#endif
        ]

let ofResultTests =
    testList
        "Option.ofResult Tests"
        [ testCase "ofResult simple cases"
          <| fun _ ->
              Expect.equal (Option.ofResult (Ok 123)) (Some 123) "Ok int"
              Expect.equal (Option.ofResult (Ok "abc")) (Some "abc") "Ok string"
              Expect.equal (Option.ofResult (Error "x")) None "Error _" ]


let allTests =
    testList
        "Option Tests"
        [ traverseResultTests
          tryParseTests
          tryGetValueTests
          ofResultTests ]
