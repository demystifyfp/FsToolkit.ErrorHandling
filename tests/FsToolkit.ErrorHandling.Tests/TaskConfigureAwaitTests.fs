module TaskConfigureAwaitTests

open Expecto
open FsToolkit.ErrorHandling
open ConfigureAwaitTestHelpers
open System
open System.Threading
open System.Threading.Tasks

let private taskResultTests =
    let case name expected start =
        contextCase name (Ok expected: Result<bool, string>) start

    testList "taskResult" [
        yield!
            sourceCases
                case
                (fun configure context gate ->
                    taskResult {
                        let! () = gate.ConfigureAwait(configure)

                        if onContext context then return true else return false
                    }
                )
                (fun configure context gate ->
                    taskResult {
                        let! () = gate.ConfigureAwait(configure)
                        return onContext context
                    }
                )
                (fun configure context gate ->
                    taskResult {
                        let! () = ValueTask<unit>(gate).ConfigureAwait(configure)
                        return onContext context
                    }
                )
                (fun configure context gate ->
                    taskResult {
                        do! (gate :> Task).ConfigureAwait(configure)
                        return onContext context
                    }
                )

        valueCase "return! of a ConfiguredTaskAwaitable<'T>" (Ok 1: Result<int, string>)
        <| fun () -> taskResult { return! Task.FromResult(1).ConfigureAwait(false) }

        valueCase
            "and! of ConfiguredTaskAwaitable<'T> sources keeps the order"
            (Ok(1, 2): Result<_, string>)
        <| fun () ->
            taskResult {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            }

        case "and! of ConfiguredTaskAwaitable<'T> sources with ConfigureAwait(false)" true
        <| fun context gate ->
            taskResult {
                let! () = gate.ConfigureAwait(false)
                and! () = Task.FromResult(()).ConfigureAwait(false)
                return onContext context
            }

        yield!
            failureCases
                (fun gate ->
                    taskResult {
                        let! () = gate.ConfigureAwait(false)
                        return "done"
                    }
                )
                (fun local gate ->
                    taskResult {
                        let! () = gate.ConfigureAwait(false)
                        return local.Value
                    }
                )
                (Ok "caller": Result<string, string>)

        for continueOnCapturedContext in
            [
                false
                true
            ] do
            case
                $"let! of a ConfiguredTaskAwaitable<'T> with ConfigureAwait(%b{continueOnCapturedContext}) in the dynamic state machine"
                continueOnCapturedContext
            <| fun context gate ->
                TestHelpers.dynamicTaskResult {
                    let! () = gate.ConfigureAwait(continueOnCapturedContext)

                    if onContext context then return true else return false
                }

        valueCase
            "and! of a ConfiguredTaskAwaitable<'T> and a source of the builder's type keeps the order"
            (Ok(1, 2): Result<_, string>)
        <| fun () ->
            taskResult {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(Ok 2: Result<int, string>)
                return a, b
            }

        valueCase
            "and! of a source of the builder's type and a ConfiguredTaskAwaitable<'T> keeps the order"
            (Ok(1, 2): Result<_, string>)
        <| fun () ->
            taskResult {
                let! a = Task.FromResult(Ok 1: Result<int, string>)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            }

        valueCase
            "and! gets the awaiter of the left source before it evaluates the right source"
            (Ok true: Result<_, string>)
        <| fun () ->
            let started = ref false

            taskResult {
                let! () = GetAwaiterProbe(fun () -> started.Value <- true)
                and! seen = Task.FromResult(started.Value)
                return seen
            }

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time"
            (Ok 2: Result<_, string>)
        <| fun source ->
            taskResult {
                let! x = source
                return x + 1
            }

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time in the dynamic state machine"
            (Ok 2: Result<_, string>)
        <| fun source ->
            TestHelpers.dynamicTaskResult {
                let! x = source
                return x + 1
            }
    ]

let private taskValidationTests =
    let case name expected start =
        contextCase name (Ok expected: Validation<bool, string>) start

    testList "taskValidation" [
        yield!
            sourceCases
                case
                (fun configure context gate ->
                    taskValidation {
                        let! () = gate.ConfigureAwait(configure)

                        if onContext context then return true else return false
                    }
                )
                (fun configure context gate ->
                    taskValidation {
                        let! () = gate.ConfigureAwait(configure)
                        return onContext context
                    }
                )
                (fun configure context gate ->
                    taskValidation {
                        let! () = ValueTask<unit>(gate).ConfigureAwait(configure)
                        return onContext context
                    }
                )
                (fun configure context gate ->
                    taskValidation {
                        do! (gate :> Task).ConfigureAwait(configure)
                        return onContext context
                    }
                )

        valueCase "return! of a ConfiguredTaskAwaitable<'T>" (Ok 1: Validation<int, string>)
        <| fun () -> taskValidation { return! Task.FromResult(1).ConfigureAwait(false) }

        valueCase
            "and! of ConfiguredTaskAwaitable<'T> sources keeps the order"
            (Ok(1, 2): Validation<_, string>)
        <| fun () ->
            taskValidation {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            }

        case "and! of ConfiguredTaskAwaitable<'T> sources with ConfigureAwait(false)" true
        <| fun context gate ->
            taskValidation {
                let! () = gate.ConfigureAwait(false)
                and! () = Task.FromResult(()).ConfigureAwait(false)
                return onContext context
            }

        yield!
            failureCases
                (fun gate ->
                    taskValidation {
                        let! () = gate.ConfigureAwait(false)
                        return "done"
                    }
                )
                (fun local gate ->
                    taskValidation {
                        let! () = gate.ConfigureAwait(false)
                        return local.Value
                    }
                )
                (Ok "caller": Validation<string, string>)

        for continueOnCapturedContext in
            [
                false
                true
            ] do
            case
                $"let! of a ConfiguredTaskAwaitable<'T> with ConfigureAwait(%b{continueOnCapturedContext}) in the dynamic state machine"
                continueOnCapturedContext
            <| fun context gate ->
                TestHelpers.dynamicTaskValidation {
                    let! () = gate.ConfigureAwait(continueOnCapturedContext)

                    if onContext context then return true else return false
                }

        valueCase
            "and! of a ConfiguredTaskAwaitable<'T> and a source of the builder's type keeps the order"
            (Ok(1, 2): Validation<_, string>)
        <| fun () ->
            taskValidation {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(Ok 2: Validation<int, string>)
                return a, b
            }

        valueCase
            "and! of a source of the builder's type and a ConfiguredTaskAwaitable<'T> keeps the order"
            (Ok(1, 2): Validation<_, string>)
        <| fun () ->
            taskValidation {
                let! a = Task.FromResult(Ok 1: Validation<int, string>)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            }

        valueCase
            "and! gets the awaiter of the left source before it evaluates the right source"
            (Ok true: Validation<_, string>)
        <| fun () ->
            let started = ref false

            taskValidation {
                let! () = GetAwaiterProbe(fun () -> started.Value <- true)
                and! seen = Task.FromResult(started.Value)
                return seen
            }

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time"
            (Ok 2: Validation<_, string>)
        <| fun source ->
            taskValidation {
                let! x = source
                return x + 1
            }

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time in the dynamic state machine"
            (Ok 2: Validation<_, string>)
        <| fun source ->
            TestHelpers.dynamicTaskValidation {
                let! x = source
                return x + 1
            }
    ]

let private taskOptionTests =
    let case name expected start = contextCase name (Some expected) start

    testList "taskOption" [
        yield!
            sourceCases
                case
                (fun configure context gate ->
                    taskOption {
                        let! () = gate.ConfigureAwait(configure)

                        if onContext context then return true else return false
                    }
                )
                (fun configure context gate ->
                    taskOption {
                        let! () = gate.ConfigureAwait(configure)
                        return onContext context
                    }
                )
                (fun configure context gate ->
                    taskOption {
                        let! () = ValueTask<unit>(gate).ConfigureAwait(configure)
                        return onContext context
                    }
                )
                (fun configure context gate ->
                    taskOption {
                        do! (gate :> Task).ConfigureAwait(configure)
                        return onContext context
                    }
                )

        valueCase "return! of a ConfiguredTaskAwaitable<'T>" (Some 1)
        <| fun () -> taskOption { return! Task.FromResult(1).ConfigureAwait(false) }

        valueCase
            "let! of a ConfiguredTaskAwaitable of an option binds the whole option"
            (Some(Some 1))
        <| fun () ->
            taskOption {
                let! x = Task.FromResult(Some 1).ConfigureAwait(false)
                return x
            }

        valueCase "and! of ConfiguredTaskAwaitable<'T> sources keeps the order" (Some(1, 2))
        <| fun () ->
            taskOption {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            }

        case "and! of ConfiguredTaskAwaitable<'T> sources with ConfigureAwait(false)" true
        <| fun context gate ->
            taskOption {
                let! () = gate.ConfigureAwait(false)
                and! () = Task.FromResult(()).ConfigureAwait(false)
                return onContext context
            }

        yield!
            failureCases
                (fun gate ->
                    taskOption {
                        let! () = gate.ConfigureAwait(false)
                        return "done"
                    }
                )
                (fun local gate ->
                    taskOption {
                        let! () = gate.ConfigureAwait(false)
                        return local.Value
                    }
                )
                (Some "caller")

        for continueOnCapturedContext in
            [
                false
                true
            ] do
            case
                $"let! of a ConfiguredTaskAwaitable<'T> with ConfigureAwait(%b{continueOnCapturedContext}) in the dynamic state machine"
                continueOnCapturedContext
            <| fun context gate ->
                TestHelpers.dynamicTaskOption {
                    let! () = gate.ConfigureAwait(continueOnCapturedContext)

                    if onContext context then return true else return false
                }

        valueCase
            "and! of a ConfiguredTaskAwaitable<'T> and a source of the builder's type keeps the order"
            (Some(1, 2))
        <| fun () ->
            taskOption {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(Some 2)
                return a, b
            }

        valueCase
            "and! of a source of the builder's type and a ConfiguredTaskAwaitable<'T> keeps the order"
            (Some(1, 2))
        <| fun () ->
            taskOption {
                let! a = Task.FromResult(Some 1)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            }

        valueCase
            "and! gets the awaiter of the left source before it evaluates the right source"
            (Some true)
        <| fun () ->
            let started = ref false

            taskOption {
                let! () = GetAwaiterProbe(fun () -> started.Value <- true)
                and! seen = Task.FromResult(started.Value)
                return seen
            }

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time"
            (Some 2)
        <| fun source ->
            taskOption {
                let! x = source
                return x + 1
            }

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time in the dynamic state machine"
            (Some 2)
        <| fun source ->
            TestHelpers.dynamicTaskOption {
                let! x = source
                return x + 1
            }
    ]

let private taskValueOptionTests =
    let case name expected start =
        contextCase name (ValueSome expected) start

    testList "taskValueOption" [
        yield!
            sourceCases
                case
                (fun configure context gate ->
                    taskValueOption {
                        let! () = gate.ConfigureAwait(configure)

                        if onContext context then return true else return false
                    }
                )
                (fun configure context gate ->
                    taskValueOption {
                        let! () = gate.ConfigureAwait(configure)
                        return onContext context
                    }
                )
                (fun configure context gate ->
                    taskValueOption {
                        let! () = ValueTask<unit>(gate).ConfigureAwait(configure)
                        return onContext context
                    }
                )
                (fun configure context gate ->
                    taskValueOption {
                        do! (gate :> Task).ConfigureAwait(configure)
                        return onContext context
                    }
                )

        valueCase "return! of a ConfiguredTaskAwaitable<'T>" (ValueSome 1)
        <| fun () -> taskValueOption { return! Task.FromResult(1).ConfigureAwait(false) }

        valueCase
            "let! of a ConfiguredTaskAwaitable of an option binds the whole option"
            (ValueSome(ValueSome 1))
        <| fun () ->
            taskValueOption {
                let! x = Task.FromResult(ValueSome 1).ConfigureAwait(false)
                return x
            }

        valueCase "and! of ConfiguredTaskAwaitable<'T> sources keeps the order" (ValueSome(1, 2))
        <| fun () ->
            taskValueOption {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            }

        case "and! of ConfiguredTaskAwaitable<'T> sources with ConfigureAwait(false)" true
        <| fun context gate ->
            taskValueOption {
                let! () = gate.ConfigureAwait(false)
                and! () = Task.FromResult(()).ConfigureAwait(false)
                return onContext context
            }

        yield!
            failureCases
                (fun gate ->
                    taskValueOption {
                        let! () = gate.ConfigureAwait(false)
                        return "done"
                    }
                )
                (fun local gate ->
                    taskValueOption {
                        let! () = gate.ConfigureAwait(false)
                        return local.Value
                    }
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
                TestHelpers.dynamicTaskValueOption {
                    let! () = gate.ConfigureAwait(continueOnCapturedContext)

                    if onContext context then return true else return false
                }

        valueCase
            "and! of a ConfiguredTaskAwaitable<'T> and a source of the builder's type keeps the order"
            (ValueSome(1, 2))
        <| fun () ->
            taskValueOption {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(ValueSome 2)
                return a, b
            }

        valueCase
            "and! of a source of the builder's type and a ConfiguredTaskAwaitable<'T> keeps the order"
            (ValueSome(1, 2))
        <| fun () ->
            taskValueOption {
                let! a = Task.FromResult(ValueSome 1)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a, b
            }

        valueCase
            "and! gets the awaiter of the left source before it evaluates the right source"
            (ValueSome true)
        <| fun () ->
            let started = ref false

            taskValueOption {
                let! () = GetAwaiterProbe(fun () -> started.Value <- true)
                and! seen = Task.FromResult(started.Value)
                return seen
            }

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time"
            (ValueSome 2)
        <| fun source ->
            taskValueOption {
                let! x = source
                return x + 1
            }

        pooledValueTaskCase
            "let! of a pending pooled ValueTask<'T> reads the source one time in the dynamic state machine"
            (ValueSome 2)
        <| fun source ->
            TestHelpers.dynamicTaskValueOption {
                let! x = source
                return x + 1
            }
    ]

let allTests =
    testList "ConfigureAwait in let!, do!, return!, and and!" [
        taskResultTests
        taskValidationTests
        taskOptionTests
        taskValueOptionTests
    ]
