namespace FsToolkit.ErrorHandling

open System
open System.ComponentModel
open System.Runtime.CompilerServices
open System.Threading.Tasks

/// <exclude/>
/// <summary>An awaiter whose result <c>let!</c> binds as it is.</summary>
/// <remarks>
/// The <c>Source</c> members of the task builders wrap each awaiter whose result is not the builder's
/// <c>Result</c>, <c>Validation</c>, or <c>Option</c> type. Then an awaiter of the builder's own type matches only one
/// <c>Bind</c> overload, and F# can infer the type of the bound value before it checks the continuation.
/// A custom <c>Source</c> extension that returns an awaiter of any other value must return it as
/// <c>PlainAwaiter(awaiter)</c>, or <c>fun ct -> PlainAwaiter(awaiter)</c> for a cancellable source.
/// </remarks>
[<Struct; NoComparison; NoEquality>]
[<EditorBrowsable(EditorBrowsableState.Never)>]
type PlainAwaiter<'Awaiter> =
    /// <summary>The awaiter.</summary>
    val Awaiter: 'Awaiter
    /// <summary>Wraps an awaiter.</summary>
    new(awaiter) = { Awaiter = awaiter }

/// <exclude/>
[<EditorBrowsable(EditorBrowsableState.Never)>]
[<RequireQualifiedAccess>]
module AwaiterTask =

    /// <summary>Gives a task that completes with the result of an awaiter.</summary>
    /// <remarks>Call it only one time for each awaiter, because a <c>ValueTask</c> awaiter allows one <c>GetResult</c>.</remarks>
    let inline ofAwaiter< ^Awaiter, 'T
        when ^Awaiter :> ICriticalNotifyCompletion
        and ^Awaiter: (member get_IsCompleted: unit -> bool)
        and ^Awaiter: (member GetResult: unit -> 'T)>
        (awaiter: ^Awaiter)
        : Task<'T> =
        // AsyncTaskMethodBuilder keeps the exception object of a canceled source. TaskCompletionSource keeps only the token.
        let builder = ref (AsyncTaskMethodBuilder<'T>.Create())
        let task = builder.contents.Task

        let complete () =
            try
                builder.contents.SetResult((^Awaiter: (member GetResult: unit -> 'T) awaiter))
            with e ->
                builder.contents.SetException e

        if (^Awaiter: (member get_IsCompleted: unit -> bool) awaiter) then
            complete ()
        else
            // OnCompleted runs complete in the caller's ExecutionContext. UnsafeOnCompleted does not.
            (awaiter :> INotifyCompletion).OnCompleted(fun () -> complete ())

        task
