namespace FsToolkit.ErrorHandling

open System.ComponentModel

/// <exclude/>
/// <summary>An awaitable whose result <c>let!</c> binds as it is.</summary>
/// <remarks>
/// The <c>Source</c> members of <c>taskResult</c>, <c>taskValidation</c>, <c>taskOption</c>, <c>taskValueOption</c>,
/// and <c>valueTaskValueOption</c> wrap an awaitable of a plain value, such as a <c>ConfiguredTaskAwaitable</c>.
/// Then <c>Bind</c> awaits the awaiter of that awaitable, so the code after a suspended <c>let!</c> resumes where the
/// awaiter puts it, as in <c>task { }</c>. <c>MergeSources</c> gets the awaiter only after F# evaluates all
/// <c>and!</c> operands, as <c>and!</c> in <c>task { }</c> does.
/// </remarks>
[<Struct; NoComparison; NoEquality>]
[<EditorBrowsable(EditorBrowsableState.Never)>]
type PlainAwaitable<'TaskLike> =
    /// <summary>The awaitable.</summary>
    val Awaitable: 'TaskLike
    /// <summary>Wraps an awaitable.</summary>
    new(awaitable) = { Awaitable = awaitable }
