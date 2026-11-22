module TaskConfigureAwaitTests

open Expecto
open FsToolkit.ErrorHandling
open System
open System.Threading
open System.Threading.Tasks

type private ThreadPoolPostContext() =
    inherit SynchronizationContext()

    override this.Post(callback, state) =
        ThreadPool.UnsafeQueueUserWorkItem(
            (fun _ ->
                let previous = SynchronizationContext.Current
                SynchronizationContext.SetSynchronizationContext this

                try
                    callback.Invoke state
                finally
                    SynchronizationContext.SetSynchronizationContext previous
            ),
            null
        )
        |> ignore

    override this.CreateCopy() = this

let private runWithPendingGate
    (start: SynchronizationContext -> Task<unit> -> Task<'T>)
    : Task<'T> =
    let gate =
        TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

    let context = ThreadPoolPostContext()
    let previous = SynchronizationContext.Current
    SynchronizationContext.SetSynchronizationContext context

    let running =
        try
            start context gate.Task
        finally
            SynchronizationContext.SetSynchronizationContext previous

    gate.SetResult()
    running

let private onContext (context: SynchronizationContext) =
    obj.ReferenceEquals(SynchronizationContext.Current, context)

let private contextCase
    name
    (expected: 'T)
    (start: SynchronizationContext -> Task<unit> -> Task<'T>)
    =
    testCaseTask name
    <| fun () ->
        task {
            let! actual = runWithPendingGate start

            Expect.equal
                actual
                expected
                "The value shows whether the code after the bind continued on the captured SynchronizationContext"
        }

let private valueCase name (expected: 'T) (computation: unit -> Task<'T>) =
    testCaseTask name
    <| fun () ->
        task {
            let! actual = computation ()
            Expect.equal actual expected "The computation must return the awaited values"
        }

let private startPending
    (start: Task<unit> -> Task<'T>)
    (finish: TaskCompletionSource<unit> -> unit)
    =
    let gate =
        TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

    let running = start gate.Task
    finish gate
    running

let private failureCases
    (letBang: Task<unit> -> Task<'T>)
    (readLocal: AsyncLocal<string> -> Task<unit> -> Task<'T>)
    (expectedLocal: 'T)
    =
    [
        testCaseTask "let! of a faulted ConfiguredTaskAwaitable<'T> throws the source exception"
        <| fun () ->
            task {
                let error = InvalidOperationException "source failed"
                let running = startPending letBang (fun gate -> gate.SetException error)

                let! thrown =
                    task {
                        try
                            let! _ = running
                            return None
                        with e ->
                            return Some e
                    }

                match thrown with
                | Some e when obj.ReferenceEquals(e, error) -> ()
                | other ->
                    failtest
                        $"The computation must throw the exception of the source, not %A{other}"
            }

        testCaseTask "let! of a canceled ConfiguredTaskAwaitable<'T> gives a canceled task"
        <| fun () ->
            task {
                let running = startPending letBang (fun gate -> gate.SetCanceled())

                let! _ =
                    task {
                        try
                            let! _ = running
                            return ()
                        with _ ->
                            return ()
                    }

                Expect.isTrue
                    running.IsCanceled
                    $"The task must be canceled, not %A{running.Status}"
            }

        testCaseTask "let! of a ConfiguredTaskAwaitable<'T> keeps the caller's AsyncLocal value"
        <| fun () ->
            task {
                let local = AsyncLocal<string>()
                local.Value <- "caller"
                let! actual = startPending (readLocal local) (fun gate -> gate.SetResult())

                Expect.equal
                    actual
                    expectedLocal
                    "The code after let! must see the caller's AsyncLocal value"
            }
    ]

let private sourceCases
    (case: string -> bool -> (SynchronizationContext -> Task<unit> -> Task<'T>) -> Test)
    (letBangThenBind: bool -> SynchronizationContext -> Task<unit> -> Task<'T>)
    (letBangThenReturn: bool -> SynchronizationContext -> Task<unit> -> Task<'T>)
    (letBangValueTask: bool -> SynchronizationContext -> Task<unit> -> Task<'T>)
    (doBangTask: bool -> SynchronizationContext -> Task<unit> -> Task<'T>)
    =
    [
        for continueOnCapturedContext in
            [
                false
                true
            ] do
            let configured = $"ConfigureAwait(%b{continueOnCapturedContext})"

            case
                $"let! of a ConfiguredTaskAwaitable<'T> with {configured}, then more code"
                continueOnCapturedContext
                (letBangThenBind continueOnCapturedContext)

            case
                $"let! of a ConfiguredTaskAwaitable<'T> with {configured}, then return"
                continueOnCapturedContext
                (letBangThenReturn continueOnCapturedContext)

            case
                $"let! of a ConfiguredValueTaskAwaitable<'T> with {configured}"
                continueOnCapturedContext
                (letBangValueTask continueOnCapturedContext)

            case
                $"do! of a ConfiguredTaskAwaitable with {configured}"
                continueOnCapturedContext
                (doBangTask continueOnCapturedContext)
    ]

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

        valueCase "and! of ConfiguredTaskAwaitable<'T> sources" (Ok 3: Result<int, string>)
        <| fun () ->
            taskResult {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a + b
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

        valueCase "and! of ConfiguredTaskAwaitable<'T> sources" (Ok 3: Validation<int, string>)
        <| fun () ->
            taskValidation {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a + b
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

        valueCase "and! of ConfiguredTaskAwaitable<'T> sources" (Some 3)
        <| fun () ->
            taskOption {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a + b
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

        valueCase "and! of ConfiguredTaskAwaitable<'T> sources" (ValueSome 3)
        <| fun () ->
            taskValueOption {
                let! a = Task.FromResult(1).ConfigureAwait(false)
                and! b = Task.FromResult(2).ConfigureAwait(false)
                return a + b
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
    ]

let allTests =
    testList "ConfigureAwait in let!, do!, return!, and and!" [
        taskResultTests
        taskValidationTests
        taskOptionTests
        taskValueOptionTests
    ]
