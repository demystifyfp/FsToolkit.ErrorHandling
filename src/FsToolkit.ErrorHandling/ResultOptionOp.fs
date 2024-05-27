namespace FsToolkit.ErrorHandling.Operator.ResultOption

open FsToolkit.ErrorHandling

/// <summary>
/// Operators for working with the <c>Result</c> and <c>Option</c> types.
/// </summary>
[<AutoOpen>]
module ResultOption =

    /// <summary>
    /// Shorthand for <c>ResultOption.map</c>
    /// </summary>
    /// <param name="mapper">The function to map over the <c>Result</c> value.</param>
    /// <param name="input">The <c>Result</c> value to map over.</param>
    /// <returns>The result of mapping the function over the <c>Result</c> value.</returns>
    let inline (<!>)
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: Result<'okInput option, 'error>)
        : Result<'okOutput option, 'error> =
        ResultOption.map mapper input

    /// <summary>
    /// Shortand for <c>ResultOption.apply</c>
    /// </summary>
    /// <param name="applier">The <c>Result</c> value containing the function to apply.</param>
    /// <param name="input">The <c>Result</c> value containing the value to apply the function to.</param>
    /// <returns>The result of applying the function in the <c>Result</c> value to the value in the other <c>Result</c> value.</returns>
    let inline (<*>)
        (applier: Result<('okInput -> 'okOutput) option, 'error>)
        (input: Result<'okInput option, 'error>)
        : Result<'okOutput option, 'error> =
        ResultOption.apply applier input

    /// <summary>
    /// Shorthand for <c>ResultOption.apply</c>, where the value is wrapped in an <c>Option</c> for you
    /// </summary>
    /// <param name="applier">The <c>Result</c> value containing the function to apply.</param>
    /// <param name="input">The <c>Result</c> value containing the value to apply the function to.</param>
    /// <returns>The result of applying the function in the <c>Result</c> value to the value in the other <c>Result</c> value.</returns>
    let inline (<*^>)
        (applier: Result<('okInput -> 'okOutput) option, 'error>)
        (input: Result<'okInput, 'error>)
        : Result<'okOutput option, 'error> =
        ResultOption.apply applier (Result.map Some input)

    /// <summary>
    /// Shorthand for <c>ResultOption.bind</c>
    /// </summary>
    /// <param name="input">The <c>Result</c> value to bind over.</param>
    /// <param name="binder">The function to bind over the <c>Result</c> value.</param>
    /// <returns>The result of binding the function over the <c>Result</c> value.</returns>
    let inline (>>=)
        (input: Result<'okInput option, 'error>)
        ([<InlineIfLambda>] binder: 'okInput -> Result<'okOutput option, 'error>)
        : Result<'okOutput option, 'error> =
        ResultOption.bind binder input
