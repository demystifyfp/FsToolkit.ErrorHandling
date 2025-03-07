module ParallelAsyncValidationTests

#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
#endif

open FsToolkit.ErrorHandling

let zipTests =
    testList "ParallelAsyncValidation.zip tests" [
        testCaseAsync "Ok case 1"
        <| async {
            let a = async { return Ok "a" }
            let b = async { return Ok 1 }

            let! actual = ParallelAsyncValidation.zip a b

            Expect.equal actual (Ok("a", 1)) ""
        }

        testCaseAsync "Error case 1"
        <| async {
            let a = async { return Ok 1 }
            let b = async { return Error [ "x" ] }

            let! actual = ParallelAsyncValidation.zip a b

            Expect.equal actual (Error [ "x" ]) ""
        }

        testCaseAsync "Error case 2"
        <| async {
            let a = async { return Error [ "x" ] }
            let b = async { return Ok 2 }

            let! actual = ParallelAsyncValidation.zip a b

            Expect.equal actual (Error [ "x" ]) ""
        }

        testCaseAsync "Error case 3"
        <| async {
            let a = async { return Error [ "x" ] }
            let b = async { return Error [ "y" ] }

            let! actual = ParallelAsyncValidation.zip a b

            Expect.equal
                actual
                (Error [
                    "x"
                    "y"
                ])
                ""
        }

        testCaseAsync "Exception case 1"
        <| async {
            let message = "Kaboom"

            let a = async { return failwith message }
            let b = async { return Ok 2 }

            try
                let! _ = ParallelAsyncValidation.zip a b

                Expect.isTrue false "Unreachable"
            with exn ->
                Expect.equal exn.Message message ""
        }

        testCaseAsync "Exception case 2"
        <| async {
            let message = "Kaboom"

            let a = async { return Ok 1 }
            let b = async { return failwith message }

            try
                let! _ = ParallelAsyncValidation.zip a b

                Expect.isTrue false "Unreachable"
            with exn ->
                Expect.equal exn.Message message ""
        }
    ]

let allTests = testList "ParallelAsyncValidation" [ zipTests ]
