namespace FsToolkit.ErrorHandling.IcedTasks.Tests

open Expecto
open System.Threading
open System.Threading.Tasks
open IcedTasks
open FsToolkit.ErrorHandling

module OptionCompileTests =
    // Just having these compile is a test in itself
    // Ensuring we don't see https://github.com/dotnet/fsharp/issues/12761#issuecomment-1241892425 again
    let testFunctionCTO<'Dto> () =
        cancellableTaskOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

    let testFunctionBCTO<'Dto> () =
        backgroundCancellableTaskOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

module CancellableTaskOptionCE =

    let makeDisposable () =
        { new System.IDisposable with
            member this.Dispose() = ()
        }


    let cancellableTaskOptionBuilderTests =
        testList "CancellableTaskOptionBuilder" [
            testList "Return" [
                testCaseTask "return"
                <| fun () ->
                    task {
                        let data = 42
                        let ctr = cancellableTaskOption { return data }
                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return value"
                    }
            ]
            testList "ReturnFrom" [
                testCaseTask "return! cancellableTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption { return! cancellableTaskOption { return data } }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to Return! cancellableTaskOption"
                    }
                testCaseTask "return! taskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! taskOption { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return! taskOption"
                    }
                testCaseTask "return! asyncOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! asyncOption { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return! asyncOption"
                    }

                testCaseTask "return! valueTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! ValueTask.FromResult(Some data) }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return! valueTaskOption"
                    }
                testCaseTask "return! option"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! Some data }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return! option"
                    }

                testCaseTask "return! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! task { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return! task<'T>"
                    }

                testCaseTask "return! task"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskOption { return! Task.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some()) "Should be able to Return! task"
                    }

                testCaseTask "return! valuetask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! ValueTask.FromResult data }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return! valuetask<'T>"
                    }

                testCaseTask "return! valuetask"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskOption { return! ValueTask.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some()) "Should be able to Return! valuetask"
                    }

                testCaseTask "return! async<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! async { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return! async<'T>"
                    }
                testCaseTask "return! ColdTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! coldTask { return data } }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to Return! ColdTask<'T>"
                    }

                testCaseTask "return! ColdTask"
                <| fun () ->
                    task {

                        let ctr = cancellableTaskOption { return! fun () -> Task.CompletedTask }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some()) "Should be able to Return! ColdTask"
                    }

                testCaseTask "return! CancellableTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr = cancellableTaskOption { return! cancellableTask { return data } }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to Return! CancellableTask<'T>"
                    }

                testCaseTask "return! CancellableValueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption { return! cancellableValueTask { return data } }

                        let! actual = ctr CancellationToken.None

                        Expect.equal
                            actual
                            (Some data)
                            "Should be able to Return! CancellableTask<'T>"
                    }

                testCaseTask "return! CancellableTask"
                <| fun () ->
                    task {

                        let ctr =
                            cancellableTaskOption {
                                return! fun (ct: CancellationToken) -> Task.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some()) "Should be able to Return! CancellableTask"
                    }

                testCaseTask "return! CancellableValueTask"
                <| fun () ->
                    task {

                        let ctr =
                            cancellableTaskOption {
                                return! fun (ct: CancellationToken) -> ValueTask.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some()) "Should be able to Return! CancellableTask<'T>"
                    }

                testCaseTask "return! TaskLike"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskOption { return! Task.Yield() }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some()) "Should be able to Return! CancellableTask"
                    }
                testCaseTask "return! Cold TaskLike"
                <| fun () ->
                    task {
                        let ctr = cancellableTaskOption { return! fun () -> Task.Yield() }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some()) "Should be able to Return! CancellableTask"
                    }

                testCaseTask "return! Cancellable TaskLike"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableTaskOption {
                                return! fun (ct: CancellationToken) -> Task.Yield()
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some()) "Should be able to Return! CancellableTask"
                    }

            ]

            testList "Binds" [
                testCaseTask "let! cancellableTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = cancellableTaskOption { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None

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
                            cancellableTaskOption {
                                let! someValue = taskOption { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! taskOption"
                    }

                testCaseTask "let! asyncOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = asyncOption { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! asyncResult"
                    }

                testCaseTask "let! valueTaskOption"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = ValueTask.FromResult(Some data)
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! valueTaskOption"
                    }
                testCaseTask "let! Option.Some"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = Some data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! Option.Some"
                    }

                testCaseTask "let! Option.None"
                <| fun () ->
                    task {
                        let ctr =
                            cancellableTaskOption {
                                let! someValue = None
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual None "Should be able to let! Option.None"
                    }

                testCaseTask "let! task<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = task { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! task<'T>"
                    }

                testCaseTask "let! task"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = Task.CompletedTask
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! task"
                    }

                testCaseTask "let! valuetask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = ValueTask.FromResult data
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! valuetask<'T>"
                    }

                testCaseTask "let! valuetask"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = ValueTask.CompletedTask
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! valuetask"
                    }

                testCaseTask "let! async<'t>"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = async { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! async<'t>"
                    }

                testCaseTask "let! ColdTask<'t>"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = coldTask { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! ColdTask<'t>"
                    }
                testCaseTask "let! ColdTask"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = fun () -> Task.CompletedTask
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! ColdTask"
                    }
                testCaseTask "let! CancellableTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = cancellableTask { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! CancellableTask<'T>"
                    }
                testCaseTask "let! CancellableValueTask<'T>"
                <| fun () ->
                    task {
                        let data = 42

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = cancellableValueTask { return data }
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! CancellableTask<'T>"
                    }
                testCaseTask "let! CancellableTask"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = fun (ct: CancellationToken) -> Task.CompletedTask
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! CancellableTask"
                    }

                testCaseTask "do! CancellableValueTask"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                do! fun (ct: CancellationToken) -> ValueTask.CompletedTask
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to do! CancellableValueTask"
                    }
                testCaseTask "let! TaskLike"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = Task.Yield()
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! TaskLike"
                    }

                testCaseTask "let! Cold TaskLike"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = fun () -> Task.Yield()
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! TaskLike"
                    }

                testCaseTask "let! Cancellable TaskLike"
                <| fun () ->
                    task {
                        let data = ()

                        let ctr =
                            cancellableTaskOption {
                                let! someValue = fun (ct: CancellationToken) -> Task.Yield()
                                return someValue
                            }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Some data) "Should be able to let! TaskLike"
                    }

            ]
            testList "Zero/Combine/Delay" [
                testCaseAsync "if statement"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskOption {
                            let result = data

                            if true then
                                ()

                            return result
                        }

                    Expect.equal actual (Some data) "Zero/Combine/Delay should work"
                }
            ]
            testList "TryWith" [
                testCaseAsync "try with"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskOption {
                            let data = data

                            try
                                ()
                            with _ ->
                                ()

                            return data
                        }

                    Expect.equal actual (Some data) "TryWith should work"
                }
            ]


            testList "TryFinally" [
                testCaseAsync "try finally"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskOption {
                            let data = data

                            try
                                ()
                            finally
                                ()

                            return data
                        }

                    Expect.equal actual (Some data) "TryFinally should work"
                }
            ]

            testList "Using" [
                testCaseAsync "use"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskOption {
                            use d = makeDisposable ()
                            return data
                        }

                    Expect.equal actual (Some data) "Should be able to use use"
                }
                testCaseAsync "use!"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskOption {
                            use! d =
                                makeDisposable ()
                                |> async.Return

                            return data
                        }

                    Expect.equal actual (Some data) "Should be able to use use!"
                }
                testCaseAsync "null"
                <| async {
                    let data = 42

                    let! actual =
                        cancellableTaskOption {
                            use d = null
                            return data
                        }

                    Expect.equal actual (Some data) "Should be able to use null"
                }
            ]

            testList "While" [
                testCaseAsync "while 10"
                <| async {
                    let loops = 10
                    let mutable index = 0

                    let! actual =
                        cancellableTaskOption {
                            while index < loops do
                                index <- index + 1

                            return index
                        }

                    Expect.equal actual (Some loops) "Should be some"
                }

                testCaseAsync "while 10000000"
                <| async {
                    let loops = 10000000
                    let mutable index = 0

                    let! actual =
                        cancellableTaskOption {
                            while index < loops do
                                index <- index + 1

                            return index
                        }

                    Expect.equal actual (Some loops) "Should be some"
                }

                testCaseTask "while fail"
                <| fun () ->
                    task {

                        let mutable loopCount = 0
                        let mutable wasCalled = false

                        let sideEffect () =
                            wasCalled <- true
                            "some"

                        let expected = None

                        let data = [
                            Some "42"
                            Some "1024"
                            expected
                            Some "1M"
                            Some "1M"
                            Some "1M"
                        ]

                        let ctr =
                            cancellableTaskOption {
                                while loopCount < data.Length do
                                    let! x = data.[loopCount]

                                    loopCount <-
                                        loopCount
                                        + 1

                                return sideEffect ()
                            }

                        let! actual = ctr CancellationToken.None

                        Expect.equal loopCount 2 "Should only loop twice"
                        Expect.equal actual expected "Should be none"
                        Expect.isFalse wasCalled "No additional side effects should occur"
                    }

            ]

            testList "For" [
                testCaseAsync "for in"
                <| async {
                    let mutable index = 0

                    let! actual =
                        cancellableTaskOption {
                            for i in [ 1..10 ] do
                                index <- i + i

                            return index
                        }

                    Expect.equal actual (Some index) "Should be some"
                }


                testCaseAsync "for to"
                <| async {
                    let loops = 10
                    let mutable index = 0

                    let! actual =
                        cancellableTaskOption {
                            for i = 1 to loops do
                                index <- i + i

                            return index
                        }

                    Expect.equal actual (Some index) "Should be some"
                }
                testCaseTask "for in fail"
                <| fun () ->
                    task {

                        let mutable loopCount = 0
                        let expected = None

                        let data = [
                            Some "42"
                            Some "1024"
                            expected
                            Some "1M"
                            Some "1M"
                            Some "1M"
                        ]

                        let ctr =
                            cancellableTaskOption {
                                for i in data do
                                    let! x = i

                                    loopCount <-
                                        loopCount
                                        + 1

                                    ()

                                return "some"
                            }

                        let! actual = ctr CancellationToken.None

                        Expect.equal loopCount 2 "Should only loop twice"
                        Expect.equal actual expected "Should be none"
                    }
            ]
            testList "Cancellations" [
                testCaseTask "Simple Cancellation"
                <| fun () ->
                    task {
                        do!
                            Expect.CancellationRequested(
                                task {
                                    let foo = cancellableTaskOption { return "lol" }
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
                                    let work = cancellableTaskOption { someValue <- "lol" }

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
                    "CancellationToken flows from Async<T> to CancellableTaskOption<T> via Async.AwaitCancellableTask"
                <| fun () ->
                    let innerTask =
                        cancellableTaskOption {
                            return! CancellableTaskOption.getCancellationToken ()
                        }

                    let outerAsync =
                        async {
                            return!
                                innerTask
                                |> Async.AwaitCancellableTaskOption
                        }

                    use cts = new CancellationTokenSource()

                    let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)

                    Expect.equal actual (Some cts.Token) ""


                testCase "CancellationToken flows from AsyncOption<T> to CancellableTaskOption<T>"
                <| fun () ->
                    let innerTask =
                        cancellableTaskOption {
                            return! CancellableTaskOption.getCancellationToken ()
                        }

                    let outerAsync = asyncOption { return! innerTask }

                    use cts = new CancellationTokenSource()

                    let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)

                    Expect.equal actual (Some cts.Token) ""

                testCase "CancellationToken flows from CancellableTaskOption<T> to Async<unit>"
                <| fun () ->
                    let innerAsync = async { return! Async.CancellationToken }

                    let outerTask = cancellableTaskOption { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Some cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskOption<T> to AsyncOption<unit>"
                <| fun () ->
                    let innerAsync = asyncOption { return! Async.CancellationToken }

                    let outerTask = cancellableTaskOption { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Some cts.Token) ""
                testCase
                    "CancellationToken flows from CancellableTaskOption<T> to CancellableTask<T>"
                <| fun () ->
                    let innerAsync =
                        cancellableTask { return! CancellableTask.getCancellationToken () }

                    let outerTask = cancellableTaskOption { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Some cts.Token) ""

                testCase
                    "CancellationToken flows from CancellableTaskOption<T> to CancellableTaskOption<T>"
                <| fun () ->
                    let innerAsync =
                        cancellableTaskOption {
                            return! CancellableTaskOption.getCancellationToken ()
                        }

                    let outerTask = cancellableTaskOption { return! innerAsync }

                    use cts = new CancellationTokenSource()

                    let actual = (outerTask cts.Token).GetAwaiter().GetResult()

                    Expect.equal actual (Some cts.Token) ""


            ]
        ]

    let functionTests =
        testList "functions" [
            testList "singleton" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = CancellableTaskOption.some "lol"

                    let! someTask = innerCall

                    Expect.equal (Some "lol") someTask ""
                }
            ]
            testList "bind" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskOption { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskOption.bind (fun x ->
                            cancellableTaskOption { return x + "fooo" }
                        )

                    Expect.equal (Some "lolfooo") someTask ""
                }
            ]
            testList "map" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskOption { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskOption.map (fun x -> x + "fooo")

                    Expect.equal (Some "lolfooo") someTask ""
                }
            ]
            testList "apply" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskOption { return "lol" }
                    let applier = cancellableTaskOption { return fun x -> x + "fooo" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskOption.apply applier

                    Expect.equal (Some "lolfooo") someTask ""
                }
            ]

            testList "zip" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskOption { return "fooo" }
                    let innerCall2 = cancellableTaskOption { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskOption.zip innerCall2

                    Expect.equal (Some("lol", "fooo")) someTask ""
                }
            ]

            testList "parZip" [
                testCaseAsync "Simple"
                <| async {
                    let innerCall = cancellableTaskOption { return "fooo" }
                    let innerCall2 = cancellableTaskOption { return "lol" }

                    let! someTask =
                        innerCall
                        |> CancellableTaskOption.parallelZip innerCall2

                    Expect.equal (Some("lol", "fooo")) someTask ""
                }
            ]
        ]

    [<Tests>]
    let ``CancellableTaskOptionCE inference checks`` =
        testList "CancellableTaskOptionCE inference checks" [
            testCase "Inference checks"
            <| fun () ->
                // Compilation is success
                let f res = cancellableTaskOption { return! res }

                f (CancellableTaskOption.some ())
                |> ignore
        ]


    [<Tests>]
    let cancellableTaskOptionTests =
        testList "CancellableTaskOption" [
            cancellableTaskOptionBuilderTests
            functionTests
            ``CancellableTaskOptionCE inference checks``
        ]
