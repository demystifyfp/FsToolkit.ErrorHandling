module ResultTests


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
open FsToolkit.ErrorHandling.Operator.Result


let resultIsOk =
    testList "Result.isOk Tests" [
        testCase "Is Ok true"
        <| fun _ -> Expect.isTrue (Result.isOk (Ok())) "should be true"
        testCase "Is Error false"
        <| fun _ -> Expect.isFalse (Result.isOk (Error())) "should be false"
    ]

let resultIsError =
    testList "Result.isError Tests" [
        testCase "Is Ok false"
        <| fun _ -> Expect.isFalse (Result.isError (Ok())) "should be false"
        testCase "Is Error true"
        <| fun _ -> Expect.isTrue (Result.isError (Error())) "should be true"
    ]

let resultEither =
    testList "Result.either Tests" [
        testCase "Ok calls okF"
        <| fun _ ->
            let okF x = x + 1
            let errorF x = x - 1
            let actual = Result.either okF errorF (Ok 1)
            Expect.equal actual 2 "Should be 2"
        testCase "Error calls errorF"
        <| fun _ ->
            let okF x = x + 1
            let errorF x = x - 1
            let actual = Result.either okF errorF (Error 1)
            Expect.equal actual 0 "Should be 0"
    ]


let resultEitherMap =
    testList "Result.eitherMap Tests" [
        testCase "Ok calls okF"
        <| fun _ ->
            let okF x = x + 1
            let errorF x = x - 1
            let actual = Result.eitherMap okF errorF (Ok 1)
            Expect.equal actual (Ok 2) "Should be 2"
        testCase "Error calls errorF"
        <| fun _ ->
            let okF x = x + 1
            let errorF x = x - 1
            let actual = Result.eitherMap okF errorF (Error 1)
            Expect.equal actual (Error 0) "Should be 0"
    ]


let map2Tests =
    testList "Result.map2 Tests" [
        testCase "map2 with two ok parts"
        <| fun _ ->
            Result.map2 location validLatR validLngR
            |> Expect.hasOkValue validLocation

        testCase "map2 with one Error and one Ok parts"
        <| fun _ ->
            Result.map2 location invalidLatR validLngR
            |> Expect.hasErrorValue invalidLatMsg

        testCase "map2 with one Ok and one Error parts"
        <| fun _ ->
            Result.map2 location validLatR invalidLngR
            |> Expect.hasErrorValue invalidLngMsg

        testCase "map2 with two Error parts"
        <| fun _ ->
            Result.map2 location invalidLatR invalidLngR
            |> Expect.hasErrorValue invalidLatMsg
    ]


let map3Tests =
    testList "Result.map3 Tests" [
        testCase "map3 with three ok parts"
        <| fun _ ->

            Result.map3 createPostRequest validLatR validLngR validTweetR
            |> Expect.hasOkValue validCreatePostRequest

        testCase "map3 with (Error, Ok, Ok)"
        <| fun _ ->
            Result.map3 createPostRequest invalidLatR validLngR validTweetR
            |> Expect.hasErrorValue invalidLatMsg

        testCase "map3 with (Ok, Error, Ok)"
        <| fun _ ->
            Result.map3 createPostRequest validLatR invalidLngR validTweetR
            |> Expect.hasErrorValue invalidLngMsg


        testCase "map3 with (Ok, Ok, Error)"
        <| fun _ ->
            Result.map3 createPostRequest validLatR validLngR emptyInvalidTweetR
            |> Expect.hasErrorValue emptyTweetErrMsg

        testCase "map3 with (Error, Error, Error)"
        <| fun _ ->
            Result.map3 createPostRequest invalidLatR invalidLngR emptyInvalidTweetR
            |> Expect.hasErrorValue invalidLatMsg
    ]


let applyTests =

    testList "Result.apply tests" [
        testCase "apply with Ok"
        <| fun _ ->
            Tweet.TryCreate "foobar"
            |> Result.apply (Ok remainingCharacters)
            |> Expect.hasOkValue 274

        testCase "apply with Error"
        <| fun _ ->
            Result.apply (Ok remainingCharacters) emptyInvalidTweetR
            |> Expect.hasErrorValue emptyTweetErrMsg
    ]

let ofChoiceTests =

    testList "Result.ofChoice tests" [
        testCase "ofChoice with Choice1Of2"
        <| fun _ ->
            Result.ofChoice (Choice1Of2 1)
            |> Expect.hasOkValue 1

        testCase "ofChoice with Choice2Of2"
        <| fun _ ->
            Result.ofChoice (Choice2Of2 1)
            |> Expect.hasErrorValue 1
    ]


let resultCETests =
    testList "result Computation Expression tests" [
        testCase "bind with all Ok"
        <| fun _ ->
            let createPostRequest =
                result {
                    let! lat = validLatR
                    let! lng = validLngR
                    let! tweet = validTweetR
                    return createPostRequest lat lng tweet
                }

            Expect.hasOkValue validCreatePostRequest createPostRequest

        testCase "bind with Error"
        <| fun _ ->
            let post =
                result {
                    let! lat = invalidLatR
                    // Tests.failtestf "this should not get executed!"
                    let! lng = validLngR
                    let! tweet = validTweetR
                    return createPostRequest lat lng tweet
                }

            post
            |> Expect.hasErrorValue invalidLatMsg
    ]


let resultOperatorsTests =

    testList "Result Operators Tests" [
        testCase "map & apply operators"
        <| fun _ ->
            createPostRequest
            <!> validLatR
            <*> validLngR
            <*> validTweetR
            |> Expect.hasOkValue validCreatePostRequest

        testCase "bind operator"
        <| fun _ ->
            validLatR
            >>= (fun lat ->
                validLngR
                >>= (fun lng ->
                    validTweetR
                    >>= (fun tweet -> Ok(createPostRequest lat lng tweet))
                )
            )
            |> Expect.hasOkValue validCreatePostRequest
    ]


let tryCreateTests =
    testList "tryCreate Tests" [
        testCase "tryCreate happy path"
        <| fun _ ->
            Result.tryCreate "lat" lat
            |> Expect.hasOkValue validLat

        testCase "tryCreate error path"
        <| fun _ ->
            let r: Result<Latitude, (string * string)> = Result.tryCreate "lat" 200.
            Expect.hasErrorValue ("lat", invalidLatMsg) r
    ]

let orElseTests =
    testList "Result.orElseWith Tests" [
        testCase "Ok Ok takes first Ok"
        <| fun _ ->
            (Ok "First")
            |> Result.orElse (Ok "Second")
            |> Expect.hasOkValue "First"
        testCase "Ok Error takes first Ok"
        <| fun _ ->
            (Ok "First")
            |> Result.orElse (Error "Second")
            |> Expect.hasOkValue "First"
        testCase "Error Ok takes second Ok"
        <| fun _ ->
            (Error "First")
            |> Result.orElse (Ok "Second")
            |> Expect.hasOkValue "Second"
        testCase "Error Error takes second error"
        <| fun _ ->
            (Error "First")
            |> Result.orElse (Error "Second")
            |> Expect.hasErrorValue "Second"
    ]

let orElseWithTests =
    testList "Result.orElse Tests" [
        testCase "Ok Ok takes first Ok"
        <| fun _ ->
            (Ok "First")
            |> Result.orElseWith (fun _ -> Ok "Second")
            |> Expect.hasOkValue "First"
        testCase "Ok Error takes first Ok"
        <| fun _ ->
            (Ok "First")
            |> Result.orElseWith (fun _ -> Error "Second")
            |> Expect.hasOkValue "First"
        testCase "Error Ok takes second Ok"
        <| fun _ ->
            (Error "First")
            |> Result.orElseWith (fun _ -> Ok "Second")
            |> Expect.hasOkValue "Second"
        testCase "Error Error takes second error"
        <| fun _ ->
            (Error "First")
            |> Result.orElseWith (fun _ -> Error "Second")
            |> Expect.hasErrorValue "Second"
    ]


let ignoreTests =
    testList "ignore Tests" [
        testCase "ignore with Ok"
        <| fun _ ->
            Result.ignore (Ok true)
            |> Expect.hasOkValue ()

        testCase "ignore with error"
        <| fun _ ->
            Result.ignore (Error "error")
            |> Expect.hasErrorValue "error"

        testCase "can call ignore without type parameters"
        <| fun _ -> ignore Result.ignore

        testCase "can call ignore with type parameters"
        <| fun _ -> ignore<Result<int, string> -> Result<unit, string>> Result.ignore<int, string>
    ]

let err = "foobar"


let requireTrueTests =
    testList "requireTrue Tests" [
        testCase "requireTrue happy path"
        <| fun _ ->
            Result.requireTrue err true
            |> Expect.hasOkValue ()

        testCase "requireTrue error path"
        <| fun _ ->
            Result.requireTrue err false
            |> Expect.hasErrorValue err
    ]


let requireFalseTests =
    testList "requireFalse Tests" [
        testCase "requireFalse happy path"
        <| fun _ ->
            Result.requireFalse err false
            |> Expect.hasOkValue ()

        testCase "requireFalse error path"
        <| fun _ ->
            Result.requireFalse err true
            |> Expect.hasErrorValue err
    ]


let requireSomeTests =
    testList "requireSome Tests" [
        testCase "requireSome happy path"
        <| fun _ ->
            Result.requireSome err (Some 42)
            |> Expect.hasOkValue 42

        testCase "requireSome error path"
        <| fun _ ->
            Result.requireSome err None
            |> Expect.hasErrorValue err
    ]

let requireNotNullTests =
    testList "requireNotNull Tests" [
        testCase "requireNotNull happy path"
        <| fun _ ->
            Result.requireNotNull err ("test")
            |> Expect.hasOkValue "test"

        testCase "requireNotNull error path"
        <| fun _ ->
            Result.requireNotNull err (null)
            |> Expect.hasErrorValue err
    ]

let requireNoneTests =
    testList "requireNone Tests" [
        testCase "requireNone happy path"
        <| fun _ ->
            Result.requireNone err None
            |> Expect.hasOkValue ()

        testCase "requireNone error path"
        <| fun _ ->
            Result.requireNone err (Some 42)
            |> Expect.hasErrorValue err
    ]

let requireValueSomeTests =
    testList "requireValueSome Tests" [
        testCase "requireValueSome happy path"
        <| fun _ ->
            Result.requireValueSome err (ValueSome 42)
            |> Expect.hasOkValue 42

        testCase "requireValueSome error path"
        <| fun _ ->
            Result.requireValueSome err ValueNone
            |> Expect.hasErrorValue err
    ]

let requireValueNoneTests =
    testList "requireValueNone Tests" [
        testCase "requireValueNone happy path"
        <| fun _ ->
            Result.requireValueNone err ValueNone
            |> Expect.hasOkValue ()

        testCase "requireValueNone error path"
        <| fun _ ->
            Result.requireValueNone err (ValueSome 42)
            |> Expect.hasErrorValue err
    ]

let requireEqualToTests =
    testList "requireEqualTo Tests" [
        testCase "requireEqualTo happy path"
        <| fun _ ->
            Result.requireEqualTo 42 err 42
            |> Expect.hasOkValue ()

        testCase "requireEqualTo error path"
        <| fun _ ->
            Result.requireEqualTo 42 err 43
            |> Expect.hasErrorValue err
    ]


let requireEqualTests =
    testList "requireEqual Tests" [
        testCase "requireEqual happy path"
        <| fun _ ->
            Result.requireEqual 42 42 err
            |> Expect.hasOkValue ()

        testCase "requireEqual error path"
        <| fun _ ->
            Result.requireEqual 42 43 err
            |> Expect.hasErrorValue err
    ]


let requireEmptyTests =
    testList "requireEmpty Tests" [
        testCase "requireEmpty happy path"
        <| fun _ ->
            Result.requireEmpty err []
            |> Expect.hasOkValue ()

        testCase "requireEmpty error path"
        <| fun _ ->
            Result.requireEmpty err [ 42 ]
            |> Expect.hasErrorValue err
    ]


let requireNotEmptyTests =
    testList "requireNotEmpty Tests" [
        testCase "requireNotEmpty happy path"
        <| fun _ ->
            Result.requireNotEmpty err [ 42 ]
            |> Expect.hasOkValue ()

        testCase "requireNotEmpty error path"
        <| fun _ ->
            Result.requireNotEmpty err []
            |> Expect.hasErrorValue err
    ]


let requireHeadTests =
    testList "requireHead Tests" [
        testCase "requireHead happy path"
        <| fun _ ->
            Result.requireHead err [ 42 ]
            |> Expect.hasOkValue 42

        testCase "requireHead error path"
        <| fun _ ->
            Result.requireHead err []
            |> Expect.hasErrorValue err
    ]


let setErrorTests =
    testList "setError Tests" [
        testCase "setError replaces a any error value with a custom error value"
        <| fun _ ->
            Result.setError err (Error "foo")
            |> Expect.hasErrorValue err

        testCase "setError does not change an ok value"
        <| fun _ ->
            Result.setError err (Ok 42)
            |> Expect.hasOkValue 42
    ]


let withErrorTests =
    testList "withError Tests" [
        testCase "withError replaces the unit error value with a custom error value"
        <| fun _ ->
            Result.withError err (Error())
            |> Expect.hasErrorValue err

        testCase "withError does not change an ok value"
        <| fun _ ->
            Result.withError err (Ok 42)
            |> Expect.hasOkValue 42
    ]


let defaultValueTests =
    testList "defaultValue Tests" [
        testCase "defaultValue returns the ok value"
        <| fun _ ->
            let v = Result.defaultValue 43 (Ok 42)
            Expect.equal v 42 ""

        testCase "defaultValue returns the given value for Error"
        <| fun _ ->
            let v = Result.defaultValue 43 (Error err)
            Expect.equal v 43 ""
    ]


let defaultErrorTests =
    testList "defaultError Tests" [
        testCase "defaultError returns the error value"
        <| fun _ ->
            let v = Result.defaultError 43 (Error 42)
            Expect.equal v 42 ""

        testCase "defaultError returns the given value for Ok"
        <| fun _ ->
            let v = Result.defaultError 43 (Ok 42)
            Expect.equal v 43 ""
    ]


let defaultWithTests =
    testList "defaultWith Tests" [
        testCase "defaultWith returns the ok value"
        <| fun _ ->
            let v = Result.defaultWith (fun _ -> 43) (Ok 42)

            Expect.equal v 42 ""

        testCase "defaultValue invoks the given thunk for Error"
        <| fun _ ->
            let v = Result.defaultWith (fun _ -> 42) (Error err)

            Expect.equal v 42 ""
    ]


let ignoreErrorTests =
    testList "ignoreError Tests" [
        testCase "ignoreError returns the unit for ok"
        <| fun _ -> Expect.equal (Result.ignoreError (Ok())) () ""

        testCase "ignoreError returns the unit for Error"
        <| fun _ -> Expect.equal (Result.ignoreError (Error 42)) () ""

        testCase "can call ignoreError without type parameter"
        <| fun _ -> ignore Result.ignoreError

        testCase "can call ignoreError with type parameter"
        <| fun _ -> ignore<Result<unit, string> -> unit> Result.ignoreError<string>
    ]


let teeTests =

    testList "tee Tests" [
        testCase "tee executes the function for ok"
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = 0

            let bar x =
                input <- x

                foo <- "bar"

            let result = Result.tee bar (Ok 42)
            Expect.hasOkValue 42 result
            Expect.equal foo "bar" ""
            Expect.equal input 42 ""

        testCase "tee ignores the function for Error"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = Result.tee bar (Error err)
            Expect.hasErrorValue err result
            Expect.equal foo "foo" ""
    ]

let returnTrue _ = true
let returnFalse _ = false


let teeIfTests =
    testList "teeIf Tests" [
        testCase "teeIf executes the function for ok and true predicate "
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = 0
            let mutable pInput = 0

            let returnTrue x =
                pInput <- x

                true

            let bar x =
                input <- x

                foo <- "bar"

            let result = Result.teeIf returnTrue bar (Ok 42)
            Expect.hasOkValue 42 result
            Expect.equal foo "bar" ""
            Expect.equal input 42 ""
            Expect.equal pInput 42 ""

        testCase "teeIf ignores the function for Ok and false predicate"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = Result.teeIf returnFalse bar (Ok 42)
            Expect.hasOkValue 42 result
            Expect.equal foo "foo" ""

        testCase "teeIf ignores the function for Error"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = Result.teeIf returnTrue bar (Error err)
            Expect.hasErrorValue err result
            Expect.equal foo "foo" ""
    ]


let teeErrorTests =

    testList "teeError Tests" [
        testCase "teeError executes the function for Error"
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = ""

            let bar x =
                input <- x

                foo <- "bar"

            let result = Result.teeError bar (Error err)
            Expect.hasErrorValue err result
            Expect.equal foo "bar" ""
            Expect.equal input err ""

        testCase "teeError ignores the function for Ok"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = Result.teeError bar (Ok 42)
            Expect.hasOkValue 42 result
            Expect.equal foo "foo" ""
    ]


let teeErrorIfTests =
    testList "teeErrorIf Tests" [
        testCase "teeErrorIf executes the function for Error and true predicate "
        <| fun _ ->
            let mutable foo = "foo"
            let mutable input = ""
            let mutable pInput = ""

            let returnTrue x =
                pInput <- x

                true

            let bar x =
                input <- x

                foo <- "bar"

            let result = Result.teeErrorIf returnTrue bar (Error err)

            Expect.hasErrorValue err result
            Expect.equal foo "bar" ""
            Expect.equal input err ""
            Expect.equal pInput err ""

        testCase "teeErrorIf ignores the function for Error and false predicate"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = Result.teeErrorIf returnFalse bar (Error err)

            Expect.hasErrorValue err result
            Expect.equal foo "foo" ""

        testCase "teeErrorIf ignores the function for Ok"
        <| fun _ ->
            let mutable foo = "foo"

            let bar _ = foo <- "bar"

            let result = Result.teeErrorIf returnTrue bar (Ok 42)
            Expect.hasOkValue 42 result
            Expect.equal foo "foo" ""
    ]

let sequenceAsyncTests =
    testList "sequenceAsync Tests" [
        testCaseAsync "sequenceAsync returns the async value if Ok"
        <| async {
            let resAsnc =
                async { return "foo" }
                |> Ok

            let! value =
                resAsnc
                |> Result.sequenceAsync

            Expect.equal value (Ok "foo") ""
        }

        testCaseAsync "sequenceAsync returns the error value if Error"
        <| async {
            let resAsnc = Error "foo"

            let! value =
                resAsnc
                |> Result.sequenceAsync

            Expect.equal value (Error "foo") ""
        }
    ]

let traverseAsyncTests =
    testList "traverseAsync Tests" [
        testCaseAsync "traverseAsync returns the async value if Ok"
        <| async {
            let resAsnc =
                async { return "foo" }
                |> Ok

            let resFunc = id

            let! value =
                (resFunc, resAsnc)
                ||> Result.traverseAsync

            Expect.equal value (Ok "foo") ""
        }

        testCaseAsync "traverseAsync returns the error value if Error"
        <| async {
            let resAsnc = Error "foo"
            let resFunc = id

            let! value =
                (resFunc, resAsnc)
                ||> Result.traverseAsync

            Expect.equal value (Error "foo") ""
        }
    ]

let valueOrTests =
    testList "valueOrTests Tests" [
        testCase "valueOrTests returns the value if Ok"
        <| fun _ ->
            let res = Ok "foo"

            let value =
                res
                |> Result.valueOr (fun _ -> "bar")

            Expect.equal value "foo" ""

        testCase "valueOrTests returns the function's result if Error"
        <| fun _ ->
            let res = Error "bar"

            let value =
                res
                |> Result.valueOr (fun err ->
                    "foo"
                    + err
                )

            Expect.equal value "foobar" ""
    ]

let zipTests =
    testList "zip tests" [
        testCase "Ok, Ok"
        <| fun () ->
            let actual = Result.zip (Ok 1) (Ok 2)
            Expect.equal actual (Ok(1, 2)) "Should be ok"
        testCase "Ok, Error"
        <| fun () ->
            let actual = Result.zip (Ok 1) (Error "Bad")
            Expect.equal actual (Error "Bad") "Should be Error"
        testCase "Error, Ok"
        <| fun () ->
            let actual = Result.zip (Error "Bad") (Ok 1)
            Expect.equal actual (Error "Bad") "Should be Error"
        testCase "Error, Error"
        <| fun () ->
            let actual = Result.zip (Error "Bad1") (Error "Bad2")
            Expect.equal actual (Error "Bad1") "Should be Error"
    ]

let zipErrorTests =
    testList "zipError tests" [
        testCase "Ok, Ok"
        <| fun () ->
            let actual = Result.zipError (Ok 1) (Ok 2)
            Expect.equal actual (Ok(1)) "Should be ok"
        testCase "Ok, Error"
        <| fun () ->
            let actual = Result.zipError (Ok 1) (Error "Bad")
            Expect.equal actual (Ok 1) "Should be ok"
        testCase "Error, Ok"
        <| fun () ->
            let actual = Result.zipError (Error "Bad") (Ok 1)
            Expect.equal actual (Ok 1) "Should be ok"
        testCase "Error, Error"
        <| fun () ->
            let actual = Result.zipError (Error "Bad1") (Error "Bad2")

            Expect.equal actual (Error("Bad1", "Bad2")) "Should be Error"
    ]

let allTests =
    testList "Result Tests" [
        resultIsOk
        resultIsError
        resultEither
        resultEitherMap
        map2Tests
        map3Tests
        applyTests
        ofChoiceTests
        resultCETests
        resultOperatorsTests
        tryCreateTests
        orElseTests
        orElseWithTests
        ignoreTests
        requireTrueTests
        requireFalseTests
        requireSomeTests
        requireNoneTests
        requireValueSomeTests
        requireValueNoneTests
        requireNotNullTests
        requireEqualToTests
        requireEqualTests
        requireEmptyTests
        requireNotEmptyTests
        requireHeadTests
        setErrorTests
        withErrorTests
        defaultValueTests
        defaultErrorTests
        defaultWithTests
        ignoreErrorTests
        teeTests
        teeIfTests
        teeErrorTests
        teeErrorIfTests
        sequenceAsyncTests
        traverseAsyncTests
        valueOrTests
        zipTests
        zipErrorTests
    ]
