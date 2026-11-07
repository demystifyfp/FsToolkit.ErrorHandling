#if FABLE_COMPILER || !NET9_0_OR_GREATER
[<AutoOpen>]
#else
[<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
[<System.Obsolete "These shims are included solely to support inline methods compiled against builds using older FSharp.Core versions.">]
#endif
module FsToolkit.ErrorHandling.FSharpCore11ResultShims

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
#if NET9_0_OR_GREATER && !FABLE_COMPILER
[<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
[<System.Obsolete "These shims are included solely to support inline methods compiled against builds using older FSharp.Core versions.">]
#endif
module Result =

    /// Applies a transformation to the value of a <c>Result</c> to a new value using the specified mapper function.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/map</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the value of the <c>Result</c> if it is <c>Ok</c>.</param>
    /// <param name="input">The <c>Result</c> to map.</param>
    /// <returns>A new <c>Result</c>with the mapped value if the input <c>Result</c> is <c>Ok</c>, otherwise the original <c>Error</c>.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: Result<'okInput, 'error>)
        : Result<'okOutput, 'error> =
#if FABLE_COMPILER
        match input with
        | Ok x -> Ok(mapper x)
        | Error e -> Error e
#else
        Microsoft.FSharp.Core.Result.map mapper input
#endif

    /// <summary>
    /// Maps the error value of a <c>Result</c>to a new error value using the specified error mapper function.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/maperror</href>
    /// </summary>
    /// <param name="errorMapper">The function that maps the input error value to the output error value.</param>
    /// <param name="input">The <c>Result</c>value to map the error value of.</param>
    /// <returns>A new <c>Result</c>with the same Ok value and the mapped error value.</returns>
    let inline mapError
        ([<InlineIfLambda>] errorMapper: 'errorInput -> 'errorOutput)
        (input: Result<'ok, 'errorInput>)
        : Result<'ok, 'errorOutput> =
#if FABLE_COMPILER
        // Fable's intrinsic can generate an unbound variable for unit-valued errors.
        match input with
        | Ok x -> Ok x
        | Error e -> Error(errorMapper e)
#else
        Microsoft.FSharp.Core.Result.mapError errorMapper input
#endif

    /// <summary>
    /// Takes a transformation function and applies it to the <c>Result</c> if it is <c>Ok</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/bind</href>
    /// </summary>
    /// <param name="binder">The transformation function</param>
    /// <param name="input">The input result</param>
    /// <typeparam name="'okInput">The type of the successful result.</typeparam>
    /// <typeparam name="'okOutput">The type of the result after binding.</typeparam>
    /// <typeparam name="'error">The type of the error.</typeparam>
    /// <returns>Returns a new <c>Result</c> if the input is <c>Ok</c>, otherwise returns the original <c>Result</c></returns>
    let inline bind
        ([<InlineIfLambda>] binder: 'okInput -> Result<'okOutput, 'error>)
        (input: Result<'okInput, 'error>)
        : Result<'okOutput, 'error> =
#if FABLE_COMPILER
        // Fable's intrinsic can generate invalid Python for closures in computation expressions.
        match input with
        | Ok x -> binder x
        | Error e -> Error e
#else
        Microsoft.FSharp.Core.Result.bind binder input
#endif

    /// <summary>
    /// Determines whether the specified <c>Result</c> is in a successful state.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#isok</href>
    /// </summary>
    /// <param name="value">The result to check.</param>
    /// <returns>True if the result is in a successful state; otherwise, false.</returns>
    let inline isOk (value: Result<'ok, 'error>) : bool =
#if NET9_0_OR_GREATER
        Microsoft.FSharp.Core.Result.isOk value
#else
        match value with
        | Ok _ -> true
        | Error _ -> false
#endif

    /// <summary>
    /// Determines whether the specified <c>Result</c> is an error.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#iserror</href>
    /// </summary>
    /// <param name="value">The result to check.</param>
    /// <returns>True if the result is an error; otherwise, false.</returns>
    let inline isError (value: Result<'ok, 'error>) : bool =
#if NET9_0_OR_GREATER
        Microsoft.FSharp.Core.Result.isError value
#else
        match value with
        | Ok _ -> false
        | Error _ -> true
#endif

    /// <summary>
    /// Returns the contained value if <c>Ok</c>, otherwise returns the provided value
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#defaultvalue</href>
    /// </summary>
    /// <param name="ifError">The value to use if the result is <c>Error</c>.</param>
    /// <param name="result">The input result.</param>
    /// <returns>The contained value if <c>Ok</c>, otherwise <paramref name="ifError"/>.</returns>
    let inline defaultValue (ifError: 'ok) (result: Result<'ok, 'error>) : 'ok =
#if NET9_0_OR_GREATER
        Microsoft.FSharp.Core.Result.defaultValue ifError result
#else
        match result with
        | Ok x -> x
        | Error _ -> ifError
#endif

    /// <summary>
    /// Returns the contained value if <c>Ok</c>, otherwise evaluates <param name="ifErrorThunk"/> and returns the value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#defaultwith</href>
    /// </summary>
    /// <param name="ifErrorThunk">The function to evaluate if the result is <c>Error</c>.</param>
    /// <param name="result">The input result.</param>
    /// <returns>The contained value if <c>Ok</c>, otherwise the result of evaluating <paramref name="ifErrorThunk"/>.</returns>
    let inline defaultWith
        ([<InlineIfLambda>] ifErrorThunk: 'error -> 'ok)
        (result: Result<'ok, 'error>)
        : 'ok =
#if NET9_0_OR_GREATER
        Microsoft.FSharp.Core.Result.defaultWith ifErrorThunk result
#else
        match result with
        | Ok x -> x
        | Error e -> ifErrorThunk e
#endif
