namespace FsToolkit.ErrorHandling.Polyfill.RuntimeTask

#if FSTK_RUNTIME_ASYNC && NET11_0_OR_GREATER

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open FsToolkit.ErrorHandling

/// <summary>Contains the members that prepare and combine runtime-async cancellable result computations.</summary>
/// <remarks>
/// Each part of a computation is a function of the invocation token. The members inline into one runtime-async method.
/// </remarks>
[<AutoOpen>]
module RuntimeCancellableTaskResultBase =

    /// <summary>A source that the runtime-async cancellable result builders can await.</summary>
    /// <remarks>The builder calls <c>Start</c> with the invocation token at the bind, after it checks the token.</remarks>
    [<Struct; NoEquality; NoComparison>]
    type CancellableStartedResult<'Source, 'Awaiter, 'Input, 'T, 'Error> = {
        Source: 'Source
        Start: CancellationToken -> 'Source -> 'Awaiter
        Map: 'Input -> Result<'T, 'Error>
    }

    /// <summary>A part of a runtime-async cancellable result computation.</summary>
    type CancellableTaskResultCode<'T, 'Error> = CancellationToken -> Result<'T, 'Error>

    /// <exclude/>
    module RuntimeCancellableResultAwait =

        let inline prepared
            (source: 'Source)
            (start: CancellationToken -> 'Source -> 'Awaiter)
            (map: 'Input -> Result<'T, 'Error>)
            : CancellableStartedResult<'Source, 'Awaiter, 'Input, 'T, 'Error> =
            {
                Source = source
                Start = start
                Map = map
            }

        let inline ofAwaiter
            (awaiter: 'Awaiter)
            (map: 'Input -> Result<'T, 'Error>)
            : CancellableStartedResult<'Awaiter, 'Awaiter, 'Input, 'T, 'Error> =
            prepared awaiter (fun _ awaiter -> awaiter) map

        /// <summary>Starts the source with the token, suspends until it completes, and consumes its result once.</summary>
        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline awaitSource (ct: CancellationToken) source =
            let value = RuntimeResultAwait.awaitAwaiter (source.Start ct source.Source)
            source.Map value

        /// <summary>Runs the code on the thread pool when the caller has a context or a scheduler that is not the default.</summary>
        [<NoDynamicInvocation>]
        let inline startBackground (ct: CancellationToken) (run: unit -> Task<'T>) : Task<'T> =
            if
                isNull SynchronizationContext.Current
                && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
            then
                run ()
            else
                Task.Run<'T>(Func<Task<'T>>(run), ct)

    /// <summary>Provides result flow, cleanup, and source preparation for the runtime-async cancellable result builders.</summary>
    /// <remarks>The builder checks the invocation token before each <c>let!</c>, <c>do!</c>, and <c>return!</c>.</remarks>
    type RuntimeCancellableTaskResultBuilderBase() =

        /// <summary>Delays the computation until the builder runs it.</summary>
        [<NoDynamicInvocation>]
        member inline _.Delay
            (generator: unit -> CancellableTaskResultCode<'T, 'Error>)
            : CancellableTaskResultCode<'T, 'Error> =
            fun ct -> generator () ct

        /// <summary>Returns a successful computation with no value.</summary>
        [<NoDynamicInvocation>]
        member inline _.Zero() : CancellableTaskResultCode<unit, 'Error> = fun _ -> Ok()

        /// <summary>Returns the value in <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Return(value: 'T) : CancellableTaskResultCode<'T, 'Error> =
            fun _ -> Ok value

        /// <summary>Runs the second computation only when the first returns <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Combine
            (
                [<InlineIfLambda>] first: CancellableTaskResultCode<unit, 'Error>,
                second: CancellableTaskResultCode<'T, 'Error>
            ) : CancellableTaskResultCode<'T, 'Error> =
            fun ct ->
                match first ct with
                | Ok() -> second ct
                | Error error -> Error error

        /// <summary>Repeats the body while the guard is true, and stops at the first <c>Error</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.While
            (guard: unit -> bool, body: CancellableTaskResultCode<unit, 'Error>)
            : CancellableTaskResultCode<unit, 'Error> =
            fun ct ->
                let mutable result = Ok()

                while Result.isOk result
                      && guard () do
                    result <- body ct

                result

        /// <summary>Runs the handler when the body throws an exception.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryWith
            (
                body: CancellableTaskResultCode<'T, 'Error>,
                handler: exn -> CancellableTaskResultCode<'T, 'Error>
            ) : CancellableTaskResultCode<'T, 'Error> =
            fun ct ->
                try
                    body ct
                with ex ->
                    handler ex ct

        /// <summary>Runs compensation after the body, including <c>Error</c>, exceptions, and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryFinally
            (body: CancellableTaskResultCode<'T, 'Error>, compensation: unit -> unit)
            : CancellableTaskResultCode<'T, 'Error> =
            fun ct ->
                try
                    body ct
                finally
                    compensation ()

        /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
        /// <remarks>The builder does not check the token before <c>DisposeAsync</c>.</remarks>
        [<NoDynamicInvocation>]
        member inline _.Using
            (
                resource: #IAsyncDisposableNull,
                body: #IAsyncDisposableNull -> CancellableTaskResultCode<'T, 'Error>
            ) : CancellableTaskResultCode<'T, 'Error> =
            fun ct -> RuntimeResultAwait.usingAsync resource (fun () -> body resource ct)

        /// <summary>Visits each element until the body returns <c>Error</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline _.For
            (sequence: seq<'T>, body: 'T -> CancellableTaskResultCode<unit, 'Error>)
            : CancellableTaskResultCode<unit, 'Error> =
            fun ct ->
                use enumerator = sequence.GetEnumerator()
                let mutable result = Ok()

                while Result.isOk result
                      && enumerator.MoveNext() do
                    result <- body enumerator.Current ct

                result

        /// <summary>Visits each element until the body returns <c>Error</c>, and disposes the enumerator.</summary>
        /// <remarks>The builder gives the invocation token to <c>GetAsyncEnumerator</c>. It checks the token before each <c>MoveNextAsync</c>, and again before the body.</remarks>
        [<NoDynamicInvocation>]
        member inline _.For
            (sequence: IAsyncEnumerable<'T>, body: 'T -> CancellableTaskResultCode<unit, 'Error>)
            : CancellableTaskResultCode<unit, 'Error> =
            fun ct ->
                let enumerator = sequence.GetAsyncEnumerator ct

                RuntimeResultAwait.usingAsync
                    enumerator
                    (fun () ->
                        let mutable result = Ok()

                        while Result.isOk result
                              && (ct.ThrowIfCancellationRequested()
                                  AsyncHelpers.Await(enumerator.MoveNextAsync())) do
                            ct.ThrowIfCancellationRequested()
                            result <- body enumerator.Current ct

                        result
                    )

        /// <summary>Checks the token, awaits the source, and calls the continuation only for <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Bind
            (
                source: CancellableStartedResult<_, _, _, _, _>,
                continuation: 'T -> CancellableTaskResultCode<'U, 'Error>
            ) : CancellableTaskResultCode<'U, 'Error> =
            fun ct ->
                ct.ThrowIfCancellationRequested()

                match RuntimeCancellableResultAwait.awaitSource ct source with
                | Ok value -> continuation value ct
                | Error error -> Error error

        /// <summary>Checks the token, awaits the source, and maps the value only for <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.BindReturn
            (source: CancellableStartedResult<_, _, _, _, _>, mapper: 'T -> 'U)
            : CancellableTaskResultCode<'U, 'Error> =
            fun ct ->
                ct.ThrowIfCancellationRequested()

                match RuntimeCancellableResultAwait.awaitSource ct source with
                | Ok value -> Ok(mapper value)
                | Error error -> Error error

        /// <summary>Checks the token, awaits the source, and returns its <c>Result</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.ReturnFrom
            (source: CancellableStartedResult<_, _, _, 'T, 'Error>)
            : CancellableTaskResultCode<'T, 'Error> =
            fun ct ->
                ct.ThrowIfCancellationRequested()
                RuntimeCancellableResultAwait.awaitSource ct source

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeCancellableTaskResultLowestPriority =

        type RuntimeCancellableTaskResultBuilderBase with

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (awaiter: ^Awaiter)
                : CancellableStartedResult< ^Awaiter, ^Awaiter, 'T, 'T, 'Error > =
                RuntimeCancellableResultAwait.ofAwaiter awaiter Ok

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (awaitable: ^Awaitable)
                : CancellableStartedResult< ^Awaitable, ^Awaiter, 'T, 'T, 'Error > =
                RuntimeCancellableResultAwait.prepared
                    awaitable
                    (fun _ value -> (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) value))
                    Ok

            /// <summary>Calls the function with the invocation token at the bind, and awaits the awaiter that it returns.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (cancellableAwaiter: CancellationToken -> ^Awaiter)
                : CancellableStartedResult<CancellationToken -> ^Awaiter, ^Awaiter, 'T, 'T, 'Error> =
                RuntimeCancellableResultAwait.prepared
                    cancellableAwaiter
                    (fun ct start -> start ct)
                    Ok

            /// <summary>Calls the function with the invocation token at the bind, and awaits the awaitable that it returns.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (cancellableAwaitable: CancellationToken -> ^Awaitable)
                : CancellableStartedResult<CancellationToken -> ^Awaitable, ^Awaiter, 'T, 'T, 'Error> =
                RuntimeCancellableResultAwait.prepared
                    cancellableAwaitable
                    (fun ct start ->
                        let awaitable = start ct
                        (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) awaitable)
                    )
                    Ok

            /// <summary>Calls the function at the bind, and awaits the awaitable that it returns.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (coldAwaitable: unit -> ^Awaitable)
                : CancellableStartedResult<unit -> ^Awaitable, ^Awaiter, 'T, 'T, 'Error> =
                RuntimeCancellableResultAwait.prepared
                    coldAwaitable
                    (fun _ start ->
                        let awaitable = start ()
                        (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) awaitable)
                    )
                    Ok

            /// <summary>Calls the function at the bind, and awaits the awaiter that it returns.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (coldAwaiter: unit -> ^Awaiter)
                : CancellableStartedResult<unit -> ^Awaiter, ^Awaiter, 'T, 'T, 'Error> =
                RuntimeCancellableResultAwait.prepared coldAwaiter (fun _ start -> start ()) Ok

            /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Using
                (
                    resource: #IDisposableNull,
                    body: #IDisposableNull -> CancellableTaskResultCode<'T, 'Error>
                ) : CancellableTaskResultCode<'T, 'Error> =
                fun ct ->
                    try
                        body resource ct
                    finally
                        if not (isNull (box resource)) then
                            resource.Dispose()

            /// <summary>Prepares the sequence for <c>for</c>.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #seq<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeCancellableTaskResultResultSources =

        type RuntimeCancellableTaskResultBuilderBase with

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (awaiter: ^Awaiter)
                : CancellableStartedResult< ^Awaiter, ^Awaiter, Result<'T, 'Error>, 'T, 'Error > =
                RuntimeCancellableResultAwait.ofAwaiter awaiter id

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (awaitable: ^Awaitable)
                : CancellableStartedResult< ^Awaitable, ^Awaiter, Result<'T, 'Error>, 'T, 'Error > =
                RuntimeCancellableResultAwait.prepared
                    awaitable
                    (fun _ value -> (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) value))
                    id

            /// <summary>Calls the function with the invocation token at the bind, and awaits the awaiter that it returns.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (cancellableAwaiter: CancellationToken -> ^Awaiter)
                : CancellableStartedResult<
                      CancellationToken -> ^Awaiter,
                      ^Awaiter,
                      Result<'T, 'Error>,
                      'T,
                      'Error
                   >
                =
                RuntimeCancellableResultAwait.prepared
                    cancellableAwaiter
                    (fun ct start -> start ct)
                    id

            /// <summary>Calls the function with the invocation token at the bind, and awaits the awaitable that it returns.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (cancellableAwaitable: CancellationToken -> ^Awaitable)
                : CancellableStartedResult<
                      CancellationToken -> ^Awaitable,
                      ^Awaiter,
                      Result<'T, 'Error>,
                      'T,
                      'Error
                   >
                =
                RuntimeCancellableResultAwait.prepared
                    cancellableAwaitable
                    (fun ct start ->
                        let awaitable = start ct
                        (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) awaitable)
                    )
                    id

            /// <summary>Calls the function at the bind, and awaits the awaitable that it returns.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (coldAwaitable: unit -> ^Awaitable)
                : CancellableStartedResult<
                      unit -> ^Awaitable,
                      ^Awaiter,
                      Result<'T, 'Error>,
                      'T,
                      'Error
                   >
                =
                RuntimeCancellableResultAwait.prepared
                    coldAwaitable
                    (fun _ start ->
                        let awaitable = start ()
                        (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) awaitable)
                    )
                    id

            /// <summary>Calls the function at the bind, and awaits the awaiter that it returns.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (coldAwaiter: unit -> ^Awaiter)
                : CancellableStartedResult<
                      unit -> ^Awaiter,
                      ^Awaiter,
                      Result<'T, 'Error>,
                      'T,
                      'Error
                   >
                =
                RuntimeCancellableResultAwait.prepared coldAwaiter (fun _ start -> start ()) id

            /// <summary>Prepares the asynchronous sequence for <c>for</c>.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #IAsyncEnumerable<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeCancellableTaskResultPlainSources =

        type RuntimeCancellableTaskResultBuilderBase with

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task) =
                RuntimeCancellableResultAwait.ofAwaiter (task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask) =
                RuntimeCancellableResultAwait.prepared task (fun _ task -> task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task<'T>) =
                RuntimeCancellableResultAwait.ofAwaiter (task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<'T>) =
                RuntimeCancellableResultAwait.prepared task (fun _ task -> task.GetAwaiter()) Ok

            /// <summary>Starts the computation with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<'T>) =
                RuntimeCancellableResultAwait.prepared
                    computation
                    (fun ct computation ->
                        Async
                            .StartImmediateAsTask(computation, cancellationToken = ct)
                            .GetAwaiter()
                    )
                    Ok

            /// <summary>Calls the function at the bind, and awaits the task awaiter that it returns.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(coldTaskAwaiter: unit -> TaskAwaiter<'T>) =
                RuntimeCancellableResultAwait.prepared coldTaskAwaiter (fun _ start -> start ()) Ok

            /// <summary>Calls the function with the invocation token at the bind, and awaits the task awaiter that it returns.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(cancellableTaskAwaiter: CancellationToken -> TaskAwaiter<'T>) =
                RuntimeCancellableResultAwait.prepared
                    cancellableTaskAwaiter
                    (fun ct start -> start ct)
                    Ok

            /// <summary>Calls the cancellable task with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(cancellableTask: CancellationToken -> Task<'T>) =
                RuntimeCancellableResultAwait.prepared
                    cancellableTask
                    (fun ct start -> (start ct).GetAwaiter())
                    Ok

            /// <summary>Calls the cancellable task with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(cancellableTask: CancellationToken -> Task) =
                RuntimeCancellableResultAwait.prepared
                    cancellableTask
                    (fun ct start -> (start ct).GetAwaiter())
                    Ok

            /// <summary>Calls the cancellable value task with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(cancellableValueTask: CancellationToken -> ValueTask<'T>) =
                RuntimeCancellableResultAwait.prepared
                    cancellableValueTask
                    (fun ct start -> (start ct).GetAwaiter())
                    Ok

            /// <summary>Calls the cancellable value task with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(cancellableValueTask: CancellationToken -> ValueTask) =
                RuntimeCancellableResultAwait.prepared
                    cancellableValueTask
                    (fun ct start -> (start ct).GetAwaiter())
                    Ok

            /// <summary>Calls the cold task at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(coldTask: unit -> Task<'T>) =
                RuntimeCancellableResultAwait.prepared
                    coldTask
                    (fun _ start -> (start ()).GetAwaiter())
                    Ok

            /// <summary>Calls the cold task at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(coldTask: unit -> Task) =
                RuntimeCancellableResultAwait.prepared
                    coldTask
                    (fun _ start -> (start ()).GetAwaiter())
                    Ok

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeCancellableTaskResultHighPriority =

        type RuntimeCancellableTaskResultBuilderBase with

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task<Result<'T, 'Error>>) =
                RuntimeCancellableResultAwait.ofAwaiter (task.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<Result<'T, 'Error>>) =
                RuntimeCancellableResultAwait.prepared task (fun _ task -> task.GetAwaiter()) id

            /// <summary>Starts the computation with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<Result<'T, 'Error>>) =
                RuntimeCancellableResultAwait.prepared
                    computation
                    (fun ct computation ->
                        Async
                            .StartImmediateAsTask(computation, cancellationToken = ct)
                            .GetAwaiter()
                    )
                    id

            /// <summary>Starts the computation with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<Choice<'T, 'Error>>) =
                RuntimeCancellableResultAwait.prepared
                    computation
                    (fun ct computation ->
                        Async
                            .StartImmediateAsTask(computation, cancellationToken = ct)
                            .GetAwaiter()
                    )
                    Result.ofChoice

            /// <summary>Calls the cancellable task with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(cancellableTask: CancellationToken -> Task<Result<'T, 'Error>>) =
                RuntimeCancellableResultAwait.prepared
                    cancellableTask
                    (fun ct start -> (start ct).GetAwaiter())
                    id

            /// <summary>Calls the cancellable value task with the invocation token at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source
                (cancellableValueTask: CancellationToken -> ValueTask<Result<'T, 'Error>>)
                =
                RuntimeCancellableResultAwait.prepared
                    cancellableValueTask
                    (fun ct start -> (start ct).GetAwaiter())
                    id

            /// <summary>Calls the function at the bind, and awaits the task awaiter that it returns.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(coldTaskAwaiter: unit -> TaskAwaiter<Result<'T, 'Error>>) =
                RuntimeCancellableResultAwait.prepared coldTaskAwaiter (fun _ start -> start ()) id

            /// <summary>Calls the function with the invocation token at the bind, and awaits the task awaiter that it returns.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source
                (cancellableTaskAwaiter: CancellationToken -> TaskAwaiter<Result<'T, 'Error>>)
                =
                RuntimeCancellableResultAwait.prepared
                    cancellableTaskAwaiter
                    (fun ct start -> start ct)
                    id

            /// <summary>Calls the cold task at the bind.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(coldTask: unit -> Task<Result<'T, 'Error>>) =
                RuntimeCancellableResultAwait.prepared
                    coldTask
                    (fun _ start -> (start ()).GetAwaiter())
                    id

            /// <summary>Prepares the result for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(result: Result<'T, 'Error>) =
                RuntimeCancellableResultAwait.ofAwaiter
                    (ValueTask<Result<'T, 'Error>>(result).GetAwaiter())
                    id

            /// <summary>Prepares the choice for the runtime-async cancellable result computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(choice: Choice<'T, 'Error>) =
                RuntimeCancellableResultAwait.ofAwaiter
                    (ValueTask<Result<'T, 'Error>>(Result.ofChoice choice).GetAwaiter())
                    id

    /// <summary>Builds a cancellable task of a result with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    type RuntimeCancellableTaskResultBuilder() =
        inherit RuntimeCancellableTaskResultBuilderBase()

        /// <summary>Returns a function that runs the computation with the token that it gets.</summary>
        /// <remarks>If the token is canceled at the call, the task is canceled and the computation does not run.</remarks>
        [<NoDynamicInvocation>]
        member inline _.Run
            (code: CancellableTaskResultCode<'T, 'Error>)
            : CancellationToken -> Task<Result<'T, 'Error>> =
            fun ct ->
                if ct.IsCancellationRequested then
                    Task.FromCanceled<Result<'T, 'Error>>(ct)
                else
                    __runtimeAsyncReturn (code ct)

        /// <summary>Calls the cancellable task with the invocation token at the bind.</summary>
        /// <remarks>This member is on the builder type so that type inference prefers <c>CancellationToken -> Task&lt;Result&lt;'T, 'Error&gt;&gt;</c>.</remarks>
        [<NoDynamicInvocation>]
        member inline _.Source(cancellableTask: CancellationToken -> Task<Result<'T, 'Error>>) =
            RuntimeCancellableResultAwait.prepared
                cancellableTask
                (fun ct start -> (start ct).GetAwaiter())
                id

    /// <summary>Builds a cancellable task of a result with .NET runtime async, off the caller context.</summary>
    /// <remarks>If the caller has a <c>SynchronizationContext</c> or a task scheduler that is not the default at the call, the computation starts on the thread pool.</remarks>
    type RuntimeBackgroundCancellableTaskResultBuilder() =
        inherit RuntimeCancellableTaskResultBuilderBase()

        /// <summary>Returns a function that runs the computation with the token that it gets.</summary>
        /// <remarks>If the token is canceled at the call, the task is canceled and the computation does not run.</remarks>
        [<NoDynamicInvocation>]
        member inline _.Run
            (code: CancellableTaskResultCode<'T, 'Error>)
            : CancellationToken -> Task<Result<'T, 'Error>> =
            fun ct ->
                if ct.IsCancellationRequested then
                    Task.FromCanceled<Result<'T, 'Error>>(ct)
                else
                    let run () : Task<Result<'T, 'Error>> = __runtimeAsyncReturn (code ct)
                    RuntimeCancellableResultAwait.startBackground ct run

        /// <summary>Calls the cancellable task with the invocation token at the bind.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source(cancellableTask: CancellationToken -> Task<Result<'T, 'Error>>) =
            RuntimeCancellableResultAwait.prepared
                cancellableTask
                (fun ct start -> (start ct).GetAwaiter())
                id

    /// <summary>Builds a cancellable value task of a result with .NET runtime async.</summary>
    type RuntimeCancellableValueTaskResultBuilder() =
        inherit RuntimeCancellableTaskResultBuilderBase()

        /// <summary>Returns a function that runs the computation with the token that it gets.</summary>
        /// <remarks>If the token is canceled at the call, the value task is canceled and the computation does not run.</remarks>
        [<NoDynamicInvocation>]
        member inline _.Run
            (code: CancellableTaskResultCode<'T, 'Error>)
            : CancellationToken -> ValueTask<Result<'T, 'Error>> =
            fun ct ->
                if ct.IsCancellationRequested then
                    ValueTask.FromCanceled<Result<'T, 'Error>>(ct)
                else
                    __runtimeAsyncReturnValueTask (code ct)

        /// <summary>Calls the cancellable value task with the invocation token at the bind.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source
            (cancellableValueTask: CancellationToken -> ValueTask<Result<'T, 'Error>>)
            =
            RuntimeCancellableResultAwait.prepared
                cancellableValueTask
                (fun ct start -> (start ct).GetAwaiter())
                id

    /// <summary>Builds a cancellable value task of a result with .NET runtime async, off the caller context.</summary>
    /// <remarks>If the caller has a <c>SynchronizationContext</c> or a task scheduler that is not the default at the call, the computation starts on the thread pool.</remarks>
    type RuntimeBackgroundCancellableValueTaskResultBuilder() =
        inherit RuntimeCancellableTaskResultBuilderBase()

        /// <summary>Returns a function that runs the computation with the token that it gets.</summary>
        /// <remarks>If the token is canceled at the call, the value task is canceled and the computation does not run.</remarks>
        [<NoDynamicInvocation>]
        member inline _.Run
            (code: CancellableTaskResultCode<'T, 'Error>)
            : CancellationToken -> ValueTask<Result<'T, 'Error>> =
            fun ct ->
                if ct.IsCancellationRequested then
                    ValueTask.FromCanceled<Result<'T, 'Error>>(ct)
                elif
                    isNull SynchronizationContext.Current
                    && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
                then
                    __runtimeAsyncReturnValueTask (code ct)
                else
                    let run () : Task<Result<'T, 'Error>> = __runtimeAsyncReturn (code ct)

                    ValueTask<Result<'T, 'Error>>(
                        Task.Run<Result<'T, 'Error>>(Func<Task<Result<'T, 'Error>>>(run), ct)
                    )

        /// <summary>Calls the cancellable value task with the invocation token at the bind.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source
            (cancellableValueTask: CancellationToken -> ValueTask<Result<'T, 'Error>>)
            =
            RuntimeCancellableResultAwait.prepared
                cancellableValueTask
                (fun ct start -> (start ct).GetAwaiter())
                id

/// <summary>Contains the opt-in runtime-async cancellable result builders.</summary>
[<AutoOpen>]
module RuntimeCancellableTaskResults =

    /// <summary>Builds a cancellable task of a result with .NET runtime async.</summary>
    let cancellableTaskResult = RuntimeCancellableTaskResultBuilder()

    /// <summary>Builds a cancellable task of a result with .NET runtime async, off the caller context.</summary>
    let backgroundCancellableTaskResult =
        RuntimeBackgroundCancellableTaskResultBuilder()

    /// <summary>Builds a cancellable value task of a result with .NET runtime async.</summary>
    let cancellableValueTaskResult = RuntimeCancellableValueTaskResultBuilder()

    /// <summary>Builds a cancellable value task of a result with .NET runtime async, off the caller context.</summary>
    let backgroundCancellableValueTaskResult =
        RuntimeBackgroundCancellableValueTaskResultBuilder()

#endif
