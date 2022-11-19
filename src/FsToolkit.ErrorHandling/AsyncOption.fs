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

    let inline retn (value: 'value) : Async<'value option> = Async.singleton (Some value)


    let inline some (value: 'value) : Async<'value option> = Async.singleton (Some value)

    let inline apply
        (applier: Async<('input -> 'output) option>)
        (input: Async<'input option>)
        : Async<'output option> =
        bind (fun f' -> bind (fun x' -> retn (f' x')) input) applier

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
