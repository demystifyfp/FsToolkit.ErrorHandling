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
open IcedTasks
open FsToolkit.ErrorHandling

/// <summary>Contains runtime-async cancellable validation flow and source preparation.</summary>
[<AutoOpen>]
module RuntimeCancellableTaskValidationBase =

    /// <summary>A validation source that receives the invocation token when it starts.</summary>
    [<Struct; NoEquality; NoComparison>]
    type CancellableValidationSource<'Source, 'Awaiter, 'Input, 'T, 'Error> = {
        Source: 'Source
        GetAwaiter: CancellationToken -> 'Source -> 'Awaiter
        Map: 'Input -> Validation<'T, 'Error>
    }

    /// <exclude/>
    module RuntimeCancellableValidationAwait =

        let inline prepared< ^Awaiter, 'Input, 'T, 'Error
            when ^Awaiter: (member GetResult: unit -> 'Input)>
            (awaiter: ^Awaiter)
            (map: 'Input -> Validation<'T, 'Error>)
            : CancellableValidationSource< ^Awaiter, ^Awaiter, 'Input, 'T, 'Error > =
            {
                Source = awaiter
                GetAwaiter = fun _ value -> value
                Map = map
            }

        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline awaitSource ct source =
            source.Map(RuntimeResultAwait.awaitAwaiter (source.GetAwaiter ct source.Source))

        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline awaitAcquired< ^Awaiter, 'Input
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'Input)>
            (awaiter: ^Awaiter)
            (consumed: unit -> unit)
            : 'Input =
            let mutable awaiter = awaiter

            if not (^Awaiter: (member get_IsCompleted: unit -> bool) awaiter) then
                AsyncHelpers.UnsafeAwaitAwaiter awaiter

            consumed ()
            (^Awaiter: (member GetResult: unit -> 'Input) awaiter)

        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline observeAwaiter awaiter =
            let observe () : Task<unit> =
                __runtimeAsyncReturn (
                    try
                        RuntimeResultAwait.awaitAwaiter awaiter
                        |> ignore
                    with _ ->
                        ()
                )

            if
                isNull SynchronizationContext.Current
                && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
            then
                observe ()
                |> ignore
            else
                Task.Run<unit>(Func<Task<unit>>(observe))
                |> ignore

        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline merge
            (ct: CancellationToken)
            (left: CancellableValidationSource< ^LeftAwaiter, ^LeftAwaiter, _, 'T, 'Error >)
            (right: CancellableValidationSource< ^RightAwaiter, ^RightAwaiter, _, 'U, 'Error >)
            : Task<Validation<'T * 'U, 'Error>> =
            __runtimeAsyncReturn (
                let mutable leftConsumed = false
                let mutable rightConsumed = false

                try
                    ct.ThrowIfCancellationRequested()
                    let leftValue = awaitAcquired left.Source (fun () -> leftConsumed <- true)
                    let leftResult = left.Map leftValue
                    ct.ThrowIfCancellationRequested()
                    let rightValue = awaitAcquired right.Source (fun () -> rightConsumed <- true)
                    let rightResult = right.Map rightValue
                    Validation.zip leftResult rightResult
                with _ ->
                    if not leftConsumed then
                        observeAwaiter left.Source

                    if not rightConsumed then
                        observeAwaiter right.Source

                    reraise ()
            )

    /// <summary>Builds cold validation tasks with an explicit invocation token and runtime async.</summary>
    type RuntimeCancellableTaskValidationBuilder() =

        /// <summary>Delays the computation until each invocation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Delay(generator: unit -> CancellationToken -> Validation<'T, 'Error>) =
            fun ct -> generator () ct

        /// <summary>Returns a successful unit computation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Zero() : CancellationToken -> Validation<unit, 'Error> = fun _ -> Ok()

        /// <summary>Returns a successful validation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Return(value: 'T) : CancellationToken -> Validation<'T, 'Error> =
            fun _ -> Ok value

        /// <summary>Runs the second computation after a successful first computation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Combine
            (
                [<InlineIfLambda>] first: CancellationToken -> Validation<unit, 'Error>,
                second: CancellationToken -> Validation<'T, 'Error>
            ) =
            fun ct ->
                match first ct with
                | Ok() -> second ct
                | Error errors -> Error errors

        /// <summary>Repeats the body until the guard is false or validation fails.</summary>
        [<NoDynamicInvocation>]
        member inline _.While
            (guard: unit -> bool, body: CancellationToken -> Validation<unit, 'Error>)
            =
            fun ct ->
                let mutable result = Ok()

                while Result.isOk result
                      && guard () do
                    result <- body ct

                result

        /// <summary>Runs the handler when the body throws.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryWith
            (
                body: CancellationToken -> Validation<'T, 'Error>,
                handler: exn -> CancellationToken -> Validation<'T, 'Error>
            ) =
            fun ct ->
                try
                    body ct
                with error ->
                    handler error ct

        /// <summary>Runs compensation after success, failure, or cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryFinally
            (body: CancellationToken -> Validation<'T, 'Error>, compensation: unit -> unit)
            =
            fun ct ->
                try
                    body ct
                finally
                    compensation ()

        /// <summary>Awaits asynchronous disposal after every body outcome.</summary>
        [<NoDynamicInvocation>]
        member inline _.Using
            (
                resource: #IAsyncDisposableNull,
                body: #IAsyncDisposableNull -> CancellationToken -> Validation<'T, 'Error>
            ) =
            fun ct -> RuntimeResultAwait.usingAsync resource (fun () -> body resource ct)

        /// <summary>Visits elements until validation fails, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline _.For
            (sequence: seq<'T>, body: 'T -> CancellationToken -> Validation<unit, 'Error>)
            =
            fun ct ->
                use enumerator = sequence.GetEnumerator()
                let mutable result = Ok()

                while Result.isOk result
                      && enumerator.MoveNext() do
                    result <- body enumerator.Current ct

                result

        /// <summary>Visits asynchronous elements with the invocation token and awaits disposal.</summary>
        [<NoDynamicInvocation>]
        member inline _.For
            (
                sequence: IAsyncEnumerable<'T>,
                body: 'T -> CancellationToken -> Validation<unit, 'Error>
            ) =
            fun (ct: CancellationToken) ->
                let enumerator = sequence.GetAsyncEnumerator ct

                RuntimeResultAwait.usingAsync
                    enumerator
                    (fun () ->
                        let mutable result = Ok()
                        let mutable more = true

                        while Result.isOk result
                              && more do
                            ct.ThrowIfCancellationRequested()
                            more <- AsyncHelpers.Await(enumerator.MoveNextAsync())

                            if more then
                                ct.ThrowIfCancellationRequested()
                                result <- body enumerator.Current ct

                        result
                    )

        /// <summary>Awaits the source with the invocation token and continues only for Ok.</summary>
        [<NoDynamicInvocation>]
        member inline _.Bind
            (
                source: CancellableValidationSource<_, _, _, 'T, 'Error>,
                continuation: 'T -> CancellationToken -> Validation<'U, 'Error>
            ) =
            fun (ct: CancellationToken) ->
                ct.ThrowIfCancellationRequested()

                match RuntimeCancellableValidationAwait.awaitSource ct source with
                | Error errors -> Error errors
                | Ok value -> continuation value ct

        /// <summary>Awaits the source and maps its successful value.</summary>
        [<NoDynamicInvocation>]
        member inline _.BindReturn
            (source: CancellableValidationSource<_, _, _, 'T, 'Error>, mapper: 'T -> 'U)
            =
            fun (ct: CancellationToken) ->
                ct.ThrowIfCancellationRequested()

                match RuntimeCancellableValidationAwait.awaitSource ct source with
                | Error errors -> Error errors
                | Ok value -> Ok(mapper value)

        /// <summary>Awaits the source and returns its validation.</summary>
        [<NoDynamicInvocation>]
        member inline _.ReturnFrom(source: CancellableValidationSource<_, _, _, 'T, 'Error>) =
            fun (ct: CancellationToken) ->
                ct.ThrowIfCancellationRequested()
                RuntimeCancellableValidationAwait.awaitSource ct source

        /// <summary>Starts both operands before awaiting either, and accumulates errors in order.</summary>
        [<NoDynamicInvocation>]
        member inline _.MergeSources
            (
                left: CancellableValidationSource<_, _, _, 'T, 'Error>,
                right: CancellableValidationSource<_, _, _, 'U, 'Error>
            ) =
            {
                Source = (left, right)
                GetAwaiter =
                    fun ct (left, right) ->
                        ct.ThrowIfCancellationRequested()
                        let leftAwaiter = left.GetAwaiter ct left.Source

                        let rightAwaiter =
                            try
                                ct.ThrowIfCancellationRequested()
                                right.GetAwaiter ct right.Source
                            with _ ->
                                RuntimeCancellableValidationAwait.observeAwaiter leftAwaiter
                                reraise ()

                        let acquiredLeft =
                            RuntimeCancellableValidationAwait.prepared leftAwaiter left.Map

                        let acquiredRight =
                            RuntimeCancellableValidationAwait.prepared rightAwaiter right.Map

                        (RuntimeCancellableValidationAwait.merge ct acquiredLeft acquiredRight)
                            .GetAwaiter()
                Map = id
            }

        /// <summary>Prepares a cancellable validation task without starting it.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source(source: CancellationToken -> Task<Validation<'T, 'Error>>) = {
            Source = source
            GetAwaiter = fun ct start -> (start ct).GetAwaiter()
            Map = id
        }

        /// <summary>Runs the computation separately for each invocation token.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run(code: CancellationToken -> Validation<'T, 'Error>) =
            fun (ct: CancellationToken) ->
                if ct.IsCancellationRequested then
                    Task.FromCanceled<Validation<'T, 'Error>>(ct)
                else
                    __runtimeAsyncReturn (code ct)

    /// <exclude/>
    [<AutoOpen>]
    module PlainSources =
        type RuntimeCancellableTaskValidationBuilder with

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error when Awaiter<'Awaiter, 'T>>
                (awaiter: 'Awaiter)
                =
                RuntimeCancellableValidationAwait.prepared
                    awaiter
                    (Ok: 'T -> Validation<'T, 'Error>)

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when Awaitable<'Awaitable, 'Awaiter, 'T>>
                (source: 'Awaitable)
                =
                RuntimeCancellableValidationAwait.prepared
                    (Awaitable.GetAwaiter source)
                    (Ok: 'T -> Validation<'T, 'Error>)

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error when Awaiter<'Awaiter, 'T>>
                (source: CancellationToken -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> start ct)
                    Map = (Ok: 'T -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error when Awaiter<'Awaiter, 'T>>
                (source: unit -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> start ())
                    Map = (Ok: 'T -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when Awaitable<'Awaitable, 'Awaiter, 'T>>
                (source: CancellationToken -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> Awaitable.GetAwaiter(start ct))
                    Map = (Ok: 'T -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when Awaitable<'Awaitable, 'Awaiter, 'T>>
                (source: unit -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> Awaitable.GetAwaiter(start ()))
                    Map = (Ok: 'T -> Validation<'T, 'Error>)
                }

            /// <summary>Disposes a synchronous resource after the task completes.</summary>
            [<NoDynamicInvocation>]
            member inline _.Using
                (
                    resource: #IDisposableNull,
                    body: #IDisposableNull -> CancellationToken -> Validation<'T, 'Error>
                ) =
                fun (ct: CancellationToken) ->
                    try
                        body resource ct
                    finally
                        if not (isNull (box resource)) then
                            resource.Dispose()

            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #seq<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module ScalarResultSources =
        type RuntimeCancellableTaskValidationBuilder with

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (awaiter: 'Awaiter)
                =
                RuntimeCancellableValidationAwait.prepared awaiter Validation.ofResult

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (source: 'Awaitable)
                =
                RuntimeCancellableValidationAwait.prepared
                    (Awaitable.GetAwaiter source)
                    Validation.ofResult

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (source: CancellationToken -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> start ct)
                    Map = (Validation.ofResult: Result<'T, 'Error> -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (source: unit -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> start ())
                    Map = (Validation.ofResult: Result<'T, 'Error> -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (source: CancellationToken -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> Awaitable.GetAwaiter(start ct))
                    Map = (Validation.ofResult: Result<'T, 'Error> -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
                (source: unit -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> Awaitable.GetAwaiter(start ()))
                    Map = (Validation.ofResult: Result<'T, 'Error> -> Validation<'T, 'Error>)
                }

    /// <exclude/>
    [<AutoOpen>]
    module ValidationSources =
        type RuntimeCancellableTaskValidationBuilder with

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
                (awaiter: 'Awaiter)
                =
                RuntimeCancellableValidationAwait.prepared awaiter id

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
                (source: 'Awaitable)
                =
                RuntimeCancellableValidationAwait.prepared (Awaitable.GetAwaiter source) id

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
                (source: CancellationToken -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> start ct)
                    Map = (id: Validation<'T, 'Error> -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T, 'Error
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
                (source: unit -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> start ())
                    Map = (id: Validation<'T, 'Error> -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
                (source: CancellationToken -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> Awaitable.GetAwaiter(start ct))
                    Map = (id: Validation<'T, 'Error> -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T, 'Error
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
                (source: unit -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> Awaitable.GetAwaiter(start ()))
                    Map = (id: Validation<'T, 'Error> -> Validation<'T, 'Error>)
                }

            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #IAsyncEnumerable<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module TaskSources =
        type RuntimeCancellableTaskValidationBuilder with

            [<NoDynamicInvocation>]
            member inline _.Source(source: CancellationToken -> Task<'T>) = {
                Source = source
                GetAwaiter = (fun ct start -> (start ct).GetAwaiter())
                Map = (Ok: 'T -> Validation<'T, 'Error>)
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: unit -> Task<'T>) = {
                Source = source
                GetAwaiter = (fun _ start -> (start ()).GetAwaiter())
                Map = (Ok: 'T -> Validation<'T, 'Error>)
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: Task) =
                RuntimeCancellableValidationAwait.prepared (source.GetAwaiter()) Ok

            [<NoDynamicInvocation>]
            member inline _.Source(source: ValueTask) =
                RuntimeCancellableValidationAwait.prepared (source.GetAwaiter()) Ok

            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<'T>) =
                RuntimeCancellableValidationAwait.prepared (source.GetAwaiter()) Ok

            [<NoDynamicInvocation>]
            member inline _.Source(source: ValueTask<'T>) =
                RuntimeCancellableValidationAwait.prepared (source.GetAwaiter()) Ok

            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<'T>) = {
                Source = source
                GetAwaiter =
                    (fun ct work ->
                        (Async.StartImmediateAsTask(work, cancellationToken = ct)).GetAwaiter()
                    )
                Map = (Ok: 'T -> Validation<'T, 'Error>)
            }

    /// <exclude/>
    [<AutoOpen>]
    module ScalarTaskSources =
        type RuntimeCancellableTaskValidationBuilder with

            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<Result<'T, 'Error>>) =
                RuntimeCancellableValidationAwait.prepared (source.GetAwaiter()) Validation.ofResult

            [<NoDynamicInvocation>]
            member inline _.Source(source: ValueTask<Result<'T, 'Error>>) =
                RuntimeCancellableValidationAwait.prepared (source.GetAwaiter()) Validation.ofResult

            [<NoDynamicInvocation>]
            member inline _.Source(source: CancellationToken -> Task<Result<'T, 'Error>>) = {
                Source = source
                GetAwaiter = fun ct start -> (start ct).GetAwaiter()
                Map = Validation.ofResult
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: unit -> Task<Result<'T, 'Error>>) = {
                Source = source
                GetAwaiter = fun _ start -> (start ()).GetAwaiter()
                Map = Validation.ofResult
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<Result<'T, 'Error>>) = {
                Source = source
                GetAwaiter =
                    fun ct work ->
                        (Async.StartImmediateAsTask(work, cancellationToken = ct)).GetAwaiter()
                Map = Validation.ofResult
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<Choice<'T, 'Error>>) = {
                Source = source
                GetAwaiter =
                    fun ct work ->
                        (Async.StartImmediateAsTask(work, cancellationToken = ct)).GetAwaiter()
                Map = Validation.ofChoice
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: Result<'T, 'Error>) =
                RuntimeCancellableValidationAwait.prepared
                    (ValueTask<_>(source).GetAwaiter())
                    Validation.ofResult

            [<NoDynamicInvocation>]
            member inline _.Source(source: Choice<'T, 'Error>) =
                RuntimeCancellableValidationAwait.prepared
                    (ValueTask<_>(source).GetAwaiter())
                    Validation.ofChoice

    /// <exclude/>
    [<AutoOpen>]
    module HighPriority =
        type RuntimeCancellableTaskValidationBuilder with

            [<NoDynamicInvocation>]
            member inline _.Source(source: CancellationToken -> Task<Validation<'T, 'Error>>) = {
                Source = source
                GetAwaiter = (fun ct start -> (start ct).GetAwaiter())
                Map = (id: Validation<'T, 'Error> -> Validation<'T, 'Error>)
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: unit -> Task<Validation<'T, 'Error>>) = {
                Source = source
                GetAwaiter = (fun _ start -> (start ()).GetAwaiter())
                Map = (id: Validation<'T, 'Error> -> Validation<'T, 'Error>)
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: Validation<'T, 'Error>) =
                RuntimeCancellableValidationAwait.prepared (ValueTask<_>(source).GetAwaiter()) id

            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<Validation<'T, 'Error>>) =
                RuntimeCancellableValidationAwait.prepared (source.GetAwaiter()) id

            [<NoDynamicInvocation>]
            member inline _.Source(source: ValueTask<Validation<'T, 'Error>>) =
                RuntimeCancellableValidationAwait.prepared (source.GetAwaiter()) id

            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<Validation<'T, 'Error>>) = {
                Source = source
                GetAwaiter =
                    (fun ct work ->
                        (Async.StartImmediateAsTask(work, cancellationToken = ct)).GetAwaiter()
                    )
                Map = (id: Validation<'T, 'Error> -> Validation<'T, 'Error>)
            }

/// <summary>Contains the opt-in cancellable validation builder.</summary>
[<AutoOpen>]
module RuntimeCancellableTaskValidations =

    /// <summary>Builds a cold validation task with runtime async. Requires .NET 11 and preview F#.</summary>
    let cancellableTaskValidation = RuntimeCancellableTaskValidationBuilder()

#endif
