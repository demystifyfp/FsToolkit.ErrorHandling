module FsToolkit.ErrorHandling.IcedTasks.Tests.ValueTaskValueOptionCETests

open Expecto
open FsToolkit.ErrorHandling
open System.Threading.Tasks
open ConfigureAwaitTestHelpers

type private DynamicValueTaskValueOptionBuilder() =
    inherit ValueTaskValueOptionBuilderBase()

    member _.Run(code: ValueTaskValueOptionCode<'T, 'T>) =
        ValueTaskValueOptionBuilder.RunDynamic(code)

let private dynamicValueTaskValueOption = DynamicValueTaskValueOptionBuilder()

module TestFuncs =
    let testFunctionTO<'Dto> () =
        valueTaskValueOption {
            let dto = Unchecked.defaultof<'Dto>
            System.Console.WriteLine(dto)
        }

let ceTests =
    testList "ValueTaskValueOption CE" [
        testCaseTask "Return value"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return 42 }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueSome"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return! (ValueSome 42) }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone
                let! actual = valueTaskValueOption { return! ValueNone }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Async ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone
                let! actual = valueTaskValueOption { return! Async.result ValueNone }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Async"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return! Async.result 42 }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone
                let! actual = valueTaskValueOption { return! ValueTaskValueOption.none }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task Generic"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return! Task.result 42 }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom Task"
        <| fun () ->
            task {
                let expected = ValueSome()
                let! actual = valueTaskValueOption { return! Task.CompletedTask }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueTask Generic"
        <| fun () ->
            task {
                let expected = ValueSome 42
                let! actual = valueTaskValueOption { return! ValueTask.result 42 }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "ReturnFrom ValueTask"
        <| fun () ->
            task {
                let expected = ValueSome()
                let! actual = valueTaskValueOption { return! ValueTask.CompletedTask }
                Expect.equal actual expected "Should return value wrapped in voption"
            }
        testCaseTask "Bind ValueSome"
        <| fun () ->
            task {
                let expected = ValueSome 42

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueSome 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueNone
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Async ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone

                let! actual =
                    valueTaskValueOption {
                        let! value = Async.result ValueNone
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Async"
        <| fun () ->
            task {
                let expected = ValueSome 42

                let! actual =
                    valueTaskValueOption {
                        let! value = Async.result 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task ValueNone"
        <| fun () ->
            task {
                let expected = ValueNone

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueTaskValueOption.none
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task Generic"
        <| fun () ->
            task {
                let expected = ValueSome 42

                let! actual =
                    valueTaskValueOption {
                        let! value = Task.result 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind Task"
        <| fun () ->
            task {
                let expected = ValueSome()

                let! actual =
                    valueTaskValueOption {
                        let! value = Task.CompletedTask
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueTask Generic"
        <| fun () ->
            task {
                let expected = ValueSome 42

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueTask.result 42
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }
        testCaseTask "Bind ValueTask"
        <| fun () ->
            task {
                let expected = ValueSome()

                let! actual =
                    valueTaskValueOption {
                        let! value = ValueTask.CompletedTask
                        return value
                    }

                Expect.equal actual expected "Should bind value wrapped in voption"
            }

        testCaseTask "Task.Yield"
        <| fun () ->
            task {

                let! actual = valueTaskValueOption { do! Task.Yield() }

                Expect.equal actual (ValueSome()) "Should be ok"
            }
        testCaseTask "Zero/Combine/Delay/Run"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        let result = data

                        if true then
                            ()

                        return result
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "Try With"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        try
                            return data
                        with e ->
                            return raise e
                    }

                Expect.equal actual (ValueSome data) "Try with failed"
            }
        testCaseTask "Try Finally"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        try
                            return data
                        finally
                            ()
                    }

                Expect.equal actual (ValueSome data) "Try with failed"
            }
        testCaseTask "Using null"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        use d = null
                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }

        testCaseTask "use normal disposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    valueTaskValueOption {
                        use d = TestHelpers.makeDisposable (fun () -> isFinished <- true)
                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        testCaseTask "use! normal wrapped disposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    valueTaskValueOption {
                        use! d =
                            TestHelpers.makeDisposable (fun () -> isFinished <- true)
                            |> ValueSome

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
                Expect.isTrue isFinished ""
            }
        testCaseTask "use null disposable"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        use d = null
                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "use sync asyncdisposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    valueTaskValueOption {
                        use d =
                            TestHelpers.makeAsyncDisposable (
                                (fun () ->
                                    isFinished <- true
                                    ValueTask()
                                )
                            )

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
                Expect.isTrue isFinished ""
            }

        testCaseTask "use async asyncdisposable"
        <| fun () ->
            task {
                let data = 42
                let mutable isFinished = false

                let! actual =
                    valueTaskValueOption {
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

                Expect.equal actual (ValueSome data) "Should be ok"
                Expect.isTrue isFinished ""
            }
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
                            valueTaskValueOption {
                                while index < maxIndex do
                                    index <- index + 1

                                return data
                            }

                        Expect.equal index maxIndex "Index should reach maxIndex"
                        Expect.equal actual (ValueSome data) "Should be ok"
                    }
        ]

        testCaseTask "while bind error"
        <| fun () ->
            task {
                let items = [
                    ValueTaskValueOption.some 3
                    ValueTaskValueOption.some 4
                    ValueTaskValueOption.none
                ]

                let mutable index = 0

                let! actual =
                    valueTaskValueOption {
                        while index < items.Length do
                            let! _ = items[index]
                            index <- index + 1

                        return index
                    }

                Expect.equal
                    index
                    (items.Length
                     - 1)
                    "Index should reach maxIndex"

                Expect.equal actual ValueNone "Should be NOPE"
            }
        testCaseTask "while fail"
        <| fun () ->
            task {

                let mutable loopCount = 0
                let mutable wasCalled = false

                let sideEffect () =
                    wasCalled <- true
                    "ok"

                let expected = ValueNone

                let data = [
                    ValueSome "42"
                    ValueSome "1024"
                    expected
                    ValueSome "1M"
                    ValueSome "1M"
                    ValueSome "1M"
                ]

                let! actual =
                    valueTaskValueOption {
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
        testCaseTask "For in"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        for i in [ 1..10 ] do
                            ()

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "For to"
        <| fun () ->
            task {
                let data = 42

                let! actual =
                    valueTaskValueOption {
                        for i = 1 to 10 do
                            ()

                        return data
                    }

                Expect.equal actual (ValueSome data) "Should be ok"
            }
        testCaseTask "for in fail"
        <| fun () ->
            task {

                let mutable loopCount = 0
                let mutable wasCalled = false

                let sideEffect () =
                    wasCalled <- true
                    "ok"

                let expected = ValueNone

                let data = [
                    ValueSome "42"
                    ValueSome "1024"
                    expected
                    ValueSome "1M"
                    ValueSome "1M"
                    ValueSome "1M"
                ]

                let! actual =
                    valueTaskValueOption {
                        for i in data do
                            let! x = i

                            loopCount <-
                                loopCount
                                + 1

                            ()

                        return sideEffect ()
                    }

                Expect.equal loopCount 2 "Should only loop twice"
                Expect.equal actual expected "Should be an error"
                Expect.isFalse wasCalled "No additional side effects should occur"
            }
        testCaseTask "Empty result"
        <| fun () ->
            task {
                let! _ = TestFuncs.testFunctionTO ()
                ()
            }
    ]

let specialCaseTask returnValue =
#if NETSTANDARD2_0
    Unsafe.uply { return returnValue }
#else
    Task.FromResult returnValue
#endif

let ceTestsApplicative =
    testList "ValueTaskValueOptionCE applicative tests" [
        testCaseTask "Happy Path ValueOption/AsyncValueOption/Task/ValueTask"
        <| fun () ->
            task {
                let! actual =
                    valueTaskValueOption {
                        let! a = ValueSome 3

                        let! b =
                            ValueSome 1
                            |> Async.result

                        let! c = specialCaseTask (ValueSome 3)
                        let! d = ValueTask.result (ValueSome 5)

                        return
                            a + b
                            - c
                            - d
                    }

                Expect.equal actual (ValueSome -4) "Should be ok"
            }
        testCaseTask "Fail Path ValueOption/AsyncValueOption/Task/ValueTask"
        <| fun () ->
            task {
                let! actual =
                    valueTaskValueOption {
                        let! a = ValueSome 3

                        and! b =
                            ValueSome 1
                            |> Async.result

                        and! c = specialCaseTask ValueNone
                        and! d = ValueTaskValueOption.some 5

                        return
                            a + b
                            - c
                            - d
                    }

                Expect.equal actual ValueNone "Should be ok"
            }
    ]

let ``ValueTaskValueOptionCE while ValueNone async disposal Tests`` =
    testList "ValueTaskValueOptionCE while ValueNone async disposal Tests" [
        for isDynamic in
            [
                false
                true
            ] do
            for hasEarlierSuspension in
                [
                    false
                    true
                ] do
                testCaseTask
                <| sprintf
                    "%s execution, %s earlier suspension"
                    (if isDynamic then "dynamic" else "static")
                    (if hasEarlierSuspension then "with" else "without")
                <| fun () ->
                    TestHelpers.assertWhileShortCircuitWaitsForAsyncDisposal
                        ValueNone
                        hasEarlierSuspension
                        (fun earlierSuspension disposable ->
                            if isDynamic then
                                dynamicValueTaskValueOption {
                                    if hasEarlierSuspension then
                                        do! earlierSuspension

                                    use resource = disposable

                                    while true do
                                        do! ValueNone
                                }
                                |> _.AsTask()
                            else
                                valueTaskValueOption {
                                    if hasEarlierSuspension then
                                        do! earlierSuspension

                                    use resource = disposable

                                    while true do
                                        do! ValueNone
                                }
                                |> _.AsTask()
                        )
    ]

let ``ValueTaskValueOptionCE inference checks`` =
    testList "ValueTaskValueOptionCE inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = valueTaskValueOption { return! res }

            f (ValueTaskValueOption.some ())
            |> ignore
    ]

let ``ValueTaskValueOption helpers`` =
    testList "eitherBind" [
        testCaseTask "ValueSome"
        <| fun () ->
            task {
                let! actual =
                    valueTaskValueOption { return 5 }
                    |> ValueTaskValueOption.eitherBind ((+) 2) (fun () -> 42)

                Expect.equal actual 7 ""
            }
        testCaseTask "ValueNone"
        <| fun () ->
            task {
                let! actual =
                    valueTaskValueOption { return! ValueNone }
                    |> ValueTaskValueOption.eitherBind ((+) 2) (fun () -> 42)

                Expect.equal actual 42 ""
            }
    ]

let ``ValueTaskValueOptionCE async disposal outcome Tests`` =
    let useStatic
        (body: TestHelpers.AsyncDisposalBody)
        (disposable: System.IAsyncDisposable)
        : Task<unit voption> =
        valueTaskValueOption {
            use _resource = disposable

            match body with
            | TestHelpers.BodyThrows -> raise (System.ArgumentException "body")
            | TestHelpers.BodyShortCircuits -> return! ValueNone
            | TestHelpers.BodyCompletes -> return ()
        }
        |> _.AsTask()

    let useDynamic
        (body: TestHelpers.AsyncDisposalBody)
        (disposable: System.IAsyncDisposable)
        : Task<unit voption> =
        dynamicValueTaskValueOption {
            use _resource = disposable

            match body with
            | TestHelpers.BodyThrows -> raise (System.ArgumentException "body")
            | TestHelpers.BodyShortCircuits -> return! ValueNone
            | TestHelpers.BodyCompletes -> return ()
        }
        |> _.AsTask()

    TestHelpers.asyncDisposalOutcomeTests
        "ValueTaskValueOptionCE async disposal outcome Tests"
        useStatic
        useDynamic

let ``valueTaskValueOption ConfigureAwait Tests`` =
    let case name expected start =
        contextCase name (ValueSome expected) start

    testList "valueTaskValueOption ConfigureAwait in let!, do!, return!, and and!" [
        yield!
            sourceCases
                case
                (fun configure context gate ->
                    (valueTaskValueOption {
                        let! () = gate.ConfigureAwait(configure)

                        if onContext context then return true else return false
                    })
                        .AsTask()
                )
                (fun configure context gate ->
                    (valueTaskValueOption {
                        let! () = gate.ConfigureAwait(configure)
                        return onContext context
                    })
                        .AsTask()
                )
                (fun configure context gate ->
                    (valueTaskValueOption {
                        let! () = ValueTask<unit>(gate).ConfigureAwait(configure)
                        return onContext context
                    })
                        .AsTask()
                )
                (fun configure context gate ->
                    (valueTaskValueOption {
                        do! (gate :> Task).ConfigureAwait(configure)
                        return onContext context
                    })
                        .AsTask()
                )

        valueCase "return! of a ConfiguredTaskAwaitable<'T>" (ValueSome 1)
        <| fun () ->
            (valueTaskValueOption { return! Task.FromResult(1).ConfigureAwait(false) }).AsTask()

        valueCase
            "let! of a ConfiguredTaskAwaitable of an option binds the whole option"
            (ValueSome(ValueSome 1))
        <| fun () ->
            (valueTaskValueOption {
                let! x = Task.FromResult(ValueSome 1).ConfigureAwait(false)
                return x
            })
                .AsTask()

        valueCase "and! of ConfiguredTaskAwaitable<'T> sources keeps the order" (ValueSome(1, 2))
        <| fun () ->
            (valueTaskValueOption {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            })
                .AsTask()

        case "and! of ConfiguredTaskAwaitable<'T> sources with ConfigureAwait(false)" true
        <| fun context gate ->
            (valueTaskValueOption {
                let! () = gate.ConfigureAwait(false)
                and! () = Task.FromResult(()).ConfigureAwait(false)
                return onContext context
            })
                .AsTask()

        yield!
            failureCases
                (fun gate ->
                    (valueTaskValueOption {
                        let! () = gate.ConfigureAwait(false)
                        return "done"
                    })
                        .AsTask()
                )
                (fun local gate ->
                    (valueTaskValueOption {
                        let! () = gate.ConfigureAwait(false)
                        return local.Value
                    })
                        .AsTask()
                )
                (ValueSome "caller")

        for continueOnCapturedContext in
            [
                false
                true
            ] do
            case
                $"let! of a ConfiguredTaskAwaitable<'T> with ConfigureAwait(%b{continueOnCapturedContext}) in the dynamic state machine"
                continueOnCapturedContext
            <| fun context gate ->
                (dynamicValueTaskValueOption {
                    let! () = gate.ConfigureAwait(continueOnCapturedContext)

                    if onContext context then return true else return false
                })
                    .AsTask()

        valueCase
            "and! of a ConfiguredTaskAwaitable<'T> and a source of the builder's type keeps the order"
            (ValueSome(1, 2))
        <| fun () ->
            (valueTaskValueOption {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = ValueTask<_>(ValueSome 2)
                return a, b
            })
                .AsTask()

        valueCase
            "and! of a source of the builder's type and a ConfiguredTaskAwaitable<'T> keeps the order"
            (ValueSome(1, 2))
        <| fun () ->
            (valueTaskValueOption {
                let! a = ValueTask<_>(ValueSome 1)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            })
                .AsTask()

        valueCase
            "and! gets the awaiter of the left source one time, after it evaluates the right source"
            (ValueSome(0, 1))
        <| fun () ->
            let calls = ref 0

            (valueTaskValueOption {
                let! () =
                    GetAwaiterProbe(fun () ->
                        calls.Value <-
                            calls.Value
                            + 1
                    )

                and! seen = Task.FromResult(calls.Value)
                return seen, calls.Value
            })
                .AsTask()

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time"
            (ValueSome 2)
        <| fun source ->
            (valueTaskValueOption {
                let! x = source
                return x + 1
            })
                .AsTask()

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time in the dynamic state machine"
            (ValueSome 2)
        <| fun source ->
            (dynamicValueTaskValueOption {
                let! x = source
                return x + 1
            })
                .AsTask()

        leftWatchCase
            "and! gets the awaiter of the left source after it evaluates the right source, as in task"
            (ValueSome "")
        <| fun watch right ->
            (valueTaskValueOption {
                let! _ = watch
                and! b = right ()
                return b
            })
                .AsTask()

        leftFaultCase "and! keeps the fault of the left source when the right GetAwaiter throws"
        <| fun left right ->
            (valueTaskValueOption {
                let! a = left.ConfigureAwait(false)
                and! b = right
                return a + b
            })
                .AsTask()
    ]


[<Tests>]
let allTests =
    testList "ValueTaskValueOption CE Tests" [
        ``valueTaskValueOption ConfigureAwait Tests``
        ceTests
        ceTestsApplicative
        ``ValueTaskValueOptionCE while ValueNone async disposal Tests``
        ``ValueTaskValueOptionCE async disposal outcome Tests``
        ``ValueTaskValueOptionCE inference checks``
        ``ValueTaskValueOption helpers``
    ]
