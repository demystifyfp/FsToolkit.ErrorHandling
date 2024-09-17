namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Seq =

    let inline traverseResultM'
        state
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        xs
        =
        let folder state x =
            match state, f x with
            | Error e, _ -> Error e
            | Ok oks, Ok ok ->
                Seq.singleton ok
                |> Seq.append oks
                |> Ok
            | Ok _, Error e -> Error e

        Seq.fold folder state xs

    let traverseResultM f xs = traverseResultM' (Ok Seq.empty) f xs

    let sequenceResultM xs = traverseResultM id xs

    let inline traverseResultA'
        state
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        xs
        =
        let folder state x =
            match state, f x with
            | Error errors, Error e ->
                Seq.append (Seq.singleton e) errors
                |> Error
            | Ok oks, Ok ok ->
                Seq.append oks (Seq.singleton ok)
                |> Ok
            | Ok _, Error e ->
                Seq.singleton e
                |> Error
            | Error _, Ok _ -> state

        Seq.fold folder state xs

    let traverseResultA f xs = traverseResultA' (Ok Seq.empty) f xs

    let sequenceResultA xs = traverseResultA id xs

    let inline traverseAsyncResultM'
        state
        ([<InlineIfLambda>] f: 'okInput -> Async<Result<'okOutput, 'error>>)
        xs
        =
        let folder state x =
            async {
                let! state = state
                let! result = f x

                return
                    match state, result with
                    | Error e, _ -> Error e
                    | Ok oks, Ok ok ->
                        Seq.singleton ok
                        |> Seq.append oks
                        |> Ok
                    | Ok _, Error e -> Error e
            }

        Seq.fold folder state xs

    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (async { return Ok Seq.empty }) f xs

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    let inline traverseAsyncResultA'
        state
        ([<InlineIfLambda>] f: 'okInput -> Async<Result<'okOutput, 'error>>)
        xs
        =
        let folder state x =
            async {
                let! state = state
                let! result = f x

                return
                    match state, result with
                    | Error errors, Error e ->
                        Seq.append (Seq.singleton e) errors
                        |> Error
                    | Ok oks, Ok ok ->
                        Seq.append oks (Seq.singleton ok)
                        |> Ok
                    | Ok _, Error e ->
                        Seq.singleton e
                        |> Error
                    | Error _, Ok _ -> state
            }

        Seq.fold folder state xs

    let traverseAsyncResultA f xs =
        traverseAsyncResultA' (async { return Ok Seq.empty }) f xs

    let sequenceAsyncResultA xs = traverseAsyncResultA id xs
