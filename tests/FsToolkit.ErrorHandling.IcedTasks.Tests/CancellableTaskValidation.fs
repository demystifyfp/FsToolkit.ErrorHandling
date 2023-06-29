module CancellableTaskValidationTests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open IcedTasks
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.CancellableTaskValidation

let lift = CancellableTaskValidation.ofResult

let map2Tests =
    testList "CancellableTaskValidation.map2 Tests" [
        testCaseAsync "map2 with two ok parts"
        <| async {
            let! result = CancellableTaskValidation.map2 location (lift validLatR) (lift validLngR)

            result
            |> Expect.hasOkValue validLocation
        }
        testCaseAsync "map2 with one Error and one Ok parts"
        <| async {
            let! result =
                CancellableTaskValidation.map2 location (lift invalidLatR) (lift validLngR)

            result
            |> Expect.hasErrorValue [ invalidLatMsg ]
        }
        testCaseAsync "map2 with one Ok and one Error parts"
        <| async {
            let! result =
                CancellableTaskValidation.map2 location (lift validLatR) (lift invalidLngR)

            result
            |> Expect.hasErrorValue [ invalidLngMsg ]
        }
        testCaseAsync "map2 with two Error parts"
        <| async {
            let! result =
                CancellableTaskValidation.map2 location (lift invalidLatR) (lift invalidLngR)

            return
                result
                |> Expect.hasErrorValue [
                    invalidLatMsg
                    invalidLngMsg
                ]
        }
    ]

let map3Tests =
    testList "CancellableTaskValidation.map3 Tests" [
        testCaseAsync "map3 with three ok parts"
        <| async {
            let! result =
                CancellableTaskValidation.map3
                    createPostRequest
                    (lift validLatR)
                    (lift validLngR)
                    (lift validTweetR)

            return
                result
                |> Expect.hasOkValue validCreatePostRequest
        }

        testCaseAsync "map3 with (Error, Ok, Ok)"
        <| async {
            let! result =
                CancellableTaskValidation.map3
                    createPostRequest
                    (lift invalidLatR)
                    (lift validLngR)
                    (lift validTweetR)

            return
                result
                |> Expect.hasErrorValue [ invalidLatMsg ]
        }

        testCaseAsync "map3 with (Ok, Error, Ok)"
        <| async {
            let! result =
                CancellableTaskValidation.map3
                    createPostRequest
                    (lift validLatR)
                    (lift invalidLngR)
                    (lift validTweetR)

            return
                result
                |> Expect.hasErrorValue [ invalidLngMsg ]
        }


        testCaseAsync "map3 with (Ok, Ok, Error)"
        <| async {
            let! result =
                CancellableTaskValidation.map3
                    createPostRequest
                    (lift validLatR)
                    (lift validLngR)
                    (lift emptyInvalidTweetR)

            return
                result
                |> Expect.hasErrorValue [ emptyTweetErrMsg ]
        }

        testCaseAsync "map3 with (Error, Error, Error)"
        <| async {
            let! result =
                CancellableTaskValidation.map3
                    createPostRequest
                    (lift invalidLatR)
                    (lift invalidLngR)
                    (lift emptyInvalidTweetR)

            return
                result
                |> Expect.hasErrorValue [
                    invalidLatMsg
                    invalidLngMsg
                    emptyTweetErrMsg
                ]
        }
    ]


let applyTests =

    testList "CancellableTaskValidation.apply tests" [
        testCaseAsync "apply with Ok"
        <| async {
            let! result =
                Tweet.TryCreate "foobar"
                |> lift
                |> CancellableTaskValidation.apply (
                    Ok remainingCharacters
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasOkValue 274
        }

        testCaseAsync "apply with Error"
        <| async {
            let! result =
                CancellableTaskValidation.apply
                    (Ok remainingCharacters
                     |> CancellableTask.singleton)
                    (lift emptyInvalidTweetR)

            return
                result
                |> Expect.hasErrorValue [ emptyTweetErrMsg ]
        }
    ]


let operatorsTests =

    testList "CancellableTaskValidation Operators Tests" [
        testCaseAsync "map, apply & bind operators"
        <| async {
            let! result =
                createPostRequest
                <!> (lift validLatR)
                <*> (lift validLngR)
                <*> (lift validTweetR)
                >>= (fun tweet ->
                    Ok tweet
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasOkValue validCreatePostRequest
        }
        testCaseAsync "map^ & apply^ operators"
        <| async {
            let! result =
                createPostRequest
                <!^> validLatR
                <*^> validLngR
                <*^> validTweetR

            return
                result
                |> Expect.hasOkValue validCreatePostRequest
        }
    ]

let zipTests =
    testList "zip tests" [
        testCaseAsync "Ok, Ok"
        <| async {
            let! actual =
                CancellableTaskValidation.zip
                    (Ok 1
                     |> CancellableTask.singleton)
                    (Ok 2
                     |> CancellableTask.singleton)

            Expect.equal actual (Ok(1, 2)) "Should be ok"
        }
        testCaseAsync "Ok, Error"
        <| async {
            let! actual =
                CancellableTaskValidation.zip
                    (Ok 1
                     |> CancellableTask.singleton)
                    (CancellableTaskValidation.error "Bad")

            Expect.equal actual (Error [ "Bad" ]) "Should be Error"
        }
        testCaseAsync "Error, Ok"
        <| async {
            let! actual =
                CancellableTaskValidation.zip
                    (CancellableTaskValidation.error "Bad")
                    (Ok 1
                     |> CancellableTask.singleton)

            Expect.equal actual (Error [ "Bad" ]) "Should be Error"
        }
        testCaseAsync "Error, Error"
        <| async {
            let! actual =
                CancellableTaskValidation.zip
                    (CancellableTaskValidation.error "Bad1")
                    (CancellableTaskValidation.error "Bad2")

            Expect.equal
                actual
                (Error [
                    "Bad1"
                    "Bad2"
                ])
                "Should be Error"
        }
    ]


let orElseTests =
    testList "CancellableTaskValidation.orElseWith Tests" [
        testCaseAsync "Ok Ok takes first Ok"
        <| async {
            let! result =
                (Ok "First"
                 |> CancellableTask.singleton)
                |> CancellableTaskValidation.orElse (
                    Ok "Second"
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasOkValue "First"
        }
        testCaseAsync "Ok Error takes first Ok"
        <| async {
            let! result =
                (Ok "First"
                 |> CancellableTask.singleton)
                |> CancellableTaskValidation.orElse (
                    Error [ "Second" ]
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasOkValue "First"
        }
        testCaseAsync "Error Ok takes second Ok"
        <| async {
            let! result =
                (Error [ "First" ]
                 |> CancellableTask.singleton)
                |> CancellableTaskValidation.orElse (
                    Ok "Second"
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasOkValue "Second"
        }
        testCaseAsync "Error Error takes second error"
        <| async {
            let! result =
                (Error [ "First" ]
                 |> CancellableTask.singleton)
                |> CancellableTaskValidation.orElse (
                    Error [ "Second" ]
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasErrorValue [ "Second" ]
        }
    ]

let orElseWithTests =
    testList "CancellableTaskValidation.orElse Tests" [
        testCaseAsync "Ok Ok takes first Ok"
        <| async {
            let! result =
                (Ok "First"
                 |> CancellableTask.singleton)
                |> CancellableTaskValidation.orElseWith (fun _ ->
                    Ok "Second"
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasOkValue "First"
        }
        testCaseAsync "Ok Error takes first Ok"
        <| async {
            let! result =
                (Ok "First"
                 |> CancellableTask.singleton)
                |> CancellableTaskValidation.orElseWith (fun _ ->
                    Error [ "Second" ]
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasOkValue "First"
        }
        testCaseAsync "Error Ok takes second Ok"
        <| async {
            let! result =
                (Error [ "First" ]
                 |> CancellableTask.singleton)
                |> CancellableTaskValidation.orElseWith (fun _ ->
                    Ok "Second"
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasOkValue "Second"
        }
        testCaseAsync "Error Error takes second error"
        <| async {
            let! result =
                (Error [ "First" ]
                 |> CancellableTask.singleton)
                |> CancellableTaskValidation.orElseWith (fun _ ->
                    Error [ "Second" ]
                    |> CancellableTask.singleton
                )

            return
                result
                |> Expect.hasErrorValue [ "Second" ]
        }
    ]

let allTests =
    testList "CancellableTaskValidationTests" [
        map2Tests
        map3Tests
        applyTests
        operatorsTests
        orElseTests
        orElseWithTests
        zipTests
    ]
