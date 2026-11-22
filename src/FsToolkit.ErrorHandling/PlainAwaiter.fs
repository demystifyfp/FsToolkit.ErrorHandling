namespace FsToolkit.ErrorHandling

open System.ComponentModel

/// <exclude/>
/// <summary>The awaiter of an awaitable whose result <c>let!</c> binds as it is.</summary>
/// <remarks>
/// The <c>Source</c> members of <c>taskResult</c>, <c>taskValidation</c>, <c>taskOption</c>, <c>taskValueOption</c>,
/// and <c>valueTaskValueOption</c> wrap the awaiter of an awaitable of a plain value, such as a
/// <c>ConfiguredTaskAwaitable</c>. Then <c>Bind</c> awaits that awaiter, so the code after a suspended <c>let!</c>
/// resumes where the awaiter puts it, as in <c>task { }</c>.
/// </remarks>
[<Struct; NoComparison; NoEquality>]
[<EditorBrowsable(EditorBrowsableState.Never)>]
type PlainAwaiter<'Awaiter> =
    /// <summary>The awaiter.</summary>
    val Awaiter: 'Awaiter
    /// <summary>Wraps an awaiter.</summary>
    new(awaiter) = { Awaiter = awaiter }

/// <exclude/>
/// <summary>An awaitable that gives an awaiter that already exists.</summary>
/// <remarks>The <c>MergeSources</c> members use it to await a <c>PlainAwaiter</c> in <c>task { }</c>.</remarks>
[<Struct; NoComparison; NoEquality>]
[<EditorBrowsable(EditorBrowsableState.Never)>]
type AwaiterAwaitable<'Awaiter> =
    /// <summary>The awaiter.</summary>
    val Awaiter: 'Awaiter
    /// <summary>Wraps an awaiter.</summary>
    new(awaiter) = { Awaiter = awaiter }
    /// <summary>Gives the awaiter.</summary>
    member this.GetAwaiter() = this.Awaiter
