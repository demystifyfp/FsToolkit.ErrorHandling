module ValidationTests


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
open FsToolkit.ErrorHandling.Operator.Validation

let lift = Validation.ofResult


let map2Tests =
    testList "Validation.map2 Tests" [
        testCase "map2 with two ok parts"
        <| fun _ ->
            Validation.map2 location (lift validLatR) (lift validLngR)
            |> Expect.hasOkValue validLocation

        testCase "map2 with one Error and one Ok parts"
        <| fun _ ->
            Validation.map2 location (lift invalidLatR) (lift validLngR)
            |> Expect.hasErrorValue [ invalidLatMsg ]

        testCase "map2 with one Ok and one Error parts"
        <| fun _ ->
            Validation.map2 location (lift validLatR) (lift invalidLngR)
            |> Expect.hasErrorValue [ invalidLngMsg ]

        testCase "map2 with two Error parts"
        <| fun _ ->
            Validation.map2 location (lift invalidLatR) (lift invalidLngR)
            |> Expect.hasErrorValue [
                invalidLatMsg
                invalidLngMsg
            ]
    ]


let map3Tests =
    testList "Validation.map3 Tests" [
        testCase "map3 with three ok parts"
        <| fun _ ->
            Validation.map3 createPostRequest (lift validLatR) (lift validLngR) (lift validTweetR)
            |> Expect.hasOkValue validCreatePostRequest

        testCase "map3 with (Error, Ok, Ok)"
        <| fun _ ->
            Validation.map3 createPostRequest (lift invalidLatR) (lift validLngR) (lift validTweetR)
            |> Expect.hasErrorValue [ invalidLatMsg ]

        testCase "map3 with (Ok, Error, Ok)"
        <| fun _ ->
            Validation.map3 createPostRequest (lift validLatR) (lift invalidLngR) (lift validTweetR)
            |> Expect.hasErrorValue [ invalidLngMsg ]


        testCase "map3 with (Ok, Ok, Error)"
        <| fun _ ->
            Validation.map3
                createPostRequest
                (lift validLatR)
                (lift validLngR)
                (lift emptyInvalidTweetR)
            |> Expect.hasErrorValue [ emptyTweetErrMsg ]

        testCase "map3 with (Error, Error, Error)"
        <| fun _ ->
            Validation.map3
                createPostRequest
                (lift invalidLatR)
                (lift invalidLngR)
                (lift emptyInvalidTweetR)
            |> Expect.hasErrorValue [
                invalidLatMsg
                invalidLngMsg
                emptyTweetErrMsg
            ]
    ]


let applyTests =

    testList "Validation.apply tests" [
        testCase "apply with Ok"
        <| fun _ ->
            Tweet.TryCreate "foobar"
            |> lift
            |> Validation.apply (Ok remainingCharacters)
            |> Expect.hasOkValue 274

        testCase "apply with Error"
        <| fun _ ->
            Validation.apply (Ok remainingCharacters) (lift emptyInvalidTweetR)
            |> Expect.hasErrorValue [ emptyTweetErrMsg ]
    ]


let operatorsTests =

    testList "Validation Operators Tests" [
        testCase "map, apply & bind operators"
        <| fun _ ->
            createPostRequest
            <!> (lift validLatR)
            <*> (lift validLngR)
            <*> (lift validTweetR)
            >>= (fun tweet -> Ok tweet)
            |> Expect.hasOkValue validCreatePostRequest
        testCase "map^ & apply^ operators"
        <| fun _ ->
            createPostRequest
            <!^> validLatR
            <*^> validLngR
            <*^> validTweetR
            |> Expect.hasOkValue validCreatePostRequest
    ]

let zipTests =
    testList "zip tests" [
        testCase "Ok, Ok"
        <| fun () ->
            let actual = Validation.zip (Ok 1) (Ok 2)
            Expect.equal actual (Ok(1, 2)) "Should be ok"
        testCase "Ok, Error"
        <| fun () ->
            let actual = Validation.zip (Ok 1) (Validation.error "Bad")

            Expect.equal actual (Error [ "Bad" ]) "Should be Error"
        testCase "Error, Ok"
        <| fun () ->
            let actual = Validation.zip (Validation.error "Bad") (Ok 1)

            Expect.equal actual (Error [ "Bad" ]) "Should be Error"
        testCase "Error, Error"
        <| fun () ->
            let actual = Validation.zip (Validation.error "Bad1") (Validation.error "Bad2")

            Expect.equal
                actual
                (Error [
                    "Bad1"
                    "Bad2"
                ])
                "Should be Error"
    ]


let orElseTests =
    testList "Validation.orElseWith Tests" [
        testCase "Ok Ok takes first Ok"
        <| fun _ ->
            (Ok "First")
            |> Validation.orElse (Ok "Second")
            |> Expect.hasOkValue "First"
        testCase "Ok Error takes first Ok"
        <| fun _ ->
            (Ok "First")
            |> Validation.orElse (Error [ "Second" ])
            |> Expect.hasOkValue "First"
        testCase "Error Ok takes second Ok"
        <| fun _ ->
            (Error [ "First" ])
            |> Validation.orElse (Ok "Second")
            |> Expect.hasOkValue "Second"
        testCase "Error Error takes second error"
        <| fun _ ->
            (Error [ "First" ])
            |> Validation.orElse (Error [ "Second" ])
            |> Expect.hasErrorValue [ "Second" ]
    ]

let orElseWithTests =
    testList "Validation.orElse Tests" [
        testCase "Ok Ok takes first Ok"
        <| fun _ ->
            (Ok "First")
            |> Validation.orElseWith (fun _ -> Ok "Second")
            |> Expect.hasOkValue "First"
        testCase "Ok Error takes first Ok"
        <| fun _ ->
            (Ok "First")
            |> Validation.orElseWith (fun _ -> Error [ "Second" ])
            |> Expect.hasOkValue "First"
        testCase "Error Ok takes second Ok"
        <| fun _ ->
            (Error [ "First" ])
            |> Validation.orElseWith (fun _ -> Ok "Second")
            |> Expect.hasOkValue "Second"
        testCase "Error Error takes second error"
        <| fun _ ->
            (Error [ "First" ])
            |> Validation.orElseWith (fun _ -> Error [ "Second" ])
            |> Expect.hasErrorValue [ "Second" ]
    ]

let allTests =
    testList "ValidationTests" [
        map2Tests
        map3Tests
        applyTests
        operatorsTests
        orElseTests
        orElseWithTests
        zipTests
    ]
