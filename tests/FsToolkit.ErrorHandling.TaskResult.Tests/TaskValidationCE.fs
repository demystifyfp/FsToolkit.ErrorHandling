module TaskValidationCETests

open Expecto
open FsToolkit.ErrorHandling
open System.Threading.Tasks

[<Tests>]
let ``TaskValidationCE return Tests`` =
    testList "TaskValidationCE  Tests" [
        testCaseTask "Return string"
        <| fun () ->
            task {
                let data = "Foo"
                let! actual = taskValidation { return data }
                Expect.equal actual (Validation.ok data) "Should be ok"
            }
    ]

[<Tests>]
let ``TaskValidationCE return! Tests`` =
    testList "TaskValidationCE return! Tests" [
        testCaseTask "Return Ok Validation"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Validation.ok innerData
                let! actual = taskValidation { return! data }

                Expect.equal actual (data) "Should be ok"
            }
        testCaseTask "Return Ok Choice"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Choice1Of2 innerData
                let! actual = taskValidation { return! data }
                Expect.equal actual (Validation.ok innerData) "Should be ok"
            }

        testCaseTask "Return Ok AsyncValidation"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Validation.ok innerData
                let! actual = taskValidation { return! Async.singleton data }

                Expect.equal actual (data) "Should be ok"
            }
        testCaseTask "Return Ok TaskValidation"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Validation.ok innerData
                let! actual = taskValidation { return! Task.FromResult data }

                Expect.equal actual (data) "Should be ok"
            }
        testCaseTask "Return Async"
        <| fun () ->
            task {
                let innerData = "Foo"
                let! actual = taskValidation { return! Async.singleton innerData }

                Expect.equal actual (Validation.ok innerData) "Should be ok"
            }
        testCaseTask "Return Task Generic"
        <| fun () ->
            task {
                let innerData = "Foo"
                let! actual = taskValidation { return! Task.singleton innerData }

                Expect.equal actual (Validation.ok innerData) "Should be ok"
            }
        testCaseTask "Return Task"
        <| fun () ->
            task {
                let innerData = "Foo"
                let! actual = taskValidation { return! Task.FromResult innerData :> Task }

                Expect.equal actual (Validation.ok ()) "Should be ok"
            }
        testCaseTask "Return ValueTask Generic"
        <| fun () ->
            task {
                let innerData = "Foo"
                let! actual = taskValidation { return! ValueTask.FromResult innerData }

                Expect.equal actual (Validation.ok innerData) "Should be ok"
            }
        testCaseTask "Return ValueTask"
        <| fun () ->
            task {
                let! actual = taskValidation { return! ValueTask.CompletedTask }

                Expect.equal actual (Validation.ok ()) "Should be ok"
            }
    ]

[<Tests>]
let ``TaskValidationCE bind Tests`` =
    testList "TaskValidationCE bind Tests" [
        testCaseTask "Bind Ok Validation"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Validation.ok innerData

                let! actual =
                    taskValidation {
                        let! data = data
                        return data
                    }

                Expect.equal actual (data) "Should be ok"

            }
        testCaseTask "Bind Ok Choice"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Choice1Of2 innerData

                let! actual =
                    taskValidation {
                        let! data = data
                        return data
                    }

                Expect.equal actual (Validation.ok innerData) "Should be ok"
            }


        testCaseTask "Bind Ok AsyncValidation"
        <| fun () ->
            task {
                let innerData = "Foo"

                let data =
                    Validation.ok innerData
                    |> Async.singleton

                let! actual =
                    taskValidation {
                        let! data = data
                        return data
                    }

                Expect.equal
                    actual
                    (data
                     |> Async.RunSynchronously)
                    "Should be ok"
            }
        testCaseTask "Bind Ok TaskValidation"
        <| fun () ->
            task {
                let innerData = "Foo"

                let data =
                    Validation.ok innerData
                    |> Task.singleton

                let! actual =
                    taskValidation {
                        let! data = data
                        return data
                    }

                Expect.equal actual (data.Result) "Should be ok"
            }
        testCaseTask "Bind Async"
        <| fun () ->
            task {
                let innerData = "Foo"

                let! actual =
                    taskValidation {
                        let! data = Async.singleton innerData
                        return data
                    }

                Expect.equal actual (Validation.ok innerData) "Should be ok"
            }
        testCaseTask "Bind Task Generic"
        <| fun () ->
            task {
                let innerData = "Foo"

                let! actual =
                    taskValidation {
                        let! data = Task.FromResult innerData
                        return data
                    }

                Expect.equal actual (Validation.ok innerData) "Should be ok"
            }
        testCaseTask "Bind Task"
        <| fun () ->
            task {
                let innerData = "Foo"
                let! actual = taskValidation { do! Task.FromResult innerData :> Task }

                Expect.equal actual (Validation.ok ()) "Should be ok"
            }
        testCaseTask "Bind ValueTask Generic"
        <| fun () ->
            task {
                let innerData = "Foo"

                let! actual =
                    taskValidation {
                        let! data = ValueTask.FromResult innerData
                        return data
                    }

                Expect.equal actual (Validation.ok innerData) "Should be ok"
            }
        testCaseTask "Bind ValueTask"
        <| fun () ->
            task {
                let! actual = taskValidation { do! ValueTask.CompletedTask }

                Expect.equal actual (Validation.ok ()) "Should be ok"
            }

        testCaseTask "Task.Yield"
        <| fun () ->
            task {

                let! actual = taskValidation { do! Task.Yield() }

                Expect.equal actual (Validation.ok ()) "Should be ok"
            }
    ]

[<Tests>]
let ``TaskValidationCE combine/zero/delay/run Tests`` =
    testList "TaskValidationCE combine/zero/delay/run Tests" [
        testCaseTask "Zero/Combine/Delay/Run"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskValidation {
                        let result = data

                        if true then
                            ()

                        return result
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
            }
        testCaseTask "If do!"
        <| fun () ->
            task {
                let data = 42

                let taskVal (call: unit -> Task) maybeCall : Task<Validation<int, unit>> =
                    taskValidation {
                        if true then
                            do! call ()

                        let! (res: string) = maybeCall (): Task<Validation<string, unit>>
                        return data
                    }

                ()
            }
    ]

[<Tests>]
let ``TaskValidationCE try Tests`` =
    testList "TaskValidationCE try Tests" [
        testCaseTask "Try With"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskValidation {
                        let data = data

                        try
                            ()
                        with _ ->
                            ()

                        return data
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
            }
        testCaseTask "Try Finally"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskValidation {
                        let data = data

                        try
                            ()
                        finally
                            ()

                        return data
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
            }
    ]

[<Tests>]
let ``TaskValidationCE using Tests`` =
    testList "TaskValidationCE using Tests" [
        testCaseTask "use normal disposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    taskValidation {
                        use d = TestHelpers.makeDisposable (fun () -> isFinished <- true)
                        return data
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        testCaseTask "use! normal wrapped disposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    taskValidation {
                        use! d =
                            TestHelpers.makeDisposable (fun () -> isFinished <- true)
                            |> Validation.ok

                        return data
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        testCaseTask "use null disposable"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskValidation {
                        use d = null
                        return data
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
            }
        testCaseTask "use sync asyncdisposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    taskValidation {
                        use d =
                            TestHelpers.makeAsyncDisposable (
                                (fun () ->
                                    isFinished <- true
                                    ValueTask()
                                )
                            )

                        return data
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
                Expect.isTrue isFinished ""
            }

        testCaseTask "use async asyncdisposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    taskValidation {
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

                Expect.equal actual (Validation.ok data) "Should be ok"
                Expect.isTrue isFinished ""
            }
    ]

[<Tests>]
let ``TaskValidationCE loop Tests`` =
    testList "TaskValidationCE loop Tests" [
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
                            taskValidation {
                                while index < maxIndex do
                                    index <- index + 1

                                return data
                            }

                        Expect.equal index maxIndex "Index should reach maxIndex"
                        Expect.equal actual (Validation.ok data) "Should be ok"
                    }
        ]


        testCaseTask "while fail"
        <| fun () ->
            task {

                let mutable loopCount = 0
                let mutable wasCalled = false

                let sideEffect () =
                    wasCalled <- true
                    "ok"

                let expected = Validation.error "error"

                let data = [
                    Validation.ok "42"
                    Validation.ok "1024"
                    expected
                    Validation.ok "1M"
                    Validation.ok "1M"
                    Validation.ok "1M"
                ]

                let! actual =
                    taskValidation {
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
        testCaseTask "for in"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskValidation {
                        for i in [ 1..10 ] do
                            ()

                        return data
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
            }
        testCaseTask "for to"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskValidation {
                        for i = 1 to 10 do
                            ()

                        return data
                    }

                Expect.equal actual (Validation.ok data) "Should be ok"
            }
        testCaseTask "for in fail"
        <| fun () ->
            task {

                let mutable loopCount = 0
                let expected = Validation.error "error"

                let data = [
                    Validation.ok "42"
                    Validation.ok "1024"
                    expected
                    Validation.ok "1M"
                    Validation.ok "1M"
                    Validation.ok "1M"
                ]

                let! actual =
                    taskValidation {
                        for i in data do
                            let! x = i

                            loopCount <-
                                loopCount
                                + 1

                            ()

                        return "ok"
                    }

                Expect.equal loopCount 2 "Should only loop twice"
                Expect.equal actual expected "Should be an error"
            }
    ]

[<Tests>]
let ``TaskValidationCE applicative tests`` =
    testList "TaskValidationCE applicative tests" [
        testCaseTask "Happy Path TaskValidation"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = TaskValidation.ok 3
                        and! b = TaskValidation.ok 2
                        and! c = TaskValidation.ok 1
                        return a + b - c
                    }

                Expect.equal actual (Validation.ok 4) "Should be ok"
            }

        testCaseTask "Happy Path AsyncValidation"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = AsyncValidation.ok 3
                        and! b = AsyncValidation.ok 2
                        and! c = AsyncValidation.ok 1
                        return a + b - c
                    }

                Expect.equal actual (Validation.ok 4) "Should be ok"
            }


        testCaseTask "Happy Path Validation"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = Validation.ok 3
                        and! b = Validation.ok 2
                        and! c = Validation.ok 1
                        return a + b - c
                    }

                Expect.equal actual (Validation.ok 4) "Should be ok"
            }

        testCaseTask "Happy Path Choice"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = Choice1Of2 3
                        and! b = Choice1Of2 2
                        and! c = Choice1Of2 1
                        return a + b - c
                    }

                Expect.equal actual (Validation.ok 4) "Should be ok"
            }

        testCaseTask "Happy Path Async"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = Async.singleton 3 //: Async<int>
                        and! b = Async.singleton 2 //: Async<int>
                        and! c = Async.singleton 1 //: Async<int>
                        return a + b - c
                    }

                Expect.equal actual (Validation.ok 4) "Should be ok"
            }

        testCaseTask "Happy Path 2 Async"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = Async.singleton 3 //: Async<int>
                        and! b = Async.singleton 2 //: Async<int>
                        return a + b
                    }

                Expect.equal actual (Validation.ok 5) "Should be ok"
            }

        testCaseTask "Happy Path 2 Task"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = Task.FromResult 3
                        and! b = Task.FromResult 2
                        return a + b
                    }

                Expect.equal actual (Validation.ok 5) "Should be ok"
            }
        let specialCaseTask returnValue = Task.FromResult returnValue

        testCaseTask "Happy Path Validation/Choice/AsyncValidation/Ply/ValueTask"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = Validation.ok 3
                        and! b = Choice1Of2 2

                        and! c =
                            Validation.ok 1
                            |> Async.singleton

                        and! d = specialCaseTask (Validation.ok 3)
                        and! e = ValueTask.FromResult(Validation.ok 5)

                        return
                            a + b
                            - c
                            - d
                            + e
                    }

                Expect.equal actual (Validation.ok 6) "Should be ok"
            }

        testCaseTask "Fail Path Result"
        <| fun () ->
            task {
                let expected =
                    Error [
                        "Error 1"
                        "Error 2"
                    ]

                let! actual =
                    taskValidation {
                        let! a = Ok 3
                        and! b = Ok 2
                        and! c = Error "Error 1"
                        and! d = Error "Error 2"

                        return
                            a + b
                            - c
                            - d
                    }

                Expect.equal actual expected "Should be Error"
            }

        testCaseTask "Fail Path Validation"
        <| fun () ->
            task {
                let expected = Validation.error "TryParse failure"

                let! actual =
                    taskValidation {
                        let! a = Validation.ok 3
                        and! b = Validation.ok 2
                        and! c = expected
                        return a + b - c
                    }

                Expect.equal actual expected "Should be Error"
            }

        testCaseTask "Fail Path Choice"
        <| fun () ->
            task {
                let errorMsg = "TryParse failure"

                let! actual =
                    taskValidation {
                        let! a = Choice1Of2 3
                        and! b = Choice1Of2 2
                        and! c = Choice2Of2 errorMsg
                        return a + b - c
                    }

                Expect.equal actual (Validation.error errorMsg) "Should be Error"
            }

        testCaseTask "Fail Path Validation/Choice/AsyncValidation"
        <| fun () ->
            task {
                let errorMsg = "TryParse failure"

                let! actual =
                    taskValidation {
                        let! a = Choice1Of2 3

                        and! b =
                            Validation.ok 2
                            |> Async.singleton

                        and! c = Validation.error errorMsg
                        return a + b - c
                    }

                Expect.equal actual (Validation.error errorMsg) "Should be Error"
            }
    ]

[<Tests>]
let ``TaskValidationCE inference checks`` =
    testList "TaskValidationCE inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = taskValidation { return! res () }

            f (TaskValidation.ok)
            |> ignore
    ]
