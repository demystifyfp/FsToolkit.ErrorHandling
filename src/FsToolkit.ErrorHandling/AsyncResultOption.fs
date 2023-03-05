namespace FsToolkit.ErrorHandling


type AsyncResultOption<'ok, 'error> = Async<Result<Option<'ok>, 'error>>

[<RequireQualifiedAccess>]
module AsyncResultOption =

    let inline map
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: Async<Result<'okInput option, 'error>>)
        : Async<Result<'okOutput option, 'error>> =
        AsyncResult.map (Option.map mapper) input

    let inline bind
        ([<InlineIfLambda>] binder: 'okInput -> Async<Result<'okOutput option, 'error>>)
        (input: Async<Result<'okInput option, 'error>>)
        : Async<Result<'okOutput option, 'error>> =
        AsyncResult.bind
            (fun opt ->
                match opt with
                | Some x -> binder x
                | None -> AsyncResult.retn None
            )
            input


    let inline map2
        ([<InlineIfLambda>] mapper: 'okInput1 -> 'okInput2 -> 'okOutput)
        (input1: Async<Result<'okInput1 option, 'error>>)
        (input2: Async<Result<'okInput2 option, 'error>>)
        : Async<Result<'okOutput option, 'error>> =
        AsyncResult.map2 (Option.map2 mapper) input1 input2

    let inline map3
        ([<InlineIfLambda>] mapper: 'okInput1 -> 'okInput2 -> 'okInput3 -> 'okOutput)
        (input1: Async<Result<'okInput1 option, 'error>>)
        (input2: Async<Result<'okInput2 option, 'error>>)
        (input3: Async<Result<'okInput3 option, 'error>>)
        : Async<Result<'okOutput option, 'error>> =
        AsyncResult.map3 (Option.map3 mapper) input1 input2 input3

    let inline retn (value: 'ok) : Async<Result<'ok option, 'error>> = AsyncResult.retn (Some value)

    let apply
        (applier: Async<Result<('okInput -> 'okOutput) option, 'error>>)
        (input: Async<Result<'okInput option, 'error>>)
        : Async<Result<'okOutput option, 'error>> =
        map2 (fun f x -> f x) applier input

    /// Replaces the wrapped value with unit
    let ignore<'ok, 'error>
        (value: Async<Result<'ok option, 'error>>)
        : Async<Result<unit option, 'error>> =
        value
        |> map ignore<'ok>

    let inline ofResult (r: Result<'ok, 'error>) =
        r
        |> Result.map Some
        |> Async.singleton


    let inline ofAsyncResult (r: Async<Result<'ok, 'error>>) =
        r
        |> AsyncResult.map Some

    let inline ofOption (r: Option<'ok>) =
        r
        |> Ok
        |> Async.singleton

    let inline ofAsyncOption (r: Async<Option<'ok>>) =
        r
        |> Async.map Ok
