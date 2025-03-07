module ParallelAsyncResultCETests

#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open System.Threading.Tasks
open FsToolkit.ErrorHandling

let ``return Tests`` =
    testList "ParallelAsyncResultCE return tests" [
        testCaseAsync "Return string"
        <| async {
            let data = "Foo"
            let! actual = parallelAsyncResult { return data }
            Expect.equal actual (Result.Ok data) ""
        }
    ]

let ``return! Tests`` =
    testList "ParallelAsyncResultCE return! Tests" [
        testCaseAsync "Return Ok Result"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = parallelAsyncResult { return! data }

            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Ok Choice"
        <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = parallelAsyncResult { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

        testCaseAsync "Return Ok AsyncResult"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = parallelAsyncResult { return! Async.singleton data }

            Expect.equal actual (data) "Should be ok"
        }

        testCaseAsync "Return Async"
        <| async {
            let innerData = "Foo"
            let! actual = parallelAsyncResult { return! Async.singleton innerData }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
#if !FABLE_COMPILER
        testCaseAsync "Return Ok TaskResult"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = parallelAsyncResult { return! Task.FromResult data }

            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Task Generic"
        <| async {
            let innerData = "Foo"
            let! actual = parallelAsyncResult { return! Task.FromResult innerData }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Return Task"
        <| async {
            let innerData = "Foo"
            let! actual = parallelAsyncResult { return! Task.FromResult innerData :> Task }

            Expect.equal actual (Result.Ok()) "Should be ok"
        }
#endif


    ]


let ``bind Tests`` =
    testList "ParallelAsyncResultCE bind Tests" [
        testCaseAsync "Bind Ok Result"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData

            let! actual =
                parallelAsyncResult {
                    let! data = data
                    return data
                }

            Expect.equal actual (data) "Should be ok"

        }
        testCaseAsync "Bind Ok Choice"
        <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData

            let! actual =
                parallelAsyncResult {
                    let! data = data
                    return data
                }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

        testCaseAsync "Bind Ok AsyncResult"
        <| async {
            let innerData = "Foo"

            let data =
                Result.Ok innerData
                |> Async.singleton

            let! actual =
                parallelAsyncResult {
                    let! data = data
                    return data
                }

            let! data = data
            Expect.equal actual (data) "Should be ok"
        }

        testCaseAsync "Bind Async"
        <| async {
            let innerData = "Foo"
            let d = Async.singleton innerData

            let! actual =
                parallelAsyncResult {
                    let! data = d
                    return data
                }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }


#if !FABLE_COMPILER
        testCaseAsync "Bind Ok TaskResult"
        <| async {
            let innerData = "Foo"

            let data =
                Result.Ok innerData
                |> Task.FromResult

            let! actual =
                parallelAsyncResult {
                    let! data = data
                    return data
                }

            Expect.equal actual (data.Result) "Should be ok"
        }
        testCaseAsync "Bind Task Generic"
        <| async {
            let innerData = "Foo"

            let! actual =
                parallelAsyncResult {
                    let! data = Task.FromResult innerData
                    return data
                }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Bind Task"
        <| async {
            let innerData = "Foo"
            let! actual = parallelAsyncResult { do! Task.FromResult innerData :> Task }

            Expect.equal actual (Result.Ok()) "Should be ok"
        }
#endif
    ]

let ``combine/zero/delay/run Tests`` =
    testList "ParallelAsyncResultCE combine/zero/delay/run Tests" [
        testCaseAsync "Zero/Combine/Delay/Run"
        <| async {
            let data = 42

            let! actual =
                parallelAsyncResult {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]


let ``try Tests`` =
    testList "ParallelAsyncResultCE try Tests" [
        testCaseAsync "Try With"
        <| async {
            let data = 42

            let! actual =
                parallelAsyncResult {
                    let data = data

                    try
                        ()
                    with _ ->
                        ()

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "Try Finally"
        <| async {
            let data = 42

            let! actual =
                parallelAsyncResult {
                    let data = data

                    try
                        ()
                    finally
                        ()

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]

let ``using Tests`` =
    testList "ParallelAsyncResultCE using Tests" [
        testCaseAsync "use normal disposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                parallelAsyncResult {
                    use d = TestHelpers.makeDisposable ((fun () -> isFinished <- true))
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
            Expect.isTrue isFinished "Expected disposable to be disposed"
        }
#if !FABLE_COMPILER
        testCaseAsync "use sync asyncdisposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                parallelAsyncResult {
                    use d =
                        TestHelpers.makeAsyncDisposable (
                            (fun () ->
                                isFinished <- true
                                ValueTask()
                            )
                        )

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
            Expect.isTrue isFinished "Expected disposable to be disposed"
        }
        testCaseAsync "use async asyncdisposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                parallelAsyncResult {
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

            Expect.equal actual (Result.Ok data) "Should be ok"
            Expect.isTrue isFinished "Expected disposable to be disposed"
        }
#endif

        testCaseAsync "use! normal wrapped disposable"
        <| async {
            let data = 42

            let! actual =
                parallelAsyncResult {
                    use! d =
                        TestHelpers.makeDisposable (id)
                        |> Result.Ok

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }

        testCaseAsync "disposable not disposed too early"
        <| async {
            let mutable disposed = false
            let mutable finished = false
            let f1 _ = AsyncResult.ok 42

            let! actual =
                parallelAsyncResult {
                    use d =
                        TestHelpers.makeDisposable (fun () ->
                            disposed <- true

                            if not finished then
                                failwith "Should not be disposed too early"
                        )

                    let! data = f1 d
                    finished <- true
                    return data
                }

            Expect.equal actual (Ok 42) "Should be ok"
            Expect.isTrue disposed "Should be disposed"
        }

#if !FABLE_COMPILER && NETSTANDARD2_1
        // Fable can't handle null disposables you get
        // TypeError: Cannot read property 'Dispose' of null
        testCaseAsync "use null disposable"
        <| async {
            let data = 42

            let! actual =
                parallelAsyncResult {
                    use d = null
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
#endif
    ]

let ``loop Tests`` =
    testList "ParallelAsyncResultCE loop Tests" [
        testCaseAsync "while"
        <| async {
            let data = 42
            let mutable index = 0

            let! actual =
                parallelAsyncResult {
                    while index < 10 do
                        index <- index + 1

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        yield! [
            let maxIndices = [
                10
                1000000
            ]

            for maxIndex in maxIndices do
                testCaseAsync
                <| sprintf "While - %i" maxIndex
                <| async {
                    let data = 42
                    let mutable index = 0

                    let! actual =
                        parallelAsyncResult {
                            while index < maxIndex do
                                index <- index + 1

                            return data
                        }

                    Expect.equal index maxIndex "Index should reach maxIndex"
                    Expect.equal actual (Ok data) "Should be ok"
                }
        ]

        testCaseAsync "while fail"
        <| async {

            let mutable loopCount = 0
            let mutable wasCalled = false

            let sideEffect () =
                wasCalled <- true
                "ok"

            let expected = Error "NOPE"

            let data = [
                Ok "42"
                Ok "1024"
                expected
                Ok "1M"
                Ok "1M"
                Ok "1M"
            ]

            let! actual =
                parallelAsyncResult {
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

        testCaseAsync "for in"
        <| async {
            let data = 42

            let! actual =
                parallelAsyncResult {
                    for i in [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "for to"
        <| async {
            let data = 42

            let! actual =
                parallelAsyncResult {
                    for i = 1 to 10 do
                        ()

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "for in fail"
        <| async {
            let mutable loopCount = 0
            let expected = Error "error"

            let data = [
                Ok "42"
                Ok "1024"
                expected
                Ok "1M"
            ]

            let! actual =
                parallelAsyncResult {
                    for i in data do
                        let! x = i

                        loopCount <-
                            loopCount
                            + 1

                        ()

                    return "ok"
                }

            Expect.equal 2 loopCount "Should only loop twice"
            Expect.equal actual expected "Should be and error"
        }
    ]

let ``Stack Trace Tests`` =
    let failureAsync =
        async {
            failwith "Intentional failure"
            return ()
        }

    let mainExecuctorAsync () =
        parallelAsyncResult {
            do! Ok()
            let! _ = failureAsync
            return 42
        }

    let failureAsyncResult =
        parallelAsyncResult {
            failwith "Intentional failure"
            return ()
        }

    let mainExeuctorAsyncResult () =
        parallelAsyncResult {
            do! Ok()
            let! _ = failureAsyncResult
            return 42
        }

#if !FABLE_COMPILER
    // These are intentionally marked as pending
    // This is useful for reviewing stacktrack traces but asserting against them is very brittle
    // I'm open to suggestions around Assertions
    ptestList "Stack Trace Tests" [
        testCaseAsync "Async Failure"
        <| async {
            let! r = mainExecuctorAsync ()
            ()
        }
        testCaseAsync "AsyncResult Failure"
        <| async {
            let! r = mainExeuctorAsyncResult ()
            ()
        }
    ]

#else
    testList "Stack Trace Tests" []

#endif

let ``inference checks`` =
    testList "Inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = parallelAsyncResult { return! res }

            f (AsyncResult.ok ())
            |> ignore
    ]

let ``mergeSources Tests`` =
    testList "MergeSources Tests" [
        testCaseAsync "and! 1"
        <| async {
            let! actual =
                parallelAsyncResult {
                    let! a = parallelAsyncResult { return "a" }
                    and! b = parallelAsyncResult { return 1 }

                    return a, b
                }

            Expect.equal actual (Ok("a", 1)) ""
        }

        testCaseAsync "and! 2"
        <| async {
            let! actual =
                parallelAsyncResult {
                    let! a = parallelAsyncResult { return "a" }
                    and! b = parallelAsyncResult { return 1 }
                    and! c = parallelAsyncResult { return true }

                    return a, b, c
                }

            Expect.equal actual (Ok("a", 1, true)) ""
        }

        testCaseAsync "and! 3"
        <| async {
            let! actual =
                parallelAsyncResult {
                    let! a = parallelAsyncResult { return "a" }
                    and! b = parallelAsyncResult { return 1 }

                    let! c = parallelAsyncResult { return true }
                    and! d = parallelAsyncResult { return 7L }

                    return a, b, c, d
                }

            Expect.equal actual (Ok("a", 1, true, 7L)) ""
        }

        testCaseAsync "and! error"
        <| async {
            let! actual =
                parallelAsyncResult {
                    let! a = parallelAsyncResult { return! Error "a" }
                    and! b = parallelAsyncResult { return 2 }

                    return a, b
                }

            Expect.equal actual (Error "a") ""
        }
    ]

let allTests =
    testList "ParallelAsyncResultCETests" [
        ``return Tests``
        ``return! Tests``
        ``bind Tests``
        ``combine/zero/delay/run Tests``
        ``try Tests``
        ``using Tests``
        ``loop Tests``
        ``Stack Trace Tests``
        ``inference checks``
        ``mergeSources Tests``
    ]
