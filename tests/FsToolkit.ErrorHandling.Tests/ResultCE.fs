module ResultCETests


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


let ``ResultCE return Tests`` =
    testList "ResultCE return Tests" [
        testCase "Return string"
        <| fun _ ->
            let data = "Foo"
            let actual = result { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
    ]


let ``ResultCE return! Tests`` =
    testList "ResultCE return! Tests" [
        testCase "Return Ok result"
        <| fun _ ->
            let data = Result.Ok "Foo"
            let actual = result { return! data }
            Expect.equal actual (data) "Should be ok"
        testCase "Return Error result"
        <| fun _ ->
            let data = Result.Error "Foo"
            let actual = result { return! data }
            Expect.equal actual (data) "Should be ok"
        testCase "Return Ok Choice"
        <| fun _ ->
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let actual = result { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        testCase "Return Error Choice"
        <| fun _ ->
            let innerData = "Foo"
            let data = Choice2Of2 innerData
            let actual = result { return! data }
            Expect.equal actual (Result.Error innerData) "Should be ok"
        testCase "Non-annotated overload resolution"
        <| fun _ ->
            let f res = result { return! res }

            f (Ok())
            |> ignore
    ]


let ``ResultCE bind Tests`` =
    testList "ResultCE bind Tests" [
        testCase "let! Ok result"
        <| fun _ ->
            let data = Result.Ok "Foo"

            let actual =
                result {
                    let! f = data
                    return f
                }

            Expect.equal actual (data) "Should be ok"
        testCase "let! Error result"
        <| fun _ ->
            let data = Result.Error "Foo"

            let actual =
                result {
                    let! f = data
                    return f
                }

            Expect.equal actual (data) "Should be ok"
        testCase "let! Ok Choice"
        <| fun _ ->
            let innerData = "Foo"
            let data = Choice1Of2 innerData

            let actual =
                result {
                    let! f = data
                    return f
                }

            Expect.equal actual (Result.Ok innerData) "Should be ok"
        testCase "let! Error Choice"
        <| fun _ ->
            let innerData = "Foo"
            let data = Choice2Of2 innerData

            let actual =
                result {
                    let! f = data
                    return f
                }

            Expect.equal actual (Result.Error innerData) "Should be ok"
        testCase "do! Ok result"
        <| fun _ ->
            let data = Result.Ok()
            let actual = result { do! data }
            Expect.equal actual (data) "Should be ok"
        testCase "do! Error result"
        <| fun _ ->
            let data = Result.Error()
            let actual = result { do! data }
            Expect.equal actual (data) "Should be ok"
        testCase "do! Ok Choice"
        <| fun _ ->
            let innerData = ()
            let data = Choice1Of2 innerData
            let actual = result { do! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        testCase "do! Error Choice"
        <| fun _ ->
            let innerData = ()
            let data = Choice2Of2 innerData
            let actual = result { do! data }
            Expect.equal actual (Result.Error innerData) "Should be ok"
        testCase "Non-annotated overload resolution"
        <| fun _ ->
            let f res = result { do! res }

            f (Ok())
            |> ignore
    ]


let ``ResultCE combine/zero/delay/run Tests`` =
    testList "ResultCE combine/zero/delay/run Tests" [
        testCase "Zero/Combine/Delay/Run"
        <| fun () ->
            let data = 42

            let actual =
                result {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
    ]


let ``ResultCE try Tests`` =
    testList "ResultCE try Tests" [
        testCase "Try With"
        <| fun () ->
            let data = 42

            let actual =
                result {
                    let data = data

                    try
                        ()
                    with _ ->
                        ()

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "Try Finally"
        <| fun () ->
            let data = 42

            let actual =
                result {
                    let data = data

                    try
                        ()
                    finally
                        ()

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
    ]

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }


let ``ResultCE using Tests`` =
    testList "ResultCE using Tests" [
        testCase "use normal disposable"
        <| fun () ->
            let data = 42

            let actual =
                result {
                    use d = makeDisposable ()
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "use! normal wrapped disposable"
        <| fun () ->
            let data = 42

            let actual =
                result {
                    use! d =
                        makeDisposable ()
                        |> Result.Ok

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "use null disposable"
        <| fun () ->
            let data = 42

            let actual =
                result {
                    use d = null
                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
    ]


let ``ResultCE loop Tests`` =
    testList "ResultCE loop Tests" [
        yield! [
            let maxIndices = [
                10
                10000
                1000000
            ]

            for maxIndex in maxIndices do
                testCase
                <| sprintf "While - %i" maxIndex
                <| fun () ->
                    let data = 42
                    let mutable index = 0

                    let actual =
                        result {
                            while index < maxIndex do
                                index <- index + 1

                            return data
                        }

                    Expect.equal index (maxIndex) "Index should reach maxIndex"
                    Expect.equal actual (Ok data) "Should be ok"
        ]

        testCase "while fail"
        <| fun () ->

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

            let actual =
                result {
                    while loopCount < data.Length do
                        let! x = data.[loopCount]

                        loopCount <-
                            loopCount
                            + 1

                    return sideEffect ()
                }

            Expect.equal loopCount 2 "Should only loop twice"
            Expect.equal actual (expected) "Should be an error"
            Expect.isFalse wasCalled "No additional side effects should occur"

        testCase "for in"
        <| fun () ->
            let data = 42

            let actual =
                result {
                    for i in [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "for to"
        <| fun () ->
            let data = 42

            let actual =
                result {
                    for i = 1 to 10 do
                        ()

                    return data
                }

            Expect.equal actual (Result.Ok data) "Should be ok"
    ]


let ``ResultCE inference checks`` =
    testList "ResultCE Inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = result { return! res }

            f (Ok())
            |> ignore
    ]

let allTests =
    testList "Result CE Tests" [
        ``ResultCE return Tests``
        ``ResultCE return! Tests``
        ``ResultCE bind Tests``
        ``ResultCE combine/zero/delay/run Tests``
        ``ResultCE try Tests``
        ``ResultCE using Tests``
        ``ResultCE loop Tests``
        ``ResultCE inference checks``
    ]
