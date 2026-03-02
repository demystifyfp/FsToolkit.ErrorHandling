namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module List =

    // O(n): iterative ResizeArray accumulation, early exit on first error, no List.rev copy
    let traverseResultM
        (f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput list)
        : Result<'okOutput list, 'error> =
        let oks = ResizeArray()
        let mutable err = Unchecked.defaultof<'error>
        let mutable ok = true
        let mutable current = xs

        while ok
              && not current.IsEmpty do
            match f current.Head with
            | Ok r ->
                oks.Add r
                current <- current.Tail
            | Error e ->
                err <- e
                ok <- false

        if ok then Ok(List.ofSeq oks) else Error err

    let sequenceResultM xs = traverseResultM id xs

    // O(n): iterative async, early exit on first error, no List.rev copy
    let traverseAsyncResultM
        (f: 'okInput -> Async<Result<'okOutput, 'error>>)
        (xs: 'okInput list)
        : Async<Result<'okOutput list, 'error>> =
        async {
            let oks = ResizeArray()
            let mutable err = Unchecked.defaultof<'error>
            let mutable ok = true
            let mutable current = xs

            while ok
                  && not current.IsEmpty do
                match! f current.Head with
                | Ok r ->
                    oks.Add r
                    current <- current.Tail
                | Error e ->
                    err <- e
                    ok <- false

            return if ok then Ok(List.ofSeq oks) else Error err
        }

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    // O(n): applicative — collects all errors in a single pass, no List.rev copy
    let traverseResultA
        (f: 'okInput -> Result<'okOutput, 'error>)
        (xs: 'okInput list)
        : Result<'okOutput list, 'error list> =
        let oks = ResizeArray()
        let errs = ResizeArray()

        for x in xs do
            match f x with
            | Ok r when errs.Count = 0 -> oks.Add r
            | Ok _ -> ()
            | Error e -> errs.Add e

        if errs.Count = 0 then
            Ok(List.ofSeq oks)
        else
            Error(List.ofSeq errs)

    let sequenceResultA xs = traverseResultA id xs

    // O(n): applicative async — collects all errors in a single pass, no List.rev copy
    let traverseAsyncResultA
        (f: 'okInput -> Async<Result<'okOutput, 'error>>)
        (xs: 'okInput list)
        : Async<Result<'okOutput list, 'error list>> =
        async {
            let oks = ResizeArray()
            let errs = ResizeArray()

            for x in xs do
                match! f x with
                | Ok r when errs.Count = 0 -> oks.Add r
                | Ok _ -> ()
                | Error e -> errs.Add e

            return
                if errs.Count = 0 then
                    Ok(List.ofSeq oks)
                else
                    Error(List.ofSeq errs)
        }

    let sequenceAsyncResultA xs = traverseAsyncResultA id xs

    // O(n): validation applicative — collects all error lists into one, no List.rev or @ copy
    let traverseValidationA
        (f: 'okInput -> Result<'okOutput, 'error list>)
        (xs: 'okInput list)
        : Result<'okOutput list, 'error list> =
        let oks = ResizeArray()
        let errs = ResizeArray()

        for x in xs do
            match f x with
            | Ok r when errs.Count = 0 -> oks.Add r
            | Ok _ -> ()
            | Error es -> errs.AddRange es

        if errs.Count = 0 then
            Ok(List.ofSeq oks)
        else
            Error(List.ofSeq errs)

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
    // O(n): option monad — iterative, early exit on None, no List.rev copy
    let traverseOptionM
        (f: 'okInput -> 'okOutput option)
        (xs: 'okInput list)
        : 'okOutput list option =
        let oks = ResizeArray()
        let mutable ok = true
        let mutable current = xs

        while ok
              && not current.IsEmpty do
            match f current.Head with
            | Some r ->
                oks.Add r
                current <- current.Tail
            | None -> ok <- false

        if ok then Some(List.ofSeq oks) else None

    /// <summary>
    /// Applies the monadic function <paramref name="id"/> to each element in the input list <paramref name="xs"/>,
    /// and returns the result as an option. If any element in the list is None, the entire result will be None.
    /// </summary>
    /// <param name="xs">The input list.</param>
    /// <returns>An option containing the result of applying <paramref name="id"/> to each element in <paramref name="xs"/>.</returns>
    let sequenceOptionM xs = traverseOptionM id xs

    // O(n): async option monad — iterative, early exit on None, no List.rev copy
    let traverseAsyncOptionM
        (f: 'okInput -> Async<'okOutput option>)
        (xs: 'okInput list)
        : Async<'okOutput list option> =
        async {
            let oks = ResizeArray()
            let mutable ok = true
            let mutable current = xs

            while ok
                  && not current.IsEmpty do
                match! f current.Head with
                | Some r ->
                    oks.Add r
                    current <- current.Tail
                | None -> ok <- false

            return if ok then Some(List.ofSeq oks) else None
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
    // O(n): value option monad — iterative, early exit on ValueNone, no List.rev copy
    let traverseVOptionM
        (f: 'okInput -> 'okOutput voption)
        (xs: 'okInput list)
        : 'okOutput list voption =
        let oks = ResizeArray()
        let mutable ok = true
        let mutable current = xs

        while ok
              && not current.IsEmpty do
            match f current.Head with
            | ValueSome r ->
                oks.Add r
                current <- current.Tail
            | ValueNone -> ok <- false

        if ok then ValueSome(List.ofSeq oks) else ValueNone

    /// <summary>
    /// Applies the <paramref name="id"/> function to each element in the input list <paramref name="xs"/>,
    /// and returns the result as a value option. If any element in the list is ValueNone, the entire result will be ValueNone.
    /// </summary>
    /// <param name="xs">The input list.</param>
    /// <returns>A <see cref="Option{T}"/> representing the sequence of results.</returns>
    let sequenceVOptionM xs = traverseVOptionM id xs


    open System.Threading.Tasks

    let private traverseTaskResultM' (f: 'c -> Task<Result<'a, 'b>>) (xs: 'c list) =
        let mutable state = Ok []
        let mutable index = 0

        let xs =
            xs
            |> List.toArray

        task {
            while state
                  |> Result.isOk
                  && index < xs.Length do
                let! r =
                    xs
                    |> Array.item index
                    |> f

                index <- index + 1

                match (r, state) with
                | Ok y, Ok ys -> state <- Ok(y :: ys)
                | Error e, _ -> state <- Error e
                | _, _ -> ()

            return
                state
                |> Result.map List.rev
        }

    let traverseTaskResultM f xs = traverseTaskResultM' f xs

    let sequenceTaskResultM xs = traverseTaskResultM id xs

    let private traverseTaskResultA' (f: 'c -> Task<Result<'a, 'b>>) (xs: 'c list) =
        let mutable state = Ok []

        task {
            for x in xs do
                let! r = f x

                match (r, state) with
                | Ok y, Ok ys -> state <- Ok(y :: ys)
                | Error e, Error errs -> state <- Error(e :: errs)
                | Ok _, Error e -> state <- Error e
                | Error e, Ok _ -> state <- Error [ e ]

            return
                state
                |> Result.eitherMap List.rev List.rev
        }

    let traverseTaskResultA f xs = traverseTaskResultA' f xs

    let sequenceTaskResultA xs = traverseTaskResultA id xs


#endif
