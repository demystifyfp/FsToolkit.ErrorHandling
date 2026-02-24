namespace FsToolkit.ErrorHandling.IcedTasks.Tests

open Expecto
open System.Threading
open System.Threading.Tasks
open IcedTasks
open FsToolkit.ErrorHandling

module ValueTaskOptionCompileTests =
    // Just having these compile is a test in itself
    let testFunctionCVTO<'Dto> () =
        cancellableValueTaskOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionBCVTO<'Dto> () =
        backgroundCancellableValueTaskOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

module CancellableValueTaskOptionCE =

    let makeDisposable () =
        { new System.IDisposable with
            member this.Dispose() = ()
        }


    [<Tests>]
    let cancellableValueTaskOptionBuilderTests =
        testList "CancellableValueTaskOptionBuilder" [
            testList "Return" [
                testCaseTask "return"
                <| fun () ->
                    task {
                        let data = 42
                        let ctr = cancellableValueTaskOption { return data }
                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return value"
                    }
            ]
            testList "ReturnFrom" [
                testCaseTask "return! cancellableValueTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                return! cancellableValueTaskOption { return data }
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to Return! cancellableValueTaskOption"
                    }
                testCaseTask "return! cancellableTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                return! cancellableTaskOption { return data }
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to Return! cancellableTaskOption"
                    }
                testCaseTask "return! taskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskOption { return! taskOption { return data } }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return! taskOption"
                    }
                testCaseTask "return! asyncOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskOption { return! asyncOption { return data } }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return! asyncOption"
                    }
                testCaseTask "return! valueTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption { return! ValueTask.FromResult(Some data) }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return! valueTaskOption"
                    }
                testCaseTask "return! option"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskOption { return! Some data }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return! option"
                    }
                testCaseTask "return! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskOption { return! task { return data } }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return! task<'T>"
                    }
                testCaseTask "return! valuetask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskOption { return! ValueTask.FromResult data }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return! valuetask<'T>"
                    }
                testCaseTask "return! cancellableTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption { return! cancellableTask { return data } }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to Return! cancellableTask<'T>"
                    }
                testCaseTask "return! cancellableValueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                return! cancellableValueTask { return data }
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to Return! cancellableValueTask<'T>"
                    }
                testCaseTask "return! async<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableValueTaskOption { return! async { return data } }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return! async<'T>"
                    }
            ]

            testList "Binds" [
                testCaseTask "let! cancellableValueTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                let! someValue = cancellableValueTaskOption { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to let! cancellableValueTaskOption"
                    }

                testCaseTask "let! cancellableTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                let! someValue = cancellableTaskOption { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to let! cancellableTaskOption"
                    }

                testCaseTask "let! taskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                let! someValue = taskOption { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to let! taskOption"
                    }

                testCaseTask "let! option - Some"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                let! someValue = Some data
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to let! option - Some"
                    }

                testCaseTask "let! option - None"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableValueTaskOption {
                                let! someValue = (None: int option)
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual None "Should be able to let! option - None"
                    }

                testCaseTask "let! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                let! someValue = task { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to let! task<'T>"
                    }

                testCaseTask "let! valueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                let! someValue = ValueTask.FromResult data
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to let! valueTask<'T>"
                    }

                testCaseTask "let! cancellableValueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableValueTaskOption {
                                let! someValue = cancellableValueTask { return data }
                                return someValue
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to let! cancellableValueTask<'T>"
                    }

                testCaseTask "None short-circuits"
                <| fun () ->
                    task {
                        let mutable sideEffectExecuted = false

                        let ctr =
                            cancellableValueTaskOption {
                                let! _ = (None: int option)
                                sideEffectExecuted <- true
                                return 42
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual None "Should short-circuit on None"
                        Expect.isFalse sideEffectExecuted "Side effect should not have executed"
                    }

                testCaseTask "Multiple binds - all Some"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableValueTaskOption {
                                let! a = Some 1
                                let! b = Some 2
                                let! c = cancellableValueTaskOption { return 3 }
                                return a + b + c
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some 6) "Should combine all Some values"
                    }
            ]

            testList "Cancellation" [
                testCaseTask "Cancellation is respected"
                <| fun () ->
                    task {
                        use cts = new CancellationTokenSource()
                        cts.Cancel()

                        let ctr = cancellableValueTaskOption { return 42 }

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
                            cancellableValueTaskOption {
                                let! ct = CancellableValueTaskOption.getCancellationToken ()
                                return ct.CanBeCanceled
                            }

                        let! actual = (ctr cts.Token).AsTask()

                        Expect.equal
                            actual
                            (Some true)
                            "CancellationToken should flow into computation"
                    }
            ]

            testList "try/with" [
                testCaseTask "try/with - no exception"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableValueTaskOption {
                                try
                                    return 42
                                with ex ->
                                    return -1
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some 42) "Should return normally with no exception"
                    }

                testCaseTask "try/with - exception"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableValueTaskOption {
                                try
                                    failwith "test"
                                    return 42
                                with ex ->
                                    return -1
                            }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some -1) "Should return -1 when exception is thrown"
                    }
            ]

            testList "backgroundCancellableValueTaskOption" [
                testCaseTask "return"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = backgroundCancellableValueTaskOption { return data }

                        let! actual = (ctr CancellationToken.None).AsTask()
                        Expect.equal actual (Some data) "Should be able to Return value"
                    }
            ]
        ]
