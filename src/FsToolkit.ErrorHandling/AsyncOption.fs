namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module AsyncOption =

    let inline map
        ([<InlineIfLambda>] mapper: 'input -> 'output)
        (input: Async<'input option>)
        : Async<'output option> =
        Async.map (Option.map mapper) input

    let inline bind
        ([<InlineIfLambda>] binder: 'input -> Async<'output option>)
        (input: Async<'input option>)
        : Async<'output option> =
        Async.bind
            (fun x ->
                match x with
                | Some x -> binder x
                | None -> Async.singleton None
            )
            input

    let inline some (value: 'value) : Async<'value option> = Async.singleton (Some value)

    let inline apply
        (applier: Async<('input -> 'output) option>)
        (input: Async<'input option>)
        : Async<'output option> =
        bind (fun f' -> bind (fun x' -> some (f' x')) input) applier

    /// <summary>
    /// Returns result of running <paramref name="onSome"/> if it is <c>Some</c>, otherwise returns result of running <paramref name="onNone"/>
    /// </summary>
    /// <param name="onSome">The function to run if <paramref name="input"/> is <c>Some</c></param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>None</c></param>
    /// <param name="input">The input option.</param>
    /// <returns>
    /// The result of running <paramref name="onSome"/> if the input is <c>Some</c>, else returns result of running <paramref name="onNone"/>.
    /// </returns>
    let inline either
        ([<InlineIfLambda>] onSome: 'input -> Async<'output>)
        (onNone: Async<'output>)
        (input: Async<'input option>)
        : Async<'output> =
        input
        |> Async.bind (
            function
            | Some v -> onSome v
            | None -> onNone
        )

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="value">The specified default value.</param>
    /// <param name="asyncOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the default value.
    /// </returns>
    let inline defaultValue (value: 'value) (asyncOption: Async<'value option>) =
        asyncOption
        |> Async.map (Option.defaultValue value)

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise evaluates <paramref name="defThunk"/> and returns the result.
    /// </summary>
    /// <param name="defThunk">A thunk that provides a default value when evaluated.</param>
    /// <param name="asyncOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the result of evaluating <paramref name="defThunk"/>.
    /// </returns>
    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (asyncOption: Async<'value option>)
        : Async<'value> =
        asyncOption
        |> Async.map (Option.defaultWith defThunk)

    /// <summary>
    /// Returns <paramref name="input"/> if it is <c>Some</c>, otherwise returns <paramref name="ifNone"/>
    /// </summary>
    /// <param name="ifNone">The value to use if <paramref name="input"/> is <c>None</c></param>
    /// <param name="input">The input option.</param>
    /// <remarks>
    /// </remarks>
    /// <example>
    /// <code>
    ///     None |> Async.singleton |> AsyncOption.orElse (AsyncOption.some "Second") // evaluates to Some ("Second")
    ///     None |> Async.singleton |> AsyncOption.orElse (None |> Async.singleton) // evaluates to None
    ///     AsyncOption.some "First" |> AsyncOption.orElse (AsyncOption.some "Second") // evaluates to Some ("First")
    ///     AsyncOption.some "First" |> AsyncOption.orElse (None |> Async.singleton) // evaluates to Some ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The option if the option is Some, else returns <paramref name="ifNone"/>.
    /// </returns>
    let inline orElse
        (ifNone: Async<'value option>)
        (input: Async<'value option>)
        : Async<'value option> =
        Async.bind (Option.either some (fun _ -> ifNone)) input

    /// <summary>
    /// Returns <paramref name="input"/> if it is <c>Some</c>, otherwise executes <paramref name="ifNoneFunc"/> and returns the result.
    /// </summary>
    /// <param name="ifNoneFunc">A function that provides an alternate option when evaluated.</param>
    /// <param name="input">The input option.</param>
    /// <remarks>
    /// <paramref name="ifNoneFunc"/>  is not executed unless <paramref name="input"/> is a <c>None</c>.
    /// </remarks>
    /// <example>
    /// <code>
    ///     None |> Async.singleton |> AsyncOption.orElseWith (fun _ -> AsyncOption.some "Second") // evaluates to Some ("Second")
    ///     None |> Async.singleton |> AsyncOption.orElseWith (fun _ -> None |> Async.singleton) // evaluates to None
    ///     AsyncOption.some "First" |> AsyncOption.orElseWith (fun _ -> AsyncOption.some "Second") // evaluates to Some ("First")
    ///     AsyncOption.some "First" |> AsyncOption.orElseWith (fun _ -> None |> Async.singleton) // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The option if the option is Some, else the result of executing <paramref name="ifNoneFunc"/>.
    /// </returns>
    let inline orElseWith
        ([<InlineIfLambda>] ifNoneFunc: unit -> Async<'value option>)
        (input: Async<'value option>)
        : Async<'value option> =
        Async.bind (Option.either some ifNoneFunc) input
