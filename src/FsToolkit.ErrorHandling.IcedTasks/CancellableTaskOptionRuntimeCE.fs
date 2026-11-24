namespace FsToolkit.ErrorHandling.Polyfill.RuntimeTask

#if FSTK_RUNTIME_ASYNC && NET11_0_OR_GREATER

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open IcedTasks
open IcedTasks.Polyfill.RuntimeTask
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask

/// <summary>Contains source preparation and option flow for the cancellable runtime-async builders.</summary>
[<AutoOpen>]
module RuntimeCancellableTaskOptionBase =

    /// <summary>A source that starts with the ambient token and maps its result to an option.</summary>
    [<Struct; NoEquality; NoComparison>]
    type CancellableOptionSource<'Source, 'Awaiter, 'Input, 'T> = {
        Source: 'Source
        GetAwaiter: CancellationToken -> 'Source -> 'Awaiter
        Map: 'Input -> 'T option
    }

    /// <exclude/>
    module RuntimeCancellableOptionAwait =

        let inline prepared< ^Awaiter, 'Input, 'T when ^Awaiter: (member GetResult: unit -> 'Input)>
            (awaiter: ^Awaiter)
            (map: 'Input -> 'T option)
            : CancellableOptionSource< ^Awaiter, ^Awaiter, 'Input, 'T > =
            {
                Source = awaiter
                GetAwaiter = fun _ value -> value
                Map = map
            }

        [<NoDynamicInvocation; NoEagerConstraintApplication>]
        let inline awaitSource ct source =
            source.Map(RuntimeAwait.awaitAwaiter (source.GetAwaiter ct source.Source))

    /// <summary>Provides option flow, cancellation checks, cleanup, and source preparation.</summary>
    type RuntimeCancellableTaskOptionBuilderBase() =

        [<NoDynamicInvocation>]
        member inline _.Delay(generator: unit -> CancellationToken -> 'T option) =
            fun (ct: CancellationToken) -> generator () ct

        [<NoDynamicInvocation>]
        member inline _.Zero() : CancellationToken -> unit option = fun _ -> Some()

        [<NoDynamicInvocation>]
        member inline _.Return(value: 'T) : CancellationToken -> 'T option = fun _ -> Some value

        [<NoDynamicInvocation>]
        member inline _.Combine
            (
                [<InlineIfLambda>] first: CancellationToken -> unit option,
                second: CancellationToken -> 'T option
            ) =
            fun (ct: CancellationToken) ->
                match first ct with
                | Some() -> second ct
                | None -> None

        [<NoDynamicInvocation>]
        member inline _.While(guard: unit -> bool, body: CancellationToken -> unit option) =
            fun (ct: CancellationToken) ->
                let mutable result = Some()

                while Option.isSome result
                      && guard () do
                    result <- body ct

                result

        [<NoDynamicInvocation>]
        member inline _.TryWith
            (body: CancellationToken -> 'T option, handler: exn -> CancellationToken -> 'T option)
            =
            fun (ct: CancellationToken) ->
                try
                    body ct
                with ex ->
                    handler ex ct

        [<NoDynamicInvocation>]
        member inline _.TryFinally
            (body: CancellationToken -> 'T option, compensation: unit -> unit)
            =
            fun (ct: CancellationToken) ->
                try
                    body ct
                finally
                    compensation ()

        [<NoDynamicInvocation>]
        member inline _.Using
            (
                resource: #IAsyncDisposableNull,
                body: #IAsyncDisposableNull -> CancellationToken -> 'T option
            ) =
            fun (ct: CancellationToken) ->
                RuntimeResultAwait.usingAsync resource (fun () -> body resource ct)

        [<NoDynamicInvocation>]
        member inline this.For(sequence: seq<'T>, body: 'T -> CancellationToken -> unit option) =
            fun (ct: CancellationToken) ->
                use enumerator = sequence.GetEnumerator()

                this.While
                    ((fun () -> enumerator.MoveNext()),
                     (fun (ct: CancellationToken) -> body enumerator.Current ct))
                    ct

        [<NoDynamicInvocation>]
        member inline this.For
            (sequence: IAsyncEnumerable<'T>, body: 'T -> CancellationToken -> unit option)
            =
            fun (ct: CancellationToken) ->
                this.Using
                    (sequence.GetAsyncEnumerator ct,
                     fun enumerator ct ->
                         this.While
                             ((fun () ->
                                 ct.ThrowIfCancellationRequested()
                                 AsyncHelpers.Await(enumerator.MoveNextAsync())
                              ),
                              (fun (ct: CancellationToken) ->
                                  ct.ThrowIfCancellationRequested()
                                  body enumerator.Current ct
                              ))
                             ct)
                    ct

        [<NoDynamicInvocation>]
        member inline _.Bind
            (
                source: CancellableOptionSource<_, _, _, _>,
                continuation: 'T -> CancellationToken -> 'U option
            ) =
            fun (ct: CancellationToken) ->
                ct.ThrowIfCancellationRequested()

                match RuntimeCancellableOptionAwait.awaitSource ct source with
                | Some value -> continuation value ct
                | None -> None

        [<NoDynamicInvocation>]
        member inline this.BindReturn
            (source: CancellableOptionSource<_, _, _, _>, mapper: 'T -> 'U)
            =
            this.Bind(source, fun value -> this.Return(mapper value))

        [<NoDynamicInvocation>]
        member inline _.ReturnFrom(source: CancellableOptionSource<_, _, _, _>) =
            fun (ct: CancellationToken) ->
                ct.ThrowIfCancellationRequested()
                RuntimeCancellableOptionAwait.awaitSource ct source

    /// <exclude/>
    [<AutoOpen>]
    module PlainSources =
        type RuntimeCancellableTaskOptionBuilderBase with

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T when Awaiter<'Awaiter, 'T>>(awaiter: 'Awaiter) =
                RuntimeCancellableOptionAwait.prepared awaiter Some

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T when Awaitable<'Awaitable, 'Awaiter, 'T>>
                (source: 'Awaitable)
                =
                RuntimeCancellableOptionAwait.prepared (Awaitable.GetAwaiter source) Some

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T when Awaiter<'Awaiter, 'T>>
                (source: CancellationToken -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> start ct)
                    Map = (Some: 'T -> 'T option)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T when Awaiter<'Awaiter, 'T>>
                (source: unit -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> start ())
                    Map = (Some: 'T -> 'T option)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T when Awaitable<'Awaitable, 'Awaiter, 'T>>
                (source: CancellationToken -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> Awaitable.GetAwaiter(start ct))
                    Map = (Some: 'T -> 'T option)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T when Awaitable<'Awaitable, 'Awaiter, 'T>>
                (source: unit -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> Awaitable.GetAwaiter(start ()))
                    Map = (Some: 'T -> 'T option)
                }

            [<NoDynamicInvocation>]
            member inline _.Using
                (
                    resource: #IDisposableNull,
                    body: #IDisposableNull -> CancellationToken -> 'T option
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
    module OptionSources =
        type RuntimeCancellableTaskOptionBuilderBase with

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Option<'T>)>
                (awaiter: 'Awaiter)
                =
                RuntimeCancellableOptionAwait.prepared awaiter id

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Option<'T>)>
                (source: 'Awaitable)
                =
                RuntimeCancellableOptionAwait.prepared (Awaitable.GetAwaiter source) id

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Option<'T>)>
                (source: CancellationToken -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> start ct)
                    Map = (id: 'T option -> 'T option)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaiter, 'T
                when 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Option<'T>)>
                (source: unit -> 'Awaiter)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> start ())
                    Map = (id: 'T option -> 'T option)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Option<'T>)>
                (source: CancellationToken -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun ct start -> Awaitable.GetAwaiter(start ct))
                    Map = (id: 'T option -> 'T option)
                }

            [<NoDynamicInvocation; NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'Awaiter, 'T
                when 'Awaitable: (member GetAwaiter: unit -> 'Awaiter)
                and 'Awaiter :> ICriticalNotifyCompletion
                and 'Awaiter: (member get_IsCompleted: unit -> bool)
                and 'Awaiter: (member GetResult: unit -> Option<'T>)>
                (source: unit -> 'Awaitable)
                =
                {
                    Source = source
                    GetAwaiter = (fun _ start -> Awaitable.GetAwaiter(start ()))
                    Map = (id: 'T option -> 'T option)
                }

            [<NoDynamicInvocation>]
            member inline _.Source(sequence: #IAsyncEnumerable<_>) = sequence

    /// <exclude/>
    [<AutoOpen>]
    module TaskSources =
        type RuntimeCancellableTaskOptionBuilderBase with

            [<NoDynamicInvocation>]
            member inline _.Source(source: CancellationToken -> Task<'T>) = {
                Source = source
                GetAwaiter = (fun ct start -> (start ct).GetAwaiter())
                Map = (Some: 'T -> 'T option)
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: unit -> Task<'T>) = {
                Source = source
                GetAwaiter = (fun _ start -> (start ()).GetAwaiter())
                Map = (Some: 'T -> 'T option)
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: Task) =
                RuntimeCancellableOptionAwait.prepared (source.GetAwaiter()) Some

            [<NoDynamicInvocation>]
            member inline _.Source(source: ValueTask) =
                RuntimeCancellableOptionAwait.prepared (source.GetAwaiter()) Some

            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<'T>) =
                RuntimeCancellableOptionAwait.prepared (source.GetAwaiter()) Some

            [<NoDynamicInvocation>]
            member inline _.Source(source: ValueTask<'T>) =
                RuntimeCancellableOptionAwait.prepared (source.GetAwaiter()) Some

            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<'T>) = {
                Source = source
                GetAwaiter =
                    (fun ct work ->
                        (Async.StartImmediateAsTask(work, cancellationToken = ct)).GetAwaiter()
                    )
                Map = (Some: 'T -> 'T option)
            }

    /// <exclude/>
    [<AutoOpen>]
    module HighPriority =
        type RuntimeCancellableTaskOptionBuilderBase with

            [<NoDynamicInvocation>]
            member inline _.Source(source: CancellationToken -> Task<'T option>) = {
                Source = source
                GetAwaiter = (fun ct start -> (start ct).GetAwaiter())
                Map = (id: 'T option -> 'T option)
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: unit -> Task<'T option>) = {
                Source = source
                GetAwaiter = (fun _ start -> (start ()).GetAwaiter())
                Map = (id: 'T option -> 'T option)
            }

            [<NoDynamicInvocation>]
            member inline _.Source(source: 'T option) =
                RuntimeCancellableOptionAwait.prepared (ValueTask<_>(source).GetAwaiter()) id

            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<'T option>) =
                RuntimeCancellableOptionAwait.prepared (source.GetAwaiter()) id

            [<NoDynamicInvocation>]
            member inline _.Source(source: ValueTask<'T option>) =
                RuntimeCancellableOptionAwait.prepared (source.GetAwaiter()) id

            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<'T option>) = {
                Source = source
                GetAwaiter =
                    (fun ct work ->
                        (Async.StartImmediateAsTask(work, cancellationToken = ct)).GetAwaiter()
                    )
                Map = (id: 'T option -> 'T option)
            }

    /// <summary>Builds a cold task of an option with .NET runtime async.</summary>
    type RuntimeCancellableTaskOptionBuilder() =
        inherit RuntimeCancellableTaskOptionBuilderBase()

        [<NoDynamicInvocation>]
        member inline _.Run
            (code: CancellationToken -> 'T option)
            : CancellationToken -> Task<'T option> =
            IcedTasks.Polyfill.RuntimeTask.RuntimeCancellableTasks.cancellableTask.Run code

        [<NoDynamicInvocation>]
        member inline _.Source(source: CancellationToken -> Task<'T option>) = {
            Source = source
            GetAwaiter = (fun ct start -> (start ct).GetAwaiter())
            Map = (id: 'T option -> 'T option)
        }

    /// <summary>Builds a cold task of an option that leaves the caller context when needed.</summary>
    type RuntimeBackgroundCancellableTaskOptionBuilder() =
        inherit RuntimeCancellableTaskOptionBuilderBase()

        [<NoDynamicInvocation>]
        member inline _.Run
            (code: CancellationToken -> 'T option)
            : CancellationToken -> Task<'T option> =
            IcedTasks.Polyfill.RuntimeTask.RuntimeCancellableTasks.backgroundCancellableTask.Run
                code

        [<NoDynamicInvocation>]
        member inline _.Source(source: CancellationToken -> Task<'T option>) = {
            Source = source
            GetAwaiter = (fun ct start -> (start ct).GetAwaiter())
            Map = (id: 'T option -> 'T option)
        }

/// <summary>Contains the opt-in runtime-async cancellable task option builders.</summary>
[<AutoOpen>]
module RuntimeCancellableTaskOptions =
    /// <summary>Builds a cold task of an option. Requires .NET 11 and preview F#.</summary>
    let cancellableTaskOption = RuntimeCancellableTaskOptionBuilder()

    /// <summary>Builds a cold task of an option that leaves the caller context when needed.</summary>
    let backgroundCancellableTaskOption =
        RuntimeBackgroundCancellableTaskOptionBuilder()

#endif
