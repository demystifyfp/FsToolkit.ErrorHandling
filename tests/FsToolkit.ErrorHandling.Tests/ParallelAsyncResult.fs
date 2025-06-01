module ParallelAsyncResultTests

#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif

open FsToolkit.ErrorHandling

let zipTests =
    testList "ParallelAsyncResult.zip tests" [
        testCaseAsync "Ok case 1"
        <| async {
            let a = async { return Ok "a" }
            let b = async { return Ok 1 }

            let! actual = ParallelAsyncResult.zip a b

            Expect.equal actual (Ok("a", 1)) ""
        }

        testCaseAsync "Error case 1"
        <| async {
            let a = async { return Ok 1 }
            let b = async { return Error "x" }

            let! actual = ParallelAsyncResult.zip a b

            Expect.equal actual (Error("x")) ""
        }

        testCaseAsync "Error case 2"
        <| async {
            let a = async { return Error "x" }
            let b = async { return Ok 2 }

            let! actual = ParallelAsyncResult.zip a b

            Expect.equal actual (Error("x")) ""
        }

        testCaseAsync "Error result fails fast 1"
        <| async {
            let a = async { return Error "x" }

            let b =
                async {
                    do! Async.never
                    return Error "y"
                }

            let! actual = ParallelAsyncResult.zip a b

            Expect.equal actual (Error("x")) ""
        }

        testCaseAsync "Error result fails fast 2"
        <| async {
            let a =
                async {
                    do! Async.never
                    return Error "y"
                }

            let b = async { return Error "x" }

            let! actual = ParallelAsyncResult.zip a b

            Expect.equal actual (Error("x")) ""
        }

        testCaseAsync "Exception case 1"
        <| async {
            let message = "Kaboom"

            let a = async { return failwith message }
            let b = async { return Ok 2 }

            try
                let! _ = ParallelAsyncResult.zip a b

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
                let! _ = ParallelAsyncResult.zip a b

                Expect.isTrue false "Unreachable"
            with exn ->
                Expect.equal exn.Message message ""
        }
    ]

let allTests = testList "ParallelAsyncResult" [ zipTests ]
