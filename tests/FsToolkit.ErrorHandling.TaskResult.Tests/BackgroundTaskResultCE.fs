module BackgroundTaskResultCETests


open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open System.Threading.Tasks

#if NETSTANDARD2_0 || NET5_0
let backgroundTask = FSharp.Control.Tasks.NonAffine.task
#endif

[<Tests>]
let ``BackgroundTaskResultCE return Tests`` =
    testList "BackgroundTaskResultCE  Tests" [
        testCaseTask "Return string"
        <| fun () -> backgroundTask {
            let data = "Foo"
            let! actual = backgroundTaskResult { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
           }
    ]


[<Tests>]
let ``BackgroundTaskResultCE return! Tests`` =
    testList "BackgroundTaskResultCE return! Tests" [
        testCaseTask "Return Ok Result"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = backgroundTaskResult { return! data }

            Expect.equal actual (data) "Should be ok"
           }
        testCaseTask "Return Ok Choice"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = backgroundTaskResult { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }

        testCaseTask "Return Ok AsyncResult"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = backgroundTaskResult { return! Async.singleton data }

            Expect.equal actual (data) "Should be ok"
           }
        testCaseTask "Return Ok TaskResult"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = backgroundTaskResult { return! Task.FromResult data }

            Expect.equal actual (data) "Should be ok"
           }
        testCaseTask "Return Async"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let! actual = backgroundTaskResult { return! Async.singleton innerData }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }
        testCaseTask "Return Task Generic"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let! actual = backgroundTaskResult { return! Task.singleton innerData }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }
        testCaseTask "Return Task"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let! actual = backgroundTaskResult { return! Task.FromResult innerData :> Task }

            Expect.equal actual (Result.Ok()) "Should be ok"
           }
        testCaseTask "Return ValueTask Generic"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let! actual = backgroundTaskResult { return! ValueTask.FromResult innerData }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }
        testCaseTask "Return ValueTask"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult { return! ValueTask.CompletedTask }

            Expect.equal actual (Result.Ok()) "Should be ok"
           }
#if NETSTANDARD2_0
        testCaseTask "Return Ply"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let! actual = backgroundTaskResult { return! Unsafe.uply { return innerData } }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }
#endif
    ]


[<Tests>]
let ``BackgroundTaskResultCE bind Tests`` =
    testList "BackgroundTaskResultCE bind Tests" [
        testCaseTask "Bind Ok Result"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let data = Result.Ok innerData

            let! actual = backgroundTaskResult {
                let! data = data
                return data
            }

            Expect.equal actual (data) "Should be ok"

           }
        testCaseTask "Bind Ok Choice"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let data = Choice1Of2 innerData

            let! actual = backgroundTaskResult {
                let! data = data
                return data
            }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }


        testCaseTask "Bind Ok AsyncResult"
        <| fun () -> backgroundTask {
            let innerData = "Foo"

            let data =
                Result.Ok innerData
                |> Async.singleton

            let! actual = backgroundTaskResult {
                let! data = data
                return data
            }

            Expect.equal
                actual
                (data
                 |> Async.RunSynchronously)
                "Should be ok"
           }
        testCaseTask "Bind Ok TaskResult"
        <| fun () -> backgroundTask {
            let innerData = "Foo"

            let data =
                Result.Ok innerData
                |> Task.singleton

            let! actual = backgroundTaskResult {
                let! data = data
                return data
            }

            Expect.equal actual (data.Result) "Should be ok"
           }
        testCaseTask "Bind Async"
        <| fun () -> backgroundTask {
            let innerData = "Foo"

            let! actual = backgroundTaskResult {
                let! data = Async.singleton innerData
                return data
            }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }
        testCaseTask "Bind Task Generic"
        <| fun () -> backgroundTask {
            let innerData = "Foo"

            let! actual = backgroundTaskResult {
                let! data = Task.FromResult innerData
                return data
            }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }
        testCaseTask "Bind Task"
        <| fun () -> backgroundTask {
            let innerData = "Foo"
            let! actual = backgroundTaskResult { do! Task.FromResult innerData :> Task }

            Expect.equal actual (Result.Ok()) "Should be ok"
           }
        testCaseTask "Bind ValueTask Generic"
        <| fun () -> backgroundTask {
            let innerData = "Foo"

            let! actual = backgroundTaskResult {
                let! data = ValueTask.FromResult innerData
                return data
            }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }
        testCaseTask "Bind ValueTask"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult { do! ValueTask.CompletedTask }

            Expect.equal actual (Result.Ok()) "Should be ok"
           }

        testCaseTask "Task.Yield"
        <| fun () -> backgroundTask {

            let! actual = backgroundTaskResult { do! Task.Yield() }

            Expect.equal actual (Ok()) "Should be ok"
           }
#if NETSTANDARD2_0
        testCaseTask "Bind Ply"
        <| fun () -> backgroundTask {
            let innerData = "Foo"

            let! actual = backgroundTaskResult {
                let! data = Unsafe.uply { return innerData }
                return data
            }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
           }
#endif
    ]


[<Tests>]
let ``BackgroundTaskResultCE combine/zero/delay/run Tests`` =
    testList "BackgroundTaskResultCE combine/zero/delay/run Tests" [
        testCaseTask "Zero/Combine/Delay/Run"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskResult {
                let result = data

                if true then
                    ()

                return result
            }

            Expect.equal actual (Result.Ok data) "Should be ok"
           }
    ]


[<Tests>]
let ``BackgroundTaskResultCE try Tests`` =
    testList "BackgroundTaskResultCE try Tests" [
        testCaseTask "Try With"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskResult {
                let data = data

                try
                    ()
                with _ ->
                    ()

                return data
            }

            Expect.equal actual (Result.Ok data) "Should be ok"
           }
        testCaseTask "Try Finally"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskResult {
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

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }

[<Tests>]
let ``BackgroundTaskResultCE using Tests`` =
    testList "BackgroundTaskResultCE using Tests" [
        testCaseTask "use normal disposable"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskResult {
                use d = makeDisposable ()
                return data
            }

            Expect.equal actual (Result.Ok data) "Should be ok"
           }
        testCaseTask "use! normal wrapped disposable"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskResult {
                use! d =
                    makeDisposable ()
                    |> Result.Ok

                return data
            }

            Expect.equal actual (Result.Ok data) "Should be ok"
           }
        testCaseTask "use null disposable"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskResult {
                use d = null
                return data
            }

            Expect.equal actual (Result.Ok data) "Should be ok"
           }
    ]


[<Tests>]
let ``BackgroundTaskResultCE loop Tests`` =
    testList "BackgroundTaskResultCE loop Tests" [
        yield! [
            let maxIndices = [10; 1000000]
            for maxIndex in maxIndices do
                testCaseTask <| sprintf "While - %i" maxIndex
                <|  fun () -> backgroundTask {
                    let data = 42
                    let mutable index = 0

                    let! actual = backgroundTaskResult {
                        while index < maxIndex do
                            index <- index + 1

                        return data
                    }

                    Expect.equal index maxIndex "Index should reach maxIndex"
                    Expect.equal actual (Ok data) "Should be ok"
                }
        ]
        testCaseTask "while fail"
        <| fun () -> backgroundTask {


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

            let! actual = backgroundTaskResult {
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
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskResult {
                for i in [ 1..10 ] do
                    ()

                return data
            }

            Expect.equal actual (Result.Ok data) "Should be ok"
           }
        testCaseTask "for to"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskResult {
                for i = 1 to 10 do
                    ()

                return data
            }

            Expect.equal actual (Result.Ok data) "Should be ok"
           }
        testCaseTask "for in fail"
        <| fun () -> backgroundTask {

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

            let! actual = backgroundTaskResult {
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
let ``BackgroundTaskResultCE applicative tests`` =
    testList "BackgroundTaskResultCE applicative tests" [
        testCaseTask "Happy Path TaskResult"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult {
                let! a = TaskResult.retn 3
                and! b = TaskResult.retn 2
                and! c = TaskResult.retn 1
                return a + b - c
            }

            Expect.equal actual (Ok 4) "Should be ok"
           }

        testCaseTask "Happy Path AsyncResult"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult {
                let! a = AsyncResult.retn 3
                and! b = AsyncResult.retn 2
                and! c = AsyncResult.retn 1
                return a + b - c
            }

            Expect.equal actual (Ok 4) "Should be ok"
           }


        testCaseTask "Happy Path Result"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult {
                let! a = Result.Ok 3
                and! b = Result.Ok 2
                and! c = Result.Ok 1
                return a + b - c
            }

            Expect.equal actual (Ok 4) "Should be ok"
           }

        testCaseTask "Happy Path Choice"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult {
                let! a = Choice1Of2 3
                and! b = Choice1Of2 2
                and! c = Choice1Of2 1
                return a + b - c
            }

            Expect.equal actual (Ok 4) "Should be ok"
           }

        testCaseTask "Happy Path Async"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult {
                let! a = Async.singleton 3 //: Async<int>
                and! b = Async.singleton 2 //: Async<int>
                and! c = Async.singleton 1 //: Async<int>
                return a + b - c
            }

            Expect.equal actual (Ok 4) "Should be ok"
           }

        testCaseTask "Happy Path 2 Async"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult {
                let! a = Async.singleton 3 //: Async<int>
                and! b = Async.singleton 2 //: Async<int>
                return a + b
            }

            Expect.equal actual (Ok 5) "Should be ok"
           }

        testCaseTask "Happy Path 2 Task"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult {
                let! a = Task.FromResult 3
                and! b = Task.FromResult 2
                return a + b
            }

            Expect.equal actual (Ok 5) "Should be ok"
           }
        let specialCaseTask returnValue =
#if NETSTANDARD2_0
            Unsafe.uply { return returnValue }
#else
            Task.FromResult returnValue
#endif

        testCaseTask "Happy Path Result/Choice/AsyncResult/Ply/ValueTask"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskResult {
                let! a = Ok 3
                and! b = Choice1Of2 2

                and! c =
                    Ok 1
                    |> Async.singleton

                and! d = specialCaseTask (Ok 3)
                and! e = ValueTask.FromResult(Ok 5)

                return
                    a + b
                    - c
                    - d
                    + e
            }

            Expect.equal actual (Ok 6) "Should be ok"
           }

        testCaseTask "Fail Path Result"
        <| fun () -> backgroundTask {
            let expected = Error "TryParse failure"

            let! actual = backgroundTaskResult {
                let! a = Ok 3
                and! b = Ok 2
                and! c = expected
                return a + b - c
            }

            Expect.equal actual expected "Should be Error"
           }

        testCaseTask "Fail Path Choice"
        <| fun () -> backgroundTask {
            let errorMsg = "TryParse failure"

            let! actual = backgroundTaskResult {
                let! a = Choice1Of2 3
                and! b = Choice1Of2 2
                and! c = Choice2Of2 errorMsg
                return a + b - c
            }

            Expect.equal actual (Error errorMsg) "Should be Error"
           }

        testCaseTask "Fail Path Result/Choice/AsyncResult"
        <| fun () -> backgroundTask {
            let errorMsg = "TryParse failure"

            let! actual = backgroundTaskResult {
                let! a = Choice1Of2 3

                and! b =
                    Ok 2
                    |> Async.singleton

                and! c = Error errorMsg
                return a + b - c
            }

            Expect.equal actual (Error errorMsg) "Should be Error"
           }
    ]
