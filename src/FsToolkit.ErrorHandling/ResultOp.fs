namespace FsToolkit.ErrorHandling.Operator.Result

open FsToolkit.ErrorHandling

/// <summary>
/// Operators for working with the <c>Result</c> type.
/// </summary>
[<AutoOpen>]
module Result =

    /// <summary>
    /// Shorthand for <c>Result.map</c>
    /// </summary>
    /// <param name="mapper">The function to map over the <c>Result</c> value.</param>
    /// <param name="input">The <c>Result</c> value to map over.</param>
    /// <returns>The result of mapping the function over the <c>Result</c> value.</returns>
    let inline (<!>)
        (([<InlineIfLambda>] mapper: 'okInput -> 'okOutput))
        (input: Result<'okInput, 'error>)
        : Result<'okOutput, 'error> =
        Result.map mapper input

    /// <summary>
    /// Shorthand for <c>Result.apply</c>
    /// </summary>
    /// <param name="applier">The <c>Result</c> value containing the function to apply.</param>
    /// <param name="input">The <c>Result</c> value containing the value to apply the function to.</param>
    /// <returns>The result of applying the function in the <c>Result</c> value to the value in the other <c>Result</c> value.</returns>
    let inline (<*>)
        (applier: Result<'okInput -> 'okOutput, 'error>)
        (input: Result<'okInput, 'error>)
        : Result<'okOutput, 'error> =
        Result.apply applier input

    /// <summary>
    /// Shorthand for <c>Result.bind</c>
    /// </summary>
    /// <param name="input">The <c>Result</c> value to bind over.</param>
    /// <param name="binder">The function to bind over the <c>Result</c> value.</param>
    /// <returns>The result of binding the function over the <c>Result</c> value.</returns>
    let inline (>>=)
        (input: Result<'input, 'error>)
        ([<InlineIfLambda>] binder: 'input -> Result<'okOutput, 'error>)
        : Result<'okOutput, 'error> =
        Result.bind binder input
