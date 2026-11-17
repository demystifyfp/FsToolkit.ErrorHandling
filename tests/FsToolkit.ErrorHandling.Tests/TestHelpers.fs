namespace FsToolkit.ErrorHandling

#if !FABLE_COMPILER
open System.Threading.Tasks
#endif

[<RequireQualifiedAccess>]
module Async =

    open System

#if FABLE_COMPILER && FABLE_COMPILER_JAVASCRIPT
    open Fable.Core

    /// An Async that never completes but can be cancelled
    let never<'a> : Async<'a> =
        Fable.Core.JS.Constructors.Promise.Create(fun _ _ -> ())
        |> Async.AwaitPromise
#else
    /// An Async that never completes but can be cancelled
    let never<'a> : Async<'a> =
        let granularity = TimeSpan.FromSeconds 3.

        let rec loop () =
            async {
                do! Async.Sleep(granularity)
                return! loop ()
            }

        loop ()
#endif

module TestHelpers =
    let makeDisposable (callback) =
        { new System.IDisposable with
            member this.Dispose() = callback ()
        }

    let makeAsyncDisposable (callback) =
        { new System.IAsyncDisposable with
            member this.DisposeAsync() = callback ()
        }

#if !FABLE_COMPILER
    type DynamicTaskResultBuilder() =
        inherit TaskResultBuilderBase()

        member _.Run(code: TaskResultCode<'T, 'Error, 'T>) = TaskResultBuilder.RunDynamic(code)

    type DynamicBackgroundTaskResultBuilder() =
        inherit TaskResultBuilderBase()

        member _.Run(code: TaskResultCode<'T, 'Error, 'T>) =
            BackgroundTaskResultBuilder.RunDynamic(code)

    type DynamicTaskOptionBuilder() =
        inherit TaskOptionBuilderBase()

        member _.Run(code: TaskOptionCode<'T, 'T>) = TaskOptionBuilder.RunDynamic(code)

    type DynamicBackgroundTaskOptionBuilder() =
        inherit TaskOptionBuilderBase()

        member _.Run(code: TaskOptionCode<'T, 'T>) =
            BackgroundTaskOptionBuilder.RunDynamic(code)

    type DynamicTaskValueOptionBuilder() =
        inherit TaskValueOptionBuilderBase()

        member _.Run(code: TaskValueOptionCode<'T, 'T>) = TaskValueOptionBuilder.RunDynamic(code)

    type DynamicBackgroundTaskValueOptionBuilder() =
        inherit TaskValueOptionBuilderBase()

        member _.Run(code: TaskValueOptionCode<'T, 'T>) =
            BackgroundTaskValueOptionBuilder.RunDynamic(code)

    type DynamicTaskValidationBuilder() =
        inherit TaskValidationBuilderBase()

        member _.Run(code: TaskValidationCode<'T, 'Error, 'T>) =
            TaskValidationBuilder.RunDynamic(code)

    type DynamicBackgroundTaskValidationBuilder() =
        inherit TaskValidationBuilderBase()

        member _.Run(code: TaskValidationCode<'T, 'Error, 'T>) =
            BackgroundTaskValidationBuilder.RunDynamic(code)

    let dynamicTaskResult = DynamicTaskResultBuilder()
    let dynamicBackgroundTaskResult = DynamicBackgroundTaskResultBuilder()

    let dynamicTaskOption = DynamicTaskOptionBuilder()
    let dynamicBackgroundTaskOption = DynamicBackgroundTaskOptionBuilder()

    let dynamicTaskValueOption = DynamicTaskValueOptionBuilder()

    let dynamicBackgroundTaskValueOption = DynamicBackgroundTaskValueOptionBuilder()

    let dynamicTaskValidation = DynamicTaskValidationBuilder()

    let dynamicBackgroundTaskValidation = DynamicBackgroundTaskValidationBuilder()

    let assertWhileShortCircuitWaitsForAsyncDisposal
        (expected: 'Result)
        (hasEarlierSuspension: bool)
        (start: Task -> System.IAsyncDisposable -> Task<'Result>)
        =
        task {
            let earlierSuspension =
                TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

            let disposalStarted =
                TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

            let releaseDisposal =
                TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

            let disposalCompleted =
                TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

            let disposable =
                makeAsyncDisposable (fun () ->
                    disposalStarted.SetResult(())

                    ValueTask(
                        task {
                            do! releaseDisposal.Task
                            disposalCompleted.SetResult(())
                        }
                    )
                )

            let resultTask = start earlierSuspension.Task disposable

            if hasEarlierSuspension then
                Expecto.Expect.isFalse
                    resultTask.IsCompleted
                    "The computation must suspend before the loop"

                Expecto.Expect.isFalse
                    disposalStarted.Task.IsCompleted
                    "Disposal must not start before the earlier suspension completes"

            earlierSuspension.SetResult(())
            do! disposalStarted.Task

            let completedBeforeDisposal = resultTask.IsCompleted
            releaseDisposal.SetResult(())
            do! disposalCompleted.Task
            let! actual = resultTask

            Expecto.Expect.isFalse
                completedBeforeDisposal
                "The result task must wait for asynchronous disposal"

            Expecto.Expect.equal actual expected "The short-circuit result must be preserved"
        }

    let assertWhileErrorWaitsForAsyncDisposal hasEarlierSuspension start =
        assertWhileShortCircuitWaitsForAsyncDisposal (Error "error") hasEarlierSuspension start

    type AsyncDisposalOutcome =
        | DisposalSucceeds
        | DisposalFaults
        | DisposalCancels

    type AsyncDisposalBody =
        | BodyCompletes
        | BodyThrows
        | BodyShortCircuits

    type ObservedDisposalSource() =
        let mutable core =
            System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore<unit>()

        let continuationRegistered =
            TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

        let mutable getResultCalls = 0
        do core.RunContinuationsAsynchronously <- true

        member _.ContinuationRegistered = continuationRegistered.Task
        member _.GetResultCalls = getResultCalls

        member this.ValueTask() =
            ValueTask(this :> System.Threading.Tasks.Sources.IValueTaskSource, core.Version)

        member _.Complete(outcome) =
            match outcome with
            | DisposalSucceeds -> core.SetResult(())
            | DisposalFaults -> core.SetException(System.InvalidOperationException "dispose")
            | DisposalCancels -> core.SetException(System.OperationCanceledException "dispose")

        interface System.Threading.Tasks.Sources.IValueTaskSource with
            member _.GetStatus(token) = core.GetStatus(token)

            member _.OnCompleted(continuation, state, token, flags) =
                core.OnCompleted(continuation, state, token, flags)

                continuationRegistered.TrySetResult(())
                |> ignore

            member _.GetResult(token) =
                getResultCalls <-
                    getResultCalls
                    + 1

                core.GetResult(token)

    let singleItemAsyncEnumerable (disposeAsync: unit -> ValueTask) =
        { new System.Collections.Generic.IAsyncEnumerable<int> with
            member _.GetAsyncEnumerator(_) =
                let moved = ref false

                { new System.Collections.Generic.IAsyncEnumerator<int> with
                    member _.MoveNextAsync() =
                        let hasItem = not moved.Value
                        moved.Value <- true
                        ValueTask.FromResult hasItem

                    member _.Current = 1
                    member _.DisposeAsync() = disposeAsync ()
                }
        }

    let assertAsyncDisposalOutcome
        (outcome: AsyncDisposalOutcome)
        (isPending: bool)
        (body: AsyncDisposalBody)
        (start: (unit -> ValueTask) -> Task<'Result>)
        =
        task {
            let source = ObservedDisposalSource()

            if not isPending then
                source.Complete outcome

            let resultTask = start source.ValueTask

            if isPending then
                do! source.ContinuationRegistered.WaitAsync(System.TimeSpan.FromSeconds 10.)
                source.Complete outcome

            let! observed =
                task {
                    try
                        let! _ = resultTask
                        return "completed"
                    with
                    | :? System.InvalidOperationException as e -> return $"faulted: {e.Message}"
                    | :? System.ArgumentException as e -> return $"faulted: {e.Message}"
                    | :? System.OperationCanceledException -> return "canceled"
                }

            let expected, expectedStatus =
                match outcome, body with
                | DisposalFaults, _ -> "faulted: dispose", TaskStatus.Faulted
                | DisposalCancels, _ -> "canceled", TaskStatus.Canceled
                | DisposalSucceeds, BodyThrows -> "faulted: body", TaskStatus.Faulted
                | DisposalSucceeds, (BodyCompletes | BodyShortCircuits) ->
                    "completed", TaskStatus.RanToCompletion

            Expecto.Expect.equal
                observed
                expected
                "A DisposeAsync fault or cancellation replaces the body result"

            Expecto.Expect.equal resultTask.Status expectedStatus "The result task status"

            Expecto.Expect.equal
                source.GetResultCalls
                1
                "The DisposeAsync result must be read exactly once"
        }

    let private asyncDisposalOutcomeCases
        name
        (start: bool -> AsyncDisposalBody -> (unit -> ValueTask) -> Task<'Result>)
        =
        Expecto.Tests.testList name [
            for isDynamic in
                [
                    false
                    true
                ] do
                for outcome in
                    [
                        DisposalSucceeds
                        DisposalFaults
                        DisposalCancels
                    ] do
                    for isPending in
                        [
                            false
                            true
                        ] do
                        for body in
                            [
                                BodyCompletes
                                BodyThrows
                                BodyShortCircuits
                            ] do
                            Expecto.Tests.testCaseTask
                                (sprintf
                                    "%s execution, %A, %s disposal, %A"
                                    (if isDynamic then "dynamic" else "static")
                                    outcome
                                    (if isPending then "pending" else "completed")
                                    body)
                                (fun () ->
                                    assertAsyncDisposalOutcome
                                        outcome
                                        isPending
                                        body
                                        (start isDynamic body)
                                )
        ]

    let asyncDisposalOutcomeTests
        name
        (useStatic: AsyncDisposalBody -> System.IAsyncDisposable -> Task<'Result>)
        (useDynamic: AsyncDisposalBody -> System.IAsyncDisposable -> Task<'Result>)
        =
        asyncDisposalOutcomeCases
            name
            (fun isDynamic body disposeAsync ->
                (if isDynamic then useDynamic else useStatic)
                    body
                    (makeAsyncDisposable disposeAsync)
            )

    let asyncEnumeratorDisposalOutcomeTests
        name
        (forStatic:
            AsyncDisposalBody -> System.Collections.Generic.IAsyncEnumerable<int> -> Task<'Result>)
        (forDynamic:
            AsyncDisposalBody -> System.Collections.Generic.IAsyncEnumerable<int> -> Task<'Result>)
        =
        asyncDisposalOutcomeCases
            name
            (fun isDynamic body disposeAsync ->
                (if isDynamic then forDynamic else forStatic)
                    body
                    (singleItemAsyncEnumerable disposeAsync)
            )
#endif

#if !FABLE_COMPILER
    open System.Collections.Generic
    open System.Threading.Tasks

    /// Creates a simple IAsyncEnumerable<'T> from a list, for use in tests.
    let toAsyncEnumerable (items: 'T list) : IAsyncEnumerable<'T> =
        let arr = List.toArray items

        { new IAsyncEnumerable<'T> with
            member _.GetAsyncEnumerator(_ct) =
                let mutable index = -1

                { new IAsyncEnumerator<'T> with
                    member _.MoveNextAsync() =
                        index <- index + 1
                        ValueTask.FromResult(index < arr.Length)

                    member _.Current = arr.[index]
                    member _.DisposeAsync() = ValueTask()
                }
        }
#endif

type MemoryStreamNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    System.IO.MemoryStream | null
#else
    System.IO.MemoryStream
#endif

type UriNull =
#if NET9_0_OR_GREATER && !FABLE_COMPILER
    System.Uri | null
#else
    System.Uri
#endif
