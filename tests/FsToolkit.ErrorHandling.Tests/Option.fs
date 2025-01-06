module OptionTests

open System
#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Option

let map2Tests =
    testList "Option.map2 Tests" [
        testCase "map2 with two Some parts"
        <| fun _ ->
            Option.map2 (+) (Some 1) (Some 2)
            |> Expect.hasSomeValue 3

        testCase "map2 with (None, Some)"
        <| fun _ ->
            let opt = Option.map2 (+) None (Some 2)
            Expect.isNone opt "Should be None"

        testCase "map2 with (Some, None)"
        <| fun _ ->
            let opt = Option.map2 (+) (Some 1) None
            Expect.isNone opt "Should be None"

        testCase "map2 with (None, None)"
        <| fun _ ->
            let opt = Option.map2 (+) None None
            Expect.isNone opt "Should be None"
    ]

let map3Tests =
    testList "Option.map3 Tests" [
        testCase "map3 with (Some, Some, Some)"
        <| fun _ ->
            Option.map3 (fun x y z -> x + y + z) (Some 1) (Some 2) (Some 3)
            |> Expect.hasSomeValue 6

        testCase "map3 with (None, Some, Some)"
        <| fun _ ->
            let opt = Option.map3 (fun x y z -> x + y + z) None (Some 2) (Some 3)
            Expect.isNone opt "Should be None"

        testCase "map3 with (Some, None, Some)"
        <| fun _ ->
            let opt = Option.map3 (fun x y z -> x + y + z) (Some 1) None (Some 3)
            Expect.isNone opt "Should be None"

        testCase "map3 with (Some, Some, None)"
        <| fun _ ->
            let opt = Option.map3 (fun x y z -> x + y + z) (Some 1) (Some 2) None
            Expect.isNone opt "Should be None"

        testCase "map3 with (None, None, Some)"
        <| fun _ ->
            let opt = Option.map3 (fun x y z -> x + y + z) None None (Some 3)
            Expect.isNone opt "Should be None"

        testCase "map3 with (None, Some, None)"
        <| fun _ ->
            let opt = Option.map3 (fun x y z -> x + y + z) None (Some 2) None
            Expect.isNone opt "Should be None"

        testCase "map3 with (Some, None, None)"
        <| fun _ ->
            let opt = Option.map3 (fun x y z -> x + y + z) (Some 1) None None
            Expect.isNone opt "Should be None"

        testCase "map3 with (None, None, None)"
        <| fun _ ->
            let opt = Option.map3 (fun x y z -> x + y + z) None None None
            Expect.isNone opt "Should be None"
    ]

let ignoreTests =
    testList "Option.ignore Tests" [
        testCase "ignore with Some"
        <| fun _ ->
            Option.ignore (Some 1)
            |> Expect.hasSomeValue ()

        testCase "ignore with None"
        <| fun _ ->
            let opt = Option.ignore None
            Expect.isNone opt "Should be None"
    ]

let teeSomeTests =
    testList "teeSome Tests" [
        testCase "teeSome executes the function for Some"
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = 0

            let bar x =
                input <- x

                foo <- "bar"

            let opt = Option.teeSome bar (Some 42)
            Expect.hasSomeValue 42 opt
            Expect.equal foo "bar" ""
            Expect.equal input 42 ""

        testCase "teeSome ignores the function for None"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let opt = Option.teeSome bar None
            Expect.isNone opt "Should be None"
            Expect.equal foo "foo" ""
    ]

let teeNoneTests =
    testList "teeNone Tests" [
        testCase "teeNone executes the function for None"
        <| fun _ ->
            let mutable foo = "foo"

            let bar () = foo <- "bar"

            let opt = Option.teeNone bar None
            Expect.isNone opt "Should be None"
            Expect.equal foo "bar" ""

        testCase "teeNone ignores the function for Some"
        <| fun _ ->
            let mutable foo = "foo"

            let bar () = foo <- "bar"

            let opt = Option.teeNone bar (Some 42)
            Expect.hasSomeValue 42 opt
            Expect.equal foo "foo" ""
    ]

let teeIfTests =
    testList "teeIf Tests" [
        testCase "teeIf executes the function for Some, if predicate is true"
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = 0

            let bar x =
                input <- x

                foo <- "bar"

            let opt = Option.teeIf (fun x -> x > 0) bar (Some 42)
            Expect.hasSomeValue 42 opt
            Expect.equal foo "bar" ""
            Expect.equal input 42 ""

        testCase "teeIf ignores the function for Some, if predicate is false"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let opt = Option.teeIf (fun x -> x > 0) bar (Some -42)
            Expect.hasSomeValue -42 opt
            Expect.equal foo "foo" ""

        testCase "teeIf ignores the function for None"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let opt = Option.teeIf (fun x -> x > 0) bar None
            Expect.isNone opt "Should be None"
            Expect.equal foo "foo" ""
    ]

let sequenceAsyncTests =
    testList "Option.sequenceAsync Tests" [
        testCaseAsync "sequenceAsync returns the async value if Some"
        <| async {
            let optAsync =
                async { return "foo" }
                |> Some

            let! value =
                optAsync
                |> Option.sequenceAsync

            Expect.equal value (Some "foo") ""
        }

        testCaseAsync "sequenceAsync returns None if None"
        <| async {
            let optAsync = None

            let! value =
                optAsync
                |> Option.sequenceAsync

            Expect.equal value None ""
        }
    ]

let traverseAsyncTests =
    testList "Option.traverseAsync Tests" [
        testCaseAsync "traverseAsync returns the async value if Some"
        <| async {
            let optAsync = Some "foo"

            let optFunc =
                id
                >> Async.singleton

            let! value =
                (optFunc, optAsync)
                ||> Option.traverseAsync

            Expect.equal value (Some "foo") ""
        }

        testCaseAsync "traverseAsync returns None if None"
        <| async {
            let optAsync = None

            let optFunc =
                id
                >> Async.singleton

            let! value =
                (optFunc, optAsync)
                ||> Option.traverseAsync

            Expect.equal value None ""
        }
    ]

let traverseResultTests =
    testList "Option.traverseResult Tests" [
        testCase "traverseResult with Some of valid data"
        <| fun _ ->
            let (latitude, longitude) = (Some lat), (Some lng)

            latitude
            |> Option.traverseResult Latitude.TryCreate
            |> Expect.hasOkValue (Some validLat)

            longitude
            |> Option.traverseResult Longitude.TryCreate
            |> Expect.hasOkValue (Some validLng)
    ]


let tryParseTests =
    testList "Option.tryParse" [
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

            let parsedValue = Option.tryParse<Guid> (string expectedGuid)

            Expect.equal parsedValue (Some expectedGuid) "Should be same guid"
#endif
    ]

let tryGetValueTests =
    testList "Option.tryGetValue" [
#if !FABLE_COMPILER
        testCase "Can Parse int"
        <| fun _ ->
            let expectedValue = 3
            let expectedKey = "someId"
            let dictToWorkOn = dict [ (expectedKey, expectedValue) ]

            let actual =
                dictToWorkOn
                |> Option.tryGetValue expectedKey

            Expect.equal actual (Some expectedValue) "Should be some value"
#endif
    ]

let ofResultTests =
    testList "Option.ofResult Tests" [
        testCase "ofResult simple cases"
        <| fun _ ->
            Expect.equal (Option.ofResult (Ok 123)) (Some 123) "Ok int"
            Expect.equal (Option.ofResult (Ok "abc")) (Some "abc") "Ok string"
            Expect.equal (Option.ofResult (Error "x")) None "Error _"
    ]


let ofNullTests =
    testList "Option.ofNull Tests" [
        testCase "A not null value"
        <| fun _ ->
            let someValue = "hello"
            Expect.equal (Option.ofNull someValue) (Some someValue) ""
        testCase "A null value"
        <| fun _ ->
            let (someValue: string) = null
            Expect.equal (Option.ofNull someValue) (None) ""
    ]

let bindNullTests =
    testList "Option.bindNull Tests" [
        testCase "Some notNull"
        <| fun _ ->
            let value1 = Some "world"
            let someBinder _ = "hello"
            Expect.equal (Option.bindNull someBinder value1) (Some "hello") ""
        testCase "Some null"
        <| fun _ ->
            let value1 = Some "world"
            let someBinder _ = null
            Expect.equal (Option.bindNull someBinder value1) (None) ""
        testCase "None"
        <| fun _ ->
            let value1 = None
            let someBinder _ = "won't hit here"
            Expect.equal (Option.bindNull someBinder value1) (None) ""
    ]

let eitherTests =
    testList "Option.either Tests" [
        testCase "Some"
        <| fun _ ->
            let value1 = Some 5
            let f () = 42
            let add2 = (+) 2
            Expect.equal (Option.either add2 f value1) 7 ""
        testCase "None"
        <| fun _ ->
            let value1 = None
            let f () = 42
            let add2 = (+) 2
            Expect.equal (Option.either add2 f value1) 42 ""
    ]

let ofPairTests =
    testList "Option.ofPair Tests" [
        testCase "Int32.TryParse => Some Int32"
        <| fun _ ->
            let input = "1989"
            let pair = Int32.TryParse input
            Expect.equal (Option.ofPair pair) (Some 1989) ""
        testCase "Int32.TryParse => None"
        <| fun _ ->
            let input = "FsToolkit.ErrorHandling"
            let pair = Int32.TryParse input
            Expect.equal (Option.ofPair pair) (None) ""
        testCase "Int64.TryParse => Some Int64"
        <| fun _ ->
            let input = "1989"
            let pair = Int64.TryParse input
            Expect.equal (Option.ofPair pair) (Some 1989L) ""
        testCase "Int64.TryParse => None"
        <| fun _ ->
            let input = "FsToolkit.ErrorHandling"
            let pair = Int64.TryParse input
            Expect.equal (Option.ofPair pair) (None) ""
        testCase "Decimal.TryParse => Some Decimal"
        <| fun _ ->
            let input = "1989"
            let pair = Decimal.TryParse input
            Expect.equal (Option.ofPair pair) (Some 1989M) ""
        testCase "Decimal.TryParse => None"
        <| fun _ ->
            let input = "FsToolkit.ErrorHandling"
            let pair = Decimal.TryParse input
            Expect.equal (Option.ofPair pair) (None) ""
        testCase "Guid.TryParse => Some Guid"
        <| fun _ ->
            let guid = Guid.NewGuid()
            let input = guid.ToString()
            let pair = Guid.TryParse input
            Expect.equal (Option.ofPair pair) (Some guid) ""
        testCase "Guid.TryParse => None"
        <| fun _ ->
            let input = "FsToolkit.ErrorHandling"
            let pair = Guid.TryParse input
            Expect.equal (Option.ofPair pair) (None) ""
    ]

let optionOperatorsTests =
    testList "Option Operators Tests" [
        testCase "bind operator"
        <| fun _ ->
            let evenInt x = if x % 2 = 0 then Some x else None

            let tryParseInt (x: string) =
                match Int32.TryParse x with
                | true, value -> Some value
                | _ -> None

            let tryParseEvenInt str =
                tryParseInt str
                >>= evenInt


            tryParseEvenInt "2"
            |> Expect.hasSomeValue 2
    ]

let allTests =
    testList "Option Tests" [
        sequenceAsyncTests
        traverseAsyncTests
        traverseResultTests
        tryParseTests
        tryGetValueTests
        ofResultTests
        ofNullTests
        bindNullTests
        eitherTests
        ofPairTests
        optionOperatorsTests
        map2Tests
        map3Tests
        ignoreTests
        teeSomeTests
        teeNoneTests
        teeIfTests
    ]
