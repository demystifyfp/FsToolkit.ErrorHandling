module ConfigureAwaitTestHelpers

open Expecto
open System
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Channels
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

let private deadline = TimeSpan.FromSeconds 30.

type GetAwaiterProbe(onGetAwaiter: unit -> unit) =
    member _.GetAwaiter() =
        onGetAwaiter ()
        Task.FromResult(()).GetAwaiter()

type WatchAwaiter(gate: Task<int>, events: ResizeArray<string>) =
    member _.IsCompleted = gate.IsCompleted

    member _.GetResult() =
        events.Add "read"
        gate.GetAwaiter().GetResult()

    interface ICriticalNotifyCompletion with
        member _.OnCompleted(continuation) =
            events.Add "register"
            gate.GetAwaiter().OnCompleted(continuation)

        member _.UnsafeOnCompleted(continuation) =
            events.Add "register"
            gate.GetAwaiter().UnsafeOnCompleted(continuation)

type Watch(gate: Task<int>, events: ResizeArray<string>) =
    member _.GetAwaiter() =
        events.Add "acquire"
        WatchAwaiter(gate, events)

type ThrowingGetAwaiter(error: exn) =
    member _.GetAwaiter() : TaskAwaiter<int> = raise error

type GatedProbe(name: string, gate: Task<int>, log: ResizeArray<string>) =
    member _.GetAwaiter() =
        log.Add name
        gate.GetAwaiter()

type TestResource() =
    interface IDisposable with
        member _.Dispose() = ()

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
            let! actual = (runWithPendingGate start).WaitAsync(deadline)

            Expect.equal
                actual
                expected
                "The value shows whether the code after the bind continued on the captured SynchronizationContext"
        }

let valueCase name (expected: 'T) (computation: unit -> Task<'T>) =
    testCaseTask name
    <| fun () ->
        task {
            let! actual = (computation ()).WaitAsync(deadline)
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

                let! _ = Task.WhenAny(running).WaitAsync(deadline)

                match running.Exception with
                | null -> failtest $"The task must fault, not %A{running.Status}"
                | e when
                    e.InnerExceptions.Count = 1
                    && obj.ReferenceEquals(e.InnerExceptions[0], error)
                    ->
                    ()
                | e ->
                    failtest
                        $"The task must fault with the exception of the source, not %A{e.InnerExceptions}"
            }

        testCaseTask "let! of a canceled ConfiguredTaskAwaitable<'T> gives a canceled task"
        <| fun () ->
            task {
                let running = startPending letBang (fun gate -> gate.SetCanceled())

                let! _ = Task.WhenAny(running).WaitAsync(deadline)

                Expect.isTrue
                    running.IsCanceled
                    $"The task must be canceled, not %A{running.Status}"
            }

        testCaseTask "let! of a ConfiguredTaskAwaitable<'T> keeps the caller's AsyncLocal value"
        <| fun () ->
            task {
                let local = AsyncLocal<string>()
                local.Value <- "caller"

                let! actual =
                    (startPending (readLocal local) (fun gate -> gate.SetResult()))
                        .WaitAsync(deadline)

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
    (returnBangThenFinally: bool -> SynchronizationContext -> Task<unit> -> Task<'T>)
    (useBang: bool -> SynchronizationContext -> Task<unit> -> Task<'T>)
    (matchBang: bool -> SynchronizationContext -> Task<unit> -> Task<'T>)
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

            case
                $"return! of a ConfiguredTaskAwaitable<'T> with {configured}, then finally"
                continueOnCapturedContext
                (returnBangThenFinally continueOnCapturedContext)

            case
                $"use! of a ConfiguredTaskAwaitable<'T> with {configured}"
                continueOnCapturedContext
                (useBang continueOnCapturedContext)

            case
                $"match! of a ConfiguredTaskAwaitable<'T> with {configured}"
                continueOnCapturedContext
                (matchBang continueOnCapturedContext)
    ]

let pooledValueTaskCase name (expected: 'T) (bind: ConfiguredValueTaskAwaitable<int> -> Task<'T>) =
    testCaseTask name
    <| fun () ->
        task {
            let channel =
                Channel.CreateUnbounded<int>(UnboundedChannelOptions(SingleReader = true))

            let running = bind (channel.Reader.ReadAsync().ConfigureAwait(false))

            channel.Writer.TryWrite 1
            |> ignore

            let! actual = running.WaitAsync(deadline)
            Expect.equal actual expected "The computation must read the pooled source one time"
        }

let leftWatchCase name (expected: 'T) (merge: Watch -> (unit -> Task<string>) -> Task<'T>) =
    testCaseTask name
    <| fun () ->
        task {
            let gate =
                TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)

            let events = ResizeArray<string>()

            let running =
                merge
                    (Watch(gate.Task, events))
                    (fun () -> Task.FromResult(String.Join(",", events)))

            gate.SetResult 1
            let! actual = running.WaitAsync(deadline)
            Expect.equal actual expected "The value is what the right source saw of the left source"
        }

let leftFaultCase name (merge: Task<int> -> ThrowingGetAwaiter -> Task<'T>) =
    testCaseTask name
    <| fun () ->
        task {
            let left = FormatException "left"
            let right = OperationCanceledException "right"
            let running = merge (Task.FromException<int> left) (ThrowingGetAwaiter right)
            let! _ = Task.WhenAny(running).WaitAsync(deadline)

            match running.Exception with
            | null -> failtest $"The task must fault, not %A{running.Status}"
            | e when
                e.InnerExceptions.Count = 1
                && obj.ReferenceEquals(e.InnerExceptions[0], left)
                ->
                ()
            | e ->
                failtest
                    $"The task must fault with the exception of the left source, not %A{e.InnerExceptions}"
        }

let threeOperandCase
    name
    (expected: 'T)
    (merge: GatedProbe -> GatedProbe -> GatedProbe -> Task<'T>)
    =
    testCaseTask name
    <| fun () ->
        task {
            let gate =
                TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)

            let log = ResizeArray<string>()

            let running =
                merge
                    (GatedProbe("1", gate.Task, log))
                    (GatedProbe("2", gate.Task, log))
                    (GatedProbe("3", gate.Task, log))

            let startedBeforeCompletion = String.Join(",", log)
            gate.SetResult 1
            let! actual = running.WaitAsync(deadline)

            Expect.equal
                startedBeforeCompletion
                "2,3,1"
                "Each operand must get its awaiter before any operand completes. F# nests the merges, so the last pair is first"

            Expect.equal actual expected "The computation must return the awaited values"
        }

let backgroundContextCases
    (builder: string)
    (wrap: bool -> 'T)
    (start: bool -> SynchronizationContext -> Task<unit> -> Task<'T>)
    =
    [
        for continueOnCapturedContext in
            [
                false
                true
            ] do
            testCaseTask
                $"let! of a ConfiguredTaskAwaitable<'T> with ConfigureAwait(%b{continueOnCapturedContext}), context set inside {builder}"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<unit>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = ThreadPoolPostContext()

                    let started =
                        TaskCompletionSource<Task<'T>>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let thread =
                        Thread(fun () ->
                            try
                                started.SetResult(
                                    start continueOnCapturedContext context gate.Task
                                )
                            with e ->
                                started.SetException e
                        )

                    thread.IsBackground <- true
                    thread.Start()
                    let! running = started.Task.WaitAsync(deadline)
                    gate.SetResult()
                    let! actual = running.WaitAsync(deadline)

                    Expect.equal
                        actual
                        (wrap continueOnCapturedContext)
                        "The value shows whether the code after the bind continued on the context that the computation set"
                }
    ]
