module AsyncResultCETests


#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open SampleDomain
open TestData
open TestHelpers
open System.Threading.Tasks
open FsToolkit.ErrorHandling


let ``AsyncResultCE return Tests`` =
    testList "AsyncResultCE  Tests" [
        testCaseAsync "Return string"
        <| async {
            let data = "Foo"
            let! actual = asyncResult { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]


let ``AsyncResultCE return! Tests`` =
    testList "AsyncResultCE return! Tests" [
        testCaseAsync "Return Ok Result"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { return! data }

            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Ok Choice"
        <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = asyncResult { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

        testCaseAsync "Return Ok AsyncResult"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { return! Async.singleton data }

            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Async"
        <| async {
            let innerData = "Foo"
            let! actual = asyncResult { return! Async.singleton innerData }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
#if !FABLE_COMPILER
        testCaseAsync "Return Ok TaskResult"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { return! Task.FromResult data }

            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Task Generic"
        <| async {
            let innerData = "Foo"
            let! actual = asyncResult { return! Task.FromResult innerData }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Return Task"
        <| async {
            let innerData = "Foo"
            let! actual = asyncResult { return! Task.FromResult innerData :> Task }

            Expect.equal actual (Result.Ok()) "Should be ok"
        }
#endif


    ]


let ``AsyncResultCE bind Tests`` =
    testList "AsyncResultCE bind Tests" [
        testCaseAsync "Bind Ok Result"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData

            let! actual =
                asyncResult {
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
                asyncResult {
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
                asyncResult {
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
                asyncResult {
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
                asyncResult {
                    let! data = data
                    return data
                }

            Expect.equal actual (data.Result) "Should be ok"
        }
        testCaseAsync "Bind Task Generic"
        <| async {
            let innerData = "Foo"

            let! actual =
                asyncResult {
                    let! data = Task.FromResult innerData
                    return data
                }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Bind Task"
        <| async {
            let innerData = "Foo"
            let! actual = asyncResult { do! Task.FromResult innerData :> Task }

            Expect.equal actual (Result.Ok()) "Should be ok"
        }
#endif
    ]


let ``AsyncResultCE combine/zero/delay/run Tests`` =
    testList "AsyncResultCE combine/zero/delay/run Tests" [
        testCaseAsync "Zero/Combine/Delay/Run"
        <| async {
            let data = 42

            let! actual =
                asyncResult {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]

let ``AsyncResultCE try Tests`` =
    testList "AsyncResultCE try Tests" [
        testCaseAsync "Try With"
        <| async {
            let data = 42

            let! actual =
                asyncResult {
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
                asyncResult {
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

let ``AsyncResultCE using Tests`` =
    testList "AsyncResultCE using Tests" [
        testCaseAsync "use normal disposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncResult {
                    use d = TestHelpers.makeDisposable ((fun () -> isFinished <- true))
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
            Expect.isTrue isFinished ""
        }
#if !FABLE_COMPILER
        testCaseAsync "use sync asyncdisposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncResult {
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
            Expect.isTrue isFinished ""
        }
        testCaseAsync "use async asyncdisposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncResult {
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
            Expect.isTrue isFinished ""
        }
#endif

        testCaseAsync "use! normal wrapped disposable"
        <| async {
            let data = 42

            let! actual =
                asyncResult {
                    use! d =
                        TestHelpers.makeDisposable (id)
                        |> Result.Ok

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
#if !FABLE_COMPILER && NETSTANDARD2_1
        // Fable can't handle null disposables you get
        // TypeError: Cannot read property 'Dispose' of null
        testCaseAsync "use null disposable"
        <| async {
            let data = 42

            let! actual =
                asyncResult {
                    use d = null
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
#endif
    ]


let ``AsyncResultCE loop Tests`` =
    testList "AsyncResultCE loop Tests" [
        testCaseAsync "while"
        <| async {
            let data = 42
            let mutable index = 0

            let! actual =
                asyncResult {
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
                        asyncResult {
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
                asyncResult {
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
                asyncResult {
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
                asyncResult {
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
                asyncResult {
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


#if !FABLE_COMPILER
let toTaskResult v =
    v
    |> Ok
    |> Task.FromResult
#endif


let ``AsyncResultCE Stack Trace Tests`` =


    let failureAsync =
        async {
            failwith "Intentional failure"
            return ()
        }

    let mainExeuctorAsync () =
        asyncResult {
            do! Ok()
            let! _ = failureAsync
            return 42
        }

    let failureAsyncResult =
        asyncResult {
            failwith "Intentional failure"
            return ()
        }

    let mainExeuctorAsyncResult () =
        asyncResult {
            do! Ok()
            let! _ = failureAsyncResult
            return 42
        }

#if !FABLE_COMPILER
    // These are intentionally marked as pending
    // This is useful for reviewing stacktrack traces but asserting against them is very brittle
    // I'm open to suggestions around Assertions
    ptestList "AsyncResultCE Stack Trace Tests" [
        testCaseAsync "Async Failure"
        <| async {
            let! r = mainExeuctorAsync ()
            ()
        }
        testCaseAsync "AsyncResult Failure"
        <| async {
            let! r = mainExeuctorAsyncResult ()
            ()
        }


    ]

#else
    testList "AsyncResultCE Stack Trace Tests" []

#endif


let ``AsyncResultCE inference checks`` =
    testList "AsyncResultCEInference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = asyncResult { return! res }

            f (AsyncResult.retn ())
            |> ignore
    ]


let allTests =
    testList "AsyncResultCETests" [
        ``AsyncResultCE return Tests``
        ``AsyncResultCE return! Tests``
        ``AsyncResultCE bind Tests``
        ``AsyncResultCE combine/zero/delay/run Tests``
        ``AsyncResultCE try Tests``
        ``AsyncResultCE using Tests``
        ``AsyncResultCE loop Tests``
        ``AsyncResultCE Stack Trace Tests``
        ``AsyncResultCE inference checks``
    ]
