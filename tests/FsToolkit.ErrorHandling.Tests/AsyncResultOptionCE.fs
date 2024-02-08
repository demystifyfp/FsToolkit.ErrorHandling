module AsyncResultOptionCETests
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
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResultOption
open System


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

let inline OkSome (value: 'a) = Ok(Some(value))
let inline OkNone _ = Ok(None)

let ``AsyncResultOptionCE return Tests`` =
    testList "AsyncResultOptionCE  Tests" [
        testCaseAsync "Return string"
        <| async {
            let data = "Foo"
            let! actual = asyncResultOption { return data }
            Expect.equal actual (OkSome data) "Should be ok"
        }
    ]


let ``AsyncResultOptionCE return! Tests`` =
    testList "AsyncResultOptionCE return! Tests" [
        testCaseAsync "Return Ok Result"
        <| async {
            let innerData = "Foo"
            let data = Ok innerData
            let! actual = asyncResultOption { return! data }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Return Ok Choice"
        <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = asyncResultOption { return! data }
            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Return Some"
        <| async {
            let innerData = "Foo"
            let data = Some innerData
            let! actual = asyncResultOption { return! data }
            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Return Ok AsyncResultOption"
        <| async {
            let innerData = "Foo"
            let data = AsyncResultOption.retn innerData
            let! actual = asyncResultOption { return! data }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Return Ok AsyncResult"
        <| async {
            let innerData = "Foo"
            let data = AsyncResult.retn innerData
            let! actual = asyncResultOption { return! data }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Return Ok AsyncOption"
        <| async {
            let innerData = "Foo"
            let data = AsyncOption.retn innerData
            let! actual = asyncResultOption { return! data }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Return Async"
        <| async {
            let innerData = "Foo"
            let! actual = asyncResultOption { return! Async.singleton innerData }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
#if !FABLE_COMPILER

        testCaseAsync "Return Ok TaskResultOption"
        <| async {
            let innerData = "Foo"
            let data = Task.FromResult(Ok(Some innerData))
            let! actual = asyncResultOption { return! data }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Return Ok TaskResult"
        <| async {
            let innerData = "Foo"
            let data = Task.FromResult(Ok innerData)
            let! actual = asyncResultOption { return! data }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Return Ok TaskOption"
        <| async {
            let innerData = "Foo"
            let data = Task.FromResult(Some innerData)
            let! actual = asyncResultOption { return! data }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Return Task<'T>"
        <| async {
            let innerData = "Foo"
            let! actual = asyncResultOption { return! Task.FromResult innerData }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Return Task"
        <| async {
            let innerData = ()
            let data: Task = Task.FromResult()
            let! actual = asyncResultOption { return! data }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
#endif


    ]


let ``AsyncResultOptionCE bind Tests`` =
    testList "AsyncResultOptionCE bind Tests" [
        testCaseAsync "Bind Ok Result.Ok"
        <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Bind Ok Result.Error"
        <| async {
            let innerData = "Foo"
            let data = Result.Error innerData

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (Error innerData) "Should be ok"
        }
        testCaseAsync "Bind Ok Choice1Of2"
        <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Bind Ok Choice2Of2"
        <| async {
            let innerData = "Foo"
            let data = Choice2Of2 innerData

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (Error innerData) "Should be ok"
        }

        testCaseAsync "Bind Option.None"
        <| async {
            let innerData = "Foo"
            let data = None

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (OkNone innerData) "Should be ok"
        }


        testCaseAsync "Bind Option.Some"
        <| async {
            let innerData = "Foo"
            let data = Some innerData

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Bind Ok AsyncResult"
        <| async {
            let innerData = "Foo"

            let data =
                Result.Ok innerData
                |> Async.singleton

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            let! data = data
            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Bind Ok AsyncOption"
        <| async {
            let innerData = "Foo"

            let data =
                Some innerData
                |> Async.singleton

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            let! data = data
            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Bind Ok AsyncResultOption"
        <| async {
            let innerData = "Foo"

            let data = AsyncResultOption.retn innerData

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            let! data = data
            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Bind Async"
        <| async {
            let innerData = "Foo"
            let d = Async.singleton innerData

            let! actual =
                asyncResultOption {
                    let! data = d
                    return data
                }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }


#if !FABLE_COMPILER
        testCaseAsync "Bind Ok TaskResult"
        <| async {
            let innerData = "Foo"

            let data =
                Result.Ok innerData
                |> Task.FromResult

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Bind Ok TaskOption"
        <| async {
            let innerData = "Foo"

            let data =
                Some innerData
                |> Task.FromResult

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }


        testCaseAsync "Bind Ok TaskResultOption"
        <| async {
            let innerData = "Foo"

            let data =
                Some innerData
                |> Ok
                |> Task.FromResult

            let! actual =
                asyncResultOption {
                    let! data = data
                    return data
                }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }

        testCaseAsync "Bind Task Generic"
        <| async {
            let innerData = "Foo"

            let! actual =
                asyncResultOption {
                    let! data = Task.FromResult innerData
                    return data
                }

            Expect.equal actual (OkSome innerData) "Should be ok"
        }
        testCaseAsync "Bind Task"
        <| async {
            let innerData = "Foo"
            let! actual = asyncResultOption { do! Task.FromResult innerData :> Task }

            Expect.equal actual (OkSome()) "Should be ok"
        }
#endif
    ]


let ``AsyncResultOptionCE combine/zero/delay/run Tests`` =
    testList "AsyncResultOptionCE combine/zero/delay/run Tests" [
        testCaseAsync "Zero/Combine/Delay/Run"
        <| async {
            let data = 42

            let! actual =
                asyncResultOption {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (OkSome data) "Should be ok"
        }
    ]

let ``AsyncResultOptionCE try Tests`` =
    testList "AsyncResultOptionCE try Tests" [
        testCaseAsync "Try With"
        <| async {
            let data = 42

            let! actual =
                asyncResultOption {
                    let data = data

                    try
                        ()
                    with _ ->
                        ()

                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
        }
        testCaseAsync "Try Finally"
        <| async {
            let data = 42

            let! actual =
                asyncResultOption {
                    let data = data

                    try
                        ()
                    finally
                        ()

                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
        }
    ]

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }

let makeAsyncDisposable (callback) =
    { new System.IAsyncDisposable with
        member this.DisposeAsync() = callback ()
    }


let ``AsyncResultOptionCE using Tests`` =
    testList "AsyncResultOptionCE using Tests" [
        testCaseAsync "use normal disposable"
        <| async {
            let data = 42

            let! actual =
                asyncResultOption {
                    use d = makeDisposable ()
                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
        }
#if !FABLE_COMPILER
        testCaseAsync "use sync asyncdisposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncResultOption {
                    use d =
                        makeAsyncDisposable (
                            (fun () ->
                                isFinished <- true
                                ValueTask()
                            )
                        )

                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
            Expect.isTrue isFinished ""
        }
        testCaseAsync "use async asyncdisposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncResultOption {
                    use d =
                        makeAsyncDisposable (
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

            Expect.equal actual (OkSome data) "Should be ok"
            Expect.isTrue isFinished ""
        }
#endif

        testCaseAsync "use! normal wrapped disposable"
        <| async {
            let data = 42

            let! actual =
                asyncResultOption {
                    use! d =
                        makeDisposable ()
                        |> Result.Ok

                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
        }
#if !FABLE_COMPILER
        // Fable can't handle null disposables you get
        // TypeError: Cannot read property 'Dispose' of null
        testCaseAsync "use null disposable"
        <| async {
            let data = 42

            let! actual =
                asyncResultOption {
                    use d = null
                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
        }
#endif
    ]


let ``AsyncResultOptionCE loop Tests`` =
    testList "AsyncResultOptionCE loop Tests" [
        testCaseAsync "while"
        <| async {
            let data = 42
            let mutable index = 0

            let! actual =
                asyncResultOption {
                    while index < 10 do
                        index <- index + 1

                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
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
                        asyncResultOption {
                            while index < maxIndex do
                                index <- index + 1

                            return data
                        }

                    Expect.equal index maxIndex "Index should reach maxIndex"
                    Expect.equal actual (OkSome data) "Should be ok"
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
                asyncResultOption {
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
                asyncResultOption {
                    for i in [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
        }
        testCaseAsync "for to"
        <| async {
            let data = 42

            let! actual =
                asyncResultOption {
                    for i = 1 to 10 do
                        ()

                    return data
                }

            Expect.equal actual (OkSome data) "Should be ok"
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
                asyncResultOption {
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


let ``AsyncResultOptionCE inference checks`` =
    testList "AsyncResultOptionCE Inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = asyncResultOption { return! res }

            f (AsyncResultOption.retn ())
            |> ignore
    ]


let allTests =
    testList "AsyncResultCETests" [
        ``AsyncResultOptionCE return Tests``
        ``AsyncResultOptionCE return! Tests``
        ``AsyncResultOptionCE bind Tests``
        ``AsyncResultOptionCE combine/zero/delay/run Tests``
        ``AsyncResultOptionCE try Tests``
        ``AsyncResultOptionCE using Tests``
        ``AsyncResultOptionCE loop Tests``
        ``AsyncResultOptionCE inference checks``
    ]
