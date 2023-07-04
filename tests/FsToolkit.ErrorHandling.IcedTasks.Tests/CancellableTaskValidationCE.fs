namespace FsToolkit.ErrorHandling.IcedTasks.Tests

open IcedTasks
open SampleDomain
open TestData
open TestHelpers
open Expecto
open System.Threading
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.CancellableTaskValidation

module ValidationCompileTests =
    // Just having these compile is a test in itself
    // Ensuring we don't see https://github.com/dotnet/fsharp/issues/12761#issuecomment-1241892425 again
    let testFunctionCTV<'Dto> () =
        cancellableTaskValidation {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionBCTV<'Dto> () =
        backgroundCancellableTaskValidation {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

module CancellableTaskValidationCE =

    let lift = CancellableTaskValidation.ofResult

    let makeDisposable () =
        { new System.IDisposable with
            member this.Dispose() = ()
        }

    let cancellableTaskValidationBuilderTests =
        testList "CancellableTaskValidationBuilder" [
            testList "Return" [
                testCaseTask "return"
                <| fun () ->
                    task {
                        let data = 42
                        let ctr = cancellableTaskValidation { return data }
                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return value"
                    }
            ]

            testList "ReturnFrom" [
                testCaseTask "return! cancellableTaskValidation"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                return! cancellableTaskValidation { return data }
                            }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to Return! cancellableTaskValidation"
                    }

                testCaseTask "return! taskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskValidation { return! taskResult { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! taskResult"
                    }

                testCaseTask "return! asyncResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskValidation { return! asyncResult { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! asyncResult"
                    }

                testCaseTask "return! asyncValidation"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation { return! asyncValidation { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! asyncValidation"
                    }

                testCaseTask "return! asyncChoice"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation { return! async { return Choice1Of2 data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! asyncChoice"
                    }

                testCaseTask "return! valueTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation { return! ValueTask.FromResult(Ok data) }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! valueTaskResult"
                    }

                testCaseTask "return! result"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskValidation { return! Ok data }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! result"
                    }

                testCaseTask "return! validation"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                return!
                                    Ok data
                                    |> Validation.ofResult
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! validation"
                    }

                testCaseTask "return! choice"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskValidation { return! Choice1Of2 data }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! choice"
                    }

                testCaseTask "return! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskValidation { return! task { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! task<'T>"
                    }

                testCaseTask "return! task"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskValidation { return! Task.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! task"
                    }

                testCaseTask "return! valuetask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskValidation { return! ValueTask.FromResult data }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! valuetask<'T>"
                    }

                testCaseTask "return! valuetask"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskValidation { return! ValueTask.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! valuetask"
                    }

                testCaseTask "return! async<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskValidation { return! async { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! async<'T>"
                    }

                testCaseTask "return! ColdTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskValidation { return! coldTask { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! ColdTask<'T>"
                    }

                testCaseTask "return! ColdTask"
                <| fun () ->
                    task {

                        let ctr = cancellableTaskValidation { return! fun () -> Task.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! ColdTask"
                    }

                testCaseTask "return! CancellableTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation { return! cancellableTask { return data } }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to Return! CancellableTask<'T>"
                    }

                testCaseTask "return! CancellableValueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                return! cancellableValueTask { return data }
                            }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to Return! CancellableTask<'T>"
                    }

                testCaseTask "return! CancellableTask"
                <| fun () ->
                    task {

                        let ctr =
                            cancellableTaskValidation {
                                return! fun (ct: CancellationToken) -> Task.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask"
                    }

                testCaseTask "return! CancellableValueTask"
                <| fun () ->
                    task {

                        let ctr =
                            cancellableTaskValidation {
                                return! fun (ct: CancellationToken) -> ValueTask.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask<'T>"
                    }

                testCaseTask "return! TaskLike"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskValidation { return! Task.Yield() }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask"
                    }

                testCaseTask "return! Cold TaskLike"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskValidation { return! fun () -> Task.Yield() }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask"
                    }

                testCaseTask "return! Cancellable TaskLike"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableTaskValidation {
                                return! fun (ct: CancellationToken) -> Task.Yield()
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask"
                    }

            ]

            testList "Binds" [
                testCaseTask "let! cancellableTaskValidation"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = cancellableTaskValidation { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to let! cancellableTaskValidation"
                    }

                testCaseTask "let! taskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = taskResult { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! taskResult"
                    }

                testCaseTask "let! asyncResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = asyncResult { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! asyncResult"
                    }

                testCaseTask "let! asyncValidation"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = asyncValidation { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! asyncResult"
                    }

                testCaseTask "let! asyncChoice"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = async { return Choice1Of2 data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! asyncChoice"
                    }
                testCaseTask "let! valueTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = ValueTask.FromResult(Ok data)
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! valueTaskResult"
                    }
                testCaseTask "let! Result.Ok"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = Ok data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! Result.Ok"
                    }

                testCaseTask "let! Result.Error"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = Error data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Error [ data ]) "Should be able to let! Result.Error"
                    }

                testCaseTask "let! Validation.Ok"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue =
                                    Ok data
                                    |> Validation.ofResult

                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! Validation.Ok"
                    }

                testCaseTask "let! Validation.Error"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue =
                                    Error data
                                    |> Validation.ofResult

                                return someValue
                            }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Error [ [ data ] ])
                            "Should be able to let! Validation.Error"
                    }

                testCaseTask "let! Choice1Of2"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = Choice1Of2 data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! choice"
                    }
                testCaseTask "let! Choice2Of2"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = Choice2Of2 data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Error [ data ]) "Should be able to let! choice"
                    }
                testCaseTask "let! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = task { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! task<'T>"
                    }

                testCaseTask "let! task"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = Task.CompletedTask
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! task"
                    }

                testCaseTask "let! valuetask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = ValueTask.FromResult data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! valuetask<'T>"
                    }

                testCaseTask "let! valuetask"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = ValueTask.CompletedTask
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! valuetask"
                    }

                testCaseTask "let! async<'t>"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = async { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! async<'t>"
                    }

                testCaseTask "let! ColdTask<'t>"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = coldTask { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! ColdTask<'t>"
                    }

                testCaseTask "let! ColdTask"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = fun () -> Task.CompletedTask
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! ColdTask"
                    }

                testCaseTask "let! CancellableTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = cancellableTask { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! CancellableTask<'T>"
                    }

                testCaseTask "let! CancellableValueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = cancellableValueTask { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! CancellableTask<'T>"
                    }
                testCaseTask "let! CancellableTask"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = fun (ct: CancellationToken) -> Task.CompletedTask
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! CancellableTask"
                    }

                testCaseTask "do! CancellableValueTask"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                do! fun (ct: CancellationToken) -> ValueTask.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! CancellableTask"
                    }

                testCaseTask "let! TaskLike"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = Task.Yield()
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! TaskLike"
                    }

                testCaseTask "let! Cold TaskLike"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = fun () -> Task.Yield()
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! TaskLike"
                    }

                testCaseTask "let! Cancellable TaskLike"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskValidation {
                                let! someValue = fun (ct: CancellationToken) -> Task.Yield()
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! TaskLike"
                    }

            ]

            testList "Zero/Combine/Delay" [
                testCaseAsync "if statement"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskValidation {
                            let result = data

                            if true then
                                ()

                            return result
                        }

                    Expect.equal actual (Ok data) "Zero/Combine/Delay should work"
                }
            ]

            testList "TryWith" [
                testCaseAsync "try with"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskValidation {
                            let data = data

                            try
                                ()
                            with _ ->
                                ()

                            return data
                        }

                    Expect.equal actual (Ok data) "TryWith should work"
                }
            ]

            testList "TryFinally" [
                testCaseAsync "try finally"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskValidation {
                            let data = data

                            try
                                ()
                            finally
                                ()

                            return data
                        }

                    Expect.equal actual (Ok data) "TryFinally should work"
                }
            ]

            testList "Using" [
                testCaseAsync "use"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskValidation {
                            use d = makeDisposable ()
                            return data
                        }

                    Expect.equal actual (Ok data) "Should be able to use use"
                }
                testCaseAsync "use!"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskValidation {
                            use! d =
                                makeDisposable ()
                                |> async.Return

                            return data
                        }

                    Expect.equal actual (Ok data) "Should be able to use use!"
                }
                testCaseAsync "null"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskValidation {
                            use d = null
                            return data
                        }

                    Expect.equal actual (Ok data) "Should be able to use null"
                }
            ]

            testList "While" [
                testCaseAsync "while 10"
                <| async {
                    let loops = 10
                    let mutable index = 0

                    let! actual =
                        cancellableTaskValidation {
                            while index < loops do
                                index <- index + 1

                            return index
                        }

                    Expect.equal actual (Ok loops) "Should be ok"
                }

                testCaseAsync "while 10000000"
                <| async {
                    let loops = 10000000
                    let mutable index = 0

                    let! actual =
                        cancellableTaskValidation {
                            while index < loops do
                                index <- index + 1

                            return index
                        }

                    Expect.equal actual (Ok loops) "Should be ok"
                }

                testCaseTask "while fail"
                <| fun () ->
                    task {

                        let mutable loopCount = 0
                        let mutable wasCalled = false

                        let sideEffect () =
                            wasCalled <- true
                            "ok"

                        let expected = Error [ "error" ]

                        let data = [
                            Ok "42"
                            Ok "1024"
                            Error "error"
                            Ok "1M"
                            Ok "1M"
                            Ok "1M"
                        ]

                        let ctr =
                            cancellableTaskValidation {
                                while loopCount < data.Length do
                                    let! x = data.[loopCount]

                                    loopCount <-
                                        loopCount
                                        + 1

                                return sideEffect ()
                            }

                        let! actual = ctr CancellationToken.None

                        Expect.equal loopCount 2 "Should only loop twice"
                        Expect.equal actual expected "Should be an error"
                        Expect.isFalse wasCalled "No additional side effects should occur"
                    }

            ]

            testList "For" [
                testCaseAsync "for in"
                <| async {
                    let loops = 10
                    let mutable index = 0

                    let! actual =
                        cancellableTaskValidation {
                            for i in [ 1..10 ] do
                                index <- i + i

                            return index
                        }

                    Expect.equal actual (Ok index) "Should be ok"
                }


                testCaseAsync "for to"
                <| async {
                    let loops = 10
                    let mutable index = 0

                    let! actual =
                        cancellableTaskValidation {
                            for i = 1 to loops do
                                index <- i + i

                            return index
                        }

                    Expect.equal actual (Ok index) "Should be ok"
                }
                testCaseTask "for in fail"
                <| fun () ->
                    task {

                        let mutable loopCount = 0
                        let expected = Error [ "error" ]

                        let data = [
                            Ok "42"
                            Ok "1024"
                            Error "error"
                            Ok "1M"
                            Ok "1M"
                            Ok "1M"
                        ]

                        let ctr =
                            cancellableTaskValidation {
                                for i in data do
                                    let! x = i

                                    loopCount <-
                                        loopCount
                                        + 1

                                    ()

                                return "ok"
                            }

                        let! actual = ctr CancellationToken.None

                        Expect.equal loopCount 2 "Should only loop twice"
                        Expect.equal actual expected "Should be an error"
                    }
            ]

            testList "applicatives" [
                testCaseTask "Happy Path Result"
                <| fun () ->
                    task {
                        let actual =
                            cancellableTaskValidation {
                                let! a = Ok 3
                                and! b = Ok 2
                                and! c = Ok 1
                                return a + b - c
                            }

                        let! actual = actual CancellationToken.None
                        Expect.equal actual (Ok 4) "Should be ok"
                    }
                testCaseTask "Happy Path Validation"
                <| fun () ->
                    task {
                        let actual =
                            cancellableTaskValidation {
                                let! a = CancellableTaskValidation.ok 3
                                and! b = CancellableTaskValidation.ok 2
                                and! c = CancellableTaskValidation.ok 1
                                return a + b - c
                            }

                        let! actual = actual CancellationToken.None
                        Expect.equal actual (Ok 4) "Should be ok"
                    }

                testCaseTask "Happy Path Result/Valiation"
                <| fun () ->
                    task {
                        let actual =
                            cancellableTaskValidation {
                                let! a = CancellableTaskValidation.ok 3
                                and! b = Ok 2
                                and! c = CancellableTaskValidation.ok 1
                                return a + b - c
                            }

                        let! actual = actual CancellationToken.None
                        Expect.equal actual (Ok 4) "Should be ok"
                    }

                testCaseTask "Happy Path Choice"
                <| fun () ->
                    task {
                        let actual =
                            cancellableTaskValidation {
                                let! a = Choice1Of2 3
                                and! b = Choice1Of2 2
                                and! c = Choice1Of2 1
                                return a + b - c
                            }

                        let! actual = actual CancellationToken.None
                        Expect.equal actual (Ok 4) "Should be ok"
                    }

                testCaseTask "Happy Path Result/Choice/Validation"
                <| fun () ->
                    task {
                        let actual =
                            cancellableTaskValidation {
                                let! a = Ok 3
                                and! b = Choice1Of2 2
                                and! c = CancellableTaskValidation.ok 1
                                return a + b - c
                            }

                        let! actual = actual CancellationToken.None
                        Expect.equal actual (Ok 4) "Should be ok"
                    }

                testCaseTask "Fail Path Result"
                <| fun () ->
                    task {
                        let expected =
                            Error [
                                "Error 1"
                                "Error 2"
                            ]

                        let actual =
                            cancellableTaskValidation {
                                let! a = Ok 3
                                and! b = Ok 2
                                and! c = Error "Error 1"
                                and! d = Error "Error 2"

                                return
                                    a + b
                                    - c
                                    - d
                            }

                        let! actual = actual CancellationToken.None
                        Expect.equal actual expected "Should be Error"
                    }

                testCaseTask "Fail Path Validation"
                <| fun () ->
                    task {
                        let expected = CancellableTaskValidation.error "TryParse failure"
                        let! expected' = expected CancellationToken.None

                        let actual =
                            cancellableTaskValidation {
                                let! a = CancellableTaskValidation.ok 3
                                and! b = CancellableTaskValidation.ok 2
                                and! c = expected
                                return a + b - c
                            }

                        let! actual = actual CancellationToken.None
                        Expect.equal actual expected' "Should be Error"
                    }
            ]

            testList "Cancellations" [
                testCaseTask "Simple Cancellation"
                <| fun () ->
                    task {
                        do!
                            Expect.CancellationRequested(
                                task {
                                    let foo = cancellableTaskValidation { return "lol" }
                                    use cts = new CancellationTokenSource()
                                    cts.Cancel()
                                    let! result = foo cts.Token
                                    failtestf "Should not get here"
                                }
                            )
                    }

                testCaseTask "CancellableTasks are lazily evaluated"
                <| fun () ->
                    task {

                        let mutable someValue = null

                        do!
                            Expect.CancellationRequested(
                                task {
                                    let work = cancellableTaskValidation { someValue <- "lol" }

                                    do! Async.Sleep(100)
                                    Expect.equal someValue null ""
                                    use cts = new CancellationTokenSource()
                                    cts.Cancel()
                                    let workInProgress = work cts.Token
                                    do! Async.Sleep(100)
                                    Expect.equal someValue null ""

                                    let! _ = workInProgress

                                    failtestf "Should not get here"
                                }
                            )
                    }

                testCase
                    "CancellationToken flows from Async<T> to CancellableTaskValidation<T> via Async.AwaitCancellableTask"
                <| fun () ->
                    let innerTask =
                        cancellableTaskValidation {
                            return! CancellableTaskValidation.getCancellationToken ()
                        }

                    let outerAsync =
                        async {
                            return!
                                innerTask
                                |> Async.AwaitCancellableTaskValidation
                        }

                    use cts = new CancellationTokenSource()

                    let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)

                    Expect.equal actual (Ok cts.Token) ""

                testCase
                    "CancellationToken flows from AsyncResult<T> to CancellableTaskValidation<T>"
                <| fun () ->
                    let innerTask =
                        cancellableTaskValidation {
                            return! CancellableTaskValidation.getCancellationToken ()
                        }

                    let outerAsync = asyncResult { return! innerTask }

                    use cts = new CancellationTokenSource()

                    let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)

                    Expect.equal actual (Ok cts.Token) ""

                testCase "CancellationToken flows from CancellableTaskValidation<T> to Async<unit>"
                <| fun () ->
                    let innerAsync = async { return! Async.CancellationToken }

                    let outerTask = cancellableTaskValidation { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskValidation<T> to AsyncResult<unit>"
                <| fun () ->
                    let innerAsync = asyncResult { return! Async.CancellationToken }

                    let outerTask = cancellableTaskValidation { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskValidation<T> to CancellableTask<T>"
                <| fun () ->
                    let innerAsync =
                        cancellableTask { return! CancellableTask.getCancellationToken () }

                    let outerTask = cancellableTaskValidation { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskResult<T> to CancellableTaskValidation<T>"
                <| fun () ->
                    let innerAsync =
                        cancellableTask {
                            return! CancellableTaskValidation.getCancellationToken ()
                        }

                    let outerTask = cancellableTaskValidation { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskValidation<T> to CancellableTaskResult<T>"
                <| fun () ->
                    let innerAsync =
                        cancellableTaskValidation {
                            return! CancellableTaskValidation.getCancellationToken ()
                        }

                    let outerTask = cancellableTask { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskValidation<T> to CancellableTaskValidation<T>"
                <| fun () ->
                    let innerAsync =
                        cancellableTaskValidation {
                            return! CancellableTaskValidation.getCancellationToken ()
                        }

                    let outerTask = cancellableTaskValidation { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""
            ]
        ]

    let functionTests =
        testList "functions" [
            testList "singleton" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = CancellableTaskValidation.ok "lol"

                    let! someTask = innerCall

                    Expect.equal (Ok "lol") someTask ""
                }
            ]

            testList "bind" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskValidation { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskValidation.bind (fun x ->
                            cancellableTaskValidation { return x + "fooo" }
                        )

                    Expect.equal (Ok "lolfooo") someTask ""
                }
            ]

            testList "map" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskValidation { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskValidation.map (fun x -> x + "fooo")

                    Expect.equal (Ok "lolfooo") someTask ""
                }
            ]

            testList "mapError" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskValidation { return! Error "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskValidation.mapError (fun x -> x + x)

                    Expect.equal (Error [ "lollol" ]) someTask ""
                }
            ]

            testList "mapErrors" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskValidation { return! Error "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskValidation.mapErrors (fun x ->
                            "added"
                            :: x
                        )

                    Expect.equal
                        (Error [
                            "added"
                            "lol"
                        ])
                        someTask
                        ""
                }
            ]

            testList "map2" [
                testCaseAsync "map2 with two ok parts"
                <| async {
                    let! result =
                        CancellableTaskValidation.map2 location (lift validLatR) (lift validLngR)

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
                        CancellableTaskValidation.map2
                            location
                            (lift invalidLatR)
                            (lift invalidLngR)

                    return
                        result
                        |> Expect.hasErrorValue [
                            invalidLatMsg
                            invalidLngMsg
                        ]
                }
            ]

            testList "map3" [
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

            testList "apply" [
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

            testList "orElse" [
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

            testList "orElseWith" [
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

            testList "zip" [
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

            testList "parallelZip" [
                testCaseAsync "Ok, Ok"
                <| async {
                    let! actual =
                        CancellableTaskValidation.parallelZip
                            (Ok 1
                             |> CancellableTask.singleton)
                            (Ok 2
                             |> CancellableTask.singleton)

                    Expect.equal actual (Ok(1, 2)) "Should be ok"
                }
                testCaseAsync "Ok, Error"
                <| async {
                    let! actual =
                        CancellableTaskValidation.parallelZip
                            (Ok 1
                             |> CancellableTask.singleton)
                            (CancellableTaskValidation.error "Bad")

                    Expect.equal actual (Error [ "Bad" ]) "Should be Error"
                }
                testCaseAsync "Error, Ok"
                <| async {
                    let! actual =
                        CancellableTaskValidation.parallelZip
                            (CancellableTaskValidation.error "Bad")
                            (Ok 1
                             |> CancellableTask.singleton)

                    Expect.equal actual (Error [ "Bad" ]) "Should be Error"
                }
                testCaseAsync "Error, Error"
                <| async {
                    let! actual =
                        CancellableTaskValidation.parallelZip
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

            testList "operators" [
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
        ]

    [<Tests>]
    let cancellableTaskValidationTests =
        testList "CancellableTaskValidation" [
            cancellableTaskValidationBuilderTests
            functionTests
        ]
