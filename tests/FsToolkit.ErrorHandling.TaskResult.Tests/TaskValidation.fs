module TaskValidationTests


open Expecto

open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.TaskValidation
open System.Threading.Tasks

let lift = TaskValidation.ofResult

let map2Tests =
    testList "TaskValidation.map2 Tests" [
        testCaseTask "map2 with two ok parts"
        <| fun () ->
            task {
                let! result = TaskValidation.map2 location (lift validLatR) (lift validLngR)

                return
                    result
                    |> Expect.hasOkValue validLocation
            }

        testCaseTask "map2 with one Error and one Ok parts"
        <| fun () ->
            task {
                let! result = TaskValidation.map2 location (lift invalidLatR) (lift validLngR)

                return
                    result
                    |> Expect.hasErrorValue [ invalidLatMsg ]
            }

        testCaseTask "map2 with one Ok and one Error parts"
        <| fun () ->
            task {
                let! result = TaskValidation.map2 location (lift validLatR) (lift invalidLngR)

                return
                    result
                    |> Expect.hasErrorValue [ invalidLngMsg ]
            }

        testCaseTask "map2 with two Error parts"
        <| fun () ->
            task {
                let! result = TaskValidation.map2 location (lift invalidLatR) (lift invalidLngR)

                return
                    result
                    |> Expect.hasErrorValue [
                        invalidLatMsg
                        invalidLngMsg
                    ]
            }
    ]

let map3Tests =
    testList "TaskValidation.map3 Tests" [
        testCaseTask "map3 with three ok parts"
        <| fun () ->
            task {
                let! result =
                    TaskValidation.map3
                        createPostRequest
                        (lift validLatR)
                        (lift validLngR)
                        (lift validTweetR)

                return
                    result
                    |> Expect.hasOkValue validCreatePostRequest
            }

        testCaseTask "map3 with (Error, Ok, Ok)"
        <| fun () ->
            task {
                let! result =
                    TaskValidation.map3
                        createPostRequest
                        (lift invalidLatR)
                        (lift validLngR)
                        (lift validTweetR)

                return
                    result
                    |> Expect.hasErrorValue [ invalidLatMsg ]
            }

        testCaseTask "map3 with (Ok, Error, Ok)"
        <| fun () ->
            task {
                let! result =
                    TaskValidation.map3
                        createPostRequest
                        (lift validLatR)
                        (lift invalidLngR)
                        (lift validTweetR)

                return
                    result
                    |> Expect.hasErrorValue [ invalidLngMsg ]
            }


        testCaseTask "map3 with (Ok, Ok, Error)"
        <| fun () ->
            task {
                let! result =
                    TaskValidation.map3
                        createPostRequest
                        (lift validLatR)
                        (lift validLngR)
                        (lift emptyInvalidTweetR)

                return
                    result
                    |> Expect.hasErrorValue [ emptyTweetErrMsg ]
            }

        testCaseTask "map3 with (Error, Error, Error)"
        <| fun () ->
            task {
                let! result =
                    TaskValidation.map3
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

    testList "TaskValidation.apply tests" [
        testCaseTask "apply with Ok"
        <| fun () ->
            task {
                let! result =
                    Tweet.TryCreate "foobar"
                    |> lift
                    |> TaskValidation.apply (
                        Ok remainingCharacters
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasOkValue 274
            }

        testCaseTask "apply with Error"
        <| fun () ->
            task {
                let! result =
                    TaskValidation.apply
                        (Ok remainingCharacters
                         |> Task.FromResult)
                        (lift emptyInvalidTweetR)

                return
                    result
                    |> Expect.hasErrorValue [ emptyTweetErrMsg ]
            }
    ]


let operatorsTests =

    testList "TaskValidation Operators Tests" [
        testCaseTask "map, apply & bind operators"
        <| fun () ->
            task {
                let! result =
                    createPostRequest
                    <!> (lift validLatR)
                    <*> (lift validLngR)
                    <*> (lift validTweetR)
                    >>= (fun tweet ->
                        Ok tweet
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasOkValue validCreatePostRequest
            }
        testCaseTask "map^ & apply^ operators"
        <| fun () ->
            task {
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
        testCaseTask "Ok, Ok"
        <| fun () ->
            task {
                let! actual =
                    TaskValidation.zip
                        (Ok 1
                         |> Task.FromResult)
                        (Ok 2
                         |> Task.FromResult)

                Expect.equal actual (Ok(1, 2)) "Should be ok"
            }
        testCaseTask "Ok, Error"
        <| fun () ->
            task {
                let! actual =
                    TaskValidation.zip
                        (Ok 1
                         |> Task.FromResult)
                        (TaskValidation.error "Bad")

                Expect.equal actual (Error [ "Bad" ]) "Should be Error"
            }
        testCaseTask "Error, Ok"
        <| fun () ->
            task {
                let! actual =
                    TaskValidation.zip
                        (TaskValidation.error "Bad")
                        (Ok 1
                         |> Task.FromResult)

                Expect.equal actual (Error [ "Bad" ]) "Should be Error"
            }
        testCaseTask "Error, Error"
        <| fun () ->
            task {
                let! actual =
                    TaskValidation.zip (TaskValidation.error "Bad1") (TaskValidation.error "Bad2")

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
    testList "TaskValidation.orElseWith Tests" [
        testCaseTask "Ok Ok takes first Ok"
        <| fun () ->
            task {
                let! result =
                    (Ok "First"
                     |> Task.FromResult)
                    |> TaskValidation.orElse (
                        Ok "Second"
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasOkValue "First"
            }
        testCaseTask "Ok Error takes first Ok"
        <| fun () ->
            task {
                let! result =
                    (Ok "First"
                     |> Task.FromResult)
                    |> TaskValidation.orElse (
                        Error [ "Second" ]
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasOkValue "First"
            }
        testCaseTask "Error Ok takes second Ok"
        <| fun () ->
            task {
                let! result =
                    (Error [ "First" ]
                     |> Task.FromResult)
                    |> TaskValidation.orElse (
                        Ok "Second"
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasOkValue "Second"
            }
        testCaseTask "Error Error takes second error"
        <| fun () ->
            task {
                let! result =
                    (Error [ "First" ]
                     |> Task.FromResult)
                    |> TaskValidation.orElse (
                        Error [ "Second" ]
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasErrorValue [ "Second" ]
            }
    ]

let orElseWithTests =
    testList "TaskValidation.orElse Tests" [
        testCaseTask "Ok Ok takes first Ok"
        <| fun () ->
            task {
                let! result =
                    (Ok "First"
                     |> Task.FromResult)
                    |> TaskValidation.orElseWith (fun _ ->
                        Ok "Second"
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasOkValue "First"
            }
        testCaseTask "Ok Error takes first Ok"
        <| fun () ->
            task {
                let! result =
                    (Ok "First"
                     |> Task.FromResult)
                    |> TaskValidation.orElseWith (fun _ ->
                        Error [ "Second" ]
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasOkValue "First"
            }
        testCaseTask "Error Ok takes second Ok"
        <| fun () ->
            task {
                let! result =
                    (Error [ "First" ]
                     |> Task.FromResult)
                    |> TaskValidation.orElseWith (fun _ ->
                        Ok "Second"
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasOkValue "Second"
            }
        testCaseTask "Error Error takes second error"
        <| fun () ->
            task {
                let! result =
                    (Error [ "First" ]
                     |> Task.FromResult)
                    |> TaskValidation.orElseWith (fun _ ->
                        Error [ "Second" ]
                        |> Task.FromResult
                    )

                return
                    result
                    |> Expect.hasErrorValue [ "Second" ]
            }
    ]

let allTests =
    testList "TaskValidationTests" [
        map2Tests
        map3Tests
        applyTests
        operatorsTests
        orElseTests
        orElseWithTests
        zipTests
    ]
