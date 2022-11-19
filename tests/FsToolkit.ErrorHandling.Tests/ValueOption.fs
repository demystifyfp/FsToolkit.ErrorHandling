module ValueOptionTests


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

#if !FABLE_COMPILER

let traverseResultTests =
    testList "ValueOption.traverseResult Tests" [
        testCase "traverseResult with ValueSome of valid data"
        <| fun _ ->
            let (latitude, longitude) = (ValueSome lat), (ValueSome lng)

            latitude
            |> ValueOption.traverseResult Latitude.TryCreate
            |> Expect.hasOkValue (ValueSome validLat)

            longitude
            |> ValueOption.traverseResult Longitude.TryCreate
            |> Expect.hasOkValue (ValueSome validLng)
    ]


let tryParseTests =
    testList "ValueOption.tryParse" [
        testCase "Can Parse int"
        <| fun _ ->
            let expected = 3

            let actual = ValueOption.tryParse<int> (string expected)

            Expect.equal actual (ValueSome expected) "Should be parsed"

        testCase "Can Parse double"
        <| fun _ ->
            let expected: float = 3.0

            let actual = ValueOption.tryParse<float> (string expected)

            Expect.equal actual (ValueSome expected) "Should be parsed"

        testCase "Can Parse Guid"
        <| fun _ ->
            let expectedGuid = Guid.NewGuid()

            let parsedValue = ValueOption.tryParse<Guid> (string expectedGuid)

            Expect.equal parsedValue (ValueSome expectedGuid) "Should be same guid"

    ]


let tryGetValueTests =
    testList "ValueOption.tryGetValue" [
        testCase "Can Parse int"
        <| fun _ ->
            let expectedValue = 3
            let expectedKey = "someId"
            let dictToWorkOn = dict [ (expectedKey, expectedValue) ]

            let actual =
                dictToWorkOn
                |> ValueOption.tryGetValue expectedKey

            Expect.equal actual (ValueSome expectedValue) "Should be some value"
    ]

let ofResultTests =
    testList "ValueOption.ofResult Tests" [
        testCase "ofResult simple cases"
        <| fun _ ->
            Expect.equal (ValueOption.ofResult (Ok 123)) (ValueSome 123) "Ok int"
            Expect.equal (ValueOption.ofResult (Ok "abc")) (ValueSome "abc") "Ok string"
            Expect.equal (ValueOption.ofResult (Error "x")) ValueNone "Error _"
    ]

let ofNullTests =
    testList "ValueOption.ofNull Tests" [
        testCase "A not null value"
        <| fun _ ->
            let someValue = "hello"
            Expect.equal (ValueOption.ofNull someValue) (ValueSome someValue) ""
        testCase "A null value"
        <| fun _ ->
            let (someValue: string) = null
            Expect.equal (ValueOption.ofNull someValue) (ValueNone) ""
    ]

let bindNullTests =
    testList "ValueOption.bindNull Tests" [
        testCase "ValueSome notNull"
        <| fun _ ->
            let value1 = ValueSome "world"
            let someBinder _ = "hello"
            Expect.equal (ValueOption.bindNull someBinder value1) (ValueSome "hello") ""
        testCase "ValueSome null"
        <| fun _ ->
            let value1 = ValueSome "world"
            let someBinder _ = null
            Expect.equal (ValueOption.bindNull someBinder value1) (ValueNone) ""
        testCase "ValueNone"
        <| fun _ ->
            let value1 = ValueNone
            let someBinder _ = "won't hit here"
            Expect.equal (ValueOption.bindNull someBinder value1) (ValueNone) ""
    ]


let eitherTests =
    testList "ValueOption.either Tests" [
        testCase "Some"
        <| fun _ ->
            let value1 = ValueSome 5
            let f () = 42
            let add2 = (+) 2
            Expect.equal (ValueOption.either add2 f value1) 7 ""
        testCase "None"
        <| fun _ ->
            let value1 = ValueNone
            let f () = 42
            let add2 = (+) 2
            Expect.equal (ValueOption.either add2 f value1) 42 ""
    ]

let allTests =
    testList "ValueOption Tests" [
        traverseResultTests
        tryParseTests
        tryGetValueTests
        ofResultTests
        ofNullTests
        bindNullTests
        eitherTests
    ]
#else

let allTests = testList "ValueOption Tests" []
#endif
