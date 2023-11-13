namespace FsToolkit.ErrorHandling

open System.Threading.Tasks

/// TaskValidation<'a, 'err> is defined as Async<Result<'a, 'err list>> meaning you can use many of the functions found in the Result and Async module.
type TaskValidation<'ok, 'error> = Task<Result<'ok, 'error list>>

[<RequireQualifiedAccess>]
module TaskValidation =
    let inline ok (value: 'ok) : TaskValidation<'ok, 'error> =
        Ok value
        |> Task.FromResult

    let inline error (error: 'error) : TaskValidation<'ok, 'error> =
        Error [ error ]
        |> Task.FromResult

    let inline ofResult (result: Result<'ok, 'error>) : TaskValidation<'ok, 'error> =
        Result.mapError List.singleton result
        |> Task.FromResult


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

    let inline retn (value: 'ok) : TaskValidation<'ok, 'error> = ok value

    let inline returnError (error: 'error) : TaskValidation<'ok, 'error> =
        Error [ error ]
        |> Task.FromResult

    let inline orElse
        (ifError: TaskValidation<'ok, 'errorOutput>)
        (result: TaskValidation<'ok, 'errorInput>)
        : TaskValidation<'ok, 'errorOutput> =
        task {
            let! result = result

            return!
                result
                |> Result.either ok (fun _ -> ifError)
        }

    let inline orElseWith
        ([<InlineIfLambda>] ifErrorFunc: 'errorInput list -> TaskValidation<'ok, 'errorOutput>)
        (result: TaskValidation<'ok, 'errorInput>)
        : TaskValidation<'ok, 'errorOutput> =
        task {
            let! result = result

            return!
                match result with
                | Ok x -> ok x
                | Error err -> ifErrorFunc err
        }

    let inline map
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: TaskValidation<'okInput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        task {
            let! input = input
            return Result.map mapper input
        }

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

    let inline mapError
        ([<InlineIfLambda>] errorMapper: 'errorInput -> 'errorOutput)
        (input: TaskValidation<'ok, 'errorInput>)
        : TaskValidation<'ok, 'errorOutput> =
        task {
            let! input = input
            return Result.mapError (List.map errorMapper) input
        }

    let inline mapErrors
        ([<InlineIfLambda>] errorMapper: 'errorInput list -> 'errorOutput list)
        (input: TaskValidation<'ok, 'errorInput>)
        : TaskValidation<'ok, 'errorOutput> =
        task {
            let! input = input
            return Result.mapError errorMapper input
        }

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
