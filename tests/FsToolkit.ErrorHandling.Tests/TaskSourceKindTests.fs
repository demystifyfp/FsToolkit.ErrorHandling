module TaskSourceKindTests

open System
open System.Threading
open System.Threading.Tasks
open Expecto
open FsToolkit.ErrorHandling

// Each source kind reaches Bind, BindReturn, ReturnFrom, and MergeSources in a different form.

let private pendingInt (value: int) : Task<int> =
    task {
        do! Task.Yield()
        return value
    }

let private pendingOk () : Task<Result<int, string>> =
    task {
        do! Task.Yield()
        return Ok 1
    }

let private pendingValidation () : Task<Validation<int, string>> =
    task {
        do! Task.Yield()
        return Ok 1
    }

let private pendingSome () : Task<int option> =
    task {
        do! Task.Yield()
        return Some 1
    }

let private pendingValueSome () : Task<int voption> =
    task {
        do! Task.Yield()
        return ValueSome 1
    }

let private pendingFault () : Task<int> =
    task {
        do! Task.Yield()
        return failwith "boom"
    }

type private MyCanceled(message: string) =
    inherit OperationCanceledException(message)

let private pendingCancel () : Task<int> =
    task {
        do! Task.Yield()
        return raise (MyCanceled "mine")
    }

// GetResult reads an AsyncLocal. OnCompleted flows the ExecutionContext. UnsafeOnCompleted runs the continuation without it.
type LocalReaderAwaiter(gate: Task, local: AsyncLocal<int>) =
    member _.IsCompleted = gate.IsCompleted
    member _.GetResult() = local.Value

    interface System.Runtime.CompilerServices.ICriticalNotifyCompletion with
        member _.OnCompleted(continuation) =
            gate.ContinueWith(fun (_: Task) -> continuation.Invoke())
            |> ignore

        member _.UnsafeOnCompleted(continuation) =
            gate.ConfigureAwait(false).GetAwaiter().UnsafeOnCompleted(continuation)

// A ValueTask backed by this source allows one GetResult call.
type CountingValueTaskSource<'T>() =
    [<DefaultValue(false)>]
    val mutable private core: System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore<'T>

    member val GetResultCalls = 0 with get, set
    member this.ValueTask = ValueTask<'T>(this, this.core.Version)
    member this.SetResult(value: 'T) = this.core.SetResult value

    interface System.Threading.Tasks.Sources.IValueTaskSource<'T> with
        member this.GetResult(token) =
            this.GetResultCalls <-
                this.GetResultCalls
                + 1

            this.core.GetResult token

        member this.GetStatus(token) = this.core.GetStatus token

        member this.OnCompleted(continuation, state, token, flags) =
            this.core.OnCompleted(continuation, state, token, flags)

type LocalReader(gate: Task, local: AsyncLocal<int>) =
    member _.GetAwaiter() = LocalReaderAwaiter(gate, local)

// Runs each posted callback on the thread pool, with this context set.
type private PostContext() =
    inherit SynchronizationContext()

    override this.Post(callback, state) =
        ThreadPool.QueueUserWorkItem(fun _ ->
            let previous = SynchronizationContext.Current
            SynchronizationContext.SetSynchronizationContext(this)

            try
                callback.Invoke(state)
            finally
                SynchronizationContext.SetSynchronizationContext(previous)
        )
        |> ignore

let rec private innermost (e: exn) =
    match e with
    | :? AggregateException as a ->
        match a.InnerException with
        | null -> e
        | inner -> innermost inner
    | e -> e

let allTests =
    testList "Task builder source kinds" [
        testList "taskResult" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult 1
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = pendingInt 1
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = pendingOk ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = pendingOk ()
                            return a + b
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Error "a": Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Error "a": Result<int, string>) ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual = taskResult { return! Task.FromResult(Ok 1: Result<int, string>) }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual = taskResult { return! (Ok 1: Result<int, string>) }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = taskResult { return! Task.FromResult 1 }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = taskResult { return! pendingInt 1 }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskResult {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskResult {
                            let! a = pendingFault ()
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        taskResult {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
            testCaseTask "and! keeps the exception object of a canceled source"
            <| fun () ->
                task {
                    let computation () =
                        taskResult {
                            let! a = Task.FromException<int>(MyCanceled "mine")
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! keeps the exception object of a pending canceled source"
            <| fun () ->
                task {
                    let computation () =
                        taskResult {
                            let! a = pendingCancel ()
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! reads a pending plain ValueTask from an IValueTaskSource one time"
            <| fun () ->
                task {
                    let source = CountingValueTaskSource<int>()

                    let computation =
                        taskResult {
                            let! a = source.ValueTask
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test completes it"

                    source.SetResult(1)
                    let! actual = computation
                    Expect.equal actual (Ok 3: Result<int, string>) ""
                    Expect.equal source.GetResultCalls 1 "GetResult must run one time"
                }
            testCaseTask "and! reads a pending Result ValueTask from an IValueTaskSource one time"
            <| fun () ->
                task {
                    let source = CountingValueTaskSource<Result<int, string>>()

                    let computation =
                        taskResult {
                            let! a = source.ValueTask
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test completes it"

                    source.SetResult(Ok 1)
                    let! actual = computation
                    Expect.equal actual (Ok 3: Result<int, string>) ""
                    Expect.equal source.GetResultCalls 1 "GetResult must run one time"
                }
            testCaseTask "and! reads a pending custom source in the caller's ExecutionContext"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<unit>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let local = AsyncLocal<int>()
                    local.Value <- 40

                    let computation =
                        taskResult {
                            let! a = LocalReader(gate.Task, local)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test releases it"

                    local.Value <- 0
                    gate.SetResult()
                    let! actual = computation
                    Expect.equal actual (Ok 42: Result<int, string>) ""
                }
            testCaseTask
                "let! of a plain source with ConfigureAwait(false) does not resume on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            taskResult {
                                let! _ = gate.Task.ConfigureAwait(false)
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (Ok false: Result<bool, string>) ""
                }
            testCaseTask "let! of a plain source resumes on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            taskResult {
                                let! _ = gate.Task
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (Ok true: Result<bool, string>) ""
                }
        ]
        testList "backgroundTaskResult" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult 1
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = pendingInt 1
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = pendingOk ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = pendingOk ()
                            return a + b
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Error "a": Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Error "a": Result<int, string>) ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult { return! Task.FromResult(Ok 1: Result<int, string>) }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual = backgroundTaskResult { return! (Ok 1: Result<int, string>) }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = backgroundTaskResult { return! Task.FromResult 1 }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = backgroundTaskResult { return! pendingInt 1 }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        backgroundTaskResult {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        backgroundTaskResult {
                            let! a = pendingFault ()
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        backgroundTaskResult {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
            testCaseTask "and! keeps the exception object of a canceled source"
            <| fun () ->
                task {
                    let computation () =
                        backgroundTaskResult {
                            let! a = Task.FromException<int>(MyCanceled "mine")
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! keeps the exception object of a pending canceled source"
            <| fun () ->
                task {
                    let computation () =
                        backgroundTaskResult {
                            let! a = pendingCancel ()
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! reads a pending plain ValueTask from an IValueTaskSource one time"
            <| fun () ->
                task {
                    let source = CountingValueTaskSource<int>()

                    let computation =
                        backgroundTaskResult {
                            let! a = source.ValueTask
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test completes it"

                    source.SetResult(1)
                    let! actual = computation
                    Expect.equal actual (Ok 3: Result<int, string>) ""
                    Expect.equal source.GetResultCalls 1 "GetResult must run one time"
                }
            testCaseTask "and! reads a pending Result ValueTask from an IValueTaskSource one time"
            <| fun () ->
                task {
                    let source = CountingValueTaskSource<Result<int, string>>()

                    let computation =
                        backgroundTaskResult {
                            let! a = source.ValueTask
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test completes it"

                    source.SetResult(Ok 1)
                    let! actual = computation
                    Expect.equal actual (Ok 3: Result<int, string>) ""
                    Expect.equal source.GetResultCalls 1 "GetResult must run one time"
                }
            testCaseTask "and! reads a pending custom source in the caller's ExecutionContext"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<unit>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let local = AsyncLocal<int>()
                    local.Value <- 40

                    let computation =
                        backgroundTaskResult {
                            let! a = LocalReader(gate.Task, local)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test releases it"

                    local.Value <- 0
                    gate.SetResult()
                    let! actual = computation
                    Expect.equal actual (Ok 42: Result<int, string>) ""
                }
            testCaseTask
                "let! of a plain source with ConfigureAwait(false) does not resume on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            backgroundTaskResult {
                                let! _ = gate.Task.ConfigureAwait(false)
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (Ok false: Result<bool, string>) ""
                }
            testCaseTask "let! of a plain source does not capture the context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            backgroundTaskResult {
                                let! _ = gate.Task
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (Ok false: Result<bool, string>) ""
                }
        ]
        testList "taskValidation" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            and! b = Task.FromResult(Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            and! b = Task.FromResult(Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult 1
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = pendingInt 1
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = pendingValidation ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            and! b = pendingValidation ()
                            return a + b
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.error "a": Validation<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Error [ "a" ]: Validation<int, string>) ""
                }
            testCaseTask "and! collects the errors of two Value sources"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.error "a": Validation<int, string>)
                            and! b = (Validation.error "b": Validation<int, string>)
                            return a + b
                        }

                    Expect.equal
                        actual
                        (Error [
                            "a"
                            "b"
                        ]
                        : Validation<int, string>)
                        ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            return! Task.FromResult(Validation.ok 1: Validation<int, string>)
                        }

                    Expect.equal actual (Ok 1: Validation<int, string>) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation { return! (Validation.ok 1: Validation<int, string>) }

                    Expect.equal actual (Ok 1: Validation<int, string>) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = taskValidation { return! Task.FromResult 1 }

                    Expect.equal actual (Ok 1: Validation<int, string>) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = taskValidation { return! pendingInt 1 }

                    Expect.equal actual (Ok 1: Validation<int, string>) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskValidation {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskValidation {
                            let! a = pendingFault ()
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        taskValidation {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
            testCaseTask "and! keeps the exception object of a canceled source"
            <| fun () ->
                task {
                    let computation () =
                        taskValidation {
                            let! a = Task.FromException<int>(MyCanceled "mine")
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! keeps the exception object of a pending canceled source"
            <| fun () ->
                task {
                    let computation () =
                        taskValidation {
                            let! a = pendingCancel ()
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! reads a pending plain ValueTask from an IValueTaskSource one time"
            <| fun () ->
                task {
                    let source = CountingValueTaskSource<int>()

                    let computation =
                        taskValidation {
                            let! a = source.ValueTask
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test completes it"

                    source.SetResult(1)
                    let! actual = computation
                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                    Expect.equal source.GetResultCalls 1 "GetResult must run one time"
                }
            testCaseTask
                "and! reads a pending Validation ValueTask from an IValueTaskSource one time"
            <| fun () ->
                task {
                    let source = CountingValueTaskSource<Validation<int, string>>()

                    let computation =
                        taskValidation {
                            let! a = source.ValueTask
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test completes it"

                    source.SetResult(Ok 1)
                    let! actual = computation
                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                    Expect.equal source.GetResultCalls 1 "GetResult must run one time"
                }
            testCaseTask "and! reads a pending custom source in the caller's ExecutionContext"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<unit>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let local = AsyncLocal<int>()
                    local.Value <- 40

                    let computation =
                        taskValidation {
                            let! a = LocalReader(gate.Task, local)
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test releases it"

                    local.Value <- 0
                    gate.SetResult()
                    let! actual = computation
                    Expect.equal actual (Ok 42: Validation<int, string>) ""
                }
            testCaseTask
                "let! of a plain source with ConfigureAwait(false) does not resume on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            taskValidation {
                                let! _ = gate.Task.ConfigureAwait(false)
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (Ok false: Validation<bool, string>) ""
                }
            testCaseTask "let! of a plain source resumes on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            taskValidation {
                                let! _ = gate.Task
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (Ok true: Validation<bool, string>) ""
                }
        ]
        testList "taskOption" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            and! b = Task.FromResult(Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            and! b = Task.FromResult(Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult 1
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = pendingInt 1
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = pendingSome ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            and! b = pendingSome ()
                            return a + b
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (None: int option)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (None: int option) ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            return a + 1
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual = taskOption { return! Task.FromResult(Some 1) }

                    Expect.equal actual (Some 1) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            return a + 1
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual = taskOption { return! (Some 1) }

                    Expect.equal actual (Some 1) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = taskOption { return! Task.FromResult 1 }

                    Expect.equal actual (Some 1) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = taskOption { return! pendingInt 1 }

                    Expect.equal actual (Some 1) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskOption {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (Some 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskOption {
                            let! a = pendingFault ()
                            and! b = (Some 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        taskOption {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (Some 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
            testCaseTask "and! keeps the exception object of a canceled source"
            <| fun () ->
                task {
                    let computation () =
                        taskOption {
                            let! a = Task.FromException<int>(MyCanceled "mine")
                            and! b = (Some 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! keeps the exception object of a pending canceled source"
            <| fun () ->
                task {
                    let computation () =
                        taskOption {
                            let! a = pendingCancel ()
                            and! b = (Some 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! reads a pending plain ValueTask from an IValueTaskSource one time"
            <| fun () ->
                task {
                    let source = CountingValueTaskSource<int>()

                    let computation =
                        taskOption {
                            let! a = source.ValueTask
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test completes it"

                    source.SetResult(1)
                    let! actual = computation
                    Expect.equal actual (Some 3) ""
                    Expect.equal source.GetResultCalls 1 "GetResult must run one time"
                }
            testCaseTask "and! reads a pending custom source in the caller's ExecutionContext"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<unit>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let local = AsyncLocal<int>()
                    local.Value <- 40

                    let computation =
                        taskOption {
                            let! a = LocalReader(gate.Task, local)
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test releases it"

                    local.Value <- 0
                    gate.SetResult()
                    let! actual = computation
                    Expect.equal actual (Some 42) ""
                }
            testCaseTask
                "let! of a plain source with ConfigureAwait(false) does not resume on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            taskOption {
                                let! _ = gate.Task.ConfigureAwait(false)
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (Some false) ""
                }
            testCaseTask "let! of a plain source resumes on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            taskOption {
                                let! _ = gate.Task
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (Some true) ""
                }
        ]
        testList "taskValueOption" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            and! b = Task.FromResult(ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            and! b = Task.FromResult(ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult 1
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = pendingInt 1
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = pendingValueSome ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            and! b = pendingValueSome ()
                            return a + b
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueNone: int voption)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueNone: int voption) ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            return a + 1
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual = taskValueOption { return! Task.FromResult(ValueSome 1) }

                    Expect.equal actual (ValueSome 1) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            return a + 1
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual = taskValueOption { return! (ValueSome 1) }

                    Expect.equal actual (ValueSome 1) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = taskValueOption { return! Task.FromResult 1 }

                    Expect.equal actual (ValueSome 1) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = taskValueOption { return! pendingInt 1 }

                    Expect.equal actual (ValueSome 1) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskValueOption {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskValueOption {
                            let! a = pendingFault ()
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        taskValueOption {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
            testCaseTask "and! keeps the exception object of a canceled source"
            <| fun () ->
                task {
                    let computation () =
                        taskValueOption {
                            let! a = Task.FromException<int>(MyCanceled "mine")
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! keeps the exception object of a pending canceled source"
            <| fun () ->
                task {
                    let computation () =
                        taskValueOption {
                            let! a = pendingCancel ()
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return
                                    (let e = innermost e in

                                     e.GetType().Name
                                     + ": "
                                     + e.Message)
                        }

                    Expect.equal outcome "MyCanceled: mine" ""
                }
            testCaseTask "and! reads a pending plain ValueTask from an IValueTaskSource one time"
            <| fun () ->
                task {
                    let source = CountingValueTaskSource<int>()

                    let computation =
                        taskValueOption {
                            let! a = source.ValueTask
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test completes it"

                    source.SetResult(1)
                    let! actual = computation
                    Expect.equal actual (ValueSome 3) ""
                    Expect.equal source.GetResultCalls 1 "GetResult must run one time"
                }
            testCaseTask "and! reads a pending custom source in the caller's ExecutionContext"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<unit>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let local = AsyncLocal<int>()
                    local.Value <- 40

                    let computation =
                        taskValueOption {
                            let! a = LocalReader(gate.Task, local)
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.isFalse
                        computation.IsCompleted
                        "The source must be pending when the test releases it"

                    local.Value <- 0
                    gate.SetResult()
                    let! actual = computation
                    Expect.equal actual (ValueSome 42) ""
                }
            testCaseTask
                "let! of a plain source with ConfigureAwait(false) does not resume on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            taskValueOption {
                                let! _ = gate.Task.ConfigureAwait(false)
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (ValueSome false) ""
                }
            testCaseTask "let! of a plain source resumes on the captured context"
            <| fun () ->
                task {
                    let gate =
                        TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        )

                    let context = PostContext()
                    let previous = SynchronizationContext.Current
                    SynchronizationContext.SetSynchronizationContext(context)

                    let computation =
                        try
                            taskValueOption {
                                let! _ = gate.Task
                                return obj.ReferenceEquals(SynchronizationContext.Current, context)
                            }
                        finally
                            SynchronizationContext.SetSynchronizationContext(previous)

                    gate.SetResult 1
                    let! actual = computation
                    Expect.equal actual (ValueSome true) ""
                }
        ]
    ]
