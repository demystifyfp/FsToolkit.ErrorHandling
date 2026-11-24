namespace FsToolkit.ErrorHandling.Polyfill.RuntimeTask

#if FSTK_RUNTIME_ASYNC && NET11_0_OR_GREATER

open System.Threading.Tasks

/// <summary>Contains the opt-in runtime-async value task value option builder.</summary>
[<AutoOpen>]
module RuntimeValueTaskValueOptions =
    /// <summary>Builds a value task of a value option with .NET runtime async.</summary>
    type RuntimeValueTaskValueOptionBuilder() =
        inherit RuntimeTaskValueOptionBuilderBase()

        [<NoDynamicInvocation>]
        member inline _.Run(code: unit -> 'T voption) : ValueTask<'T voption> =
            IcedTasks.Polyfill.RuntimeTask.RuntimeTasks.valueTask.Run code

        [<NoDynamicInvocation>]
        member inline _.Source(source: ValueTask<'T voption>) =
            let started = RuntimeOptionAwait.startValueTask source
            RuntimeOptionAwait.prepared (started.GetAwaiter()) id

    /// <summary>Builds a value task of a value option. Requires .NET 11 and preview F#.</summary>
    let valueTaskValueOption = RuntimeValueTaskValueOptionBuilder()

#endif
