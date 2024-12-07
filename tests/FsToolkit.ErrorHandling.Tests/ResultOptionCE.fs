module ResultOptionCETests


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
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

let ``ResultOptionCE return Tests`` =
    testList "ResultOptionCE return Tests" [
        testList "return" [
            testCase "string"
            <| fun _ ->
                let data = 1
                let actual = resultOption { return data }
                Expect.equal actual (Ok(Some data)) "Should be ok some"

            testCase "unit"
            <| fun _ ->
                let data = ()
                let actual = resultOption { return data }
                Expect.equal actual (Ok(Some data)) "Should be ok some"
        ]

        testList "return!" [
            testCase "Ok result"
            <| fun _ ->
                let data = "Foo"
                let actual = resultOption { return! Ok data }

                Expect.equal actual (Ok(Some data)) "Should be ok some"

            testCase "Error result"
            <| fun _ ->
                let data = Error "Foo"
                let actual = resultOption { return! data }
                Expect.equal actual (data) "Should be error"

            testCase "Option Some"
            <| fun _ ->
                let data = Some "Foo"
                let actual = resultOption { return! data }
                Expect.equal actual (Ok data) "Should be ok some"

            testCase "Option None"
            <| fun _ ->
                let data = None
                let actual = resultOption { return! data }
                Expect.equal actual (Ok data) "Should be ok none"

            testCase "Ok Choice"
            <| fun _ ->
                let innerData = "Foo"
                let data = Choice1Of2 innerData
                let actual = resultOption { return! data }

                Expect.equal actual (Ok(Some innerData)) "Should be ok some"

            testCase "Error Choice"
            <| fun _ ->
                let innerData = "Foo"
                let data = Choice2Of2 innerData
                let actual = resultOption { return! data }
                Expect.equal actual (Error innerData) "Should be error"

            testCase "Non-annotated overload resolution"
            <| fun _ ->
                let f res = resultOption { return! res }

                f (Ok(Some()))
                |> ignore
        ]
    ]

let ``ResultOptionCE bind Tests`` =
    testList "ResultOptionCE bind Tests" [
        testList "let!" [
            testCase "Ok result"
            <| fun _ ->
                let data = "Foo"

                let actual =
                    resultOption {
                        let! f = Ok data
                        return f
                    }

                Expect.equal actual (Ok(Some data)) "Should be ok some"

            testCase "Error result"
            <| fun _ ->
                let data = Error "Foo"

                let actual =
                    resultOption {
                        let! f = data
                        return f
                    }

                Expect.equal actual (data) "Should be error"

            testCase "Option Some"
            <| fun _ ->
                let data = Some "Foo"

                let actual =
                    resultOption {
                        let! f = data
                        return f
                    }

                Expect.equal actual (Ok data) "Should be ok some"

            testCase "Option None"
            <| fun _ ->
                let data = None

                let actual =
                    resultOption {
                        let! f = data
                        return f
                    }

                Expect.equal actual (Ok data) "Should be ok none"

            testCase "Ok Choice"
            <| fun _ ->
                let innerData = "Foo"
                let data = Choice1Of2 innerData

                let actual =
                    resultOption {
                        let! f = data
                        return f
                    }

                Expect.equal actual (Ok(Some innerData)) "Should be ok some"

            testCase "Error Choice"
            <| fun _ ->
                let innerData = "Foo"
                let data = Choice2Of2 innerData

                let actual =
                    resultOption {
                        let! f = data
                        return f
                    }

                Expect.equal actual (Error innerData) "Should be ok"
        ]

        testList "do!" [
            testCase "Ok result"
            <| fun _ ->
                let data = ()
                let actual = resultOption { do! Ok data }

                Expect.equal actual (Ok(Some data)) "Should be ok some"

            testCase "Error result"
            <| fun _ ->
                let data = Error()
                let actual = resultOption { do! data }
                Expect.equal actual (data) "Should be ok"

            testCase "Option some"
            <| fun _ ->
                let data = ()
                let actual = resultOption { do! Some data }

                Expect.equal actual (Ok(Some data)) "Should be ok some"

            testCase "Option none"
            <| fun _ ->
                let data = ()
                let actual = resultOption { do! None }

                Expect.equal actual (Ok None) "Should be ok none"

            testCase "Ok Choice"
            <| fun _ ->
                let innerData = ()
                let data = Choice1Of2 innerData
                let actual = resultOption { do! data }
                Expect.equal actual (Ok(Some innerData)) "Should be ok"

            testCase "Error Choice"
            <| fun _ ->
                let innerData = ()
                let data = Choice2Of2 innerData
                let actual = resultOption { do! data }
                Expect.equal actual (Error innerData) "Should be ok"

            testCase "Non-annotated overload resolution"
            <| fun _ ->
                let f res = resultOption { do! res }

                f (Ok(Some()))
                |> ignore
        ]
    ]


let ``ResultOptionCE combine/zero/delay/run Tests`` =
    testList "ResultOptionCE combine/zero/delay/run Tests" [
        testCase "Zero/Combine/Delay/Run"
        <| fun () ->
            let data = 42

            let actual =
                resultOption {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (Ok(Some data)) "Should be ok some"
    ]


let ``ResultOptionCE try Tests`` =
    testList "ResultOptionCE try Tests" [
        testCase "Try With"
        <| fun () ->
            let data = 42

            let actual =
                resultOption {
                    let data = data

                    try
                        ()
                    with _ ->
                        ()

                    return data
                }

            Expect.equal actual (Ok(Some data)) "Should be ok some"
        testCase "Try Finally"
        <| fun () ->
            let data = 42

            let actual =
                resultOption {
                    let data = data

                    try
                        ()
                    finally
                        ()

                    return data
                }

            Expect.equal actual (Ok(Some data)) "Should be ok some"
    ]

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }

let ``ResultOptionCE using Tests`` =
    testList "ResultOptionCE using Tests" [
        testList "use" [
            testCase "normal disposable"
            <| fun () ->
                let data = 42

                let actual =
                    resultOption {
                        use d = makeDisposable ()
                        return data
                    }

                Expect.equal actual (Ok(Some data)) "Should be ok some"

            testCase "null disposable"
            <| fun () ->
                let data = 42

                let actual =
                    resultOption {
                        use d = null
                        return data
                    }

                Expect.equal actual (Ok(Some data)) "Should be ok some"
        ]

        testList "use!" [
            testCase "normal wrapped disposable"
            <| fun () ->
                let data = 42

                let actual =
                    resultOption {
                        use! d =
                            makeDisposable ()
                            |> Ok

                        return data
                    }

                Expect.equal actual (Ok(Some data)) "Should be ok some"
        ]
    ]

let ``ResultOptionCE loop Tests`` =
    testList "ResultOptionCE loop Tests" [
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
                        resultOption {
                            while index < maxIndex do
                                index <- index + 1

                            return data
                        }

                    Expect.equal index (maxIndex) "Index should reach maxIndex"
                    Expect.equal actual (Ok(Some data)) "Should be ok some"
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
                resultOption {
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
                resultOption {
                    for i in [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (Ok(Some data)) "Should be ok some"
        testCase "for to"
        <| fun () ->
            let data = 42

            let actual =
                resultOption {
                    for i = 1 to 10 do
                        ()

                    return data
                }

            Expect.equal actual (Ok(Some data)) "Should be ok some"
    ]


let ``ResultOptionCE inference checks`` =
    testList "ResultOptionCE Inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = resultOption { return! res }

            f (Ok(Some()))
            |> ignore
    ]


let allTests =
    testList "Result CE Tests" [
        ``ResultOptionCE return Tests``
        ``ResultOptionCE bind Tests``
        ``ResultOptionCE combine/zero/delay/run Tests``
        ``ResultOptionCE try Tests``
        ``ResultOptionCE using Tests``
        ``ResultOptionCE loop Tests``
        ``ResultOptionCE inference checks``
    ]
