namespace FsToolkit.ErrorHandling

type AsyncResultOption<'ok, 'error> = Async<Result<'ok option, 'error>>

[<RequireQualifiedAccess>]
module AsyncResultOption =

    let inline some value = AsyncResult.ok (Some value)

    let inline none<'ok, 'error> : Async<Result<'ok option, 'error>> =
        AsyncResult.ok None

    [<System.Obsolete "Please use some instead of ok">]
    let inline ok x = some x

    [<System.Obsolete "Please use AsyncResult.error instead of error">]
    let inline error x = AsyncResult.error x

    [<System.Obsolete "Please use some instead of singleton">]
    let inline singleton (value: 'ok) : Async<Result<'ok option, 'error>> = some value

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
                | None -> AsyncResult.ok None
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

    let apply
        (applier: Async<Result<('okInput -> 'okOutput) option, 'error>>)
        (input: Async<Result<'okInput option, 'error>>)
        : Async<Result<'okOutput option, 'error>> =
        map2 (fun f x -> f x) applier input

#if !FABLE_COMPILER_PYTHON
    // https://github.com/fable-compiler/Fable/issues/4125
    /// Replaces the wrapped value with unit
    let ignore<'ok, 'error>
        (value: Async<Result<'ok option, 'error>>)
        : Async<Result<unit option, 'error>> =
        value
        |> map ignore<'ok>
#endif

    let inline ofResult (r: Result<'ok, 'error>) =
        r
        |> Result.map Some
        |> Async.result

    let inline ofAsyncResult (r: Async<Result<'ok, 'error>>) =
        r
        |> AsyncResult.map Some

    let inline ofOption (r: 'ok option) =
        r
        |> Ok
        |> Async.result

    let inline ofAsyncOption (r: Async<'ok option>) =
        r
        |> Async.map Ok

    /// Bind the Async&lt;'input option&gt; with a synchronous Result-returning function.
    /// A None input will be mapped to Ok None.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: Async<'input option>)
        : Async<Result<'output option, 'error>> =
        input
        |> Async.bindResult (
            function
            | Some x ->
                binder x
                |> Result.map Some
            | None -> Ok None
        )

    let inline req ([<InlineIfLambda>] reqF) errOrF value = bindResult (reqF errOrF) value
    let inline reqFilter predicate error value = req (Req.filter predicate) error value
