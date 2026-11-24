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

/// <summary>Contains the source and await helpers that the runtime-async option builders share.</summary>
[<AutoOpen>]
module RuntimeTaskOptionShared =

    /// <summary>A source that a runtime-async option builder can await.</summary>
    [<Struct; NoEquality; NoComparison>]
    type StartedOption<'Source, 'Awaiter, 'Input, 'Output> = {
        Source: 'Source
        GetAwaiter: 'Source -> 'Awaiter
        Map: 'Input -> 'Output
    }

    /// <exclude/>
    module RuntimeOptionAwait =

        let inline prepared< ^Awaiter, 'Input, 'Output
            when ^Awaiter: (member GetResult: unit -> 'Input)>
            (awaiter: ^Awaiter)
            (map: 'Input -> 'Output)
            : StartedOption< ^Awaiter, ^Awaiter, 'Input, 'Output > =
            {
                Source = awaiter
                GetAwaiter = id
                Map = map
            }

        /// <summary>Awaits a prepared source and consumes its result once.</summary>
        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline awaitSource source =
            let value = RuntimeResultAwait.awaitAwaiter (source.GetAwaiter source.Source)
            source.Map value

        /// <summary>Starts the source in a separate runtime-async call.</summary>
        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline start source : Task<'Output> =
            __runtimeAsyncReturn (awaitSource source)

        /// <summary>Maps a task value in a separate runtime-async call.</summary>
        [<NoDynamicInvocation>]
        let inline mapTask (map: 'T -> 'U) (source: Task<'T>) : Task<'U> =
            __runtimeAsyncReturn (map (AsyncHelpers.Await source))

        /// <summary>Starts a value task in a separate runtime-async call.</summary>
        [<NoDynamicInvocation>]
        let inline startValueTask (source: ValueTask<'T>) : Task<'T> =
            __runtimeAsyncReturn (AsyncHelpers.Await source)

        /// <summary>Awaits both tasks and combines their values.</summary>
        [<NoDynamicInvocation>]
        let inline merge
            (zip: 'Left -> 'Right -> 'Output)
            (left: Task<'Left>)
            (right: Task<'Right>)
            : Task<'Output> =
            __runtimeAsyncReturn (
                let leftValue =
                    try
                        AsyncHelpers.Await left
                    with _ ->
                        RuntimeResultAwait.observeFault right
                        reraise ()

                let rightValue = AsyncHelpers.Await right
                zip leftValue rightValue
            )

        /// <summary>Starts both operands and prepares their combined value for <c>and!</c>.</summary>
        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline mergeSources zip left right =
            let leftTask = start left
            let rightTask = start right
            let merged = merge zip leftTask rightTask
            prepared (merged.GetAwaiter()) id

        /// <summary>Prepares an awaitable whose awaiter the builder gets when it awaits the source.</summary>
        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline awaitable< ^Awaitable, ^Awaiter, 'T, 'Output
            when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'T)>
            (map: 'T -> 'Output)
            (awaitable: ^Awaitable)
            : StartedOption< ^Awaitable, ^Awaiter, 'T, 'Output > =
            {
                Source = awaitable
                GetAwaiter = fun value -> (^Awaitable: (member GetAwaiter: unit -> ^Awaiter) value)
                Map = map
            }

        /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
        [<NoDynamicInvocation>]
        let inline using<'Resource, 'T when 'Resource :> IDisposableNull>
            (resource: 'Resource)
            (body: 'Resource -> 'T)
            : 'T =
            try
                body resource
            finally
                if not (isNull (box resource)) then
                    resource.Dispose()

/// <summary>Contains the members that prepare and combine runtime-async option computations.</summary>
[<AutoOpen>]
module RuntimeTaskOptionBase =

    /// <summary>Provides option flow, cleanup, and source preparation for the runtime-async option builders.</summary>
    type RuntimeTaskOptionBuilderBase() =

        /// <summary>Delays the computation until the builder runs it.</summary>
        [<NoDynamicInvocation>]
        member inline _.Delay(generator: unit -> 'T option) = generator

        /// <summary>Returns <c>Some</c> with no value.</summary>
        [<NoDynamicInvocation>]
        member inline _.Zero() : unit option = Some()

        /// <summary>Returns the value in <c>Some</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Return(value: 'T) : 'T option = Some value

        /// <summary>Runs the second computation only when the first returns <c>Some</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Combine(first: unit option, second: unit -> 'T option) : 'T option =
            match first with
            | Some() -> second ()
            | None -> None

        /// <summary>Repeats the body while the guard is true, and stops at the first <c>None</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.While(guard: unit -> bool, body: unit -> unit option) : unit option =
            let mutable result = Some()

            while result.IsSome
                  && guard () do
                result <- body ()

            result

        /// <summary>Runs the handler when the body throws an exception.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryWith(body: unit -> 'T option, handler: exn -> 'T option) =
            try
                body ()
            with ex ->
                handler ex

        /// <summary>Runs compensation after the body, including <c>None</c>, exceptions, and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryFinally(body: unit -> 'T option, compensation: unit -> unit) =
            try
                body ()
            finally
                compensation ()

        /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Using
            (resource: #IAsyncDisposableNull, body: #IAsyncDisposableNull -> 'T option)
            =
            RuntimeResultAwait.usingAsync resource (fun () -> body resource)

        /// <summary>Visits each element until the body returns <c>None</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For(sequence: seq<'T>, body: 'T -> unit option) =
            use enumerator = sequence.GetEnumerator()
            this.While((fun () -> enumerator.MoveNext()), (fun () -> body enumerator.Current))

        /// <summary>Visits each element until the body returns <c>None</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For(sequence: IAsyncEnumerable<'T>, body: 'T -> unit option) =
            this.Using(
                sequence.GetAsyncEnumerator CancellationToken.None,
                fun enumerator ->
                    this.While(
                        (fun () -> AsyncHelpers.Await(enumerator.MoveNextAsync())),
                        (fun () -> body enumerator.Current)
                    )
            )

        /// <summary>Awaits the source and calls the continuation only for <c>Some</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Bind
            (source: StartedOption<_, _, _, 'T option>, continuation: 'T -> 'U option)
            : 'U option =
            match RuntimeOptionAwait.awaitSource source with
            | Some value -> continuation value
            | None -> None

        /// <summary>Awaits the source and maps the value only for <c>Some</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.BindReturn(source: StartedOption<_, _, _, 'T option>, mapper: 'T -> 'U) =
            match RuntimeOptionAwait.awaitSource source with
            | Some value -> Some(mapper value)
            | None -> None

        /// <summary>Awaits the source and returns its option.</summary>
        [<NoDynamicInvocation>]
        member inline _.ReturnFrom(source: StartedOption<_, _, _, 'T option>) : 'T option =
            RuntimeOptionAwait.awaitSource source

        /// <summary>Starts both operands and combines their options for <c>and!</c>.</summary>
        /// <remarks>The merged task captures the caller context. Both operands complete before the builder selects <c>None</c>. A left exception takes precedence.</remarks>
        [<NoDynamicInvocation>]
        member inline _.MergeSources
            (left: StartedOption<_, _, _, 'T1 option>, right: StartedOption<_, _, _, 'T2 option>)
            =
            RuntimeOptionAwait.mergeSources Option.zip left right

        /// <summary>Prepares the source for the runtime-async option computation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source(task: Task<'T option>) =
            RuntimeOptionAwait.prepared (task.GetAwaiter()) id

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeTaskOptionLowestPriority =

        type RuntimeTaskOptionBuilderBase with

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            /// <remarks>An awaitable of an option binds the whole option.</remarks>
            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source< ^Awaitable, ^Awaiter, 'T
                when ^Awaitable: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (awaitable: ^Awaitable)
                : StartedOption< ^Awaitable, ^Awaiter, 'T, 'T option > =
                RuntimeOptionAwait.awaitable Some awaitable

            /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Using(resource: #IDisposableNull, body: #IDisposableNull -> 'T option) =
                RuntimeOptionAwait.using resource body

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #seq<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeTaskOptionAsyncEnumerableSources =

        type RuntimeTaskOptionBuilderBase with

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #IAsyncEnumerable<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeTaskOptionPlainSources =

        type RuntimeTaskOptionBuilderBase with

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task) =
                // .NET 11.0.0 rejects direct Await of an inlined Task<T> upcast in optimized runtime-async methods.
                RuntimeOptionAwait.prepared (task.GetAwaiter()) Some

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask) =
                RuntimeOptionAwait.prepared (task.GetAwaiter()) Some

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: Task<'T>) =
                let mapped = RuntimeOptionAwait.mapTask Some task
                RuntimeOptionAwait.prepared (mapped.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<'T>) =
                RuntimeOptionAwait.prepared (task.GetAwaiter()) Some

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(option: 'T option) =
                RuntimeOptionAwait.prepared (ValueTask<'T option>(option).GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<'T>) =
                let task = Async.StartImmediateAsTask computation
                let mapped = RuntimeOptionAwait.mapTask Some task
                RuntimeOptionAwait.prepared (mapped.GetAwaiter()) id

    /// <exclude/>
    [<AutoOpen>]
    module RuntimeTaskOptionHighPriority =

        type RuntimeTaskOptionBuilderBase with

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(computation: Async<'T option>) =
                let task = Async.StartImmediateAsTask computation
                RuntimeOptionAwait.prepared (task.GetAwaiter()) id

            /// <summary>Prepares the source for the runtime-async option computation.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(task: ValueTask<'T option>) =
                let started = RuntimeOptionAwait.startValueTask task
                RuntimeOptionAwait.prepared (started.GetAwaiter()) id

    /// <summary>Builds a task of an option with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    type RuntimeTaskOptionBuilder() =
        inherit RuntimeTaskOptionBuilderBase()

        /// <summary>Runs the computation as a runtime-async method and returns its task.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> 'T option) : Task<'T option> =
            __runtimeAsyncReturn (code ())

    /// <summary>Builds a runtime-async option computation on the default task scheduler.</summary>
    type RuntimeBackgroundTaskOptionBuilder() =
        inherit RuntimeTaskOptionBuilderBase()

        /// <summary>Starts the computation on the thread pool when the caller has a context or a custom scheduler.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> 'T option) : Task<'T option> =
            RuntimeTaskScheduling.runBackground code

/// <summary>Contains the opt-in runtime-async task option builders.</summary>
[<AutoOpen>]
module RuntimeTaskOptions =

    /// <summary>Builds a task of an option with .NET runtime async.</summary>
    /// <remarks>Requires .NET 11 or later and <c>LangVersion</c> <c>preview</c>. Open <c>FsToolkit.ErrorHandling.Polyfill.RuntimeTask</c> after <c>FsToolkit.ErrorHandling</c>.</remarks>
    let taskOption = RuntimeTaskOptionBuilder()

    /// <summary>Builds a runtime-async task option on the default task scheduler.</summary>
    let backgroundTaskOption = RuntimeBackgroundTaskOptionBuilder()

#endif
