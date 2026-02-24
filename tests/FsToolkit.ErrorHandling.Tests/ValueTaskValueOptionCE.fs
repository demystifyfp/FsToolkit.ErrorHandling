module ValueTaskValueOptionCETests

open Expecto
open FsToolkit.ErrorHandling
open System.Threading.Tasks


module TestFuncs =
    let testFunctionTO<'Dto> () =
        valueTaskValueOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

let ceTests =
    testList "ValueTaskValueOption CE" [
        testCaseTask "Return value"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return 42 }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueSome"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return! (ValueSome 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone
                let! actual = valueTaskValueOption { return! ValueNone }
                Expect.equal actual expected "Should return value wrapped in voption"
            }

        testCaseTask "ReturnFrom Async ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone
                let! actual = valueTaskValueOption { return! (async.Return ValueNone) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Async"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return! (async.Return 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone
                let! actual = valueTaskValueOption { return! (Task.FromResult ValueNone) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task Generic"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return! (Task.FromResult 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task"
        <| fun () ->
            task {
                let expected = ValueSome()
                let! actual = valueTaskValueOption { return! Task.CompletedTask }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueTask Generic"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return! (ValueTask.FromResult 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueTask"
        <| fun () ->
            task {
                let expected = ValueSome()
                let! actual = valueTaskValueOption { return! ValueTask.CompletedTask }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "Bind ValueSome"
        <| fun () ->
            task {
                let expected = ValueSome 42

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueSome 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueNone
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Async ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone

                let! actual =
                    valueTaskValueOption {
                        let! value = async.Return(ValueNone)
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Async"
        <| fun () ->
            task {
                let expected = ValueSome 42

                let! actual =
                    valueTaskValueOption {
                        let! value = async.Return 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone

                let! actual =
                    valueTaskValueOption {
                        let! value = Task.FromResult ValueNone
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task Generic"
        <| fun () ->
            task {
                let expected = ValueSome 42

                let! actual =
                    valueTaskValueOption {
                        let! value = Task.FromResult 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task"
        <| fun () ->
            task {
                let expected = ValueSome()

                let! actual =
                    valueTaskValueOption {
                        let! value = Task.CompletedTask
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueTask Generic"
        <| fun () ->
            task {
                let expected = ValueSome 42

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueTask.FromResult 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueTask"
        <| fun () ->
            task {
                let expected = ValueSome()

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueTask.CompletedTask
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }

        testCaseTask "Task.Yield"
        <| fun () ->
            task {

                let! actual = valueTaskValueOption { do! Task.Yield() }

                Expect.equal actual (ValueSome()) "Should be ok"
            }
        testCaseTask "Zero/Combine/Delay/Run"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        let result = data

                        if true then
                            ()

                        return result
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "Try With"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        try
                            return data
                        with e ->
                            return raise e
                    }

                Expect.equal actual (ValueSome data) "Try with failed"
            }
        testCaseTask "Try Finally"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        try
                            return data
                        finally
                            ()
                    }

                Expect.equal actual (ValueSome data) "Try with failed"
            }
        testCaseTask "Using null"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        use d = null
                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }

        testCaseTask "use normal disposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    valueTaskValueOption {
                        use d = TestHelpers.makeDisposable (fun () -> isFinished <- true)
                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        testCaseTask "use! normal wrapped disposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    valueTaskValueOption {
                        use! d =
                            TestHelpers.makeDisposable (fun () -> isFinished <- true)
                            |> ValueSome

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        testCaseTask "use null disposable"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        use d = null
                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "use sync asyncdisposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    valueTaskValueOption {
                        use d =
                            TestHelpers.makeAsyncDisposable (
                                (fun () ->
                                    isFinished <- true
                                    ValueTask()
                                )
                            )

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
                Expect.isTrue isFinished ""
            }

        testCaseTask "use async asyncdisposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    valueTaskValueOption {
                        use d =
                            TestHelpers.makeAsyncDisposable (
                                (fun () ->
                                    task {
                                        do! Task.Yield()
                                        isFinished <- true
                                    }
                                    :> Task
                                    |> ValueTask
                                )
                            )

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        yield! [
            let maxIndices = [
                10
                1000000
            ]

            for maxIndex in maxIndices do
                testCaseTask
                <| sprintf "While - %i" maxIndex
                <| fun () ->
                    task {
                        let data = 42
                        let mutable index = 0

                        let! actual =
                            valueTaskValueOption {
                                while index < maxIndex do
                                    index <- index + 1

                                return data
                            }

                        Expect.equal index maxIndex "Index should reach maxIndex"
                        Expect.equal actual (ValueSome data) "Should be ok"
                    }
        ]

        testCaseTask "while bind error"
        <| fun () ->
            task {
                let items = [
                    ValueTaskValueOption.valueSome 3
                    ValueTaskValueOption.valueSome 4
                    ValueTask<_>(ValueNone)
                ]

                let mutable index = 0

                let! actual =
                    valueTaskValueOption {
                        while index < items.Length do
                            let! _ = items[index]
                            index <- index + 1

                        return index
                    }

                Expect.equal
                    index
                    (items.Length
                     - 1)
                    "Index should reach maxIndex"

                Expect.equal actual ValueNone "Should be NOPE"
            }
        testCaseTask "while fail"
        <| fun () ->
            task {

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
                    valueTaskValueOption {
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
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        for i in [ 1..10 ] do
                            ()

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "For to"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        for i = 1 to 10 do
                            ()

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "for in fail"
        <| fun () ->
            task {

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
                    valueTaskValueOption {
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
        testCaseTask "Empty result"
        <| fun () ->
            task {
                let! _ = TestFuncs.testFunctionTO ()
                ()
            }
    ]

let specialCaseTask returnValue =
#if NETSTANDARD2_0
    Unsafe.uply { return returnValue }
#else
    Task.FromResult returnValue
#endif

let ceTestsApplicative =
    testList "ValueTaskValueOptionCE applicative tests" [
        testCaseTask "Happy Path ValueOption/AsyncValueOption/Task/ValueTask"
        <| fun () ->
            task {
                let! actual =
                    valueTaskValueOption {
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
        testCaseTask "Fail Path ValueOption/AsyncValueOption/Task/ValueTask"
        <| fun () ->
            task {
                let! actual =
                    valueTaskValueOption {
                        let! a = ValueSome 3

                        and! b =
                            ValueSome 1
                            |> Async.singleton

                        and! c = specialCaseTask ValueNone
                        and! d = ValueTask.FromResult(ValueSome 5)

                        return
                            a + b
                            - c
                            - d
                    }

                Expect.equal actual ValueNone "Should be ok"
            }
    ]

let ``ValueTaskValueOptionCE inference checks`` =
    testList "ValueTaskValueOptionCE inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = valueTaskValueOption { return! res }

            f (ValueTaskValueOption.valueSome ())
            |> ignore
    ]

let allTests =
    testList "ValueTaskValueOption CE Tests" [
        ceTests
        ceTestsApplicative
        ``ValueTaskValueOptionCE inference checks``
    ]
