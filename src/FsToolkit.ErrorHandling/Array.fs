namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Array =
    let inline traverseResultM
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let results = ResizeArray<'okOutput>(xs.Length)
        let mutable index = 0
        let mutable error = Unchecked.defaultof<'error>
        let mutable ok = true

        while ok
              && index < xs.Length do
            match f xs[index] with
            | Ok value ->
                results.Add value
                index <- index + 1
            | Error e ->
                error <- e
                ok <- false

        if ok then Ok(results.ToArray()) else Error error

    let sequenceResultM xs = traverseResultM id xs

    let traverseAsyncResultM f (xs: _[]) =
        async {
            let results = ResizeArray(xs.Length)
            let mutable index = 0
            let mutable error = Unchecked.defaultof<_>
            let mutable ok = true

            while ok
                  && index < xs.Length do
                let! result = f xs[index]

                match result with
                | Ok value ->
                    results.Add value
                    index <- index + 1
                | Error e ->
                    error <- e
                    ok <- false

            return if ok then Ok(results.ToArray()) else Error error
        }

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    let inline traverseResultA
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput[])
        =
        let oks = ResizeArray<'okOutput>(xs.Length)
        let mutable errors: ResizeArray<'error> option = None
        let mutable ok = true

        for x in xs do
            match f x with
            | Ok value when ok -> oks.Add value
            | Ok _ -> ()
            | Error e ->
                let errorBuffer =
                    match errors with
                    | Some errors -> errors
                    | None ->
                        let buffer = ResizeArray<'error>()
                        errors <- Some buffer
                        buffer

                errorBuffer.Add e
                ok <- false

        if ok then
            Ok(oks.ToArray())
        else
            match errors with
            | Some errors -> Error(errors.ToArray())
            | None -> Error [||]

    let sequenceResultA xs = traverseResultA id xs

    let inline traverseValidationA
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error[]>)
        (xs: 'okInput[])
        =
        let oks = ResizeArray<'okOutput>(xs.Length)
        let mutable errors: ResizeArray<'error> option = None
        let mutable ok = true

        for x in xs do
            match f x with
            | Ok value when ok -> oks.Add value
            | Ok _ -> ()
            | Error errs ->
                let errorBuffer =
                    match errors with
                    | Some errors -> errors
                    | None ->
                        let buffer = ResizeArray<'error>()
                        errors <- Some buffer
                        buffer

                errorBuffer.AddRange errs
                ok <- false

        if ok then
            Ok(oks.ToArray())
        else
            match errors with
            | Some errors -> Error(errors.ToArray())
            | None -> Error [||]

    let sequenceValidationA xs = traverseValidationA id xs

    let traverseAsyncResultA (f: 'okInput -> Async<Result<'okOutput, 'error>>) (xs: 'okInput[]) =
        async {
            let oks = ResizeArray<'okOutput>(xs.Length)
            let mutable errors: ResizeArray<'error> option = None
            let mutable ok = true

            for x in xs do
                let! result = f x

                match result with
                | Ok value when ok -> oks.Add value
                | Ok _ -> ()
                | Error e ->
                    let errorBuffer =
                        match errors with
                        | Some errors -> errors
                        | None ->
                            let buffer = ResizeArray<'error>()
                            errors <- Some buffer
                            buffer

                    errorBuffer.Add e
                    ok <- false

            return
                if ok then
                    Ok(oks.ToArray())
                else
                    match errors with
                    | Some errors -> Error(errors.ToArray())
                    | None -> Error [||]
        }

    let sequenceAsyncResultA xs = traverseAsyncResultA id xs

    /// <summary>
    /// Applies the given function <paramref name="f"/> to each element in the input array <paramref name="xs"/>,
    /// and returns an option containing an array of the results. If any of the function applications return None,
    /// the entire result will be None.
    /// </summary>
    /// <param name="f">The function to apply to each element in the input array.</param>
    /// <param name="xs">The input array.</param>
    /// <returns>An option containing an array of the results of applying the function to each element in the input array,
    /// or None if any of the function applications return None.</returns>
    let inline traverseOptionM
        ([<InlineIfLambda>] f: 'okInput -> 'okOutput option)
        (xs: 'okInput[])
        =
        let results = ResizeArray<'okOutput>(xs.Length)
        let mutable index = 0
        let mutable ok = true

        while ok
              && index < xs.Length do
            match f xs[index] with
            | Some value ->
                results.Add value
                index <- index + 1
            | None -> ok <- false

        if ok then Some(results.ToArray()) else None

    /// <summary>
    /// Applies the monadic function <paramref name="id"/> to each element in the input array <paramref name="xs"/>,
    /// and returns the result as an option. If any element in the list is None, the entire result will be None.
    /// </summary>
    /// <param name="xs">The input array.</param>
    /// <returns>An option containing the result of applying <paramref name="id"/> to each element in <paramref name="xs"/>.</returns>
    let sequenceOptionM xs = traverseOptionM id xs

    let traverseAsyncOptionM f (xs: _[]) =
        async {
            let results = ResizeArray(xs.Length)
            let mutable index = 0
            let mutable ok = true

            while ok
                  && index < xs.Length do
                let! result = f xs[index]

                match result with
                | Some value ->
                    results.Add value
                    index <- index + 1
                | None -> ok <- false

            return if ok then Some(results.ToArray()) else None
        }

    let sequenceAsyncOptionM xs = traverseAsyncOptionM id xs

#if !FABLE_COMPILER
    /// <summary>
    /// Applies the given function <paramref name="f"/> to each element in the input array <paramref name="xs"/>,
    /// and returns an option containing an array of the results. If any of the function applications return ValueNone,
    /// the entire result will be ValueNone.
    /// </summary>
    /// <param name="f">The function to apply to each element in the input array.</param>
    /// <param name="xs">The input array.</param>
    /// <returns>An Option monad containing the collected results.</returns>
    let inline traverseVOptionM
        ([<InlineIfLambda>] f: 'okInput -> 'okOutput voption)
        (xs: 'okInput[])
        =
        let results = ResizeArray<'okOutput>(xs.Length)
        let mutable index = 0
        let mutable ok = true

        while ok
              && index < xs.Length do
            match f xs[index] with
            | ValueSome value ->
                results.Add value
                index <- index + 1
            | ValueNone -> ok <- false

        if ok then ValueSome(results.ToArray()) else ValueNone

    /// <summary>
    /// Applies the <paramref name="id"/> function to each element in the input array <paramref name="xs"/>,
    /// and returns the result as a value option. If any element in the list is ValueNone, the entire result will be ValueNone.
    /// </summary>
    /// <param name="xs">The input array.</param>
    /// <returns>A <see cref="Option{T}"/> representing the sequence of results.</returns>
    let sequenceVOptionM xs = traverseVOptionM id xs

#endif

    /// <summary>
    /// Partitions an array of results into a tuple of the ok values and the error values.
    /// </summary>
    /// <param name="input">The input array of results.</param>
    /// <returns>A tuple where the first element is an array of all Ok values and the second is an array of all Error values.</returns>
    let partitionResults (input: Result<'ok, 'error>[]) : 'ok[] * 'error[] =
        if System.Object.ReferenceEquals(input, null) then
            nullArg (nameof input)

        let oks = ResizeArray()
        let errors = ResizeArray()

        for x in input do
            match x with
            | Ok v -> oks.Add v
            | Error e -> errors.Add e

        oks.ToArray(), errors.ToArray()
