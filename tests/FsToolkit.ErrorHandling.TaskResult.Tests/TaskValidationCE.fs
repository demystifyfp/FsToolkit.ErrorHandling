module TaskValidationCETests

open Expecto

open TestHelpers
open FsToolkit.ErrorHandling

let ``TaskValidationCE return Tests`` =
    testList "TaskValidationCE return Tests" [
        testCaseTask "Return string"
        <| fun () ->
            task {
                let data = "Foo"
                let! actual = taskValidation { return data }
                Expect.equal actual (Ok data) "Should be ok"
            }
    ]

let ``TaskValidationCE return! Tests`` =
    testList "TaskValidationCE return! Tests" [
        testCaseTask "Return Ok result"
        <| fun () ->
            task {
                let data = Ok "Foo"
                let! actual = taskValidation { return! data }
                Expect.equal actual (data) "Should be ok"
            }
        testCaseTask "Return Error result"
        <| fun () ->
            task {
                let innerData = "Foo"
                let! expected = TaskValidation.error innerData
                let data = Error innerData
                let! actual = taskValidation { return! data }
                Expect.equal actual expected "Should be error"
            }
        testCaseTask "Return Ok Choice"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Choice1Of2 innerData
                let! actual = taskValidation { return! data }
                Expect.equal actual (Ok innerData) "Should be ok"
            }
        testCaseTask "Return Error Choice"
        <| fun () ->
            task {
                let innerData = "Foo"
                let! expected = TaskValidation.error innerData
                let data = Choice2Of2 innerData
                let! actual = taskValidation { return! data }
                Expect.equal actual expected "Should be error"
            }
        testCaseTask "Return Ok Validation"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Validation.ok innerData
                let! actual = taskValidation { return! data }
                Expect.equal actual (Ok innerData) "Should be ok"
            }
        testCaseTask "Return Error Validation"
        <| fun () ->
            task {
                let innerData = "Foo"
                let expected = Validation.error innerData
                let data = TaskValidation.error innerData
                let! actual = taskValidation { return! data }
                Expect.equal actual expected "Should be ok"
            }
    ]


let ``TaskValidationCE bind Tests`` =
    testList "TaskValidationCE bind Tests" [
        testCaseTask "let! Task"
        <| fun () ->
            task {
                let data = "Foo"

                let! actual =
                    taskValidation {
                        let! f = task { return data }
                        return f
                    }

                Expect.equal actual (Ok data) "Should be ok"
            }
        testCaseTask "let! Ok result"
        <| fun () ->
            task {
                let data = Ok "Foo"

                let! actual =
                    taskValidation {
                        let! f = data
                        return f
                    }

                Expect.equal actual (data) "Should be ok"
            }
        testCaseTask "let! Error result"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Error innerData
                let! expected = TaskValidation.error innerData

                let! actual =
                    taskValidation {
                        let! f = data
                        return f
                    }

                Expect.equal actual expected "Should be ok"
            }
        testCaseTask "let! Ok Choice"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Choice1Of2 innerData

                let! actual =
                    taskValidation {
                        let! f = data
                        return f
                    }

                Expect.equal actual (Ok innerData) "Should be ok"
            }
        testCaseTask "let! Error Choice"
        <| fun () ->
            task {
                let innerData = "Foo"
                let data = Choice2Of2 innerData
                let! expected = TaskValidation.error innerData

                let! actual =
                    taskValidation {
                        let! f = data
                        return f
                    }

                Expect.equal actual expected "Should be ok"
            }
        testCaseTask "let! Ok Validation"
        <| fun () ->
            task {
                let innerData = "Foo"

                let! actual =
                    taskValidation {
                        let! f = validation { return innerData }
                        return f
                    }

                Expect.equal actual (Ok innerData) "Should be ok"
            }
        testCaseTask "let! Error Validation"
        <| fun () ->
            task {
                let innerData = "Foo"
                let expected = Error [ innerData ]

                let! actual =
                    taskValidation {
                        let! f = validation { return! expected }
                        return f
                    }

                Expect.equal actual expected "Should be ok"
            }
        testCaseTask "do! Ok result"
        <| fun () ->
            task {
                let data = Ok()
                let! actual = taskValidation { do! data }
                Expect.equal actual (data) "Should be ok"
            }
        testCaseTask "do! Error result"
        <| fun () ->
            task {
                let innerData = ()
                let data = Error innerData
                let! expected = TaskValidation.error innerData
                let! actual = taskValidation { do! data }
                Expect.equal actual expected "Should be error"
            }
        testCaseTask "do! Ok Choice"
        <| fun () ->
            task {
                let innerData = ()
                let! expected = TaskValidation.ok innerData
                let data = Choice1Of2 innerData
                let! actual = taskValidation { do! data }
                Expect.equal actual expected "Should be ok"
            }
        testCaseTask "do! Error Choice"
        <| fun () ->
            task {
                let innerData = ()
                let! expected = TaskValidation.error innerData
                let data = Choice2Of2 innerData
                let! actual = taskValidation { do! data }
                Expect.equal actual expected "Should be error"
            }
        testCaseTask "do! Ok Validation"
        <| fun () ->
            task {
                let innerData = ()
                let! expected = TaskValidation.ok innerData
                let data = TaskValidation.ok innerData
                let! actual = taskValidation { do! data }
                Expect.equal actual expected "Should be ok"
            }
        testCaseTask "do! Error Validation"
        <| fun () ->
            task {
                let innerData = ()
                let! expected = TaskValidation.error innerData
                let data = TaskValidation.error innerData
                let! actual = taskValidation { do! data }
                Expect.equal actual expected "Should be error"
            }
    ]

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

                Expect.equal actual (Ok data) "Should be ok"
            }
    ]


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

                Expect.equal actual (Ok data) "Should be ok"
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

                Expect.equal actual (Ok data) "Should be ok"
            }
    ]

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }

let ``TaskValidationCE using Tests`` =
    testList "TaskValidationCE using Tests" [
        testCaseTask "use normal disposable"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskValidation {
                        use d = makeDisposable ()
                        return data
                    }

                Expect.equal actual (Ok data) "Should be ok"
            }
        testCaseTask "use! normal wrapped disposable"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    taskValidation {
                        use! d =
                            makeDisposable ()
                            |> Ok

                        return data
                    }

                Expect.equal actual (Ok data) "Should be ok"
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

                Expect.equal actual (Ok data) "Should be ok"
            }
    ]

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
                        Expect.equal actual (Ok data) "Should be ok"
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
                    taskValidation {
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

                Expect.equal actual (Ok data) "Should be ok"
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

                Expect.equal actual (Ok data) "Should be ok"
            }
    ]

let ``TaskValidationCE applicative tests`` =
    testList "TaskValidationCE applicative tests" [
        testCaseTask "Happy Path Result"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = Ok 3
                        and! b = Ok 2
                        and! c = Ok 1
                        return a + b - c
                    }

                Expect.equal actual (Ok 4) "Should be ok"
            }
        testCaseTask "Happy Path Valiation"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = TaskValidation.ok 3
                        and! b = TaskValidation.ok 2
                        and! c = TaskValidation.ok 1
                        return a + b - c
                    }

                Expect.equal actual (Ok 4) "Should be ok"
            }

        testCaseTask "Happy Path Result/Valiation"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = TaskValidation.ok 3
                        and! b = Ok 2
                        and! c = TaskValidation.ok 1
                        return a + b - c
                    }

                Expect.equal actual (Ok 4) "Should be ok"
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

                Expect.equal actual (Ok 4) "Should be ok"
            }

        testCaseTask "Happy Path Result/Choice/Validation"
        <| fun () ->
            task {
                let! actual =
                    taskValidation {
                        let! a = Ok 3
                        and! b = Choice1Of2 2
                        and! c = TaskValidation.ok 1
                        return a + b - c
                    }

                Expect.equal actual (Ok 4) "Should be ok"
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
                let expected = TaskValidation.error "TryParse failure"
                let! expected' = expected

                let! actual =
                    taskValidation {
                        let! a = TaskValidation.ok 3
                        and! b = TaskValidation.ok 2
                        and! c = expected
                        return a + b - c
                    }

                Expect.equal actual expected' "Should be Error"
            }
    ]

let allTests =
    testList "Validation CE Tests" [
        ``TaskValidationCE return Tests``
        ``TaskValidationCE return! Tests``
        ``TaskValidationCE bind Tests``
        ``TaskValidationCE combine/zero/delay/run Tests``
        ``TaskValidationCE try Tests``
        ``TaskValidationCE using Tests``
        ``TaskValidationCE loop Tests``
        ``TaskValidationCE applicative tests``
    ]
