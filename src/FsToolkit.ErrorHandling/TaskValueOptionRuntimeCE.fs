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

/// <summary>Contains the members that prepare and combine runtime-async value option computations.</summary>
[<AutoOpen>]
module RuntimeTaskValueOptionBase =

    /// <summary>Provides value option flow, cleanup, and source preparation for the runtime-async value option builders.</summary>
    type RuntimeTaskValueOptionBuilderBase() =

        /// <summary>Delays the computation until the builder runs it.</summary>
        [<NoDynamicInvocation>]
        member inline _.Delay(generator: unit -> 'T voption) = generator

        /// <summary>Returns <c>ValueSome</c> with no value.</summary>
        [<NoDynamicInvocation>]
        member inline _.Zero() : unit voption = ValueSome()

        /// <summary>Returns the value in <c>ValueSome</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Return(value: 'T) : 'T voption = ValueSome value

        /// <summary>Runs the second computation only when the first returns <c>ValueSome</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Combine(first: unit voption, second: unit -> 'T voption) : 'T voption =
            match first with
            | ValueSome() -> second ()
            | ValueNone -> ValueNone

        /// <summary>Repeats the body while the guard is true, and stops at the first <c>ValueNone</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.While(guard: unit -> bool, body: unit -> unit voption) : unit voption =
            let mutable result = ValueSome()

            while result.IsSome
                  && guard () do
                result <- body ()

            result

        /// <summary>Runs the handler when the body throws an exception.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryWith(body: unit -> 'T voption, handler: exn -> 'T voption) =
            try
                body ()
            with ex ->
                handler ex

        /// <summary>Runs compensation after the body, including <c>ValueNone</c>, exceptions, and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryFinally(body: unit -> 'T voption, compensation: unit -> unit) =
            try
                body ()
            finally
                compensation ()

        /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Using
            (resource: #IAsyncDisposableNull, body: #IAsyncDisposableNull -> 'T voption)
            =
            RuntimeResultAwait.usingAsync resource (fun () -> body resource)

        /// <summary>Visits each element until the body returns <c>ValueNone</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For(sequence: seq<'T>, body: 'T -> unit voption) =
            use enumerator = sequence.GetEnumerator()
            this.While((fun () -> enumerator.MoveNext()), (fun () -> body enumerator.Current))

        /// <summary>Visits each element until the body returns <c>ValueNone</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For(sequence: IAsyncEnumerable<'T>, body: 'T -> unit voption) =
            this.Using(
                sequence.GetAsyncEnumerator CancellationToken.None,
                fun enumerator ->
                    this.While(
                        (fun () -> AsyncHelpers.Await(enumerator.MoveNextAsync())),
                        (fun () -> body enumerator.Current)
                    )
            )

        /// <summary>Awaits the source and calls the continuation only for <c>ValueSome</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Bind
            (source: StartedOption<_, _, _, 'T voption>, continuation: 'T -> 'U voption)
            : 'U voption =
            match RuntimeOptionAwait.awaitSource source with
            | ValueSome value -> continuation value
            | ValueNone -> ValueNone

        /// <summary>Awaits the source and maps the value only for <c>ValueSome</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.BindReturn(source: StartedOption<_, _, _, 'T voption>, mapper: 'T -> 'U) =
            match RuntimeOptionAwait.awaitSource source with
            | ValueSome value -> ValueSome(mapper value)
            | ValueNone -> ValueNone

        /// <summary>Awaits the source and returns its value option.</summary>
        [<NoDynamicInvocation>]
        member inline _.ReturnFrom(source: StartedOption<_, _, _, 'T voption>) : 'T voption =
            RuntimeOptionAwait.awaitSource source

        /// <summary>Starts both operands and combines their value options for <c>and!</c>.</summary>
        /// <remarks>The merged task captures the caller context. Both operands complete before the builder selects <c>ValueNone</c>. A left exception takes precedence.</remarks>
        [<NoDynamicInvocation>]
        member inline _.MergeSources
            (left: StartedOption<_, _, _, 'T1 voption>, right: StartedOption<_, _, _, 'T2 voption>)
            =
            RuntimeOptionAwait.mergeSources ValueOption.zip left right

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeTaskValueOptionLowestPriority =

        type RuntimeTaskValueOptionBuilderBase with

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            /// <remarks>An awaitable of a value option binds the whole option.</remarks>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (awaitable: ^Awaitable)
                : StartedOption< ^Awaitable, ^Awaiter, 'T, 'T voption > =
                RuntimeOptionAwait.awaitable ValueSome awaitable

            /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Using
                (resource: #IDisposableNull, body: #IDisposableNull -> 'T voption)
                =
                RuntimeOptionAwait.using resource body

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #seq<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeTaskValueOptionAsyncEnumerableSources =

        type RuntimeTaskValueOptionBuilderBase with

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #IAsyncEnumerable<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeTaskValueOptionPlainSources =

        type RuntimeTaskValueOptionBuilderBase with

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task) =
                // .NET 11.0.0 rejects direct Await of an inlined Task<T> upcast in optimized runtime-async methods.
                RuntimeOptionAwait.prepared (task.GetAwaiter()) ValueSome

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask) =
                RuntimeOptionAwait.prepared (task.GetAwaiter()) ValueSome

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task<'T>) =
                let mapped = RuntimeOptionAwait.mapTask ValueSome task
                RuntimeOptionAwait.prepared (mapped.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<'T>) =
                RuntimeOptionAwait.prepared (task.GetAwaiter()) ValueSome

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(valueOption: 'T voption) =
                RuntimeOptionAwait.prepared (ValueTask<'T voption>(valueOption).GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<'T>) =
                let task = Async.StartImmediateAsTask computation
                let mapped = RuntimeOptionAwait.mapTask ValueSome task
                RuntimeOptionAwait.prepared (mapped.GetAwaiter()) id

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeTaskValueOptionHighPriority =

        type RuntimeTaskValueOptionBuilderBase with

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task<'T voption>) =
                RuntimeOptionAwait.prepared (task.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<'T voption>) =
                let task = Async.StartImmediateAsTask computation
                RuntimeOptionAwait.prepared (task.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async value option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<'T voption>) =
                let started = RuntimeOptionAwait.startValueTask task
                RuntimeOptionAwait.prepared (started.GetAwaiter()) id

    /// <summary>Builds a task of a value option with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    type RuntimeTaskValueOptionBuilder() =
        inherit RuntimeTaskValueOptionBuilderBase()

        /// <summary>Runs the computation as a runtime-async method and returns its task.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> 'T voption) : Task<'T voption> =
            __runtimeAsyncReturn (code ())

        /// <summary>Prepares the source for the runtime-async value option computation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source(task: Task<'T voption>) =
            RuntimeOptionAwait.prepared (task.GetAwaiter()) id

    /// <summary>Builds a runtime-async value option computation on the default task scheduler.</summary>
    type RuntimeBackgroundTaskValueOptionBuilder() =
        inherit RuntimeTaskValueOptionBuilderBase()

        /// <summary>Starts the computation on the thread pool when the caller has a context or a custom scheduler.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> 'T voption) : Task<'T voption> =
            RuntimeTaskScheduling.runBackground code

        /// <summary>Prepares the source for the runtime-async value option computation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source(task: Task<'T voption>) =
            RuntimeOptionAwait.prepared (task.GetAwaiter()) id

/// <summary>Contains the opt-in runtime-async task value option builders.</summary>
[<AutoOpen>]
module RuntimeTaskValueOptions =

    /// <summary>Builds a task of a value option with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    let taskValueOption = RuntimeTaskValueOptionBuilder()

    /// <summary>Builds a runtime-async task value option on the default task scheduler.</summary>
    let backgroundTaskValueOption = RuntimeBackgroundTaskValueOptionBuilder()

#endif
