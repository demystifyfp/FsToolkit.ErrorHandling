namespace FsToolkit.ErrorHandling.Operator.Option

open FsToolkit.ErrorHandling

/// <summary>
/// Operators for working with the <c>Option</c> type.
/// </summary>
[<AutoOpen>]
module Option =

    /// <summary>
    /// Shorthand for <c>Option.map</c>
    /// </summary>
    /// <param name="input">The <c>Option</c> value to bind over.</param>
    /// <param name="binder">The function to bind over the <c>Option</c> value.</param>
    /// <returns>The result of binding the function over the <c>Option</c> value.</returns>
    let inline (>>=)
        (input: 'input option)
        ([<InlineIfLambda>] binder: 'input -> 'output option)
        : 'output option =
        Option.bind binder input
