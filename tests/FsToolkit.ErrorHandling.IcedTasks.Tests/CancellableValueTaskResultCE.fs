namespace FsToolkit.ErrorHandling.IcedTasks.Tests

open Expecto
open System.Threading
open System.Threading.Tasks
open IcedTasks
open FsToolkit.ErrorHandling

module ValueTaskResultCompileTests =
    // Just having these compile is a test in itself
    let testFunctionCVTR<'Dto> () =
        cancellableValueTaskResult {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionBCVTR<'Dto> () =
        backgroundCancellableValueTaskResult {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

module CancellableValueTaskResultCE =

    let makeDisposable () =
        { new System.IDisposable with
            member this.Dispose() = ()
        }


    [<Tests>]
    let cancellableValueTaskResultBuilderTests =
        testList "CancellableValueTaskResultBuilder" [
            testList "Return" [
                testCaseTask "return"
                <| fun () ->
                    task {
                        let data = 42
                        let ctr = cancellableValueTaskResult { return data }
                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to Return value"
                    }
            ]
            testList "ReturnFrom" [
                testCaseTask "return! cancellableValueTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                return! cancellableValueTaskResult { return data }
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to Return! cancellableValueTaskResult"
                    }
                testCaseTask "return! cancellableTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                return! cancellableTaskResult { return data }
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to Return! cancellableTaskResult"
                    }
                testCaseTask "return! taskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskResult { return! taskResult { return data } }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to Return! taskResult"
                    }
                testCaseTask "return! result"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskResult { return! Ok data }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to Return! result"
                    }
                testCaseTask "return! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskResult { return! task { return data } }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to Return! task<'T>"
                    }
                testCaseTask "return! valueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskResult { return! ValueTask.FromResult data }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to Return! valueTask<'T>"
                    }
                testCaseTask "return! cancellableValueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                return! cancellableValueTask { return data }
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to Return! cancellableValueTask<'T>"
                    }
                testCaseTask "return! async<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskResult { return! async { return data } }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to Return! async<'T>"
                    }
            ]

            testList "Binds" [
                testCaseTask "let! cancellableValueTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                let! someValue = cancellableValueTaskResult { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to let! cancellableValueTaskResult"
                    }

                testCaseTask "let! cancellableTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                let! someValue = cancellableTaskResult { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to let! cancellableTaskResult"
                    }

                testCaseTask "let! taskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                let! someValue = taskResult { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to let! taskResult"
                    }

                testCaseTask "let! result - Ok"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                let! someValue = Ok data
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to let! result - Ok"
                    }

                testCaseTask "let! result - Error"
                <| fun () ->
                    task {
                        let errMsg = "SomeError"

                        let ctr =
                            cancellableValueTaskResult {
                                let! someValue = Error errMsg
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Error errMsg) "Should be able to let! result - Error"
                    }

                testCaseTask "let! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                let! someValue = task { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to let! task<'T>"
                    }

                testCaseTask "let! valueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                let! someValue = ValueTask.FromResult data
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to let! valueTask<'T>"
                    }

                testCaseTask "let! cancellableValueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskResult {
                                let! someValue = cancellableValueTask { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to let! cancellableValueTask<'T>"
                    }

                testCaseTask "Error short-circuits"
                <| fun () ->
                    task {
                        let mutable sideEffectExecuted = false
                        let errMsg = "SomeError"

                        let ctr =
                            cancellableValueTaskResult {
                                let! _ = Error errMsg
                                sideEffectExecuted <- true
                                return "something"
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Error errMsg) "Should short-circuit on Error"
                        Expect.isFalse sideEffectExecuted "Side effect should not have executed"
                    }

                testCaseTask "Multiple binds - all Ok"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableValueTaskResult {
                                let! a = Ok 1
                                let! b = Ok 2
                                let! c = cancellableValueTaskResult { return 3 }
                                return a + b + c
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok 6) "Should combine all Ok values"
                    }
            ]

            testList "Cancellation" [
                testCaseTask "Cancellation is respected"
                <| fun () ->
                    task {
                        use cts = new CancellationTokenSource()
                        cts.Cancel()

                        let ctr = cancellableValueTaskResult { return 42 }

                        let! exn =
                            Async.StartImmediateAsTask(
                                async {
                                    try
                                        let! _ =
                                            (ctr cts.Token).AsTask()
                                            |> Async.AwaitTask

                                        return None
                                    with ex ->
                                        return Some ex
                                }
                            )

                        Expect.isSome exn "Should have thrown a cancellation exception"
                    }

                testCaseTask "CancellationToken flows into computation"
                <| fun () ->
                    task {
                        use cts = new CancellationTokenSource()

                        let ctr =
                            cancellableValueTaskResult {
                                let! ct = CancellableValueTaskResult.getCancellationToken ()
                                return ct.CanBeCanceled
                            }

                        let! actual = (ctr cts.Token).AsTask()

                        Expect.equal
                            actual
                            (Ok true)
                            "CancellationToken should flow into computation"
                    }
            ]

            testList "try/with" [
                testCaseTask "try/with - no exception"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableValueTaskResult {
                                try
                                    return 42
                                with ex ->
                                    return -1
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok 42) "Should return normally with no exception"
                    }

                testCaseTask "try/with - exception"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableValueTaskResult {
                                try
                                    failwith "test"
                                    return 42
                                with ex ->
                                    return -1
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok -1) "Should return -1 when exception is thrown"
                    }
            ]

            testList "backgroundCancellableValueTaskResult" [
                testCaseTask "return"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = backgroundCancellableValueTaskResult { return data }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Ok data) "Should be able to Return value"
                    }
            ]
        ]
