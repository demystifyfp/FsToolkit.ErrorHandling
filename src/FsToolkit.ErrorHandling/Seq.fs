[<RequireQualifiedAccess>]
module FsToolkit.ErrorHandling.Seq

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single result
    /// </summary>
    /// <param name="state">The initial state</param>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>A result with the ok elements in a sequence or the first error occurring in the sequence</returns>
    let inline traverseResultM'
        state
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput seq)
        =
        match state with
        | Error _ -> state
        | Ok oks ->
            use enumerator = xs.GetEnumerator()

            let rec loop oks =
                if enumerator.MoveNext() then
                    match f enumerator.Current with
                    | Ok ok ->
                        loop (
                            seq {
                                yield ok
                                yield! oks
                            }
                        )
                    | Error e -> Error e
                else
                    Ok(Seq.rev oks)

            loop oks

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single result
    /// </summary>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>A result with the ok elements in a sequence or the first error occurring in the sequence</returns>
    /// <remarks>This function is equivalent to <see cref="traverseResultM'"/> but applying and initial state of 'Seq.empty'</remarks>
    let traverseResultM f xs = traverseResultM' (Ok Seq.empty) f xs

    /// <summary>
    /// Converts a sequence of results into a single result
    /// </summary>
    /// <param name="xs">The input sequence</param>
    /// <returns>A result with the ok elements in a sequence or the first error occurring in the sequence</returns>
    /// <remarks>This function is equivalent to <see cref="traverseResultM"/> but auto-applying the 'id' function</remarks>
    let sequenceResultM xs = traverseResultM id xs

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single result
    /// </summary>
    /// <param name="state">The initial state</param>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>A result with the ok elements in a sequence or a sequence of all errors occuring in the original sequence</returns>
    let inline traverseResultA'
        state
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        xs
        =
        let folder state x =
            match state, f x with
            | Error errors, Error e ->
                Seq.append errors (Seq.singleton e)
                |> Error
            | Ok oks, Ok ok ->
                Seq.append oks (Seq.singleton ok)
                |> Ok
            | Ok _, Error e ->
                Seq.singleton e
                |> Error
            | Error _, Ok _ -> state

        Seq.fold folder state xs

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single result
    /// </summary>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>A result with the ok elements in a sequence or a sequence of all errors occuring in the original sequence</returns>
    /// <remarks>This function is equivalent to <see cref="traverseResultA'"/> but applying and initial state of 'Seq.empty'</remarks>
    let traverseResultA f xs = traverseResultA' (Ok Seq.empty) f xs

    /// <summary>
    /// Converts a sequence of results into a single result
    /// </summary>
    /// <param name="xs">The input sequence</param>
    /// <returns>A result with the ok elements in a sequence or a sequence of all errors occuring in the original sequence</returns>
    /// <remarks>This function is equivalent to <see cref="traverseResultA"/> but auto-applying the 'id' function</remarks>
    let sequenceResultA xs = traverseResultA id xs

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single async result
    /// </summary>
    /// <param name="state">The initial state</param>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async result with the ok elements in a sequence or the first error occurring in the sequence</returns>
    let inline traverseAsyncResultM'
        state
        ([<InlineIfLambda>] f: 'okInput -> Async<Result<'okOutput, 'error>>)
        (xs: 'okInput seq)
        =
        async {
            match! state with
            | Error _ -> return! state
            | Ok oks ->
                use enumerator = xs.GetEnumerator()

                let rec loop oks =
                    async {
                        if enumerator.MoveNext() then
                            match! f enumerator.Current with
                            | Ok ok ->
                                return!
                                    loop (
                                        seq {
                                            yield ok
                                            yield! oks
                                        }
                                    )
                            | Error e -> return Error e
                        else
                            return Ok(Seq.rev oks)
                    }

                return! loop oks
        }

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single async result
    /// </summary>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async result with the ok elements in a sequence or the first error occurring in the sequence</returns>
    /// <remarks>This function is equivalent to <see cref="traverseAsyncResultM'"/> but applying and initial state of 'Seq.empty'</remarks>
    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (async { return Ok Seq.empty }) f xs

    /// <summary>
    /// Converts a sequence of async results into a single async result
    /// </summary>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async result with the ok elements in a sequence or the first error occurring in the sequence</returns>
    /// <remarks>This function is equivalent to <see cref="traverseAsyncResultM"/> but auto-applying the 'id' function</remarks>
    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single async result
    /// </summary>
    /// <param name="state">The initial state</param>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async result with the ok elements in a sequence or a sequence of all errors occuring in the original sequence</returns>
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
                        Seq.append errors (Seq.singleton e)
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

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single async result
    /// </summary>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async result with the ok elements in a sequence or a sequence of all errors occuring in the original sequence</returns>
    /// <remarks>This function is equivalent to <see cref="traverseAsyncResultA'"/> but applying and initial state of 'Seq.empty'</remarks>
    let traverseAsyncResultA f xs =
        traverseAsyncResultA' (async { return Ok Seq.empty }) f xs

    /// <summary>
    /// Converts a sequence of async results into a single async result
    /// </summary>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async result with the ok elements in a sequence or a sequence of all errors occuring in the original sequence</returns>
    /// <remarks>This function is equivalent to <see cref="traverseAsyncResultA"/> but auto-applying the 'id' function</remarks>
    let sequenceAsyncResultA xs = traverseAsyncResultA id xs

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single option
    /// </summary>
    /// <param name="state">The initial state</param>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>An option containing Some sequence of elements or None if any of the function applications return None</returns>
    let inline traverseOptionM'
        state
        ([<InlineIfLambda>] f: 'okInput -> 'okOutput option)
        (xs: 'okInput seq)
        =
        match state with
        | None -> state
        | Some values ->
            use enumerator = xs.GetEnumerator()

            let rec loop values =
                if enumerator.MoveNext() then
                    match f enumerator.Current with
                    | Some value ->
                        loop (
                            seq {
                                yield value
                                yield! values
                            }
                        )
                    | None -> None
                else
                    Some(Seq.rev values)

            loop values

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single option
    /// </summary>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>An option containing Some sequence of elements or None if any of the function applications return None</returns>
    /// <remarks>This function is equivalent to <see cref="traverseOptionM'"/> but applying and initial state of 'Seq.empty'</remarks>
    let traverseOptionM f xs = traverseOptionM' (Some Seq.empty) f xs

    /// <summary>
    /// Converts a sequence of options into a single option
    /// </summary>
    /// <param name="xs">The input sequence</param>
    /// <returns>An option containing Some sequence of elements or None if any of the function applications return None</returns>
    /// <remarks>This function is equivalent to <see cref="traverseOptionM"/> but auto-applying the 'id' function</remarks>
    let sequenceOptionM xs = traverseOptionM id xs

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single async option
    /// </summary>
    /// <param name="state">The initial state</param>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async option containing Some sequence of elements or None if any of the function applications return None</returns>
    let inline traverseAsyncOptionM'
        state
        ([<InlineIfLambda>] f: 'okInput -> Async<'okOutput option>)
        (xs: 'okInput seq)
        =
        async {
            match! state with
            | None -> return! state
            | Some values ->
                use enumerator = xs.GetEnumerator()

                let rec loop values =
                    async {
                        if enumerator.MoveNext() then
                            match! f enumerator.Current with
                            | Some value ->
                                return!
                                    loop (
                                        seq {
                                            yield value
                                            yield! values
                                        }
                                    )
                            | None -> return None
                        else
                            return Some(Seq.rev values)
                    }

                return! loop values
        }

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single async option
    /// </summary>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async option containing Some sequence of elements or None if any of the function applications return None</returns>
    /// <remarks>This function is equivalent to <see cref="traverseAsyncOptionM'"/> but applying and initial state of 'Async { return Some Seq.empty }'</remarks>
    let traverseAsyncOptionM f xs =
        traverseAsyncOptionM' (async { return Some Seq.empty }) f xs

    /// <summary>
    /// Converts a sequence of async options into a single async option
    /// </summary>
    /// <param name="xs">The input sequence</param>
    /// <returns>An async option containing Some sequence of elements or None if any of the function applications return None</returns>
    /// <remarks>This function is equivalent to <see cref="traverseAsyncOptionM"/> but auto-applying the 'id' function</remarks>
    let sequenceAsyncOptionM xs = traverseAsyncOptionM id xs

#if !FABLE_COMPILER

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single voption
    /// </summary>
    /// <param name="state">The initial state</param>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>A voption containing Some sequence of elements or None if any of the function applications return None</returns>
    let inline traverseVOptionM'
        state
        ([<InlineIfLambda>] f: 'okInput -> 'okOutput voption)
        (xs: 'okInput seq)
        =
        match state with
        | ValueNone -> state
        | ValueSome values ->
            use enumerator = xs.GetEnumerator()

            let rec loop values =
                if enumerator.MoveNext() then
                    match f enumerator.Current with
                    | ValueSome value ->
                        loop (
                            seq {
                                yield value
                                yield! values
                            }
                        )
                    | ValueNone -> ValueNone
                else
                    ValueSome(Seq.rev values)

            loop values

    /// <summary>
    /// Applies a function to each element of a sequence and returns a single voption
    /// </summary>
    /// <param name="f">The function to apply to each element</param>
    /// <param name="xs">The input sequence</param>
    /// <returns>A voption containing Some sequence of elements or None if any of the function applications return None</returns>
    /// <remarks>This function is equivalent to <see cref="traverseVOptionM'"/> but applying and initial state of 'ValueSome Seq.empty'</remarks>
    let traverseVOptionM f xs =
        traverseVOptionM' (ValueSome Seq.empty) f xs

    /// <summary>
    /// Converts a sequence of voptions into a single voption
    /// </summary>
    /// <param name="xs">The input sequence</param>
    /// <returns>A voption containing Some sequence of elements or None if any of the function applications return None</returns>
    /// <remarks>This function is equivalent to <see cref="traverseVOptionM"/> but auto-applying the 'id' function</remarks>
    let sequenceVOptionM xs = traverseVOptionM id xs

#endif
