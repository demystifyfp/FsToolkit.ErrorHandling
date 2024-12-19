namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module List =

    let rec private traverseResultM' (state: Result<_, _>) (f: _ -> Result<_, _>) xs =
        match xs with
        | [] ->
            state
            |> Result.map List.rev
        | x :: xs ->
            let r =
                result {
                    let! y = f x
                    let! ys = state
                    return y :: ys
                }

            match r with
            | Ok _ -> traverseResultM' r f xs
            | Error _ -> r

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
                | Ok _ -> return! traverseAsyncResultM' (Async.singleton r) f xs
                | Error _ -> return r
            }

    let traverseResultM f xs = traverseResultM' (Ok []) f xs

    let sequenceResultM xs = traverseResultM id xs

    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (AsyncResult.ok []) f xs

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    let rec private traverseResultA' state f xs =
        match xs with
        | [] ->
            state
            |> Result.eitherMap List.rev List.rev
        | x :: xs ->

            match state, f x with
            | Ok ys, Ok y -> traverseResultA' (Ok(y :: ys)) f xs
            | Error errs, Error e -> traverseResultA' (Error(e :: errs)) f xs
            | Ok _, Error e -> traverseResultA' (Error [ e ]) f xs
            | Error e, Ok _ -> traverseResultA' (Error e) f xs

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
                    return! traverseAsyncResultA' (AsyncResult.returnError (e :: errs)) f xs
                | Ok _, Error e ->
                    return! traverseAsyncResultA' (AsyncResult.returnError [ e ]) f xs
                | Error e, Ok _ -> return! traverseAsyncResultA' (AsyncResult.returnError e) f xs
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

    let rec private traverseOptionM' (state: Option<_>) (f: _ -> Option<_>) xs =
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

    let rec private traverseAsyncOptionM' (state: Async<Option<_>>) (f: _ -> Async<Option<_>>) xs =
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
                | Some _ -> return! traverseAsyncOptionM' (Async.singleton o) f xs
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
    /// Applies the monadic function <paramref name="id"/> to each element in the input list <paramref name="xs"/>,
    /// and returns the result as an option. If any element in the list is None, the entire result will be None.
    /// </summary>
    /// <param name="xs">The input list.</param>
    /// <returns>An option containing the result of applying <paramref name="id"/> to each element in <paramref name="xs"/>.</returns>
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
