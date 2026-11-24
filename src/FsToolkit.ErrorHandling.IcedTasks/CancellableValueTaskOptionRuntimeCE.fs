namespace FsToolkit.ErrorHandling.Polyfill.RuntimeTask
#if FSTK_RUNTIME_ASYNC && NET11_0_OR_GREATER
open System
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask

[<AutoOpen>]
module RuntimeCancellableValueTaskOptions =
    type RuntimeCancellableValueTaskOptionBuilder() =
        inherit RuntimeCancellableTaskOptionBuilderBase()

        [<NoDynamicInvocation>]
        member inline _.Run
            (code: CancellationToken -> 'T option)
            : CancellationToken -> ValueTask<'T option> =
            fun ct ->
                if ct.IsCancellationRequested then
                    ValueTask.FromCanceled<'T option>(ct)
                else
                    __runtimeAsyncReturnValueTask (code ct)

        [<NoDynamicInvocation>]
        member inline _.Source(source: CancellationToken -> ValueTask<'T option>) = {
            Source = source
            GetAwaiter = (fun ct start -> (start ct).GetAwaiter())
            Map = id
        }

    let cancellableValueTaskOption = RuntimeCancellableValueTaskOptionBuilder()

    type RuntimeBackgroundCancellableValueTaskOptionBuilder() =
        inherit RuntimeCancellableTaskOptionBuilderBase()

        [<NoDynamicInvocation>]
        member inline _.Run
            (code: CancellationToken -> 'T option)
            : CancellationToken -> ValueTask<'T option> =
            fun ct ->
                if ct.IsCancellationRequested then
                    ValueTask.FromCanceled<'T option>(ct)
                elif
                    isNull SynchronizationContext.Current
                    && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
                then
                    __runtimeAsyncReturnValueTask (code ct)
                else
                    let run () : Task<'T option> = __runtimeAsyncReturn (code ct)
                    ValueTask<'T option>(Task.Run<'T option>(Func<Task<'T option>>(run), ct))

        [<NoDynamicInvocation>]
        member inline _.Source(source: CancellationToken -> ValueTask<'T option>) = {
            Source = source
            GetAwaiter = (fun ct start -> (start ct).GetAwaiter())
            Map = id
        }

    let backgroundCancellableValueTaskOption =
        RuntimeBackgroundCancellableValueTaskOptionBuilder()
#endif
