module ReqTests

#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif

open TestHelpers
open FsToolkit.ErrorHandling

let err = "foobar"

let requireTrueTests =
    testList "requireTrue Tests" [
        testCase "requireTrue happy path"
        <| fun _ ->
            true
            |> Req.isTrue err
            |> Expect.hasOkValue ()

        testCase "requireTrue error path"
        <| fun _ ->
            false
            |> Req.isTrue err
            |> Expect.hasErrorValue err

        testCase "requireTrueWith"
        <| fun _ ->
            true
            |> Req.isTrueWith (fun () -> failwith "factory should not run")
            |> Expect.hasOkValue ()

            false
            |> Req.isTrueWith (fun () -> err)
            |> Expect.hasErrorValue err
    ]

let requireFalseTests =
    testList "requireFalse Tests" [
        testCase "requireFalse happy path"
        <| fun _ ->
            false
            |> Req.isFalse err
            |> Expect.hasOkValue ()

        testCase "requireFalse error path"
        <| fun _ ->
            true
            |> Req.isFalse err
            |> Expect.hasErrorValue err

        testCase "requireFalseWith"
        <| fun _ ->
            false
            |> Req.isFalseWith (fun () -> failwith "factory should not run")
            |> Expect.hasOkValue ()

            true
            |> Req.isFalseWith (fun () -> err)
            |> Expect.hasErrorValue err
    ]

let requireSomeTests =
    testList "requireSome Tests" [
        testCase "requireSome happy path"
        <| fun _ ->
            Some 42
            |> Req.some err
            |> Expect.hasOkValue 42
        testCase "requireSome error path"
        <| fun _ ->
            None
            |> Req.some err
            |> Expect.hasErrorValue err

        testCase "requireSomeWith happy path"
        <| fun _ ->
            Some 42
            |> Req.someWith (fun () -> err)
            |> Expect.hasOkValue 42
        testCase "requireSomeWith error path"
        <| fun _ ->
            None
            |> Req.someWith (fun () -> err)
            |> Expect.hasErrorValue err
    ]

let requireNotNullTests =
    testList "requireNotNull Tests" [
        testCase "requireNotNull happy path"
        <| fun _ ->
            ("test": StringNull)
            |> Req.notNull err
            |> Expect.hasOkValue "test"

        testCase "requireNotNull error path"
        <| fun _ ->
            null
            |> Req.notNull err
            |> Expect.hasErrorValue err
    ]

let requireNoneTests =
    testList "requireNone Tests" [
        testCase "requireNone happy path"
        <| fun _ ->
            None
            |> Req.none err
            |> Expect.hasOkValue ()
        testCase "requireNone error path"
        <| fun _ ->
            Some 42
            |> Req.none err
            |> Expect.hasErrorValue err

        testCase "requireNoneWith happy path"
        <| fun _ ->
            None
            |> Req.noneWith (fun () -> err)
            |> Expect.hasOkValue ()
        testCase "requireNoneWith error path"
        <| fun _ ->
            Some 42
            |> Req.noneWith (fun () -> err)
            |> Expect.hasErrorValue err
    ]

let requireValueSomeTests =
    testList "requireValueSome Tests" [
        testCase "requireValueSome happy path"
        <| fun _ ->
            ValueSome 42
            |> Req.valueSome err
            |> Expect.hasOkValue 42

        testCase "requireValueSome error path"
        <| fun _ ->
            ValueNone
            |> Req.valueSome err
            |> Expect.hasErrorValue err

        testCase "requireValueSomeWith"
        <| fun _ ->
            ValueSome 42
            |> Req.valueSomeWith (fun () -> failwith "factory should not run")
            |> Expect.hasOkValue 42

            ValueNone
            |> Req.valueSomeWith (fun () -> err)
            |> Expect.hasErrorValue err
    ]

let requireValueNoneTests =
    testList "requireValueNone Tests" [
        testCase "requireValueNone happy path"
        <| fun _ ->
            ValueNone
            |> Req.valueNone err
            |> Expect.hasOkValue ()
        testCase "requireValueNone error path"
        <| fun _ ->
            ValueSome 42
            |> Req.valueNone err
            |> Expect.hasErrorValue err

        testCase "requireValueNoneWith"
        <| fun _ ->
            ValueNone
            |> Req.valueNoneWith (fun () -> failwith "factory should not run")
            |> Expect.hasOkValue ()

            ValueSome 42
            |> Req.valueNoneWith (fun () -> err)
            |> Expect.hasErrorValue err
    ]

let requireEqualToTests =
    testList "requireEqualTo Tests" [
        testCase "requireEqualTo happy path"
        <| fun _ ->
            42
            |> Req.equalTo 42 err
            |> Expect.hasOkValue ()

        testCase "requireEqualTo error path"
        <| fun _ ->
            43
            |> Req.equalTo 42 err
            |> Expect.hasErrorValue err
    ]


let requireEqualTests =
    testList "requireEqual Tests" [
        testCase "requireEqual happy path"
        <| fun _ ->
            (42, 42)
            |> Req.equal err
            |> Expect.hasOkValue ()

        testCase "requireEqual error path"
        <| fun _ ->
            (42, 43)
            |> Req.equal err
            |> Expect.hasErrorValue err
    ]


let requireEmptyTests =
    testList "requireEmpty Tests" [
        testCase "requireEmpty happy path"
        <| fun _ ->
            []
            |> Req.empty err
            |> Expect.hasOkValue ()

        testCase "requireEmpty error path"
        <| fun _ ->
            [ 42 ]
            |> Req.empty err
            |> Expect.hasErrorValue err
    ]


let requireNotEmptyTests =
    testList "requireNotEmpty Tests" [
        testCase "requireNotEmpty happy path"
        <| fun _ ->
            [ 42 ]
            |> Req.notEmpty err
            |> Expect.hasOkValue ()

        testCase "requireNotEmpty error path"
        <| fun _ ->
            []
            |> Req.notEmpty err
            |> Expect.hasErrorValue err
    ]


let requireHeadTests =
    testList "requireHead Tests" [
        testCase "requireHead happy path"
        <| fun _ ->
            [ 42 ]
            |> Req.head err
            |> Expect.hasOkValue 42

        testCase "requireHead error path"
        <| fun _ ->
            []
            |> Req.head err
            |> Expect.hasErrorValue err
    ]

let requireTests =
    testList "require tests" [
        testCase "False, Error"
        <| fun () ->

            let output =
                Error "Something went wrong"
                |> Result.reqFilter (fun _ -> false) "Error"

            Expect.equal output (Error "Something went wrong") "Should not be Error"

        testCase "True, Ok"
        <| fun () ->
            let output =
                Ok 1
                |> Result.reqFilter (fun _ -> true) "Error"

            Expect.equal output (Ok 1) "Should be Ok"

        testCase "False, Ok"
        <| fun () ->
            let output =
                Ok 1
                |> Result.reqFilter (fun _ -> false) "Error"

            Expect.equal output (Error("Error")) "Should be Error"

        testCase "True, Ok using Ok value in predicate"
        <| fun () ->
            let output =
                Ok 1
                |> Result.reqFilter (fun number -> number = 1) "Error"

            Expect.equal output (Ok 1) "Should be Ok"

        testCase "False, Ok using Ok value in predicate"
        <| fun () ->
            let output =
                Ok 1
                |> Result.reqFilter (fun x -> x <> 1) "Error"

            Expect.equal output (Error("Error")) "Should be Error"
    ]


let checkTests =
    testList "check tests" [
        testCase "Ok, Error"
        <| (fun () ->
            let output = Result.check (fun _ -> Ok()) (Error(1))
            Expect.equal output (Error(1)) "Should be error"
        )

        testCase "OK, Ok"
        <| (fun () ->
            let output = Result.check (fun _ -> Ok()) (Ok(1))
            Expect.equal output (Ok(1)) "Should be Ok"
        )

        testCase "Error, Error"
        <| (fun () ->
            let output = Result.check (fun _ -> Error(2)) (Error(1))
            Expect.equal output (Error(1)) "Should be Error"
        )

        testCase "Error, Ok"
        <| (fun () ->
            let output = Result.check (fun _ -> Error(2)) (Ok(1))
            Expect.equal output (Error(2)) "Should be Error"
        )

        testCase "Using the result value in the predicate"
        <| (fun () ->
            let output =
                Result.check (fun number -> if number = 1 then Error(2) else Ok()) (Ok(1))

            Expect.equal output (Error(2)) "Should be Error"
        )
    ]

let allTests =
    testList "Req Tests" [
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
        requireTests
        checkTests
    ]
