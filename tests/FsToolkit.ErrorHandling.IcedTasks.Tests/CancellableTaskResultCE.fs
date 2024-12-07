namespace FsToolkit.ErrorHandling.IcedTasks.Tests

open Expecto
open System.Threading
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open IcedTasks

module CompileTests =
    // Just having these compile is a test in itself
    // Ensuring we don't see https://github.com/dotnet/fsharp/issues/12761#issuecomment-1241892425 again
    let testFunctionCTR<'Dto> () =
        cancellableTaskResult {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionBCTR<'Dto> () =
        backgroundCancellableTaskResult {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionCanT<'Dto> () =
        cancellableTask {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionCoT<'Dto> () =
        coldTask {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionBTR<'Dto> () =
        backgroundTaskResult {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionBTO<'Dto> () =
        backgroundTaskOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionTR<'Dto> () =
        taskResult {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionTO<'Dto> () =
        taskOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

module CancellableTaskResultCE =

    let makeDisposable () =
        { new System.IDisposable with
            member this.Dispose() = ()
        }


    let cancellableTaskResultBuilderTests =
        testList "CancellableTaskResultBuilder" [
            testList "Return" [
                testCaseTask "return"
                <| fun () ->
                    task {
                        let data = 42
                        let ctr = cancellableTaskResult { return data }
                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return value"
                    }
            ]
            testList "ReturnFrom" [
                testCaseTask "return! cancellableTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskResult { return! cancellableTaskResult { return data } }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Ok data)
                            "Should be able to Return! cancellableTaskResult"
                    }
                testCaseTask "return! taskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! taskResult { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! taskResult"
                    }
                testCaseTask "return! asyncResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! asyncResult { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! asyncResult"
                    }
                testCaseTask "return! asyncChoice"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! async { return Choice1Of2 data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! asyncChoice"
                    }

                testCaseTask "return! valueTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! ValueTask.FromResult(Ok data) }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! valueTaskResult"
                    }
                testCaseTask "return! result"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! Ok data }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! result"
                    }
                testCaseTask "return! choice"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! Choice1Of2 data }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! choice"
                    }

                testCaseTask "return! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! task { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! task<'T>"
                    }

                testCaseTask "return! task"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskResult { return! Task.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! task"
                    }

                testCaseTask "return! valuetask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! ValueTask.FromResult data }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! valuetask<'T>"
                    }

                testCaseTask "return! valuetask"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskResult { return! ValueTask.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! valuetask"
                    }

                testCaseTask "return! async<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! async { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! async<'T>"
                    }
                testCaseTask "return! ColdTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! coldTask { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! ColdTask<'T>"
                    }

                testCaseTask "return! ColdTask"
                <| fun () ->
                    task {

                        let ctr = cancellableTaskResult { return! fun () -> Task.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! ColdTask"
                    }

                testCaseTask "return! CancellableTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskResult { return! cancellableTask { return data } }

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
                            cancellableTaskResult { return! cancellableValueTask { return data } }

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
                            cancellableTaskResult {
                                return! fun (ct: CancellationToken) -> Task.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask"
                    }

                testCaseTask "return! CancellableValueTask"
                <| fun () ->
                    task {

                        let ctr =
                            cancellableTaskResult {
                                return! fun (ct: CancellationToken) -> ValueTask.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask<'T>"
                    }

                testCaseTask "return! TaskLike"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskResult { return! Task.Yield() }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask"
                    }
                testCaseTask "return! Cold TaskLike"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskResult { return! fun () -> Task.Yield() }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask"
                    }

                testCaseTask "return! Cancellable TaskLike"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableTaskResult {
                                return! fun (ct: CancellationToken) -> Task.Yield()
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok()) "Should be able to Return! CancellableTask"
                    }

            ]

            testList "Binds" [
                testCaseTask "let! cancellableTaskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskResult {
                                let! someValue = cancellableTaskResult { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! cancellableTaskResult"
                    }

                testCaseTask "let! taskResult"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskResult {
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
                            cancellableTaskResult {
                                let! someValue = asyncResult { return data }
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
                                let! someValue = Error data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Error data) "Should be able to let! Result.Error"
                    }
                testCaseTask "let! Choice1Of2"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskResult {
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
                            cancellableTaskResult {
                                let! someValue = Choice2Of2 data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Error data) "Should be able to let! choice"
                    }
                testCaseTask "let! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
                                do! fun (ct: CancellationToken) -> ValueTask.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to let! CancellableTask"
                    // return someValue
                    }
                testCaseTask "let! TaskLike"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                            cancellableTaskResult {
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
                        cancellableTaskResult {
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
                        cancellableTaskResult {
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
                        cancellableTaskResult {
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
                        cancellableTaskResult {
                            use d = makeDisposable ()
                            return data
                        }

                    Expect.equal actual (Ok data) "Should be able to use use"
                }
                testCaseAsync "use!"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskResult {
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
                        cancellableTaskResult {
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
                        cancellableTaskResult {
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
                        cancellableTaskResult {
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

                        let expected = Error "error"

                        let data = [
                            Ok "42"
                            Ok "1024"
                            expected
                            Ok "1M"
                            Ok "1M"
                            Ok "1M"
                        ]

                        let ctr =
                            cancellableTaskResult {
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
                        cancellableTaskResult {
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
                        cancellableTaskResult {
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
                        let expected = Error "error"

                        let data = [
                            Ok "42"
                            Ok "1024"
                            expected
                            Ok "1M"
                            Ok "1M"
                            Ok "1M"
                        ]

                        let ctr =
                            cancellableTaskResult {
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
            testList "Cancellations" [
                testCaseTask "Simple Cancellation"
                <| fun () ->
                    task {
                        do!
                            Expect.CancellationRequested(
                                task {
                                    let foo = cancellableTaskResult { return "lol" }
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
                                    let work = cancellableTaskResult { someValue <- "lol" }

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
                    "CancellationToken flows from Async<T> to CancellableTaskResult<T> via Async.AwaitCancellableTask"
                <| fun () ->
                    let innerTask =
                        cancellableTaskResult {
                            return! CancellableTaskResult.getCancellationToken ()
                        }

                    let outerAsync =
                        async {
                            return!
                                innerTask
                                |> Async.AwaitCancellableTaskResult
                        }

                    use cts = new CancellationTokenSource()

                    let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)

                    Expect.equal actual (Ok cts.Token) ""


                testCase "CancellationToken flows from AsyncResult<T> to CancellableTaskResult<T>"
                <| fun () ->
                    let innerTask =
                        cancellableTaskResult {
                            return! CancellableTaskResult.getCancellationToken ()
                        }

                    let outerAsync = asyncResult { return! innerTask }

                    use cts = new CancellationTokenSource()

                    let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)

                    Expect.equal actual (Ok cts.Token) ""

                testCase "CancellationToken flows from CancellableTasResultk<T> to Async<unit>"
                <| fun () ->
                    let innerAsync = async { return! Async.CancellationToken }

                    let outerTask = cancellableTaskResult { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskResult<T> to AsyncResult<unit>"
                <| fun () ->
                    let innerAsync = asyncResult { return! Async.CancellationToken }

                    let outerTask = cancellableTaskResult { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""
                testCase
                    "CancellationToken flows from CancellableTaskResult<T> to CancellableTask<T>"
                <| fun () ->
                    let innerAsync =
                        cancellableTask { return! CancellableTask.getCancellationToken () }

                    let outerTask = cancellableTaskResult { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Ok cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskResult<T> to CancellableTaskResult<T>"
                <| fun () ->
                    let innerAsync =
                        cancellableTaskResult {
                            return! CancellableTaskResult.getCancellationToken ()
                        }

                    let outerTask = cancellableTaskResult { return! innerAsync }

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
                    let innerCall = CancellableTaskResult.singleton "lol"

                    let! someTask = innerCall

                    Expect.equal (Ok "lol") someTask ""
                }
            ]
            testList "bind" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskResult { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskResult.bind (fun x ->
                            cancellableTaskResult { return x + "fooo" }
                        )

                    Expect.equal (Ok "lolfooo") someTask ""
                }
            ]
            testList "map" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskResult { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskResult.map (fun x -> x + "fooo")

                    Expect.equal (Ok "lolfooo") someTask ""
                }
            ]
            testList "apply" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskResult { return "lol" }
                    let applier = cancellableTaskResult { return fun x -> x + "fooo" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskResult.apply applier

                    Expect.equal (Ok "lolfooo") someTask ""
                }
            ]

            testList "zip" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskResult { return "fooo" }
                    let innerCall2 = cancellableTaskResult { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskResult.zip innerCall2

                    Expect.equal (Ok("lol", "fooo")) someTask ""
                }
            ]

            testList "parZip" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskResult { return "fooo" }
                    let innerCall2 = cancellableTaskResult { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskResult.parallelZip innerCall2

                    Expect.equal (Ok("lol", "fooo")) someTask ""
                }
            ]
        ]

    [<Tests>]
    let ``CancellableTaskResultCE inference checks`` =
        testList "CancellableTaskResultCE inference checks" [
            testCase "Inference checks"
            <| fun () ->
                // Compilation is success
                let f res = cancellableTaskResult { return! res }

                f (CancellableTaskResult.singleton ())
                |> ignore
        ]


    [<Tests>]
    let cancellableTaskResultTests =
        testList "CancellableTaskResult" [
            cancellableTaskResultBuilderTests
            functionTests
            ``CancellableTaskResultCE inference checks``
        ]
