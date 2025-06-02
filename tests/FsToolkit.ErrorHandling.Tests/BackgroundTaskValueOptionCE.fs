module BackgroundTaskValueOptionCETests

open Expecto
open FsToolkit.ErrorHandling
open System.Threading.Tasks

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }

let ceTests =
    testList "Background TaskValueOption CE" [
        testCaseTask "Return value"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42
                let! actual = backgroundTaskValueOption { return 42 }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueSome"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42
                let! actual = backgroundTaskValueOption { return! (ValueSome 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueNone"
        <| fun () ->
            backgroundTask {
                let expected = ValueNone
                let! actual = backgroundTaskValueOption { return! ValueNone }
                Expect.equal actual expected "Should return value wrapped in voption"
            }

        testCaseTask "ReturnFrom Async ValueNone"
        <| fun () ->
            backgroundTask {
                let expected = ValueNone
                let! actual = backgroundTaskValueOption { return! (async.Return ValueNone) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Async"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42
                let! actual = backgroundTaskValueOption { return! (async.Return 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task ValueNone"
        <| fun () ->
            backgroundTask {
                let expected = ValueNone
                let! actual = backgroundTaskValueOption { return! (Task.FromResult ValueNone) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task Generic"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42
                let! actual = backgroundTaskValueOption { return! (Task.FromResult 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome()
                let! actual = backgroundTaskValueOption { return! Task.CompletedTask }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueTask Generic"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42
                let! actual = backgroundTaskValueOption { return! (ValueTask.FromResult 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueTask"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome()
                let! actual = backgroundTaskValueOption { return! ValueTask.CompletedTask }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "Bind ValueSome"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42

                let! actual =
                    backgroundTaskValueOption {
                        let! value = ValueSome 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueNone"
        <| fun () ->
            backgroundTask {
                let expected = ValueNone

                let! actual =
                    backgroundTaskValueOption {
                        let! value = ValueNone
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Async ValueNone"
        <| fun () ->
            backgroundTask {
                let expected = ValueNone

                let! actual =
                    backgroundTaskValueOption {
                        let! value = async.Return(ValueNone)
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Async"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42

                let! actual =
                    backgroundTaskValueOption {
                        let! value = async.Return 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task ValueNone"
        <| fun () ->
            backgroundTask {
                let expected = ValueNone

                let! actual =
                    backgroundTaskValueOption {
                        let! value = Task.FromResult ValueNone
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task Generic"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42

                let! actual =
                    backgroundTaskValueOption {
                        let! value = Task.FromResult 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome()

                let! actual =
                    backgroundTaskValueOption {
                        let! value = Task.CompletedTask
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueTask Generic"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome 42

                let! actual =
                    backgroundTaskValueOption {
                        let! value = ValueTask.FromResult 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueTask"
        <| fun () ->
            backgroundTask {
                let expected = ValueSome()

                let! actual =
                    backgroundTaskValueOption {
                        let! value = ValueTask.CompletedTask
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }

        testCaseTask "Task.Yield"
        <| fun () ->
            backgroundTask {

                let! actual = backgroundTaskValueOption { do! Task.Yield() }

                Expect.equal actual (ValueSome()) "Should be ok"
            }
        testCaseTask "Zero/Combine/Delay/Run"
        <| fun () ->
            backgroundTask {
                let data = 42

                let! actual =
                    backgroundTaskValueOption {
                        let result = data

                        if true then
                            ()

                        return result
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "Try With"
        <| fun () ->
            backgroundTask {
                let data = 42

                let! actual =
                    backgroundTaskValueOption {
                        try
                            return data
                        with e ->
                            return raise e
                    }

                Expect.equal actual (ValueSome data) "Try with failed"
            }
        testCaseTask "Try Finally"
        <| fun () ->
            backgroundTask {
                let data = 42

                let! actual =
                    backgroundTaskValueOption {
                        try
                            return data
                        finally
                            ()
                    }

                Expect.equal actual (ValueSome data) "Try with failed"
            }
        testCaseTask "Using null"
        <| fun () ->
            backgroundTask {
                let data = 42

                let! actual =
                    backgroundTaskValueOption {
                        use d = null
                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "Using disposeable"
        <| fun () ->
            backgroundTask {
                let data = 42

                let! actual =
                    backgroundTaskValueOption {
                        use d = makeDisposable ()
                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "Using bind disposeable"
        <| fun () ->
            backgroundTask {
                let data = 42

                let! actual =
                    backgroundTaskValueOption {
                        use! d =
                            (makeDisposable ()
                             |> ValueSome)

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "While"
        <| fun () ->
            backgroundTask {
                let data = 42
                let mutable index = 0

                let! actual =
                    backgroundTaskValueOption {
                        while index < 10 do
                            index <- index + 1

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "while fail"
        <| fun () ->
            backgroundTask {

                let mutable loopCount = 0
                let mutable wasCalled = false

                let sideEffect () =
                    wasCalled <- true
                    "ok"

                let expected = ValueNone

                let data = [
                    ValueSome "42"
                    ValueSome "1024"
                    expected
                    ValueSome "1M"
                    ValueSome "1M"
                    ValueSome "1M"
                ]

                let! actual =
                    backgroundTaskValueOption {
                        while loopCount < data.Length do
                            let! x = data.[loopCount]

                            loopCount <-
                                loopCount
                                + 1

                        return sideEffect ()
                    }

                Expect.equal loopCount 2 "Should only loop twice"
                Expect.equal actual expected "Should be an error"
                Expect.isFalse wasCalled "No additional side effects should occur"
            }
        testCaseTask "For in"
        <| fun () ->
            backgroundTask {
                let data = 42

                let! actual =
                    backgroundTaskValueOption {
                        for i in [ 1..10 ] do
                            ()

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "For to"
        <| fun () ->
            backgroundTask {
                let data = 42

                let! actual =
                    backgroundTaskValueOption {
                        for i = 1 to 10 do
                            ()

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "for in fail"
        <| fun () ->
            backgroundTask {

                let mutable loopCount = 0
                let mutable wasCalled = false

                let sideEffect () =
                    wasCalled <- true
                    "ok"

                let expected = ValueNone

                let data = [
                    ValueSome "42"
                    ValueSome "1024"
                    expected
                    ValueSome "1M"
                    ValueSome "1M"
                    ValueSome "1M"
                ]

                let! actual =
                    backgroundTaskValueOption {
                        for i in data do
                            let! x = i

                            loopCount <-
                                loopCount
                                + 1

                            ()

                        return sideEffect ()
                    }

                Expect.equal loopCount 2 "Should only loop twice"
                Expect.equal actual expected "Should be an error"
                Expect.isFalse wasCalled "No additional side effects should occur"
            }
    ]

let specialCaseTask returnValue =
#if NETSTANDARD2_0
    Unsafe.uply { return returnValue }
#else
    Task.FromResult returnValue
#endif

let ceTestsApplicative =
    testList "BackgroundTaskValueOptionCE applicative tests" [
        testCaseTask "Happy Path Option/AsyncOption/Ply/ValueTask"
        <| fun () ->
            backgroundTask {
                let! actual =
                    backgroundTaskValueOption {
                        let! a = ValueSome 3

                        let! b =
                            ValueSome 1
                            |> Async.singleton

                        let! c = specialCaseTask (ValueSome 3)
                        let! d = ValueTask.FromResult(ValueSome 5)

                        return
                            a + b
                            - c
                            - d
                    }

                Expect.equal actual (ValueSome -4) "Should be ok"
            }
        testCaseTask "Fail Path Option/AsyncOption/Ply/ValueTask"
        <| fun () ->
            backgroundTask {
                let! actual =
                    backgroundTaskValueOption {
                        let! a = ValueSome 3

                        and! b =
                            ValueSome 1
                            |> Async.singleton

                        and! c = specialCaseTask (ValueNone)
                        and! d = ValueTask.FromResult(ValueSome 5)

                        return
                            a + b
                            - c
                            - d
                    }

                Expect.equal actual ValueNone "Should be ok"
            }
    ]

let ``BackgroundTaskValueOptionCE inference checks`` =
    testList "BackgroundTaskValueOptionCE inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res =
                backgroundTaskValueOption { return! res }

            f (TaskValueOption.valueSome ())
            |> ignore
    ]

let allTests =
    testList "BackgroundTaskValueOptionCE CE Tests" [
        ceTests
        ceTestsApplicative
        ``BackgroundTaskValueOptionCE inference checks``
    ]
