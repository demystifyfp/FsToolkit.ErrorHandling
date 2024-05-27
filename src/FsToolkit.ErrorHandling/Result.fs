namespace FsToolkit.ErrorHandling

/// <summary>
/// Helper functions for working with <c>Result</c> values.
/// </summary>
[<RequireQualifiedAccess>]
module Result =

    /// <summary>
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
        match input with
        | Ok x -> Ok(mapper x)
        | Error e -> Error e

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
        match input with
        | Ok x -> Ok x
        | Error e -> Error(errorMapper e)

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
        match input with
        | Ok x -> binder x
        | Error e -> Error e

    /// <summary>
    /// Determines whether the specified <c>Result</c> is in a successful state.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#isok</href>
    /// </summary>
    /// <param name="value">The result to check.</param>
    /// <returns>True if the result is in a successful state; otherwise, false.</returns>
    let inline isOk (value: Result<'ok, 'error>) : bool =
        match value with
        | Ok _ -> true
        | Error _ -> false

    /// <summary>
    /// Determines whether the specified <c>Result</c> is an error.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#iserror</href>
    /// </summary>
    /// <param name="value">The result to check.</param>
    /// <returns>True if the result is an error; otherwise, false.</returns>
    let inline isError (value: Result<'ok, 'error>) : bool =
        match value with
        | Ok _ -> false
        | Error _ -> true

    /// <summary>
    /// Applies the appropriate function based on the result of the input.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/eitherfunctions#result.either</href>
    /// </summary>
    /// <param name="onOk">The function to apply if the input is <c>Ok</c></param>
    /// <param name="onError">The function to apply if the input is <c>Error</c></param>
    /// <param name="input">The input result</param>
    /// <returns>The result of applying the appropriate function.</returns>
    let inline either
        ([<InlineIfLambda>] onOk: 'okInput -> 'output)
        ([<InlineIfLambda>] onError: 'errorInput -> 'output)
        (input: Result<'okInput, 'errorInput>)
        : 'output =
        match input with
        | Ok x -> onOk x
        | Error err -> onError err

    /// <summary>
    /// Maps the values of a Result to a new Result using the provided functions.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/eitherfunctions#result.eithermap</href>
    /// </summary>
    /// <param name="onOk">The function to apply to the 'ok' value of the input Result.</param>
    /// <param name="onError">The function to apply to the 'error' value of the input Result.</param>
    /// <param name="input">The input <c>Result</c>to map.</param>
    /// <returns>A new Result with the mapped values.</returns>
    let inline eitherMap
        ([<InlineIfLambda>] onOk: 'okInput -> 'okOutput)
        ([<InlineIfLambda>] onError: 'errorInput -> 'errorOutput)
        (input: Result<'okInput, 'errorInput>)
        : Result<'okOutput, 'errorOutput> =
        match input with
        | Ok x -> Ok(onOk x)
        | Error err -> Error(onError err)

    /// <summary>
    /// Applies a function to the value within a <c>Result</c> and returns a new <c>Result</c> with the output of the function.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/apply</href>
    /// </summary>
    /// <param name="applier">The <c>Result</c>containing the function to apply.</param>
    /// <param name="input">The <c>Result</c>containing the input to apply the function to.</param>
    /// <returns>A <c>Result</c>containing the output of applying the function to the input, or an <c>Error</c> if either <c>Result</c>is an <c>Error</c></returns>
    let inline apply
        (applier: Result<'okInput -> 'okOutput, 'error>)
        (input: Result<'okInput, 'error>)
        : Result<'okOutput, 'error> =
        match (applier, input) with
        | Ok f, Ok x -> Ok(f x)
        | Error e, _
        | _, Error e -> Error e

    /// <summary>
    /// Applies a mapper function to two input Results, producing a new <c>Result</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/map2</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the inputs.</param>
    /// <param name="input1">The first input Result.</param>
    /// <param name="input2">The second input Result.</param>
    /// <returns>A new <c>Result</c>containing the output of the mapper function if both input Results are Ok, otherwise an Error Result.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'okInput1 -> 'okInput2 -> 'okOutput)
        (input1: Result<'okInput1, 'error>)
        (input2: Result<'okInput2, 'error>)
        : Result<'okOutput, 'error> =
        match (input1, input2) with
        | Ok x, Ok y -> Ok(mapper x y)
        | Error e, _
        | _, Error e -> Error e

    /// <summary>
    /// Applies a mapper function to three input Results, producing a new <c>Result</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/map3</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the input results.</param>
    /// <param name="input1">The first input result.</param>
    /// <param name="input2">The second input result.</param>
    /// <param name="input3">The third input result.</param>
    /// <returns>A new <c>Result</c> with the output of the mapper function applied to the input results, if all Results are <c>Ok</c>, otherwise returns the original <c>Error</c></returns>
    let inline map3
        ([<InlineIfLambda>] mapper: 'okInput1 -> 'okInput2 -> 'okInput3 -> 'okOutput)
        (input1: Result<'okInput1, 'error>)
        (input2: Result<'okInput2, 'error>)
        (input3: Result<'okInput3, 'error>)
        : Result<'okOutput, 'error> =
        match (input1, input2, input3) with
        | Ok x, Ok y, Ok z -> Ok(mapper x y z)
        | Error e, _, _
        | _, Error e, _
        | _, _, Error e -> Error e

    /// <summary>
    /// Converts a <c>Choice</c> to a <c>Result</c>
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/transforms/ofchoice</href>
    /// </summary>
    /// <param name="input">The <c>Choice</c> value to convert.</param>
    /// <returns>A <c>Result</c> matching the types of the input <c>Choice</c></returns>
    let inline ofChoice (input: Choice<'ok, 'error>) : Result<'ok, 'error> =
        match input with
        | Choice1Of2 x -> Ok x
        | Choice2Of2 e -> Error e

    /// <summary>
    /// Calls a <c>TryCreate</c> member function on a value passed in, returning a <c>Result</c> containing the value or an error tuple
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/trycreate</href>
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="x">The value to create a result from.</param>
    /// <returns>A <c>Result</c> containing the value or an error tuple of the field name and the original error type</returns>
    let inline tryCreate (fieldName: string) (x: 'a) : Result< ^b, (string * 'c) > =
        let tryCreate' x =
            (^b: (static member TryCreate: 'a -> Result< ^b, 'c >) x)

        tryCreate' x
        |> mapError (fun z -> (fieldName, z))

    /// <summary>
    /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise returns <paramref name="ifError"/>
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/orelsefunctions#result.orelse</href>
    /// </summary>
    /// <param name="ifError">The value to use if <paramref name="result"/> is <c>Error</c></param>
    /// <param name="result">The input result.</param>
    /// <remarks>
    /// </remarks>
    /// <example>
    /// <code>
    ///     Error ("First") |> Result.orElse (Error ("Second")) // evaluates to Error ("Second")
    ///     Error ("First") |> Result.orElseWith (Ok ("Second")) // evaluates to Ok ("Second")
    ///     Ok ("First") |> Result.orElseWith (Error ("Second")) // evaluates to Ok ("First")
    ///     Ok ("First") |> Result.orElseWith (Ok ("Second")) // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is Ok, else returns <paramref name="ifError"/>.
    /// </returns>
    let inline orElse
        (ifError: Result<'ok, 'errorOutput>)
        (result: Result<'ok, 'error>)
        : Result<'ok, 'errorOutput> =
        match result with
        | Ok x -> Ok x
        | Error _ -> ifError

    /// <summary>
    /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise executes <paramref name="ifErrorFunc"/> and returns the result.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/orelsefunctions#result.orelsewith</href>
    /// </summary>
    /// <param name="ifErrorFunc">A function that provides an alternate result when evaluated.</param>
    /// <param name="result">The input result.</param>
    /// <remarks>
    /// <paramref name="ifErrorFunc"/>  is not executed unless <paramref name="result"/> is an <c>Error</c>.
    /// </remarks>
    /// <example>
    /// <code>
    ///     Error ("First") |> Result.orElseWith (fun _ -> Error ("Second")) // evaluates to Error ("Second")
    ///     Error ("First") |> Result.orElseWith (fun _ -> Ok ("Second")) // evaluates to Ok ("Second")
    ///     Ok ("First") |> Result.orElseWith (fun _ -> Error ("Second")) // evaluates to Ok ("First")
    ///     Ok ("First") |> Result.orElseWith (fun _ -> Ok ("Second")) // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is Ok, else the result of executing <paramref name="ifErrorFunc"/>.
    /// </returns>
    let inline orElseWith
        ([<InlineIfLambda>] ifErrorFunc: 'error -> Result<'ok, 'errorOutput>)
        (result: Result<'ok, 'error>)
        : Result<'ok, 'errorOutput> =
        match result with
        | Ok x -> Ok x
        | Error e -> ifErrorFunc e

    /// <summary>
    /// Ignores the value of a <c>Result</c> and returns a new <c>Result</c> with unit as the success value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#ignore</href>
    /// </summary>
    /// <param name="result">The <c>Result</c> to ignore.</param>
    /// <returns>A new <c>Result</c> with unit as the success value.</returns>
    let inline ignore<'ok, 'error> (result: Result<'ok, 'error>) : Result<unit, 'error> =
        match result with
        | Ok _ -> Ok()
        | Error e -> Error e

    /// <summary>
    /// Requires a boolean value to be true, otherwise returns an error result.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requiretrue</href>
    /// </summary>
    /// <param name="error">The error value to return if the condition is false.</param>
    /// <param name="value">The boolean value to check.</param>
    /// <returns>An <c>Ok</c> result if the condition is true, otherwise an Error result with the specified error value.</returns>
    let inline requireTrue (error: 'error) (value: bool) : Result<unit, 'error> =
        if value then Ok() else Error error

    /// <summary>
    /// Requires a boolean value to be false, otherwise returns an error result.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requirefalse</href>
    /// </summary>
    /// <param name="error">The error value to return if the condition is true.</param>
    /// <param name="value">The boolean value to check.</param>
    /// <returns>An <c>Ok</c> result if the condition is false, otherwise an Error result with the specified error value.</returns>
    let inline requireFalse (error: 'error) (value: bool) : Result<unit, 'error> =
        if not value then Ok() else Error error

    /// <summary>
    /// Requires a value to be <c>Some</c>, otherwise returns an error result.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requiresome</href>
    /// </summary>
    /// <param name="error">The error value to return if the value is <c>None</c>.</param>
    /// <param name="option">The <c>Option</c> value to check.</param>
    /// <returns>An <c>Ok</c> result if the value is <c>Some</c>, otherwise an Error result with the specified error value.</returns>
    let inline requireSome (error: 'error) (option: 'ok option) : Result<'ok, 'error> =
        match option with
        | Some x -> Ok x
        | None -> Error error

    /// <summary>
    /// Requires a value to be <c>None</c>, otherwise returns an error result.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requirenone</href>
    /// </summary>
    /// <param name="error">The error value to return if the value is <c>Some</c>.</param>
    /// <param name="option">The <c>Option</c> value to check.</param>
    /// <returns>An <c>Ok</c> result if the value is <c>None</c>, otherwise an Error result with the specified error value.</returns>
    let inline requireNone (error: 'error) (option: 'value option) : Result<unit, 'error> =
        match option with
        | Some _ -> Error error
        | None -> Ok()

    /// <summary>
    /// Requires a value to be <c>ValueSome</c>, otherwise returns an error result.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requirevaluesome</href>
    /// </summary>
    /// <param name="error">The error value to return if the value is <c>ValueNone</c>.</param>
    /// <param name="voption">The <c>ValueOption</c> value to check.</param>
    /// <returns>An <c>Ok</c> result if the value is <c>ValueSome</c>, otherwise an <c>Error</c> result with the specified error value.</returns>
    let inline requireValueSome (error: 'error) (voption: 'ok voption) : Result<'ok, 'error> =
        match voption with
        | ValueSome x -> Ok x
        | ValueNone -> Error error

    /// <summary>
    /// Requires a value to be <c>ValueNone</c>, otherwise returns an error result.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requirevaluenone</href>
    /// </summary>
    /// <param name="error">The error value to return if the value is <c>ValueSome</c>.</param>
    /// <param name="voption">The <c>ValueOption</c> value to check.</param>
    /// <returns>An <c>Ok</c> result if the value is <c>ValueNone</c>, otherwise an <c>Error</c> result with the specified error value.</returns>
    let inline requireValueNone (error: 'error) (voption: 'value voption) : Result<unit, 'error> =
        match voption with
        | ValueSome _ -> Error error
        | ValueNone -> Ok()

    /// <summary>
    /// Converts a nullable value into a <c>Result</c>, using the given error if null
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requirenotnull</href>
    /// </summary>
    /// <param name="error">The error value to return if the value is null.</param>
    /// <param name="value">The nullable value to check.</param>
    /// <returns>An <c>Ok</c> result if the value is not null, otherwise an <c>Error</c> result with the specified error value.</returns>
    let inline requireNotNull (error: 'error) (value: 'ok) : Result<'ok, 'error> =
        match value with
        | null -> Error error
        | nonnull -> Ok nonnull

    /// <summary>
    /// Returns <c>Ok</c> if the two values are equal, or the specified error if not.
    /// Same as <c>requireEqual</c>, but with a signature that fits piping better than normal function application.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requireequalto</href>
    /// </summary>
    /// <param name="other">The value to compare to.</param>
    /// <param name="error">The error value to return if the values are not equal.</param>
    /// <param name="this">The value to compare.</param>
    /// <returns>An <c>Ok</c> result if the values are equal, otherwise an <c>Error</c> result with the specified error value.</returns>
    let inline requireEqualTo
        (other: 'value)
        (error: 'error)
        (this: 'value)
        : Result<unit, 'error> =
        if this = other then Ok() else Error error

    /// <summary>
    /// Returns <c>Ok</c> if the two values are equal, or the specified error if not.
    /// Same as <c>requireEqualTo</c>, but with a signature that fits normal function application better than piping.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requireequal</href>
    /// </summary>
    /// <param name="x1">The first value to compare.</param>
    /// <param name="x2">The second value to compare.</param>
    /// <param name="error">The error value to return if the values are not equal.</param>
    /// <returns>An <c>Ok</c> result if the values are equal, otherwise an <c>Error</c> result with the specified error value.</returns>
    let inline requireEqual (x1: 'value) (x2: 'value) (error: 'error) : Result<unit, 'error> =
        if x1 = x2 then Ok() else Error error

    /// <summary>
    /// Returns <c>Ok</c> if the sequence is empty, or the specified error if not.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requireempty</href>
    /// </summary>
    /// <param name="error">The error value to return if the sequence is not empty.</param>
    /// <param name="xs">The sequence to check.</param>
    /// <returns>An <c>Ok</c> result if the sequence is empty, otherwise an <c>Error</c> result with the specified error value.</returns>
    let inline requireEmpty (error: 'error) (xs: #seq<'value>) : Result<unit, 'error> =
        if Seq.isEmpty xs then Ok() else Error error

    /// <summary>
    /// Returns <c>Ok</c> if the sequence is not empty, or the specified error if it is.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requirenotempty</href>
    /// </summary>
    /// <param name="error">The error value to return if the sequence is empty.</param>
    /// <param name="xs">The sequence to check.</param>
    /// <returns>An <c>Ok</c> result if the sequence is not empty, otherwise an <c>Error</c> result with the specified error value.</returns>
    let inline requireNotEmpty (error: 'error) (xs: #seq<'value>) : Result<unit, 'error> =
        if Seq.isEmpty xs then Error error else Ok()

    /// <summary>
    /// Returns the first item of the sequence if it exists, or the specified error if the sequence is empty.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/requirefunctions#requirehead</href>
    /// </summary>
    /// <param name="error">The error value to return if the sequence is empty.</param>
    /// <param name="xs">The sequence to check.</param>
    /// <returns>An <c>Ok</c> result containing the first item of the sequence if it exists, otherwise an <c>Error</c> result with the specified error value.</returns>
    let inline requireHead (error: 'error) (xs: #seq<'ok>) : Result<'ok, 'error> =
        match Seq.tryHead xs with
        | Some x -> Ok x
        | None -> Error error

    /// <summary>
    /// Replaces an error value with a custom error value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#seterror</href>
    /// </summary>
    /// <param name="error">The error value to replace the original error value with.</param>
    /// <param name="result">The input result.</param>
    /// <returns>A new <c>Result</c> with the same Ok value and the new error value.</returns>
    let inline setError (error: 'error) (result: Result<'ok, 'errorIgnored>) : Result<'ok, 'error> =
        result
        |> mapError (fun _ -> error)

    /// <summary>
    /// Replaces a unit error value with a custom error value. Safer than setError
    /// since you're not losing any information.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#witherror</href>
    /// </summary>
    /// <param name="error">The error value to replace the original error value with.</param>
    /// <param name="result">The input result.</param>
    /// <returns>A new <c>Result</c> with the same Ok value and the new error value.</returns>
    let inline withError (error: 'error) (result: Result<'ok, unit>) : Result<'ok, 'error> =
        result
        |> mapError (fun () -> error)

    /// <summary>
    /// Returns the contained value if <c>Ok</c>, otherwise returns the provided value
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#defaultvalue</href>
    /// </summary>
    /// <param name="ifError">The value to use if the result is <c>Error</c>.</param>
    /// <param name="result">The input result.</param>
    /// <returns>The contained value if <c>Ok</c>, otherwise <paramref name="ifError"/>.</returns>
    let inline defaultValue (ifError: 'ok) (result: Result<'ok, 'error>) : 'ok =
        match result with
        | Ok x -> x
        | Error _ -> ifError

    /// <summary>
    /// Returns the contained value if <c>Error</c>, otherwise returns the provided value
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#defaulterror</href>
    /// </summary>
    /// <param name="ifOk">The value to use if the result is <c>Ok</c>.</param>
    /// <param name="result">The input result.</param>
    /// <returns>The contained value if <c>Error</c>, otherwise <paramref name="ifOk"/>.</returns>
    let inline defaultError (ifOk: 'error) (result: Result<'ok, 'error>) : 'error =
        match result with
        | Error error -> error
        | Ok _ -> ifOk

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
        match result with
        | Ok x -> x
        | Error e -> ifErrorThunk e

    /// <summary>
    /// Same as <c>defaultValue</c> for a result where the Ok value is unit. The name
    /// describes better what is actually happening in this case.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#ignoreerror</href>
    /// </summary>
    /// <param name="result">The input result.</param>
    /// <returns>Unit if <c>Ok</c>, otherwise the provided value.</returns>
    let inline ignoreError<'error> (result: Result<unit, 'error>) : unit = defaultValue () result

    /// <summary>
    /// If the result is <c>Ok</c> and the predicate returns true, executes the function on the <c>Ok</c> value. Passes through the input value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/teefunctions#teeif</href>
    /// </summary>
    /// <param name="predicate">The predicate to evaluate on the <c>Ok</c> value.</param>
    /// <param name="sideEffect">The function to execute on the <c>Ok</c> value if the predicate returns true.</param>
    /// <param name="result">The input result.</param>
    /// <returns>The input result.</returns>
    let inline teeIf
        ([<InlineIfLambda>] predicate: 'ok -> bool)
        ([<InlineIfLambda>] sideEffect: 'ok -> unit)
        (result: Result<'ok, 'error>)
        : Result<'ok, 'error> =
        match result with
        | Ok x ->
            if predicate x then
                sideEffect x
        | Error _ -> ()

        result

    /// <summary>
    /// If the result is <c>Error</c> and the predicate returns true, executes the
    /// function on the <c>Error</c> value. Passes through the input value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/teefunctions#teeerrorif</href>
    /// </summary>
    /// <param name="predicate">The predicate to evaluate on the <c>Error</c> value.</param>
    /// <param name="sideEffect">The function to execute on the <c>Error</c> value if the predicate returns true.</param>
    /// <param name="result">The input result.</param>
    /// <returns>The input result.</returns>
    let inline teeErrorIf
        ([<InlineIfLambda>] predicate: 'error -> bool)
        ([<InlineIfLambda>] sideEffect: 'error -> unit)
        (result: Result<'ok, 'error>)
        : Result<'ok, 'error> =
        match result with
        | Ok _ -> ()
        | Error x ->
            if predicate x then
                sideEffect x

        result

    /// <summary>
    /// If the result is <c>Ok</c>, executes the function on the <c>Ok</c> value. Passes through the input value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/teefunctions#tee</href>
    /// </summary>
    /// <param name="sideEffect">The function to execute on the <c>Ok</c> value.</param>
    /// <param name="result">The input result.</param>
    /// <returns>The input result.</returns>
    let inline tee
        ([<InlineIfLambda>] sideEffect: 'ok -> unit)
        (result: Result<'ok, 'error>)
        : Result<'ok, 'error> =
        teeIf (fun _ -> true) sideEffect result

    /// <summary>
    /// If the result is <c>Error</c>, executes the function on the <c>Error</c> value. Passes through the input value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/teefunctions#teeerror</href>
    /// </summary>
    /// <param name="sideEffect">The function to execute on the <c>Error</c> value.</param>
    /// <param name="result">The input result.</param>
    /// <returns>The input result.</returns>
    let inline teeError
        ([<InlineIfLambda>] sideEffect: 'error -> unit)
        (result: Result<'ok, 'error>)
        : Result<'ok, 'error> =
        teeErrorIf (fun _ -> true) sideEffect result

    /// Converts a Result<Async<_>,_> to an Async<Result<_,_>>
    let inline sequenceAsync (resAsync: Result<Async<'ok>, 'error>) : Async<Result<'ok, 'error>> =
        async {
            match resAsync with
            | Ok asnc ->
                let! x = asnc
                return Ok x
            | Error err -> return Error err
        }

    /// <summary>
    /// Maps an Async function over a Result, returning an Async Result.
    /// </summary>
    /// <param name="f">The function to map over the Result.</param>
    /// <param name="res">The Result to map over.</param>
    /// <returns>An Async Result with the mapped value.</returns>
    let inline traverseAsync
        ([<InlineIfLambda>] f: 'okInput -> Async<'okOutput>)
        (res: Result<'okInput, 'error>)
        : Async<Result<'okOutput, 'error>> =
        sequenceAsync ((map f) res)


    /// <summary>
    /// Returns the <c>Ok</c> value or runs the specified function over the error value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/others#valueor</href>
    /// </summary>
    /// <param name="f">The function to run over the error value.</param>
    /// <param name="res">The input result.</param>
    /// <returns>The <c>Ok</c> value if the result is <c>Ok</c>, otherwise the result of running the function over the error value.</returns>
    let inline valueOr ([<InlineIfLambda>] f: 'error -> 'ok) (res: Result<'ok, 'error>) : 'ok =
        match res with
        | Ok x -> x
        | Error x -> f x

    /// <summary>
    /// Takes two results and returns a tuple of the pair
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/zip</href>
    /// </summary>
    /// <param name="left">The first input result.</param>
    /// <param name="right">The second input result.</param>
    /// <returns>A tuple of the pair of the input results.</returns>
    let zip
        (left: Result<'leftOk, 'error>)
        (right: Result<'rightOk, 'error>)
        : Result<'leftOk * 'rightOk, 'error> =
        match left, right with
        | Ok x1res, Ok x2res -> Ok(x1res, x2res)
        | Error e, _ -> Error e
        | _, Error e -> Error e

    /// <summary>
    /// Takes two results and returns a tuple of the error pair
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/result/ziperror</href>
    /// </summary>
    /// <param name="left">The first input result.</param>
    /// <param name="right">The second input result.</param>
    /// <returns>A tuple of the error pair of the input results.</returns>
    let zipError
        (left: Result<'ok, 'leftError>)
        (right: Result<'ok, 'rightError>)
        : Result<'ok, 'leftError * 'rightError> =
        match left, right with
        | Error x1res, Error x2res -> Error(x1res, x2res)
        | Ok e, _ -> Ok e
        | _, Ok e -> Ok e
