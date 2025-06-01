module ValueOptionTests


open System


#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling


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
            let (someValue: StringNull) = null
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

let ofPairTests =
    testList "ValueOption.ofPair Tests" [
        testCase "Int32.TryParse => ValueSome Int32"
        <| fun _ ->
            let input = "1989"
            let pair = Int32.TryParse input
            Expect.equal (ValueOption.ofPair pair) (ValueSome 1989) ""
        testCase "Int32.TryParse => ValueNone"
        <| fun _ ->
            let input = "FsToolkit.ErrorHandling"
            let pair = Int32.TryParse input
            Expect.equal (ValueOption.ofPair pair) (ValueNone) ""
        testCase "Int64.TryParse => ValueSome Int64"
        <| fun _ ->
            let input = "1989"
            let pair = Int64.TryParse input
            Expect.equal (ValueOption.ofPair pair) (ValueSome 1989L) ""
        testCase "Int64.TryParse => ValueNone"
        <| fun _ ->
            let input = "FsToolkit.ErrorHandling"
            let pair = Int64.TryParse input
            Expect.equal (ValueOption.ofPair pair) (ValueNone) ""
        testCase "Decimal.TryParse => ValueSome Decimal"
        <| fun _ ->
            let input = "1989"
            let pair = Decimal.TryParse input
            Expect.equal (ValueOption.ofPair pair) (ValueSome 1989M) ""
        testCase "Decimal.TryParse => ValueNone"
        <| fun _ ->
            let input = "FsToolkit.ErrorHandling"
            let pair = Decimal.TryParse input
            Expect.equal (ValueOption.ofPair pair) (ValueNone) ""
        testCase "Guid.TryParse => ValueSome Guid"
        <| fun _ ->
            let guid = Guid.NewGuid()
            let input = guid.ToString()
            let pair = Guid.TryParse input
            Expect.equal (ValueOption.ofPair pair) (ValueSome guid) ""
        testCase "Guid.TryParse => ValueNone"
        <| fun _ ->
            let input = "FsToolkit.ErrorHandling"
            let pair = Guid.TryParse input
            Expect.equal (ValueOption.ofPair pair) (ValueNone) ""
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
        ofPairTests
    ]
