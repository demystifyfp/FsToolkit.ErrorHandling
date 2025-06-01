module AsyncValidationCETests


#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif

open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

let ``AsyncValidationCE return Tests`` =
    testList "AsyncValidationCE return Tests" [
        testCaseAsync "Return string"
        <| async {
            let data = "Foo"
            let! actual = asyncValidation { return data }
            Expect.equal actual (Ok data) "Should be ok"
        }
    ]

let ``AsyncValidationCE return! Tests`` =
    testList "AsyncValidationCE return! Tests" [
        testCaseAsync "Return Ok result"
        <| async {
            let data = Ok "Foo"
            let! actual = asyncValidation { return! data }
            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Error result"
        <| async {
            let innerData = "Foo"
            let! expected = AsyncValidation.error innerData
            let data = Result.Error innerData
            let! actual = asyncValidation { return! data }
            Expect.equal actual expected "Should be error"
        }
        testCaseAsync "Return Ok Choice"
        <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = asyncValidation { return! data }
            Expect.equal actual (Ok innerData) "Should be ok"
        }
        testCaseAsync "Return Error Choice"
        <| async {
            let innerData = "Foo"
            let! expected = AsyncValidation.error innerData
            let data = Choice2Of2 innerData
            let! actual = asyncValidation { return! data }
            Expect.equal actual expected "Should be error"
        }
        testCaseAsync "Return Ok Validation"
        <| async {
            let innerData = "Foo"
            let data = Validation.ok innerData
            let! actual = asyncValidation { return! data }
            Expect.equal actual (Ok innerData) "Should be ok"
        }
        testCaseAsync "Return Error Validation"
        <| async {
            let innerData = "Foo"
            let expected = Validation.error innerData
            let data = AsyncValidation.error innerData
            let! actual = asyncValidation { return! data }
            Expect.equal actual expected "Should be ok"
        }
    ]


let ``AsyncValidationCE bind Tests`` =
    testList "AsyncValidationCE bind Tests" [
        testCaseAsync "let! Async"
        <| async {
            let data = "Foo"

            let! actual =
                asyncValidation {
                    let! f = async { return data }
                    return f
                }

            Expect.equal actual (Ok data) "Should be ok"
        }
        testCaseAsync "let! Ok result"
        <| async {
            let data = Ok "Foo"

            let! actual =
                asyncValidation {
                    let! f = data
                    return f
                }

            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "let! Error result"
        <| async {
            let innerData = "Foo"
            let data = Result.Error innerData
            let! expected = AsyncValidation.error innerData

            let! actual =
                asyncValidation {
                    let! f = data
                    return f
                }

            Expect.equal actual expected "Should be ok"
        }
        testCaseAsync "let! Ok Choice"
        <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData

            let! actual =
                asyncValidation {
                    let! f = data
                    return f
                }

            Expect.equal actual (Ok innerData) "Should be ok"
        }
        testCaseAsync "let! Error Choice"
        <| async {
            let innerData = "Foo"
            let data = Choice2Of2 innerData
            let! expected = AsyncValidation.error innerData

            let! actual =
                asyncValidation {
                    let! f = data
                    return f
                }

            Expect.equal actual expected "Should be ok"
        }
        testCaseAsync "let! Ok Validation"
        <| async {
            let innerData = "Foo"

            let! actual =
                asyncValidation {
                    let! f = validation { return innerData }
                    return f
                }

            Expect.equal actual (Ok innerData) "Should be ok"
        }
        testCaseAsync "let! Error Validation"
        <| async {
            let innerData = "Foo"
            let error = Error innerData
            let expected = Error [ innerData ]

            let! actual =
                asyncValidation {
                    let! f = validation { return! error }
                    and! _ = validation { return! Ok innerData }
                    return f
                }

            Expect.equal actual expected "Should be ok"
        }
        testCaseAsync "do! Ok result"
        <| async {
            let data = Ok()
            let! actual = asyncValidation { do! data }
            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "do! Error result"
        <| async {
            let innerData = ()
            let data = Result.Error innerData
            let! expected = AsyncValidation.error innerData
            let! actual = asyncValidation { do! data }
            Expect.equal actual expected "Should be error"
        }
        testCaseAsync "do! Ok Choice"
        <| async {
            let innerData = ()
            let! expected = AsyncValidation.ok innerData
            let data = Choice1Of2 innerData
            let! actual = asyncValidation { do! data }
            Expect.equal actual expected "Should be ok"
        }
        testCaseAsync "do! Error Choice"
        <| async {
            let innerData = ()
            let! expected = AsyncValidation.error innerData
            let data = Choice2Of2 innerData
            let! actual = asyncValidation { do! data }
            Expect.equal actual expected "Should be error"
        }
        testCaseAsync "do! Ok Validation"
        <| async {
            let innerData = ()
            let! expected = AsyncValidation.ok innerData
            let data = AsyncValidation.ok innerData
            let! actual = asyncValidation { do! data }
            Expect.equal actual expected "Should be ok"
        }
        testCaseAsync "do! Error Validation"
        <| async {
            let innerData = ()
            let! expected = AsyncValidation.error innerData
            let data = AsyncValidation.error innerData
            let! actual = asyncValidation { do! data }
            Expect.equal actual expected "Should be error"
        }
    ]

let ``AsyncValidationCE combine/zero/delay/run Tests`` =
    testList "AsyncValidationCE combine/zero/delay/run Tests" [
        testCaseAsync "Zero/Combine/Delay/Run"
        <| async {
            let data = 42

            let! actual =
                asyncValidation {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (Ok data) "Should be ok"
        }
    ]


let ``AsyncValidationCE try Tests`` =
    testList "AsyncValidationCE try Tests" [
        testCaseAsync "Try With"
        <| async {
            let data = 42

            let! actual =
                asyncValidation {
                    let data = data

                    try
                        ()
                    with _ ->
                        ()

                    return data
                }

            Expect.equal actual (Ok data) "Should be ok"
        }
        testCaseAsync "Try Finally"
        <| async {
            let data = 42

            let! actual =
                asyncValidation {
                    let data = data

                    try
                        ()
                    finally
                        ()

                    return data
                }

            Expect.equal actual (Ok data) "Should be ok"
        }
    ]

let makeDisposable callback =
    { new System.IDisposable with
        member _.Dispose() = callback ()
    }

let ``AsyncValidationCE using Tests`` =
    testList "AsyncValidationCE using Tests" [
        testCaseAsync "use normal disposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncValidation {
                    use d = makeDisposable (fun () -> isFinished <- true)
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
            Expect.isTrue isFinished "Expected disposable to be disposed"
        }

        testCaseAsync "use! normal wrapped disposable"
        <| async {
            let data = 42
            let mutable isFinished = false

            let! actual =
                asyncValidation {
                    use! d =
                        makeDisposable (fun () -> isFinished <- true)
                        |> Ok

                    return data
                }

            Expect.equal actual (Ok data) "Should be ok"
            Expect.isTrue isFinished "Expected disposable to be disposed"
        }

        testCaseAsync "disposable not disposed too early"
        <| async {
            let mutable disposed = false
            let mutable finished = false
            let f1 _ = AsyncResult.ok 42

            let! actual =
                asyncValidation {
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
                asyncValidation {
                    use d = null
                    return data
                }

            Expect.equal actual (Ok data) "Should be ok"
        }
#endif
    ]

let ``AsyncValidationCE loop Tests`` =
    testList "AsyncValidationCE loop Tests" [
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
                        asyncValidation {
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
                asyncValidation {
                    while loopCount < data.Length do
                        let! x = data.[loopCount]

                        loopCount <-
                            loopCount
                            + 1

                    return sideEffect ()
                }

            Expect.equal loopCount 2 "Should only loop twice"
            Expect.equal actual (Error [ "NOPE" ]) "Should be an error"
            Expect.isFalse wasCalled "No additional side effects should occur"
        }

        testCaseAsync "for in"
        <| async {
            let data = 42

            let! actual =
                asyncValidation {
                    for i in [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (Ok data) "Should be ok"
        }
        testCaseAsync "for to"
        <| async {
            let data = 42

            let! actual =
                asyncValidation {
                    for i = 1 to 10 do
                        ()

                    return data
                }

            Expect.equal actual (Ok data) "Should be ok"
        }
    ]

let ``AsyncValidationCE applicative tests`` =
    testList "AsyncValidationCE applicative tests" [
        testCaseAsync "Happy Path Result"
        <| async {
            let! actual =
                asyncValidation {
                    let! a = Ok 3
                    and! b = Ok 2
                    and! c = Ok 1
                    return a + b - c
                }

            Expect.equal actual (Ok 4) "Should be ok"
        }
        testCaseAsync "Happy Path Valiation"
        <| async {
            let! actual =
                asyncValidation {
                    let! a = AsyncValidation.ok 3
                    and! b = AsyncValidation.ok 2
                    and! c = AsyncValidation.ok 1
                    return a + b - c
                }

            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseAsync "Happy Path Result/Validation"
        <| async {
            let! actual =
                asyncValidation {
                    let! a = AsyncValidation.ok 3
                    and! b = Ok 2
                    and! c = AsyncValidation.ok 1
                    return a + b - c
                }

            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseAsync "Happy Path Choice"
        <| async {
            let! actual =
                asyncValidation {
                    let! a = Choice1Of2 3
                    and! b = Choice1Of2 2
                    and! c = Choice1Of2 1
                    return a + b - c
                }

            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseAsync "Happy Path Result/Choice/Task/Validation"
        <| async {
            let! actual =
                asyncValidation {
                    let! a = Ok 3
                    and! b = Choice1Of2 2
                    and! c = AsyncValidation.ok 1

                    return a + b - c
                }

            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseAsync "Sad Path Async Result/Async Result"
        <| async {
            let expected =
                Error [
                    "Hello"
                    "World"
                ]

            let! actual =
                asyncValidation {
                    let! _ = async { return Error "Hello" }
                    and! _ = async { return Error "World" }
                    return ()
                }

            Expect.equal actual expected "Should be error"
        }

#if !FABLE_COMPILER

        testCaseAsync "Sad Path Task Result/Task Result"
        <| async {
            let expected =
                Error [
                    "Hello"
                    "World"
                ]

            let! actual =
                asyncValidation {
                    let! _ = task { return Error "Hello" }
                    and! _ = task { return Error "World" }
                    return ()
                }

            Expect.equal actual expected "Should be error"
        }

#endif

        testCaseAsync "Fail Path Result"
        <| async {
            let expected =
                Error [
                    "Error 1"
                    "Error 2"
                ]

            let! actual =
                asyncValidation {
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

        testCaseAsync "Fail Path Validation"
        <| async {
            let expected = AsyncValidation.error "TryParse failure"
            let! expected' = expected

            let! actual =
                asyncValidation {
                    let! a = AsyncValidation.ok 3
                    and! b = AsyncValidation.ok 2
                    and! c = expected
                    return a + b - c
                }

            Expect.equal actual expected' "Should be Error"
        }
    ]

let allTests =
    testList "Validation CE Tests" [
        ``AsyncValidationCE return Tests``
        ``AsyncValidationCE return! Tests``
        ``AsyncValidationCE bind Tests``
        ``AsyncValidationCE combine/zero/delay/run Tests``
        ``AsyncValidationCE try Tests``
        ``AsyncValidationCE using Tests``
        ``AsyncValidationCE loop Tests``
        ``AsyncValidationCE applicative tests``
    ]
