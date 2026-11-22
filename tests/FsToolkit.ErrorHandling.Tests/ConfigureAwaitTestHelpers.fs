module ConfigureAwaitTestHelpers

open Expecto
open System
open System.Threading
open System.Threading.Tasks

type ThreadPoolPostContext() =
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

let runWithPendingGate (start: SynchronizationContext -> Task<unit> -> Task<'T>) : Task<'T> =
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

let onContext (context: SynchronizationContext) =
    obj.ReferenceEquals(SynchronizationContext.Current, context)

let contextCase name (expected: 'T) (start: SynchronizationContext -> Task<unit> -> Task<'T>) =
    testCaseTask name
    <| fun () ->
        task {
            let! actual = runWithPendingGate start

            Expect.equal
                actual
                expected
                "The value shows whether the code after the bind continued on the captured SynchronizationContext"
        }

let valueCase name (expected: 'T) (computation: unit -> Task<'T>) =
    testCaseTask name
    <| fun () ->
        task {
            let! actual = computation ()
            Expect.equal actual expected "The computation must return the awaited values"
        }

let startPending (start: Task<unit> -> Task<'T>) (finish: TaskCompletionSource<unit> -> unit) =
    let gate =
        TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

    let running = start gate.Task
    finish gate
    running

let failureCases
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

let sourceCases
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
