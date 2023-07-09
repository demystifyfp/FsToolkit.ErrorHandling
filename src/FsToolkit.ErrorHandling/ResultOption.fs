namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module ResultOption =

    let inline retn x = Ok(Some x)

    let inline map
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: Result<'okInput option, 'error>)
        : Result<'okOutput option, 'error> =
        Result.map (Option.map mapper) input

    let inline mapError
        ([<InlineIfLambda>] mapper: 'errorInput -> 'errorOutput)
        (input: Result<'ok option, 'errorInput>)
        : Result<'ok option, 'errorOutput> =
        Result.mapError mapper input

    let inline bind
        ([<InlineIfLambda>] binder: 'okInput -> Result<'okOutput option, 'error>)
        (input: Result<'okInput option, 'error>)
        : Result<'okOutput option, 'error> =
        Result.bind
            (function
            | Some x -> binder x
            | None -> Ok None)
            input

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

    let zip
        (left: Result<'leftOk option, 'error>)
        (right: Result<'rightOk option, 'error>)
        : Result<('leftOk * 'rightOk) option, 'error> =
        match left, right with
        | Ok x1res, Ok x2res -> //Ok(x1res, x2res)
            match x1res, x2res with
            | Some x1, Some x2 -> Ok(Some(x1, x2))
            | _ -> Ok None
        | Error e, _ -> Error e
        | _, Error e -> Error e

    let zipError
        (left: Result<'ok option, 'leftError>)
        (right: Result<'ok option, 'rightError>)
        : Result<'ok option, 'leftError * 'rightError> =
        match left, right with
        | Error x1res, Error x2res -> Error(x1res, x2res)
        | Ok e, _ -> Ok e
        | _, Ok e -> Ok e

    /// Replaces the wrapped value with unit
    let inline ignore<'ok, 'error> (resultOpt: Result<'ok option, 'error>) =
        resultOpt
        |> map ignore<'ok>

    let inline ofResult (result: Result<'ok, 'error>) : Result<'ok option, 'error> =
        match result with
        | Ok x -> Ok(Some x)
        | Error e -> Error e

    let inline ofOption (option: 'T option) : Result<'T option, 'error> = Ok option

    let inline ofChoice (choice: Choice<'ok, 'error>) : Result<'ok option, 'error> =
        match choice with
        | Choice1Of2 x -> Ok(Some x)
        | Choice2Of2 e -> Error e
