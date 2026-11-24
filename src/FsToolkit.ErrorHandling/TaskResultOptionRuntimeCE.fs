namespace FsToolkit.ErrorHandling.Polyfill.RuntimeTask

#if FSTK_RUNTIME_ASYNC && NET11_0_OR_GREATER

open System
open System.Runtime.CompilerServices
open System.Runtime.ExceptionServices
open System.Threading.Tasks
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open FsToolkit.ErrorHandling

/// <summary>Contains runtime-async result option computations.</summary>
[<AutoOpen>]
module RuntimeTaskResultOptionBase =

    /// <summary>Provides result flow, cleanup, and source preparation for the runtime-async builder.</summary>
    type RuntimeTaskResultOptionBuilder() =

        /// <summary>Delays the computation until the builder runs it.</summary>
        [<NoDynamicInvocation>]
        member inline _.Delay(generator: unit -> Result<'T option, 'Error>) = generator

        /// <summary>Returns a successful computation with no value.</summary>
        [<NoDynamicInvocation>]
        member inline _.Zero() : Result<unit option, 'Error> = Ok(Some())

        /// <summary>Returns the value in <c>Ok</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Return(value: 'T) : Result<'T option, 'Error> = Ok(Some value)

        /// <summary>Runs the second computation only when the first returns <c>Ok(Some())</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Combine
            (first: Result<unit option, 'Error>, second: unit -> Result<'T option, 'Error>)
            : Result<'T option, 'Error> =
            match first with
            | Ok(Some()) -> second ()
            | Ok None -> Ok None
            | Error error -> Error error

        /// <summary>Repeats the body while the guard is true, and stops at <c>Error</c> or <c>Ok None</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.While
            (guard: unit -> bool, body: unit -> Result<unit option, 'Error>)
            : Result<unit option, 'Error> =
            let mutable result = Ok(Some())

            while (match result with
                   | Ok(Some()) -> true
                   | _ -> false)
                  && guard () do
                result <- body ()

            result

        /// <summary>Runs the handler when the body throws an exception.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryWith
            (body: unit -> Result<'T option, 'Error>, handler: exn -> Result<'T option, 'Error>)
            =
            try
                body ()
            with ex ->
                handler ex

        /// <summary>Runs compensation after the body, including <c>Error</c>, <c>Ok None</c>, exceptions, and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.TryFinally
            (body: unit -> Result<'T option, 'Error>, compensation: unit -> unit)
            =
            try
                body ()
            finally
                compensation ()

        /// <summary>Runs the body and disposes a non-null resource, including failures and cancellation.</summary>
        [<NoDynamicInvocation>]
        member inline _.Using
            (
                resource: #IAsyncDisposableNull,
                body: #IAsyncDisposableNull -> Result<'T option, 'Error>
            ) =
            RuntimeResultAwait.usingAsync resource (fun () -> body resource)

        /// <summary>Visits each element until the body returns <c>Error</c>, and disposes the enumerator.</summary>
        [<NoDynamicInvocation>]
        member inline this.For(sequence: seq<'T>, body: 'T -> Result<unit option, 'Error>) =
            use enumerator = sequence.GetEnumerator()
            this.While((fun () -> enumerator.MoveNext()), (fun () -> body enumerator.Current))

        /// <summary>Awaits the source and calls the continuation only for <c>Ok(Some value)</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.Bind
            (
                source: StartedResult<_, _, _, 'T option, 'Error>,
                continuation: 'T -> Result<'U option, 'Error>
            ) : Result<'U option, 'Error> =
            match RuntimeResultAwait.awaitSource source with
            | Ok(Some value) -> continuation value
            | Ok None -> Ok None
            | Error error -> Error error

        /// <summary>Awaits the source and maps the value only for <c>Ok(Some value)</c>.</summary>
        [<NoDynamicInvocation>]
        member inline this.BindReturn
            (source: StartedResult<_, _, _, 'T option, 'Error>, mapper: 'T -> 'U)
            =
            match RuntimeResultAwait.awaitSource source with
            | Ok(Some value) -> Ok(Some(mapper value))
            | Ok None -> Ok None
            | Error error -> Error error

        /// <summary>Awaits the source and returns its <c>Result</c>.</summary>
        [<NoDynamicInvocation>]
        member inline _.ReturnFrom(source: StartedResult<_, _, _, 'T option, 'Error>) =
            RuntimeResultAwait.awaitSource source

        /// <summary>Prepares a task of a result option.</summary>
        [<NoDynamicInvocation>]
        member inline _.Source(source: Task<Result<'T option, 'Error>>) =
            RuntimeResultAwait.prepared (source.GetAwaiter()) id

        /// <summary>Runs the computation as a runtime-async method.</summary>
        [<NoDynamicInvocation>]
        member inline _.Run
            (code: unit -> Result<'T option, 'Error>)
            : Task<Result<'T option, 'Error>> =
            __runtimeAsyncReturn (code ())

    /// <exclude/>
    [<AutoOpen>]
    module PlainOptionSources =

        type RuntimeTaskResultOptionBuilder with

            /// <summary>Prepares a plain task value.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<'T>) =
                RuntimeResultAwait.prepared (source.GetAwaiter()) (Some >> Ok)

            /// <summary>Prepares a plain async value.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<'T>) =
                let started = Async.StartImmediateAsTask source
                RuntimeResultAwait.prepared (started.GetAwaiter()) (Some >> Ok)

            /// <summary>Prepares a sequence for iteration.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: #seq<_>) = source

            /// <summary>Disposes a synchronous resource after the body.</summary>
            [<NoDynamicInvocation>]
            member inline _.Using
                (resource: #IDisposableNull, body: #IDisposableNull -> Result<'T option, 'Error>)
                =
                try
                    body resource
                finally
                    if not (isNull (box resource)) then
                        resource.Dispose()

    /// <exclude/>
    [<AutoOpen>]
    module ResultOptionSources =

        type RuntimeTaskResultOptionBuilder with

            /// <summary>Prepares a result value.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Result<'T, 'Error>) =
                RuntimeResultAwait.prepared (ValueTask<_>(source).GetAwaiter()) (Result.map Some)

            /// <summary>Prepares a choice value.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Choice<'T, 'Error>) =
                RuntimeResultAwait.prepared
                    (ValueTask<_>(source).GetAwaiter())
                    (Result.ofChoice
                     >> Result.map Some)

            /// <summary>Prepares an option value.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: 'T option) =
                RuntimeResultAwait.prepared (ValueTask<_>(source).GetAwaiter()) Ok

            /// <summary>Prepares a task result value.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<Result<'T, 'Error>>) =
                RuntimeResultAwait.prepared (source.GetAwaiter()) (Result.map Some)

            /// <summary>Prepares an async result value.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Async<Result<'T, 'Error>>) =
                let started = Async.StartImmediateAsTask source
                RuntimeResultAwait.prepared (started.GetAwaiter()) (Result.map Some)

            /// <summary>Prepares a task option value.</summary>
            [<NoDynamicInvocation>]
            member inline _.Source(source: Task<'T option>) =
                RuntimeResultAwait.prepared (source.GetAwaiter()) Ok

/// <summary>Contains the runtime-async result option builder.</summary>
[<AutoOpen>]
module RuntimeTaskResultOptions =

    /// <summary>Builds a task of a result option with runtime async.</summary>
    let taskResultOption = RuntimeTaskResultOptionBuilder()

#endif
