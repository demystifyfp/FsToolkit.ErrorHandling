namespace FsToolkit.ErrorHandling

open System.ComponentModel

/// <exclude/>
/// <summary>An awaitable whose result <c>let!</c> binds as it is.</summary>
/// <remarks>
/// The <c>Source</c> members of <c>taskResult</c>, <c>taskValidation</c>, <c>taskOption</c>, and <c>taskValueOption</c>
/// wrap an awaitable of a plain value, such as a <c>ConfiguredTaskAwaitable</c>. Then <c>Bind</c> awaits the
/// awaiter of that awaitable, so the code after <c>let!</c> continues where the awaiter puts it, as in <c>task { }</c>.
/// </remarks>
[<Struct; NoComparison; NoEquality>]
[<EditorBrowsable(EditorBrowsableState.Never)>]
type PlainAwaitable<'TaskLike> =
    /// <summary>The awaitable.</summary>
    val Awaitable: 'TaskLike
    /// <summary>Wraps an awaitable.</summary>
    new(awaitable) = { Awaitable = awaitable }
