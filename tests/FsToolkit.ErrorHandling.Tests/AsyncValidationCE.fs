module AsyncValidationCETests


#if FABLE_COMPILER
open Fable.Mocha
#else
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
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]

let ``AsyncValidationCE return! Tests`` =
    testList "AsyncValidationCE return! Tests" [
        testCaseAsync "Return Ok result"
        <| async {
            let data = Result.Ok "Foo"
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
            Expect.equal actual (Result.Ok innerData) "Should be ok"
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
            let! data = AsyncValidation.ok innerData
            let! actual = asyncValidation { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Return Error Validation"
        <| async {
            let innerData = "Foo"
            let! expected = AsyncValidation.error innerData
            let data = AsyncValidation.error innerData
            let! actual = asyncValidation { return! data }
            Expect.equal actual expected "Should be ok"
        }
    ]


let ``AsyncValidationCE bind Tests`` =
    testList "AsyncValidationCE bind Tests" [
        testCaseAsync "let! Ok result"
        <| async {
            let data = Result.Ok "Foo"

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

            Expect.equal actual (Result.Ok innerData) "Should be ok"
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
            let! data = AsyncValidation.ok innerData

            let! actual =
                asyncValidation {
                    let! f = data
                    return f
                }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "let! Error Validation"
        <| async {
            let innerData = "Foo"
            let data = AsyncValidation.error innerData
            let! expected = AsyncValidation.error innerData

            let! actual =
                asyncValidation {
                    let! f = data
                    return f
                }

            Expect.equal actual expected "Should be ok"
        }
        testCaseAsync "do! Ok result"
        <| async {
            let data = Result.Ok()
            let! actual = asyncValidation { do! data }
            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "do! Error result"
        <| async {
            let innerData = ()
            let! expected = AsyncValidation.error innerData
            let data = Result.Error innerData
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

            Expect.equal actual (Result.Ok data) "Should be ok"
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

            Expect.equal actual (Result.Ok data) "Should be ok"
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

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }

let ``AsyncValidationCE using Tests`` =
    testList "AsyncValidationCE using Tests" [
        testCaseAsync "use normal disposable"
        <| async {
            let data = 42

            let! actual =
                asyncValidation {
                    use d = makeDisposable ()
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "use! normal wrapped disposable"
        <| async {
            let data = 42

            let! actual =
                asyncValidation {
                    use! d =
                        makeDisposable ()
                        |> Result.Ok

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "use null disposable"
        <| async {
            let data = 42

            let! actual =
                asyncValidation {
                    use d = null
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        }
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

            Expect.equal actual (Result.Ok data) "Should be ok"
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

            Expect.equal actual (Result.Ok data) "Should be ok"
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

        testCaseAsync "Happy Path Result/Valiation"
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

        testCaseAsync "Happy Path Result/Choice/Validation"
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
