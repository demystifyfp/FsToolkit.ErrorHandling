namespace FsToolkit.ErrorHandling

open Microsoft.FSharp.Core.CompilerServices

[<RequireQualifiedAccess>]
module List =

    let inline traverseResultM'
        state
        ([<InlineIfLambda>] f: 'okInput -> Result<'okOutput, 'error>)
        xs
        : Result<'okOutput list, 'error> =
        match xs with
        | [] -> state
        | xs ->

            match state with
            | Error e -> Error e
            | Ok initialSuccesses ->

#if FABLE_COMPILER
                let oks = ResizeArray(initialSuccesses)
#else
                let mutable oks = ListCollector()
                oks.AddMany initialSuccesses
#endif
                let mutable err = Unchecked.defaultof<'error>
                let mutable ok = true
                use e = (List.toSeq xs).GetEnumerator()

                while ok
                      && e.MoveNext() do
                    match f e.Current with
                    | Ok r -> oks.Add r
                    | Error e ->
                        err <- e
                        ok <- false

#if FABLE_COMPILER
                if ok then Ok(List.ofSeq oks) else Error err
#else
                if ok then Ok(oks.Close()) else Error err
#endif

    let rec private traverseAsyncResultM'
        (state: Async<Result<_, _>>)
        (f: _ -> Async<Result<_, _>>)
        xs
        =
        match xs with
        | [] ->
            state
            |> AsyncResult.map List.rev
        | x :: xs ->
            async {
                let! r =
                    asyncResult {
                        let! ys = state
                        let! y = f x
                        return y :: ys
                    }

                match r with
                | Ok _ -> return! traverseAsyncResultM' (Async.result r) f xs
                | Error _ -> return r
            }

    let traverseResultM f xs = traverseResultM' (Ok []) f xs

    let sequenceResultM xs = traverseResultM id xs

    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (AsyncResult.ok []) f xs

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    let inline private traverseResultA'
        (state: Result<'ok list, 'error list>)
        ([<InlineIfLambda>] f: 't -> Result<'ok, 'error>)
        (xs: 't list)
        : Result<'ok list, 'error list> =
        match xs with
        | [] -> state
        | xs ->
            match state with
            | Error failuresToDate ->
#if FABLE_COMPILER
                let errs = ResizeArray(failuresToDate)
#else
                let mutable errs = ListCollector()
                errs.AddMany failuresToDate
#endif

                for x in xs do
                    match f x with
                    | Error e -> errs.Add e
                    | Ok _ -> () // as the initial state was failure, oks are irrelevant

#if FABLE_COMPILER
                Error(List.ofSeq errs)
#else
                Error(errs.Close())
#endif
            | Ok initialSuccesses ->

                match xs with
                | [ x ] ->
                    match f x with
                    | Ok r ->
#if FABLE_COMPILER
                        let oks = ResizeArray(initialSuccesses)
#else
                        let mutable oks = ListCollector()
                        oks.AddMany initialSuccesses
#endif
                        oks.Add r
#if FABLE_COMPILER
                        Ok(Seq.toList oks)
#else
                        Ok(oks.Close())
#endif
                    | Error e -> Error [ e ]
                | xs ->

#if FABLE_COMPILER
                    let mutable oks = ResizeArray(initialSuccesses)
                    let mutable errs = ResizeArray()
#else
                    let mutable oks = ListCollector()
                    oks.AddMany initialSuccesses
                    let mutable errs = ListCollector()
#endif
                    let mutable ok = true

                    for x in xs do
                        match f x with
                        | Ok r when ok -> oks.Add r
                        | Ok _ -> () // no point saving results we won't use given the end result will be Error
                        | Error e ->
                            errs.Add e
                            ok <- false

#if FABLE_COMPILER
                    if ok then Ok(Seq.toList oks) else Error(Seq.toList errs)
#else
                    if ok then Ok(oks.Close()) else Error(errs.Close())
#endif

    let rec private traverseAsyncResultA' state f xs =
        match xs with
        | [] ->
            state
            |> AsyncResult.eitherMap List.rev List.rev

        | x :: xs ->
            async {
                let! s = state
                let! fR = f x

                match s, fR with
                | Ok ys, Ok y -> return! traverseAsyncResultA' (AsyncResult.ok (y :: ys)) f xs
                | Error errs, Error e ->
                    return! traverseAsyncResultA' (AsyncResult.error (e :: errs)) f xs
                | Ok _, Error e -> return! traverseAsyncResultA' (AsyncResult.error [ e ]) f xs
                | Error e, Ok _ -> return! traverseAsyncResultA' (AsyncResult.error e) f xs
            }

    let traverseResultA f xs = traverseResultA' (Ok []) f xs

    let sequenceResultA xs = traverseResultA id xs

    let rec private traverseValidationA' state f xs =
        match xs with
        | [] ->
            state
            |> Result.eitherMap List.rev List.rev
        | x :: xs ->
            let fR = f x

            match state, fR with
            | Ok ys, Ok y -> traverseValidationA' (Ok(y :: ys)) f xs
            | Error errs1, Error errs2 ->
                traverseValidationA'
                    (Error(
                        errs2
                        @ errs1
                    ))
                    f
                    xs
            | Ok _, Error errs
            | Error errs, Ok _ -> traverseValidationA' (Error errs) f xs

    let traverseValidationA f xs = traverseValidationA' (Ok []) f xs

    let sequenceValidationA xs = traverseValidationA id xs

    let traverseAsyncResultA f xs =
        traverseAsyncResultA' (AsyncResult.ok []) f xs

    let sequenceAsyncResultA xs = traverseAsyncResultA id xs

    let rec private traverseOptionM' (state: _ option) (f: _ -> _ option) xs =
        match xs with
        | [] ->
            state
            |> Option.map List.rev
        | x :: xs ->
            let r =
                option {
                    let! y = f x
                    let! ys = state
                    return y :: ys
                }

            match r with
            | Some _ -> traverseOptionM' r f xs
            | None -> r

    let rec private traverseAsyncOptionM' (state: Async<_ option>) (f: _ -> Async<_ option>) xs =
        match xs with
        | [] ->
            state
            |> AsyncOption.map List.rev
        | x :: xs ->
            async {
                let! o =
                    asyncOption {
                        let! y = f x
                        let! ys = state
                        return y :: ys
                    }

                match o with
                | Some _ -> return! traverseAsyncOptionM' (Async.result o) f xs
                | None -> return o
            }

    /// <summary>
    /// Applies the given function <paramref name="f"/> to each element in the input list <paramref name="xs"/>,
    /// and returns an option containing a list of the results. If any of the function applications return None,
    /// the entire result will be None.
    /// </summary>
    /// <param name="f">The function to apply to each element in the input list.</param>
    /// <param name="xs">The input list.</param>
    /// <returns>An option containing a list of the results of applying the function to each element in the input list,
    /// or None if any of the function applications return None.</returns>
    let traverseOptionM f xs = traverseOptionM' (Some []) f xs

    /// <summary>
    /// Applies the monadic function <c>id</c> to each element in the input list <paramref name="xs"/>,
    /// and returns the result as an option. If any element in the list is None, the entire result will be None.
    /// </summary>
    /// <param name="xs">The input list.</param>
    /// <returns>An option containing the result of applying <c>id</c> to each element in <paramref name="xs"/>.</returns>
    let sequenceOptionM xs = traverseOptionM id xs

    let traverseAsyncOptionM f xs =
        traverseAsyncOptionM' (AsyncOption.some []) f xs

    let sequenceAsyncOptionM xs = traverseAsyncOptionM id xs

#if !FABLE_COMPILER
    let rec private traverseVOptionM' (state: voption<_>) (f: _ -> voption<_>) xs =
        match xs with
        | [] ->
            state
            |> ValueOption.map List.rev
        | x :: xs ->
            let r =
                voption {
                    let! y = f x
                    let! ys = state
                    return y :: ys
                }

            match r with
            | ValueSome _ -> traverseVOptionM' r f xs
            | ValueNone -> r

    /// <summary>
    /// Applies the given function <paramref name="f"/> to each element in the input list <paramref name="xs"/>,
    /// and returns an option containing a list of the results. If any of the function applications return ValueNone,
    /// the entire result will be ValueNone.
    /// </summary>
    /// <param name="f">The function to apply to each element in the input list.</param>
    /// <param name="xs">The input list</param>
    /// <returns>An Option monad containing the collected results.</returns>
    let traverseVOptionM f xs = traverseVOptionM' (ValueSome []) f xs

    /// <summary>
    /// Applies the <c>id</c> function to each element in the input list <paramref name="xs"/>,
    /// and returns the result as a value option. If any element in the list is ValueNone, the entire result will be ValueNone.
    /// </summary>
    /// <param name="xs">The input list.</param>
    /// <returns>A <see cref="Option{T}"/> representing the sequence of results.</returns>
    let sequenceVOptionM xs = traverseVOptionM id xs


    open System.Threading.Tasks

    let private traverseTaskResultM' (f: 'c -> Task<Result<'a, 'b>>) (xs: 'c list) =
        let mutable state = Ok []
        let mutable remaining = xs

        task {
            while state
                  |> Result.isOk
                  && not (List.isEmpty remaining) do
                match remaining with
                | x :: xs ->
                    remaining <- xs

                    let! r = f x

                    match (r, state) with
                    | Ok y, Ok ys -> state <- Ok(y :: ys)
                    | Error e, _ -> state <- Error e
                    | _, _ -> ()
                | [] -> ()

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

    /// <summary>
    /// Partitions a list of results into a tuple of the ok values and the error values.
    /// </summary>
    /// <param name="input">The input list of results.</param>
    /// <returns>A tuple where the first element is a list of all Ok values and the second is a list of all Error values.</returns>
    let partitionResults (input: Result<'ok, 'error> list) : 'ok list * 'error list =
        if System.Object.ReferenceEquals(input, null) then
            raise (System.ArgumentNullException(nameof input))

        let oks = ResizeArray()
        let errors = ResizeArray()

        for x in input do
            match x with
            | Ok v -> oks.Add v
            | Error e -> errors.Add e

        List.ofSeq oks, List.ofSeq errors
