module AsyncOptionCETests


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


let ``AsyncOptionCE return Tests`` =
    testList "AsyncOptionCE  Tests" [
        testCaseAsync "Return string"
        <| async {
            let data = "Foo"
            let! actual = asyncOption { return data }
            Expect.equal actual (Some data) "Should be ok"
        }
    ]


let ``AsyncOptionCE return! Tests`` =
    testList "AsyncOptionCE return! Tests" [
        testCaseAsync "Return Some Result"
        <| async {
            let innerData = "Foo"
            let data = Some innerData
            let! actual = asyncOption { return! data }

            Expect.equal actual (data) "Should be ok"
        }

        testCaseAsync "Return Some AsyncOption"
        <| async {
            let innerData = "Foo"
            let data = Some innerData
            let! actual = asyncOption { return! Async.singleton data }

            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Async"
        <| async {
            let innerData = "Foo"
            let! actual = asyncOption { return! Async.singleton innerData }

            Expect.equal actual (Some innerData) "Should be ok"
        }
#if !FABLE_COMPILER
        testCaseAsync "Return Some TaskResult"
        <| async {
            let innerData = "Foo"
            let data = Some innerData
            let! actual = asyncOption { return! Task.FromResult data }

            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Task Generic"
        <| async {
            let innerData = "Foo"
            let! actual = asyncOption { return! Task.FromResult innerData }

            Expect.equal actual (Some innerData) "Should be ok"
        }
        testCaseAsync "Return Task"
        <| async {
            let innerData = "Foo"
            let! actual = asyncOption { return! Task.FromResult innerData :> Task }

            Expect.equal actual (Some()) "Should be ok"
        }
#endif


    ]


let ``AsyncOptionCE bind Tests`` =
    testList "AsyncOptionCE bind Tests" [
        testCaseAsync "Bind Some Result"
        <| async {
            let innerData = "Foo"
            let data = Some innerData

            let! actual =
                asyncOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (data) "Should be ok"

        }

        testCaseAsync "Bind Some AsyncOption"
        <| async {
            let innerData = "Foo"

            let data =
                Some innerData
                |> Async.singleton

            let! actual =
                asyncOption {
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
                asyncOption {
                    let! data = d
                    return data
                }

            Expect.equal actual (Some innerData) "Should be ok"
        }


#if !FABLE_COMPILER
        testCaseAsync "Bind Some TaskResult"
        <| async {
            let innerData = "Foo"

            let data =
                Some innerData
                |> Task.FromResult

            let! actual =
                asyncOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (data.Result) "Should be ok"
        }
        testCaseAsync "Bind Task Generic"
        <| async {
            let innerData = "Foo"

            let! actual =
                asyncOption {
                    let! data = Task.FromResult innerData
                    return data
                }

            Expect.equal actual (Some innerData) "Should be ok"
        }
        testCaseAsync "Bind Task"
        <| async {
            let innerData = "Foo"
            let! actual = asyncOption { do! Task.FromResult innerData :> Task }

            Expect.equal actual (Some()) "Should be ok"
        }
#endif
    ]


let ``AsyncOptionCE combine/zero/delay/run Tests`` =
    testList "AsyncOptionCE combine/zero/delay/run Tests" [
        testCaseAsync "Zero/Combine/Delay/Run"
        <| async {
            let data = 42

            let! actual =
                asyncOption {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (Some data) "Should be ok"
        }
    ]

let ``AsyncOptionCE try Tests`` =
    testList "AsyncOptionCE try Tests" [
        testCaseAsync "Try With"
        <| async {
            let data = 42

            let! actual =
                asyncOption {
                    let data = data

                    try
                        ()
                    with _ ->
                        ()

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseAsync "Try Finally"
        <| async {
            let data = 42

            let! actual =
                asyncOption {
                    let data = data

                    try
                        ()
                    finally
                        ()

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        }
    ]


let ``AsyncOptionCE using Tests`` =
    testList "AsyncOptionCE using Tests" [
        testCaseAsync "use normal disposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncOption {
                    use d = TestHelpers.makeDisposable ((fun () -> isFinished <- true))
                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
            Expect.isTrue isFinished ""
        }

        testCaseAsync "disposable not disposed too early"
        <| async {
            let mutable disposed = false
            let mutable finished = false
            let f1 _ = Async.retn (Some 42)

            let! actual =
                asyncOption {
                    use d =
                        makeDisposable (fun () ->
                            disposed <- true

                            if not finished then
                                failwith "Should not be disposed too early"
                        )

                    let! data = f1 d
                    finished <- true
                    return data
                }

            Expect.equal actual (Some 42) "Should be some"
            Expect.isTrue disposed "Should be disposed"
        }

#if NET7_0
        testCaseAsync "use sync asyncdisposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncOption {
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
        testCaseAsync "use async asyncdisposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncOption {
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
#endif
        testCaseAsync "use! normal wrapped disposable"
        <| async {
            let data = 42

            let! actual =
                asyncOption {
                    use! d =
                        TestHelpers.makeDisposable (id)
                        |> Some

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        }
#if !FABLE_COMPILER && NETSTANDARD2_1
        // Fable can't handle null disposables you get
        // TypeError: Cannot read property 'Dispose' of null
        testCaseAsync "use null disposable"
        <| async {
            let data = 42

            let! actual =
                asyncOption {
                    use d = null
                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        }
#endif
    ]


let ``AsyncOptionCE loop Tests`` =
    testList "AsyncOptionCE loop Tests" [
        testCaseAsync "while"
        <| async {
            let data = 42
            let mutable index = 0

            let! actual =
                asyncOption {
                    while index < 10 do
                        index <- index + 1

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
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
                        asyncOption {
                            while index < maxIndex do
                                index <- index + 1

                            return data
                        }

                    Expect.equal index maxIndex "Index should reach maxIndex"
                    Expect.equal actual (Some data) "Should be ok"
                }
        ]

        testCaseAsync "while fail"
        <| async {

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
                asyncOption {
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
                asyncOption {
                    for i in [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseAsync "for to"
        <| async {
            let data = 42

            let! actual =
                asyncOption {
                    for i = 1 to 10 do
                        ()

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseAsync "for in fail"
        <| async {
            let mutable loopCount = 0
            let expected = None

            let data = [
                Some "42"
                Some "1024"
                expected
                Some "1M"
            ]

            let! actual =
                asyncOption {
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
    |> Some
    |> Task.FromResult
#endif


let ``AsyncOptionCE Stack Trace Tests`` =


    let failureAsync =
        async {
            failwith "Intentional failure"
            return ()
        }

    let mainExeuctorAsync () =
        asyncOption {
            do! Some()
            let! _ = failureAsync
            return 42
        }

    let failureAsyncResult =
        asyncOption {
            failwith "Intentional failure"
            return ()
        }

    let mainExeuctorAsyncResult () =
        asyncOption {
            do! Some()
            let! _ = failureAsyncResult
            return 42
        }

#if !FABLE_COMPILER
    // These are intentionally marked as pending
    // This is useful for reviewing stacktrack traces but asserting against them is very brittle
    // I'm open to suggestions around Assertions
    ptestList "AsyncOptionCE Stack Trace Tests" [
        testCaseAsync "Async Failure"
        <| async {
            let! r = mainExeuctorAsync ()
            ()
        }
        testCaseAsync "AsyncOption Failure"
        <| async {
            let! r = mainExeuctorAsyncResult ()
            ()
        }


    ]

#else
    testList "AsyncOptionCE Stack Trace Tests" []
#endif


let ``AsyncOptionCE inference checks`` =
    testList "AsyncOptionCE inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = asyncOption { return! res }

            f (AsyncOption.some ())
            |> ignore
    ]

let allTests =
    testList "AsyncResultCETests" [
        ``AsyncOptionCE return Tests``
        ``AsyncOptionCE return! Tests``
        ``AsyncOptionCE bind Tests``
        ``AsyncOptionCE combine/zero/delay/run Tests``
        ``AsyncOptionCE try Tests``
        ``AsyncOptionCE using Tests``
        ``AsyncOptionCE loop Tests``
        ``AsyncOptionCE Stack Trace Tests``
        ``AsyncOptionCE inference checks``
    ]
