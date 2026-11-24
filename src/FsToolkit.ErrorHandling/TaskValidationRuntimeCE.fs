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

/// <summary>Contains runtime-async validation computations.</summary>
[<AutoOpen>]
module RuntimeTaskValidationBase =

    /// <exclude/>
    module RuntimeValidationAwait =

        [<NoDynamicInvocation>]
        let inline merge
            (left: Task<Validation<'T1, 'Error>>)
            (right: Task<Validation<'T2, 'Error>>)
            : Task<Validation<'T1 * 'T2, 'Error>> =
            __runtimeAsyncReturn (
                let leftResult =
                    try
                        AsyncHelpers.Await left
                    with _ ->
                        RuntimeResultAwait.observeFault right
                        reraise ()

                let rightResult = AsyncHelpers.Await right
                Validation.zip leftResult rightResult
            )

    /// <summary>Provides result flow, cleanup, and source preparation for the runtime-async builder.</summary>
    type RuntimeTaskValidationBuilderBase() =

        /// <summary>Delays the computation until the builder runs it.</summary>
        [<NoDynamicInvocation>]
        member inline _.Delay(generator: unit -> Validation<'T, 'Error>) = generator

        /// <summary>Returns a successful computation with no value.</summary>
        [<NoDynamicInvocation>]
        member inline _.Zero() : Validation<unit, 'Error> = Ok()

        /// <summary>Returns the value in <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Return(value: 'T) : Validation<'T, 'Error> = Ok value

        /// <summary>Runs the second computation only when the first returns <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Combine
            (first: Validation<unit, 'Error>, second: unit -> Validation<'T, 'Error>)
            : Validation<'T, 'Error> =
            match first with
            | Ok() -> second ()
            | Error error -> Error error

        /// <summary>Repeats the body while the guard is true, and stops at the first <c>Error</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.While
            (guard: unit -> bool, body: unit -> Validation<unit, 'Error>)
            : Validation<unit, 'Error> =
            let mutable result = Ok()

            while Result.isOk result
                  && guard () do
                result <- body ()

            result

        /// <summary>Runs the handler when the body throws an exception.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryWith
            (body: unit -> Validation<'T, 'Error>, handler: exn -> Validation<'T, 'Error>)
            =
            try
                body ()
            with ex ->
                handler ex

        /// <summary>Runs compensation after the body, including <c>Error</c>, exceptions, and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryFinally
            (body: unit -> Validation<'T, 'Error>, compensation: unit -> unit)
            =
            try
                body ()
            finally
                compensation ()

        /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Using
            (resource: #IAsyncDisposableNull, body: #IAsyncDisposableNull -> Validation<'T, 'Error>)
            =
            RuntimeResultAwait.usingAsync resource (fun () -> body resource)

        /// <summary>Visits each element until the body returns <c>Error</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For(sequence: seq<'T>, body: 'T -> Validation<unit, 'Error>) =
            use enumerator = sequence.GetEnumerator()
            this.While((fun () -> enumerator.MoveNext()), (fun () -> body enumerator.Current))

        /// <summary>Visits each element until the body returns <c>Error</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For
            (sequence: IAsyncEnumerable<'T>, body: 'T -> Validation<unit, 'Error>)
            =
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
            (
                source: StartedResult<_, _, _, 'T, 'Error list>,
                continuation: 'T -> Validation<'U, 'Error>
            ) : Validation<'U, 'Error> =
            match RuntimeResultAwait.awaitSource source with
            | Ok value -> continuation value
            | Error error -> Error error

        /// <summary>Awaits the source and maps the value only for <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.BindReturn
            (source: StartedResult<_, _, _, 'T, 'Error list>, mapper: 'T -> 'U)
            : Validation<'U, 'Error> =
            match RuntimeResultAwait.awaitSource source with
            | Ok value -> Ok(mapper value)
            | Error error -> Error error

        /// <summary>Awaits the source and returns its <c>Validation</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.ReturnFrom
            (source: StartedResult<_, _, _, 'T, 'Error list>)
            : Validation<'T, 'Error> =
            RuntimeResultAwait.awaitSource source

        /// <summary>Starts both operands and combines their results for <c>and!</c>.</summary>
        /// <remarks>The merged task captures the caller context. Both operands complete and their error lists accumulate in operand order. A left exception takes precedence.</remarks>
        [<NoDynamicInvocation>]
        member inline _.MergeSources
            (left: StartedResult<_, _, _, _, _>, right: StartedResult<_, _, _, _, _>)
            =
            let leftTask = RuntimeResultAwait.start left
            let rightTask = RuntimeResultAwait.start right

            let merged = RuntimeValidationAwait.merge leftTask rightTask
            RuntimeResultAwait.prepared (merged.GetAwaiter()) id

        /// <summary>Prepares the source for the runtime-async validation computation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source(task: Task<Validation<'T, 'Error>>) =
            RuntimeResultAwait.prepared (task.GetAwaiter()) id

    /// <exclude/>
    [<AutoOpen>]
    module LowestPriority =

        type RuntimeTaskValidationBuilderBase with

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (awaiter: ^Awaiter)
                : StartedResult< ^Awaiter, ^Awaiter, 'T, 'T, 'Error list > =
                RuntimeResultAwait.prepared awaiter Ok

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (awaitable: ^Awaitable)
                : StartedResult< ^Awaitable, ^Awaiter, 'T, 'T, 'Error list > =
                {
                    Source = awaitable
                    GetAwaiter =
                        fun value -> (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) value)
                    Map = Ok
                }

            /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Using
                (resource: #IDisposableNull, body: #IDisposableNull -> Validation<'T, 'Error>)
                =
                try
                    body resource
                finally
                    if not (isNull (box resource)) then
                        resource.Dispose()

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #seq<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module ResultSources =

        type RuntimeTaskValidationBuilderBase with

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaiter, 'T, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
                (awaiter: ^Awaiter)
                : StartedResult< ^Awaiter, ^Awaiter, Validation<'T, 'Error>, 'T, 'Error list > =
                RuntimeResultAwait.prepared awaiter id

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T, 'Error
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
                (awaitable: ^Awaitable)
                : StartedResult< ^Awaiter, ^Awaiter, Validation<'T, 'Error>, 'T, 'Error list > =
                let mutable awaitable = awaitable
                let awaiter = (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) awaitable)
                RuntimeResultAwait.prepared awaiter id

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #IAsyncEnumerable<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module PlainSources =

        type RuntimeTaskValidationBuilderBase with

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task) =
                // .NET 11.0.0 rejects direct Await of an inlined Task<T> upcast in optimized runtime-async methods.
                RuntimeResultAwait.prepared (task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask) =
                RuntimeResultAwait.prepared (task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task<'T>) =
                let mapped = RuntimeResultAwait.mapTask task
                RuntimeResultAwait.prepared (mapped.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<'T>) =
                RuntimeResultAwait.prepared (task.GetAwaiter()) Ok

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<'T>) =
                let task = Async.StartImmediateAsTask computation
                let mapped = RuntimeResultAwait.mapTask task
                RuntimeResultAwait.prepared (mapped.GetAwaiter()) id

    /// <exclude/>
    [<AutoOpen>]
    module ScalarResultSources =

        type RuntimeTaskValidationBuilderBase with

            /// <summary>Prepares a task result with a single error.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<Result<'T, 'Error>>) =
                RuntimeResultAwait.prepared (source.GetAwaiter()) Validation.ofResult

            /// <summary>Prepares an async result with a single error.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<Result<'T, 'Error>>) =
                let started = Async.StartImmediateAsTask source
                RuntimeResultAwait.prepared (started.GetAwaiter()) Validation.ofResult

            /// <summary>Prepares a result with a single error.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Result<'T, 'Error>) =
                RuntimeResultAwait.prepared (ValueTask<_>(source).GetAwaiter()) Validation.ofResult

    /// <exclude/>
    [<AutoOpen>]
    module HighPriority =

        type RuntimeTaskValidationBuilderBase with

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<Validation<'T, 'Error>>) =
                let started = RuntimeResultAwait.startValueTask task
                RuntimeResultAwait.prepared (started.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<Validation<'T, 'Error>>) =
                let task = Async.StartImmediateAsTask computation
                RuntimeResultAwait.prepared (task.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(result: Validation<'T, 'Error>) =
                RuntimeResultAwait.prepared (ValueTask<Validation<_, _>>(result).GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async validation computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(choice: Choice<'T, 'Error>) =
                RuntimeResultAwait.prepared
                    (ValueTask<Validation<_, _>>(Validation.ofChoice choice).GetAwaiter())
                    id

    /// <summary>Builds a task of a validation with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    type RuntimeTaskValidationBuilder() =
        inherit RuntimeTaskValidationBuilderBase()

        /// <summary>Runs the computation as a runtime-async method and returns its task.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> Validation<'T, 'Error>) : Task<Validation<'T, 'Error>> =
            __runtimeAsyncReturn (code ())

    /// <summary>Builds a validation task on the default scheduler when a caller context is present.</summary>
    type RuntimeBackgroundTaskValidationBuilder() =
        inherit RuntimeTaskValidationBuilderBase()

        /// <summary>Runs the computation with runtime async on the default scheduler.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> Validation<'T, 'Error>) : Task<Validation<'T, 'Error>> =
            if
                isNull SynchronizationContext.Current
                && TaskScheduler.Current = TaskScheduler.Default
            then
                __runtimeAsyncReturn (code ())
            else
                Task.Run<Validation<'T, 'Error>>(
                    Func<Task<Validation<'T, 'Error>>>(fun () -> __runtimeAsyncReturn (code ()))
                )

/// <summary>Contains the opt-in runtime-async <c>taskValidation</c> builder.</summary>
[<AutoOpen>]
module RuntimeTaskValidations =

    /// <summary>Builds a task of a validation with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    let taskValidation = RuntimeTaskValidationBuilder()

    /// <summary>Builds a validation task on the default scheduler with runtime async.</summary>
    let backgroundTaskValidation = RuntimeBackgroundTaskValidationBuilder()

#endif
