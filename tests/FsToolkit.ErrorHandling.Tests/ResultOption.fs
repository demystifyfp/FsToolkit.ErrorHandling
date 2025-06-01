module ResultOptionTests


#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif

open TestData
open TestHelpers
open SampleDomain
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.ResultOption


let map2Tests =
    testList "ResultOption.map2 Tests" [

        testCase "map2 with Ok(Some x) Ok(Some y)"
        <| fun _ ->
            ResultOption.map2 location (Ok(Some validLat)) (Ok(Some validLng))
            |> Expect.hasOkValue (Some validLocation)

        testCase "map2 with Ok(Some x) Ok(None)"
        <| fun _ ->
            ResultOption.map2 location (Ok(Some validLat)) (Ok None)
            |> Expect.hasOkValue None

        testCase "map2 with Ok(None) Ok(None)"
        <| fun _ ->
            ResultOption.map2 location (Ok None) (Ok None)
            |> Expect.hasOkValue None

        testCase "map2 with Error(Some x) Error(Some x)"
        <| fun _ ->
            ResultOption.map2 location (Error(Some validLat)) (Error(Some validLat2))
            |> Expect.hasErrorValue (Some validLat)

        testCase "map2 with Ok(Some x) Error(Some x)"
        <| fun _ ->
            ResultOption.map2 location (Ok(Some validLat)) (Error(Some validLng))
            |> Expect.hasErrorValue (Some validLng)
    ]


let map3Tests =
    testList "ResultOption.map3 Tests" [

        testCase "map3 with Ok(Some x) Ok(Some y) Ok (Some z)"
        <| fun _ ->
            ResultOption.map3
                createPostRequest
                (Ok(Some validLat))
                (Ok(Some validLng))
                (Ok(Some validTweet))
            |> Expect.hasOkValue (Some validCreatePostRequest)

        testCase "map3 with Ok(Some x) Ok(None) Ok(None)"
        <| fun _ ->
            ResultOption.map3 createPostRequest (Ok(Some validLat)) (Ok None) (Ok None)
            |> Expect.hasOkValue None

        testCase "map3 with Ok(None) Ok(None) (Ok None)"
        <| fun _ ->
            ResultOption.map3 createPostRequest (Ok None) (Ok None) (Ok None)
            |> Expect.hasOkValue None

        testCase "map3 with Error(Some x) Error(Some x) (Ok None)"
        <| fun _ ->
            ResultOption.map3
                createPostRequest
                (Error(Some validLat))
                (Error(Some validLat2))
                (Ok None)
            |> Expect.hasErrorValue (Some validLat)

        testCase "map3 with Ok(Some x) Error(Some x) (Ok None)"
        <| fun _ ->
            ResultOption.map3 createPostRequest (Ok(Some validLat)) (Error(Some validLng)) (Ok None)
            |> Expect.hasErrorValue (Some validLng)
    ]


let applyTests =
    testList "ResultOption.apply Tests" [
        testCase "apply with Ok(Some x)"
        <| fun _ ->
            Ok(Some validTweet)
            |> ResultOption.apply (Ok(Some remainingCharacters))
            |> Expect.hasOkValue (Some 267)

        testCase "apply with Ok(None)"
        <| fun _ ->
            Ok None
            |> ResultOption.apply (Ok(Some remainingCharacters))
            |> Expect.hasOkValue None

        testCase "apply with Error"
        <| fun _ ->
            Error "bad things happened"
            |> ResultOption.apply (Ok(Some remainingCharacters))
            |> Expect.hasErrorValue "bad things happened"
    ]


let mapTests =
    testList "ResultOption.map Tests" [
        testCase "map with Ok(Some x)"
        <| fun _ ->
            Ok(Some validTweet)
            |> ResultOption.map remainingCharacters
            |> Expect.hasOkValue (Some 267)

        testCase "map with Ok(None)"
        <| fun _ ->
            Ok None
            |> ResultOption.map remainingCharacters
            |> Expect.hasOkValue None
    ]


let bindTests =
    testList "ResultOption.bind Tests" [
        testCase "bind with Ok(Some x)"
        <| fun _ ->
            Ok(Some validTweet2)
            |> ResultOption.bind firstURLInTweet
            |> Expect.hasOkValue (Some validURL)

        testCase "bind with Error"
        <| fun _ ->
            Error "bad things happened"
            |> ResultOption.bind firstURLInTweet
            |> Expect.hasErrorValue "bad things happened"
    ]


let ignoreTests =
    testList "ResultOption.ignore Tests" [
        testCase "ignore with Ok(Some x)"
        <| fun _ ->
            Ok(Some validTweet)
            |> ResultOption.ignore
            |> Expect.hasOkValue (Some())

        testCase "ignore with Ok(None)"
        <| fun _ ->
            Ok None
            |> ResultOption.ignore
            |> Expect.hasOkValue None

        testCase "can call ignore without type parameters"
        <| fun _ -> ignore ResultOption.ignore

        testCase "can call ignore with type parameters"
        <| fun _ ->
            ignore<Result<int option, string> -> Result<unit option, string>>
                ResultOption.ignore<int, string>
    ]


let resultOptionCETests =
    testList "resultOption Computation Expression tests" [
        testCase "bind with all Ok"
        <| fun _ ->
            let createPostRequest =
                resultOption {
                    let! lat = Ok(Some validLat)
                    let! lng = Ok(Some validLng)

                    let! tweet =
                        validTweetR
                        |> Result.map Some

                    return createPostRequest lat lng tweet
                }

            Expect.hasOkValue (Some validCreatePostRequest) createPostRequest

        testCase "bind with Error"
        <| fun _ ->
            let post =
                resultOption {
                    let! lat =
                        invalidLatR
                        |> Result.map Some

                    Tests.failtestf "this should not get executed!"
                    let! lng = Ok(Some validLng)

                    let! tweet =
                        validTweetR
                        |> Result.map Some

                    return createPostRequest lat lng tweet
                }

            post
            |> Expect.hasErrorValue invalidLatMsg
    ]


let resultOptionOperatorsTests =

    testList "ResultOption Operators Tests" [
        testCase "map & apply operators"
        <| fun _ ->
            createPostRequest
            <!> Ok(Some validLat)
            <*> Ok(Some validLng)
            <*^> Ok validTweet
            |> Expect.hasOkValue (Some validCreatePostRequest)

        testCase "bind operator"
        <| fun _ ->
            Ok(Some validTweet2)
            >>= firstURLInTweet
            |> Expect.hasOkValue (Some validURL)
    ]

let zipTests =
    testList "zip tests" [
        testCase "Ok Some, Ok Some"
        <| fun () ->
            let actual = ResultOption.zip (Ok(Some 1)) (Ok(Some 2))
            Expect.equal actual (Ok(Some(1, 2))) "Should be ok some"
        testCase "Ok None, Ok None"
        <| fun () ->
            let actual = ResultOption.zip (Ok None) (Ok None)
            Expect.equal actual (Ok None) "Should be ok none"
        testCase "Ok Some, Ok None"
        <| fun () ->
            let actual = ResultOption.zip (Ok(Some 1)) (Ok None)
            Expect.equal actual (Ok None) "Should be ok none"
        testCase "Ok None, Ok Some"
        <| fun () ->
            let actual = ResultOption.zip (Ok None) (Ok(Some 1))
            Expect.equal actual (Ok None) "Should be ok none"
        testCase "Ok Some, Error"
        <| fun () ->
            let actual = ResultOption.zip (Ok(Some 1)) (Error "Bad")
            Expect.equal actual (Error "Bad") "Should be Error"
        testCase "Error, Ok Some"
        <| fun () ->
            let actual = ResultOption.zip (Error "Bad") (Ok(Some 1))
            Expect.equal actual (Error "Bad") "Should be Error"
        testCase "Ok None, Error"
        <| fun () ->
            let actual = ResultOption.zip (Ok None) (Error "Bad")
            Expect.equal actual (Error "Bad") "Should be Error"
        testCase "Error, Ok None"
        <| fun () ->
            let actual = ResultOption.zip (Error "Bad") (Ok None)
            Expect.equal actual (Error "Bad") "Should be Error"
        testCase "Error, Error"
        <| fun () ->
            let actual = ResultOption.zip (Error "Bad1") (Error "Bad2")
            Expect.equal actual (Error "Bad1") "Should be Error"
    ]

let zipErrorTests =
    testList "zipError tests" [
        testCase "Ok Some, Ok Some"
        <| fun () ->
            let actual = ResultOption.zipError (Ok(Some 1)) (Ok(Some 2))
            Expect.equal actual (Ok(Some 1)) "Should be ok some"
        testCase "Ok None, Ok None"
        <| fun () ->
            let actual = ResultOption.zipError (Ok None) (Ok None)
            Expect.equal actual (Ok None) "Should be ok none"
        testCase "Ok Some, Ok None"
        <| fun () ->
            let actual = ResultOption.zipError (Ok(Some 1)) (Ok None)
            Expect.equal actual (Ok(Some 1)) "Should be ok some"
        testCase "Ok None, Ok Some"
        <| fun () ->
            let actual = ResultOption.zipError (Ok None) (Ok(Some 1))
            Expect.equal actual (Ok None) "Should be ok none"
        testCase "Ok Some, Error"
        <| fun () ->
            let actual = ResultOption.zipError (Ok(Some 1)) (Error "Bad")
            Expect.equal actual (Ok(Some 1)) "Should be ok some"
        testCase "Error, Ok Some"
        <| fun () ->
            let actual = ResultOption.zipError (Error "Bad") (Ok(Some 1))
            Expect.equal actual (Ok(Some 1)) "Should be ok some"
        testCase "Ok None, Error"
        <| fun () ->
            let actual = ResultOption.zipError (Ok None) (Error "Bad")
            Expect.equal actual (Ok None) "Should be ok none"
        testCase "Error, Ok None"
        <| fun () ->
            let actual = ResultOption.zipError (Error "Bad") (Ok None)
            Expect.equal actual (Ok None) "Should be ok none"
        testCase "Error, Error"
        <| fun () ->
            let actual = ResultOption.zipError (Error "Bad1") (Error "Bad2")
            Expect.equal actual (Error("Bad1", "Bad2")) "Should be Error tuple"
    ]

let allTests =
    testList "Result Option Tests" [
        map2Tests
        map3Tests
        applyTests
        mapTests
        bindTests
        ignoreTests
        resultOptionCETests
        resultOptionOperatorsTests
        zipTests
        zipErrorTests
    ]
