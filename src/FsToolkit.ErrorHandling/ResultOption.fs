namespace FsToolkit.ErrorHandling

/// <summary>
/// Helper functions for working with <c>Result</c> values that contain <c>Option</c> values.
/// </summary>
[<RequireQualifiedAccess>]
module ResultOption =

    /// <summary>
    /// Converts a value to a value into a <c>Result</c> which contains the value wrapped in an <c>Option</c>.
    /// </summary>
    /// <param name="x">The value to convert.</param>
    /// <returns>The value wrapped in an <c>Option</c> and then wrapped in a <c>Result</c>.</returns>
    let inline retn x = Ok(Some x)

    /// <summary>
    /// Applies a transformation function to a <c>Result</c> value that contains an <c>Option</c> value.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map</href>
    /// </summary>
    /// <param name="mapper">The transformation function to apply.</param>
    /// <param name="input">The <c>Result</c> value to apply the transformation function to.</param>
    /// <returns>The result of applying the transformation function to the <c>Result</c> value.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: Result<'okInput option, 'error>)
        : Result<'okOutput option, 'error> =
        Result.map (Option.map mapper) input

    /// <summary>
    /// Maps the error value of a <c>Result</c> value that contains an <c>Option</c> value using the provided function.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/maperror</href>
    /// </summary>
    /// <param name="mapper">The function to map over the error value.</param>
    /// <param name="input">The <c>Result</c> value to map the error value of.</param>
    /// <returns>The result of mapping the error value of the <c>Result</c> value.</returns>
    let inline mapError
        ([<InlineIfLambda>] mapper: 'errorInput -> 'errorOutput)
        (input: Result<'ok option, 'errorInput>)
        : Result<'ok option, 'errorOutput> =
        Result.mapError mapper input

    /// <summary>
    /// Takes a transformation function and applies to to the value in a <c>Result</c> value that contains an <c>Option</c> value, if is is <c>Ok</c> and <c>Some</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/bind</href>
    /// </summary>
    /// <param name="binder">The transformation function to apply.</param>
    /// <param name="input">The <c>Result</c> value to apply the transformation function to.</param>
    /// <returns>The result of applying the transformation function to the <c>Result</c> value.</returns>
    let inline bind
        ([<InlineIfLambda>] binder: 'okInput -> Result<'okOutput option, 'error>)
        (input: Result<'okInput option, 'error>)
        : Result<'okOutput option, 'error> =
        Result.bind
            (function
            | Some x -> binder x
            | None -> Ok None)
            input

    /// <summary>
    /// Applies a transformation function to a <c>Result</c> value that contains an <c>Option</c> value and returns the result wrapped in an <c>Option</c> within a <c>Result</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/apply</href>
    /// </summary>
    /// <param name="applier">The <c>Result</c> containing a potential function to apply</param>
    /// <param name="input">The <c>Result</c> containing a potential value to apply the function to</param>
    /// <returns>The result of applying the function to the value, wrapped in an <c>Option</c> and then wrapped in a <c>Result</c>.</returns>
    let inline apply
        (applier: Result<('okInput -> 'okOutput) option, 'error>)
        (input: Result<'okInput option, 'error>)
        : Result<'okOutput option, 'error> =
        match (applier, input) with
        | Ok f, Ok x ->
            match f, x with
            | Some f', Some x' -> Ok(Some(f' x'))
            | _ -> Ok None
        | Error e, _
        | _, Error e -> Error e


    /// <summary>
    /// Combines two <c>Result</c> values that contain <c>Option</c> values into a single <c>Result</c> value that contains the result wrapped in an <c>Option</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map2</href>
    /// </summary>
    /// <param name="mapper">The function to combine the two values with.</param>
    /// <param name="input1">The first <c>Result</c> value to combine.</param>
    /// <param name="input2">The second <c>Result</c> value to combine.</param>
    /// <returns>The result of combining the two <c>Result</c> values, wrapped in an <c>Option</c> and then wrapped in a <c>Result</c>.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'okInput1 -> 'okInput2 -> 'okOutput)
        (input1: Result<'okInput1 option, 'error>)
        (input2: Result<'okInput2 option, 'error>)
        : Result<'okOutput option, 'error> =
        match (input1, input2) with
        | Ok x, Ok y ->
            match x, y with
            | Some x', Some y' -> Ok(Some(mapper x' y'))
            | _ -> Ok None
        | Error e, _
        | _, Error e -> Error e

    /// <summary>
    /// Combines three <c>Result</c> values that contain <c>Option</c> values into a single <c>Result</c> value that contains the result wrapped in an <c>Option</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/map3</href>
    /// </summary>
    /// <param name="mapper">The function to combine the three values with.</param>
    /// <param name="input1">The first <c>Result</c> value to combine.</param>
    /// <param name="input2">The second <c>Result</c> value to combine.</param>
    /// <param name="input3">The third <c>Result</c> value to combine.</param>
    /// <returns>The result of combining the three <c>Result</c> values, wrapped in an <c>Option</c> and then wrapped in a <c>Result</c>.</returns>
    let inline map3
        ([<InlineIfLambda>] mapper: 'okInput1 -> 'okInput2 -> 'okInput3 -> 'okOutput)
        (input1: Result<'okInput1 option, 'error>)
        (input2: Result<'okInput2 option, 'error>)
        (input3: Result<'okInput3 option, 'error>)
        : Result<'okOutput option, 'error> =
        match (input1, input2, input3) with
        | Ok x, Ok y, Ok z ->
            match x, y, z with
            | Some x', Some y', Some z' -> Ok(Some(mapper x' y' z'))
            | _ -> Ok None
        | Error e, _, _
        | _, Error e, _
        | _, _, Error e -> Error e

    /// <summary>
    /// Combines two <c>Result</c> values that contain <c>Option</c> values into a single <c>Result</c> value that contains the tuple of the two values wrapped in an <c>Option</c>, if they are both <c>Some</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/zip</href>
    /// </summary>
    /// <param name="input1">The first <c>Result</c> value to combine.</param>
    /// <param name="input2">The second <c>Result</c> value to combine.</param>
    /// <returns>A tuple of the two values wrapped in an <c>Option</c> and then wrapped in a <c>Result</c>.</returns>
    let zip
        (left: Result<'leftOk option, 'error>)
        (right: Result<'rightOk option, 'error>)
        : Result<('leftOk * 'rightOk) option, 'error> =
        match left, right with
        | Ok x1res, Ok x2res ->
            match x1res, x2res with
            | Some x1, Some x2 -> Ok(Some(x1, x2))
            | _ -> Ok None
        | Error e, _ -> Error e
        | _, Error e -> Error e

    /// <summary>
    /// Takes two results and returns a tuple of the error pair
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/option/ziperror</href>
    /// </summary>
    /// <param name="left">The first <c>Result</c> value to combine.</param>
    /// <param name="right">The second <c>Result</c> value to combine.</param>
    /// <returns>A tuple of the error pair of the input results.</returns>
    let zipError
        (left: Result<'ok option, 'leftError>)
        (right: Result<'ok option, 'rightError>)
        : Result<'ok option, 'leftError * 'rightError> =
        match left, right with
        | Error x1res, Error x2res -> Error(x1res, x2res)
        | Ok e, _ -> Ok e
        | _, Ok e -> Ok e

    /// <summary>
    /// Replaces the wrapped value with unit
    /// </summary>
    /// <param name="resultOpt">The <c>Result</c> value to ignore the value of.</param>
    /// <returns>The <c>Result</c> value with the value replaced with an <c>Option</c> unit.</returns>
    let inline ignore<'ok, 'error> (resultOpt: Result<'ok option, 'error>) =
        resultOpt
        |> map ignore<'ok>

    /// <summary>
    /// Converts a <c>Result</c> into a <c>Result</c> that contains the value wrapped in an <c>Option</c>.
    /// </summary>
    /// <param name="result">The <c>Result</c> to convert.</param>
    /// <returns>The <c>Result</c> with the value wrapped in an <c>Option</c>.</returns>
    let inline ofResult (result: Result<'ok, 'error>) : Result<'ok option, 'error> =
        match result with
        | Ok x -> Ok(Some x)
        | Error e -> Error e

    /// <summary>
    /// Converts an <c>Option</c> into a <c>Result</c> that contains the <c>Option</c>
    /// </summary>
    /// <param name="option">The <c>Option</c> to convert.</param>
    /// <returns>The <c>Result</c> with the <c>Option</c>.</returns>
    let inline ofOption (option: 'T option) : Result<'T option, 'error> = Ok option

    /// <summary>
    /// Converts a <c>Choice</c> into a <c>Result</c> that contains the value wrapped in an <c>Option</c>.
    /// </summary>
    /// <param name="choice">The <c>Choice</c> to convert.</param>
    /// <returns>The <c>Result</c> with the value wrapped in an <c>Option</c>.</returns>
    let inline ofChoice (choice: Choice<'ok, 'error>) : Result<'ok option, 'error> =
        match choice with
        | Choice1Of2 x -> Ok(Some x)
        | Choice2Of2 e -> Error e
