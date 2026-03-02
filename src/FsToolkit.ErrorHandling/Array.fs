namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Array =

    // O(n): iterative ResizeArray accumulation, early exit on first error
    let traverseResultM
        (f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput array)
        : Result<'okOutput array, 'error> =
        let oks = ResizeArray xs.Length
        let mutable err = Unchecked.defaultof<'error>
        let mutable ok = true
        let mutable i = 0

        while ok
              && i < xs.Length do
            match f xs.[i] with
            | Ok r ->
                oks.Add r
                i <- i + 1
            | Error e ->
                err <- e
                ok <- false

        if ok then Ok(oks.ToArray()) else Error err

    let sequenceResultM xs = traverseResultM id xs

    // O(n): iterative async, early exit on first error
    let traverseAsyncResultM
        (f: 'okInput -> Async<Result<'okOutput, 'error>>)
        (xs: 'okInput array)
        : Async<Result<'okOutput array, 'error>> =
        async {
            let oks = ResizeArray xs.Length
            let mutable err = Unchecked.defaultof<'error>
            let mutable ok = true
            let mutable i = 0

            while ok
                  && i < xs.Length do
                match! f xs.[i] with
                | Ok r ->
                    oks.Add r
                    i <- i + 1
                | Error e ->
                    err <- e
                    ok <- false

            return if ok then Ok(oks.ToArray()) else Error err
        }

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    // O(n): applicative — collects all errors across the array in a single pass
    let traverseResultA
        (f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput array)
        : Result<'okOutput array, 'error array> =
        let oks = ResizeArray xs.Length
        let errs = ResizeArray()

        for x in xs do
            match f x with
            | Ok r when errs.Count = 0 -> oks.Add r
            | Ok _ -> ()
            | Error e -> errs.Add e

        if errs.Count = 0 then
            Ok(oks.ToArray())
        else
            Error(errs.ToArray())

    let sequenceResultA xs = traverseResultA id xs

    // O(n): applicative async — collects all errors across the array in a single pass
    let traverseAsyncResultA
        (f: 'okInput -> Async<Result<'okOutput, 'error>>)
        (xs: 'okInput array)
        : Async<Result<'okOutput array, 'error array>> =
        async {
            let oks = ResizeArray xs.Length
            let errs = ResizeArray()

            for x in xs do
                match! f x with
                | Ok r when errs.Count = 0 -> oks.Add r
                | Ok _ -> ()
                | Error e -> errs.Add e

            return
                if errs.Count = 0 then
                    Ok(oks.ToArray())
                else
                    Error(errs.ToArray())
        }

    let sequenceAsyncResultA xs = traverseAsyncResultA id xs

    // O(n): validation applicative — collects all error arrays into one
    let traverseValidationA
        (f: 'okInput -> Result<'okOutput, 'error array>)
        (xs: 'okInput array)
        : Result<'okOutput array, 'error array> =
        let oks = ResizeArray xs.Length
        let errs = ResizeArray()

        for x in xs do
            match f x with
            | Ok r when errs.Count = 0 -> oks.Add r
            | Ok _ -> ()
            | Error es -> errs.AddRange es

        if errs.Count = 0 then
            Ok(oks.ToArray())
        else
            Error(errs.ToArray())

    let sequenceValidationA xs = traverseValidationA id xs

    /// <summary>
    /// Applies the given function <paramref name="f"/> to each element in the input list <paramref name="xs"/>,
    /// and returns an option containing a list of the results. If any of the function applications return None,
    /// the entire result will be None.
    /// </summary>
    /// <param name="f">The function to apply to each element in the input list.</param>
    /// <param name="xs">The input list.</param>
    /// <returns>An option containing a list of the results of applying the function to each element in the input list,
    /// or None if any of the function applications return None.</returns>
    let traverseOptionM
        (f: 'okInput -> 'okOutput option)
        (xs: 'okInput array)
        : 'okOutput array option =
        let oks = ResizeArray xs.Length
        let mutable ok = true
        let mutable i = 0

        while ok
              && i < xs.Length do
            match f xs.[i] with
            | Some r ->
                oks.Add r
                i <- i + 1
            | None -> ok <- false

        if ok then Some(oks.ToArray()) else None

    /// <summary>
    /// Applies the monadic function <paramref name="id"/> to each element in the input list <paramref name="xs"/>,
    /// and returns the result as an option. If any element in the list is None, the entire result will be None.
    /// </summary>
    /// <param name="xs">The input list.</param>
    /// <returns>An option containing the result of applying <paramref name="id"/> to each element in <paramref name="xs"/>.</returns>
    let sequenceOptionM xs = traverseOptionM id xs

    // O(n): async option monad — iterative, early exit on None
    let traverseAsyncOptionM
        (f: 'okInput -> Async<'okOutput option>)
        (xs: 'okInput array)
        : Async<'okOutput array option> =
        async {
            let oks = ResizeArray xs.Length
            let mutable ok = true
            let mutable i = 0

            while ok
                  && i < xs.Length do
                match! f xs.[i] with
                | Some r ->
                    oks.Add r
                    i <- i + 1
                | None -> ok <- false

            return if ok then Some(oks.ToArray()) else None
        }

    let sequenceAsyncOptionM xs = traverseAsyncOptionM id xs

#if !FABLE_COMPILER
    /// <summary>
    /// Applies the given function <paramref name="f"/> to each element in the input list <paramref name="xs"/>,
    /// and returns an option containing a list of the results. If any of the function applications return ValueNone,
    /// the entire result will be ValueNone.
    /// </summary>
    /// <param name="f">The function to apply to each element in the input list.</param>
    /// <param name="xs">The input list</param>
    /// <returns>An Option monad containing the collected results.</returns>
    let traverseVOptionM
        (f: 'okInput -> 'okOutput voption)
        (xs: 'okInput array)
        : 'okOutput array voption =
        let oks = ResizeArray xs.Length
        let mutable ok = true
        let mutable i = 0

        while ok
              && i < xs.Length do
            match f xs.[i] with
            | ValueSome r ->
                oks.Add r
                i <- i + 1
            | ValueNone -> ok <- false

        if ok then ValueSome(oks.ToArray()) else ValueNone

    /// <summary>
    /// Applies the <paramref name="id"/> function to each element in the input list <paramref name="xs"/>,
    /// and returns the result as a value option. If any element in the list is ValueNone, the entire result will be ValueNone.
    /// </summary>
    /// <param name="xs">The input list.</param>
    /// <returns>A <see cref="Option{T}"/> representing the sequence of results.</returns>
    let sequenceVOptionM xs = traverseVOptionM id xs

#endif
