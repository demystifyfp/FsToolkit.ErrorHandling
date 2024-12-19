module TaskOptionCETests

open Expecto
open FsToolkit.ErrorHandling
open System.Threading.Tasks


module TestFuncs =
    let testFunctionTO<'Dto> () =
        taskOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }


[<Tests>]
let ceTests =
    testList "TaskOption CE" [
        testCaseTask "Return value"
        <| fun () ->
            task {
                let expected = Some 42
                let! actual = taskOption { return 42 }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "ReturnFrom Some"
        <| fun () ->
            task {
                let expected = Some 42
                let! actual = taskOption { return! (Some 42) }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "ReturnFrom None"
        <| fun () ->
            task {
                let expected = None
                let! actual = taskOption { return! None }
                Expect.equal actual expected "Should return value wrapped in option"
            }

        testCaseTask "ReturnFrom Async None"
        <| fun () ->
            task {
                let expected = None
                let! actual = taskOption { return! (async.Return None) }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "ReturnFrom Async"
        <| fun () ->
            task {
                let expected = Some 42
                let! actual = taskOption { return! (async.Return 42) }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "ReturnFrom Task None"
        <| fun () ->
            task {
                let expected = None
                let! actual = taskOption { return! (Task.FromResult None) }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "ReturnFrom Task Generic"
        <| fun () ->
            task {
                let expected = Some 42
                let! actual = taskOption { return! (Task.FromResult 42) }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "ReturnFrom Task"
        <| fun () ->
            task {
                let expected = Some()
                let! actual = taskOption { return! Task.CompletedTask }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "ReturnFrom ValueTask Generic"
        <| fun () ->
            task {
                let expected = Some 42
                let! actual = taskOption { return! (ValueTask.FromResult 42) }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "ReturnFrom ValueTask"
        <| fun () ->
            task {
                let expected = Some()
                let! actual = taskOption { return! ValueTask.CompletedTask }
                Expect.equal actual expected "Should return value wrapped in option"
            }
        testCaseTask "Bind Some"
        <| fun () ->
            task {
                let expected = Some 42

                let! actual =
                    taskOption {
                        let! value = Some 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }
        testCaseTask "Bind None"
        <| fun () ->
            task {
                let expected = None

                let! actual =
                    taskOption {
                        let! value = None
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }
        testCaseTask "Bind Async None"
        <| fun () ->
            task {
                let expected = None

                let! actual =
                    taskOption {
                        let! value = async.Return(None)
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }
        testCaseTask "Bind Async"
        <| fun () ->
            task {
                let expected = Some 42

                let! actual =
                    taskOption {
                        let! value = async.Return 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }
        testCaseTask "Bind Task None"
        <| fun () ->
            task {
                let expected = None

                let! actual =
                    taskOption {
                        let! value = Task.FromResult None
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }
        testCaseTask "Bind Task Generic"
        <| fun () ->
            task {
                let expected = Some 42

                let! actual =
                    taskOption {
                        let! value = Task.FromResult 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }
        testCaseTask "Bind Task"
        <| fun () ->
            task {
                let expected = Some()

                let! actual =
                    taskOption {
                        let! value = Task.CompletedTask
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }
        testCaseTask "Bind ValueTask Generic"
        <| fun () ->
            task {
                let expected = Some 42

                let! actual =
                    taskOption {
                        let! value = ValueTask.FromResult 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }
        testCaseTask "Bind ValueTask"
        <| fun () ->
            task {
                let expected = Some()

                let! actual =
                    taskOption {
                        let! value = ValueTask.CompletedTask
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in option"
            }

        testCaseTask "Task.Yield"
        <| fun () ->
            task {

                let! actual = taskOption { do! Task.Yield() }

                Expect.equal actual (Some()) "Should be ok"
            }
        testCaseTask "Zero/Combine/Delay/Run"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskOption {
                        let result = data

                        if true then
                            ()

                        return result
                    }

                Expect.equal actual (Some data) "Should be ok"
            }
        testCaseTask "If do!"
        <| fun () ->
            task {
                let data = 42

                let taskRes (call: unit -> Task) maybeCall : Task<Option<int>> =
                    taskOption {
                        if true then
                            do! call ()

                        let! (res: string) = maybeCall (): Task<Option<string>>
                        return data
                    }

                ()
            }
        testCaseTask "Try With"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskOption {
                        try
                            return data
                        with e ->
                            return raise e
                    }

                Expect.equal actual (Some data) "Try with failed"
            }
        testCaseTask "Try Finally"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskOption {
                        try
                            return data
                        finally
                            ()
                    }

                Expect.equal actual (Some data) "Try with failed"
            }
        testCaseTask "Using null"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskOption {
                        use d = null
                        return data
                    }

                Expect.equal actual (Some data) "Should be ok"
            }

        testCaseTask "use normal disposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    taskOption {
                        use d = TestHelpers.makeDisposable (fun () -> isFinished <- true)
                        return data
                    }

                Expect.equal actual (Some data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        testCaseTask "use! normal wrapped disposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    taskOption {
                        use! d =
                            TestHelpers.makeDisposable (fun () -> isFinished <- true)
                            |> Some

                        return data
                    }

                Expect.equal actual (Some data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        testCaseTask "use null disposable"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskOption {
                        use d = null
                        return data
                    }

                Expect.equal actual (Some data) "Should be ok"
            }
        testCaseTask "use sync asyncdisposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    taskOption {
                        use d =
                            TestHelpers.makeAsyncDisposable (
                                (fun () ->
                                    isFinished <- true
                                    ValueTask()
                                )
                            )

                        return data
                    }

                Expect.equal actual (Some data) "Should be ok"
                Expect.isTrue isFinished ""
            }

        testCaseTask "use async asyncdisposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    taskOption {
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

                Expect.equal actual (Some data) "Should be ok"
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
                            taskOption {
                                while index < maxIndex do
                                    index <- index + 1

                                return data
                            }

                        Expect.equal index maxIndex "Index should reach maxIndex"
                        Expect.equal actual (Some data) "Should be ok"
                    }
        ]

        testCaseTask "while bind error"
        <| fun () ->
            task {
                let items = [
                    TaskOption.some 3
                    TaskOption.some 4
                    Task.singleton (None)
                ]

                let mutable index = 0

                let! actual =
                    taskOption {
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

                Expect.equal actual (None) "Should be NOPE"
            }
        testCaseTask "while fail"
        <| fun () ->
            task {

                let mutable loopCount = 0
                let mutable wasCalled = false

                let sideEffect () =
                    wasCalled <- true
                    "ok"

                let expected = None

                let data = [
                    Some "42"
                    Some "1024"
                    expected
                    Some "1M"
                    Some "1M"
                    Some "1M"
                ]

                let! actual =
                    taskOption {
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
                    taskOption {
                        for i in [ 1..10 ] do
                            ()

                        return data
                    }

                Expect.equal actual (Some data) "Should be ok"
            }
        testCaseTask "For to"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskOption {
                        for i = 1 to 10 do
                            ()

                        return data
                    }

                Expect.equal actual (Some data) "Should be ok"
            }
        testCaseTask "for in fail"
        <| fun () ->
            task {

                let mutable loopCount = 0
                let mutable wasCalled = false

                let sideEffect () =
                    wasCalled <- true
                    "ok"

                let expected = None

                let data = [
                    Some "42"
                    Some "1024"
                    expected
                    Some "1M"
                    Some "1M"
                    Some "1M"
                ]

                let! actual =
                    taskOption {
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

[<Tests>]
let ceTestsApplicative =
    testList "TaskOptionCE applicative tests" [
        testCaseTask "Happy Path Option/AsyncOption/Ply/ValueTask"
        <| fun () ->
            task {
                let! actual =
                    taskOption {
                        let! a = Some 3

                        let! b =
                            Some 1
                            |> Async.singleton

                        let! c = specialCaseTask (Some 3)
                        let! d = ValueTask.FromResult(Some 5)

                        return
                            a + b
                            - c
                            - d
                    }

                Expect.equal actual (Some -4) "Should be ok"
            }
        testCaseTask "Fail Path Option/AsyncOption/Ply/ValueTask"
        <| fun () ->
            task {
                let! actual =
                    taskOption {
                        let! a = Some 3

                        and! b =
                            Some 1
                            |> Async.singleton

                        and! c = specialCaseTask (None)
                        and! d = ValueTask.FromResult(Some 5)

                        return
                            a + b
                            - c
                            - d
                    }

                Expect.equal actual None "Should be ok"
            }
    ]


[<Tests>]
let ``TaskOptionCE inference checks`` =
    testList "TaskOptionCE inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = taskOption { return! res }

            f (TaskOption.some ())
            |> ignore
    ]
