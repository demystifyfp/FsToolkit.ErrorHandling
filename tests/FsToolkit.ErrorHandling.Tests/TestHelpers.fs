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

    let dynamicTaskResult = DynamicTaskResultBuilder()
    let dynamicBackgroundTaskResult = DynamicBackgroundTaskResultBuilder()

    let assertWhileErrorWaitsForAsyncDisposal
        (hasEarlierSuspension: bool)
        (start: Task -> System.IAsyncDisposable -> Task<Result<unit, string>>)
        =
        task {
            let expected = Error "error"

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

            Expecto.Expect.equal actual expected "The loop error must be preserved"
        }
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
