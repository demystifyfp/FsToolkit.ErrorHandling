namespace FsToolkit.ErrorHandling.Polyfill.RuntimeTask

#if FSTK_RUNTIME_ASYNC && NET11_0_OR_GREATER

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Runtime.ExceptionServices
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open FsToolkit.ErrorHandling

/// <exclude/>
module RuntimeTaskScheduling =

    [<NoDynamicInvocation>]
    let inline runBackground (code: unit -> 'T) : Task<'T> =
        let run () : Task<'T> = __runtimeAsyncReturn (code ())

        if
            isNull SynchronizationContext.Current
            && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
        then
            run ()
        else
            Task.Run<'T>(Func<Task<'T>>(run))

/// <summary>Contains the members that prepare and combine runtime-async result computations.</summary>
[<AutoOpen>]
module RuntimeTaskResultBase =

    /// <summary>A source that the runtime-async result builder can await.</summary>
    [<Struct; NoEquality; NoComparison>]
    type StartedResult<'Source, 'Awaiter, 'Input, 'T, 'Error> = {
        Source: 'Source
        GetAwaiter: 'Source -> 'Awaiter
        Map: 'Input -> Result<'T, 'Error>
    }

    /// <exclude/>
    module RuntimeResultAwait =

        let inline prepared< ^Awaiter, 'Input, 'T, 'Error
            when ^Awaiter: (member GetResult: unit -> 'Input)>
            (awaiter: ^Awaiter)
            (map: 'Input -> Result<'T, 'Error>)
            : StartedResult< ^Awaiter, ^Awaiter, 'Input, 'T, 'Error > =
            {
                Source = awaiter
                GetAwaiter = id
                Map = map
            }

        /// <summary>Suspends until the awaiter completes, and consumes its result once.</summary>
        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline awaitAwaiter< ^Awaiter, 'T
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'T)>
            (awaiter: ^Awaiter)
            : 'T =
            let mutable awaiter = awaiter

            if not (^Awaiter: (member get_IsCompleted: unit -> bool) awaiter) then
                AsyncHelpers.UnsafeAwaitAwaiter awaiter

            (^Awaiter: (member GetResult: unit -> 'T) awaiter)

        /// <summary>Awaits a prepared source and consumes its result once.</summary>
        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline awaitSource source =
            let value = awaitAwaiter (source.GetAwaiter source.Source)
            source.Map value

        /// <summary>Starts the source in a separate runtime-async call.</summary>
        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline start source : Task<Result<'T, 'Error>> =
            __runtimeAsyncReturn (awaitSource source)

        /// <summary>Maps a task value to <c>Ok</c> in a separate runtime-async call.</summary>
        [<NoDynamicInvocation>]
        let inline mapTask (source: Task<'T>) : Task<Result<'T, 'Error>> =
            __runtimeAsyncReturn (Ok(AsyncHelpers.Await source))

        /// <summary>Starts a value task in a separate runtime-async call.</summary>
        [<NoDynamicInvocation>]
        let inline startValueTask
            (source: ValueTask<Result<'T, 'Error>>)
            : Task<Result<'T, 'Error>> =
            __runtimeAsyncReturn (AsyncHelpers.Await source)

        let observeFault (source: Task) =
            if source.IsCompleted then
                source.Exception
                |> ignore
            else
                source.ContinueWith(
                    Action<Task>(fun completed ->
                        completed.Exception
                        |> ignore
                    ),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted
                    ||| TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default
                )
                |> ignore

        /// <summary>Awaits both tasks and combines their results.</summary>
        [<NoDynamicInvocation>]
        let inline merge
            (left: Task<Result<'T1, 'Error>>)
            (right: Task<Result<'T2, 'Error>>)
            : Task<Result<'T1 * 'T2, 'Error>> =
            __runtimeAsyncReturn (
                let leftResult =
                    try
                        AsyncHelpers.Await left
                    with _ ->
                        observeFault right
                        reraise ()

                let rightResult = AsyncHelpers.Await right
                Result.zip leftResult rightResult
            )

        /// <summary>Awaits resource disposal after the body, including failures and cancellation.</summary>
        [<NoDynamicInvocation>]
        let inline usingAsync<'Resource, 'T when 'Resource :> IAsyncDisposableNull>
            (resource: 'Resource)
            (body: unit -> 'T)
            : 'T =
            let mutable failure: ExceptionDispatchInfo | null = null

            let result =
                try
                    body ()
                with ex ->
                    failure <- ExceptionDispatchInfo.Capture ex
                    Unchecked.defaultof<'T>

            // A rethrow after a suspending finally loses the stack trace: https://github.com/dotnet/fsharp/issues/20869
            if not (isNull (box resource)) then
                AsyncHelpers.Await(resource.DisposeAsync())

            match failure with
            | null -> result
            | failure ->
                failure.Throw()
                result

    /// <summary>Provides result flow, cleanup, and source preparation for the runtime-async builder.</summary>
    type RuntimeTaskResultBuilderBase() =

        /// <summary>Delays the computation until the builder runs it.</summary>
        [<NoDynamicInvocation>]
        member inline _.Delay(generator: unit -> Result<'T, 'Error>) = generator

        /// <summary>Returns a successful computation with no value.</summary>
        [<NoDynamicInvocation>]
        member inline _.Zero() : Result<unit, 'Error> = Ok()

        /// <summary>Returns the value in <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Return(value: 'T) : Result<'T, 'Error> = Ok value

        /// <summary>Runs the second computation only when the first returns <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Combine
            (first: Result<unit, 'Error>, second: unit -> Result<'T, 'Error>)
            : Result<'T, 'Error> =
            match first with
            | Ok() -> second ()
            | Error error -> Error error

        /// <summary>Repeats the body while the guard is true, and stops at the first <c>Error</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.While
            (guard: unit -> bool, body: unit -> Result<unit, 'Error>)
            : Result<unit, 'Error> =
            let mutable result = Ok()

            while Result.isOk result
                  && guard () do
                result <- body ()

            result

        /// <summary>Runs the handler when the body throws an exception.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryWith
            (body: unit -> Result<'T, 'Error>, handler: exn -> Result<'T, 'Error>)
            =
            try
                body ()
            with ex ->
                handler ex

        /// <summary>Runs compensation after the body, including <c>Error</c>, exceptions, and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryFinally(body: unit -> Result<'T, 'Error>, compensation: unit -> unit) =
            try
                body ()
            finally
                compensation ()

        /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Using
            (resource: #IAsyncDisposableNull, body: #IAsyncDisposableNull -> Result<'T, 'Error>)
            =
            RuntimeResultAwait.usingAsync resource (fun () -> body resource)

        /// <summary>Visits each element until the body returns <c>Error</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For(sequence: seq<'T>, body: 'T -> Result<unit, 'Error>) =
            use enumerator = sequence.GetEnumerator()
            this.While((fun () -> enumerator.MoveNext()), (fun () -> body enumerator.Current))

        /// <summary>Visits each element until the body returns <c>Error</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For(sequence: IAsyncEnumerable<'T>, body: 'T -> Result<unit, 'Error>) =
            this.Using(
                sequence.GetAsyncEnumerator CancellationToken.None,
                fun enumerator ->
                    this.While(
                        (fun () -> AsyncHelpers.Await(enumerator.MoveNextAsync())),
                        (fun () -> body enumerator.Current)
                    )
            )

        /// <summary>Awaits the source and calls the continuation only for <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Bind
            (source: StartedResult<_, _, _, _, _>, continuation: 'T -> Result<'U, 'Error>)
            : Result<'U, 'Error> =
            match RuntimeResultAwait.awaitSource source with
            | Ok value -> continuation value
            | Error error -> Error error

        /// <summary>Awaits the source and maps the value only for <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline this.BindReturn(source: StartedResult<_, _, _, _, _>, mapper: 'T -> 'U) =
            match RuntimeResultAwait.awaitSource source with
            | Ok value -> Ok(mapper value)
            | Error error -> Error error

        /// <summary>Awaits the source and returns its <c>Result</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.ReturnFrom(source: StartedResult<_, _, _, _, _>) =
            RuntimeResultAwait.awaitSource source

        /// <summary>Starts both operands and combines their results for <c>and!</c>.</summary>
        /// <remarks>The merged task captures the caller context. Both operands complete before the builder selects the leftmost <c>Error</c>. A left exception takes precedence.</remarks>
        [<NoDynamicInvocation>]
        member inline _.MergeSources
            (left: StartedResult<_, _, _, _, _>, right: StartedResult<_, _, _, _, _>)
            =
            let leftTask = RuntimeResultAwait.start left
            let rightTask = RuntimeResultAwait.start right

            let merged = RuntimeResultAwait.merge leftTask rightTask
            RuntimeResultAwait.prepared (merged.GetAwaiter()) id

        /// <summary>Prepares the source for the runtime-async result computation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source(task: Task<Result<'T, 'Error>>) =
            RuntimeResultAwait.prepared (task.GetAwaiter()) id

    /// <exclude/>
    [<AutoOpen>]
    module LowestPriority =

        type RuntimeTaskResultBuilderBase with

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (awaiter: ^Awaiter)
                : StartedResult< ^Awaiter, ^Awaiter, 'T, 'T, 'Error > =
                RuntimeResultAwait.prepared awaiter Ok

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (awaitable: ^Awaitable)
                : StartedResult< ^Awaitable, ^Awaiter, 'T, 'T, 'Error > =
                {
                    Source = awaitable
                    GetAwaiter =
                        fun value -> (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) value)
                    Map = Ok
                }

            /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Using
                (resource: #IDisposableNull, body: #IDisposableNull -> Result<'T, 'Error>)
                =
                try
                    body resource
                finally
                    if not (isNull (box resource)) then
                        resource.Dispose()

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #seq<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module ResultSources =

        type RuntimeTaskResultBuilderBase with

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (awaiter: ^Awaiter)
                : StartedResult< ^Awaiter, ^Awaiter, Result<'T, 'Error>, 'T, 'Error > =
                RuntimeResultAwait.prepared awaiter id

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (awaitable: ^Awaitable)
                : StartedResult< ^Awaiter, ^Awaiter, Result<'T, 'Error>, 'T, 'Error > =
                let mutable awaitable = awaitable
                let awaiter = (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) awaitable)
                RuntimeResultAwait.prepared awaiter id

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #IAsyncEnumerable<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module PlainSources =

        type RuntimeTaskResultBuilderBase with

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task) =
                // .NET 11.0.0 rejects direct Await of an inlined Task<T> upcast in optimized runtime-async methods.
                RuntimeResultAwait.prepared (task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask) =
                RuntimeResultAwait.prepared (task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task<'T>) =
                let mapped = RuntimeResultAwait.mapTask task
                RuntimeResultAwait.prepared (mapped.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<'T>) =
                RuntimeResultAwait.prepared (task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<'T>) =
                let task = Async.StartImmediateAsTask computation
                let mapped = RuntimeResultAwait.mapTask task
                RuntimeResultAwait.prepared (mapped.GetAwaiter()) id

    /// <exclude/>
    [<AutoOpen>]
    module HighPriority =

        type RuntimeTaskResultBuilderBase with

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<Result<'T, 'Error>>) =
                let started = RuntimeResultAwait.startValueTask task
                RuntimeResultAwait.prepared (started.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<Result<'T, 'Error>>) =
                let task = Async.StartImmediateAsTask computation
                RuntimeResultAwait.prepared (task.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(result: Result<'T, 'Error>) =
                RuntimeResultAwait.prepared (ValueTask<Result<_, _>>(result).GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(choice: Choice<'T, 'Error>) =
                RuntimeResultAwait.prepared
                    (ValueTask<Result<_, _>>(Result.ofChoice choice).GetAwaiter())
                    id

    /// <summary>Builds a task of a result with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    type RuntimeTaskResultBuilder() =
        inherit RuntimeTaskResultBuilderBase()

        /// <summary>Runs the computation as a runtime-async method and returns its task.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> Result<'T, 'Error>) : Task<Result<'T, 'Error>> =
            __runtimeAsyncReturn (code ())

    /// <summary>Builds a runtime-async result computation on the default task scheduler.</summary>
    type RuntimeBackgroundTaskResultBuilder() =
        inherit RuntimeTaskResultBuilderBase()

        /// <summary>Starts the computation on the thread pool when the caller has a context or a custom scheduler.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> Result<'T, 'Error>) : Task<Result<'T, 'Error>> =
            RuntimeTaskScheduling.runBackground code

/// <summary>Contains the opt-in runtime-async task result builders.</summary>
[<AutoOpen>]
module RuntimeTaskResults =

    /// <summary>Builds a task of a result with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    let taskResult = RuntimeTaskResultBuilder()

    /// <summary>Builds a runtime-async task result on the default task scheduler.</summary>
    let backgroundTaskResult = RuntimeBackgroundTaskResultBuilder()

#endif
