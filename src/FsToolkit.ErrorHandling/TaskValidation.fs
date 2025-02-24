namespace FsToolkit.ErrorHandling

open System.Threading.Tasks

/// TaskValidation<'a, 'err> is defined as Task<Result<'a, 'err list>> meaning you can use many of the functions found in the Result and Task module.
type TaskValidation<'ok, 'error> = Task<Result<'ok, 'error list>>

[<RequireQualifiedAccess>]
module TaskValidation =

    let inline ok (value: 'ok) : TaskValidation<'ok, 'error> =
        Ok value
        |> Task.singleton

    let inline error (error: 'error) : TaskValidation<'ok, 'error> =
        Error [ error ]
        |> Task.singleton

    let inline ofResult (result: Result<'ok, 'error>) : TaskValidation<'ok, 'error> =
        Result.mapError List.singleton result
        |> Task.singleton

    let inline ofChoice (choice: Choice<'ok, 'error>) : TaskValidation<'ok, 'error> =
        match choice with
        | Choice1Of2 x -> ok x
        | Choice2Of2 e -> error e

    let inline apply
        (applier: TaskValidation<'okInput -> 'okOutput, 'error>)
        (input: TaskValidation<'okInput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        task {
            let! applier = applier
            let! input = input

            return
                match applier, input with
                | Ok f, Ok x -> Ok(f x)
                | Error errs, Ok _
                | Ok _, Error errs -> Error errs
                | Error errs1, Error errs2 ->
                    Error(
                        errs1
                        @ errs2
                    )
        }

    /// <summary>
    /// Returns <c>validation</c> if it is <c>Ok</c>, otherwise returns <c>ifError</c>
    /// </summary>
    /// <param name="ifError">The value to use if <c>validation</c> is <c>Error</c></param>
    /// <param name="validation">The input validation.</param>
    /// <remarks>
    /// </remarks>
    /// <example>
    /// <code>
    ///     TaskValidation.error "First" |> TaskValidation.orElse (TaskValidation.error "Second") // evaluates to Error [ "Second" ]
    ///     TaskValidation.error "First" |> TaskValidation.orElse (TaskValidation.ok "Second") // evaluates to Ok ("Second")
    ///     TaskValidation.ok "First" |> TaskValidation.orElse (TaskValidation.error "Second") // evaluates to Ok ("First")
    ///     TaskValidation.ok "First" |> TaskValidation.orElse (TaskValidation.ok "Second") // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the validation is <c>Ok</c>, else returns <c>ifError</c>.
    /// </returns>
    let inline orElse
        (ifError: TaskValidation<'ok, 'errorOutput>)
        (validation: TaskValidation<'ok, 'errorInput>)
        : TaskValidation<'ok, 'errorOutput> =
        task {
            let! validation = validation

            return!
                validation
                |> Result.either ok (fun _ -> ifError)
        }

    /// <summary>
    /// Returns <c>validation</c> if it is <c>Ok</c>, otherwise executes <c>ifErrorFunc</c> and returns the result.
    /// </summary>
    /// <param name="ifErrorFunc">A function that provides an alternate validation when evaluated.</param>
    /// <param name="validation">The input validation.</param>
    /// <remarks>
    /// <paramref name="ifErrorFunc"/> is not executed unless <c>validation</c> is an <c>Error</c>.
    /// </remarks>
    /// <example>
    /// <code>
    ///     TaskValidation.error "First" |> TaskValidation.orElseWith (fun _ -> TaskValidation.error "Second") // evaluates to Error [ "Second" ]
    ///     TaskValidation.error "First" |> TaskValidation.orElseWith (fun _ -> TaskValidation.ok "Second") // evaluates to Ok ("Second")
    ///     TaskValidation.ok "First" |> TaskValidation.orElseWith (fun _ -> TaskValidation.error "Second") // evaluates to Ok ("First")
    ///     TaskValidation.ok "First" |> TaskValidation.orElseWith (fun _ -> TaskValidation.ok "Second") // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is <c>Ok</c>, else the result of executing <paramref name="ifErrorFunc"/>.
    /// </returns>
    let inline orElseWith
        ([<InlineIfLambda>] ifErrorFunc: 'errorInput list -> TaskValidation<'ok, 'errorOutput>)
        (validation : TaskValidation<'ok, 'errorInput>)
        : TaskValidation<'ok, 'errorOutput> =
        task {
            let! validation = validation

            return!
                match validation with
                | Ok x -> ok x
                | Error err -> ifErrorFunc err
        }

    /// <summary>
    /// Applies a transformation to the value of a <c>TaskValidation</c> to a new value using the specified mapper function.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/taskvalidation/map</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the value of the <c>TaskValidation</c> if it is <c>Ok</c>.</param>
    /// <param name="input">The <c>TaskValidation</c> to map.</param>
    /// <returns>A new <c>TaskValidation</c>with the mapped value if the input <c>TaskValidation</c> is <c>Ok</c>, otherwise the original <c>Error</c>.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: TaskValidation<'okInput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        task {
            let! input = input
            return Result.map mapper input
        }

    /// <summary>
    /// Applies a mapper function to two input <c>TaskValidation</c>s, producing a new <c>TaskValidation</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/taskvalidation/map2</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the inputs.</param>
    /// <param name="input1">The first input <c>TaskValidation</c>.</param>
    /// <param name="input2">The second input <c>TaskValidation</c>.</param>
    /// <returns>A new <c>TaskValidation</c>containing the output of the mapper function if both input <c>TaskValidation</c>s are <c>Ok</c>, otherwise an <c>Error TaskValidation</c>.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'okInput1 -> 'okInput2 -> 'okOutput)
        (input1: TaskValidation<'okInput1, 'error>)
        (input2: TaskValidation<'okInput2, 'error>)
        : TaskValidation<'okOutput, 'error> =
        task {
            let! input1 = input1
            let! input2 = input2

            return
                match input1, input2 with
                | Ok x, Ok y -> Ok(mapper x y)
                | Ok _, Error errs -> Error errs
                | Error errs, Ok _ -> Error errs
                | Error errs1, Error errs2 ->
                    Error(
                        errs1
                        @ errs2
                    )
        }

    /// <summary>
    /// Applies a mapper function to three input <c>TaskValidation</c>s, producing a new <c>TaskValidation</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/taskvalidation/map3</href>
    /// </summary>
    /// <param name="mapper">The function to apply to the input <c>TaskValidation</c>s.</param>
    /// <param name="input1">The first input <c>TaskValidation</c>.</param>
    /// <param name="input2">The second input <c>TaskValidation</c>.</param>
    /// <param name="input3">The third input <c>TaskValidation</c>.</param>
    /// <returns>A new <c>TaskValidation</c> with the output of the mapper function applied to the input validations, if all <c>TaskValidation</c>s are <c>Ok</c>, otherwise returns the original <c>Error</c></returns>
    let inline map3
        ([<InlineIfLambda>] mapper: 'okInput1 -> 'okInput2 -> 'okInput3 -> 'okOutput)
        (input1: TaskValidation<'okInput1, 'error>)
        (input2: TaskValidation<'okInput2, 'error>)
        (input3: TaskValidation<'okInput3, 'error>)
        : TaskValidation<'okOutput, 'error> =
        task {
            let! input1 = input1
            let! input2 = input2
            let! input3 = input3

            return
                match input1, input2, input3 with
                | Ok x, Ok y, Ok z -> Ok(mapper x y z)
                | Error errs, Ok _, Ok _ -> Error errs
                | Ok _, Error errs, Ok _ -> Error errs
                | Ok _, Ok _, Error errs -> Error errs
                | Error errs1, Error errs2, Ok _ ->
                    Error(
                        errs1
                        @ errs2
                    )
                | Ok _, Error errs1, Error errs2 ->
                    Error(
                        errs1
                        @ errs2
                    )
                | Error errs1, Ok _, Error errs2 ->
                    Error(
                        errs1
                        @ errs2
                    )
                | Error errs1, Error errs2, Error errs3 ->
                    Error(
                        errs1
                        @ errs2
                        @ errs3
                    )
        }

    /// <summary>
    /// Maps the error value of a <c>TaskValidation</c>to a new error value using the specified error mapper function.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/taskvalidation/maperror</href>
    /// </summary>
    /// <param name="errorMapper">The function that maps the input error value to the output error value.</param>
    /// <param name="input">The <c>TaskValidation</c>value to map the error value of.</param>
    /// <returns>A new <c>TaskValidation</c>with the same Ok value and the mapped error value.</returns>
    let inline mapError
        ([<InlineIfLambda>] errorMapper: 'errorInput -> 'errorOutput)
        (input: TaskValidation<'ok, 'errorInput>)
        : TaskValidation<'ok, 'errorOutput> =
        task {
            let! input = input
            return Result.mapError (List.map errorMapper) input
        }

    /// <summary>
    /// Maps the error values of a <c>TaskValidation</c>to a new error value using the specified error mapper function.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/taskvalidation/maperror</href>
    /// </summary>
    /// <remarks>
    /// Similar to <c>TaskValidation.mapError</c>, except that the mapping function is passed the full list of errors, rather than each one individually.
    /// </remarks>
    /// <param name="errorMapper">The function that maps the input errors to the output errors.</param>
    /// <param name="input">The <c>TaskValidation</c>value to map the errors of.</param>
    /// <returns>A new <c>TaskValidation</c>with the same Ok value and the mapped errors.</returns>
    let inline mapErrors
        ([<InlineIfLambda>] errorMapper: 'errorInput list -> 'errorOutput list)
        (input: TaskValidation<'ok, 'errorInput>)
        : TaskValidation<'ok, 'errorOutput> =
        task {
            let! input = input
            return Result.mapError errorMapper input
        }

    /// <summary>
    /// Takes a transformation function and applies it to the <c>TaskValidation</c> if it is <c>Ok</c>.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/taskvalidation/bind</href>
    /// </summary>
    /// <param name="binder">The transformation function</param>
    /// <param name="input">The input validation</param>
    /// <typeparam name="'okInput">The type of the successful validation.</typeparam>
    /// <typeparam name="'okOutput">The type of the validation after binding.</typeparam>
    /// <typeparam name="'error">The type of the error.</typeparam>
    /// <returns>Returns a new <c>TaskValidation</c> if the input is <c>Ok</c>, otherwise returns the original <c>TaskValidation</c></returns>
    let inline bind
        ([<InlineIfLambda>] binder: 'okInput -> TaskValidation<'okOutput, 'error>)
        (input: TaskValidation<'okInput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        task {
            let! input = input

            match input with
            | Ok x -> return! binder x
            | Error e -> return Error e
        }

    /// <summary>
    /// Takes two <c>TaskValidation</c>s and returns a tuple of the pair or <c>Error</c> if either of them are <c>Error</c>
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/taskvalidation/zip</href>
    /// </summary>
    /// <remarks>
    /// If both validations are <c>Error</c>, the returned <c>Error</c> contains the concatenated lists of errors
    /// </remarks>
    /// <param name="left">The first input validation.</param>
    /// <param name="right">The second input validation.</param>
    /// <returns>A tuple of the pair of the input validation.</returns>
    let inline zip
        (left: TaskValidation<'left, 'error>)
        (right: TaskValidation<'right, 'error>)
        : TaskValidation<'left * 'right, 'error> =
        task {
            let! left = left
            let! right = right

            return
                match left, right with
                | Ok x1res, Ok x2res -> Ok(x1res, x2res)
                | Error e, Ok _ -> Error e
                | Ok _, Error e -> Error e
                | Error e1, Error e2 -> Error(e1 @ e2)
        }
