namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module List =
    open Microsoft.FSharp.Core.CompilerServices
    let rec private traverseResultM' (state: Result<_, _>) (f: _ -> Result<_, _>) xs =
        match xs with
        | [] -> state |> Result.map List.rev
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

    let rec private traverseAsyncResultM' (state: Async<Result<_, _>>) (f: _ -> Async<Result<_, _>>) xs =
        match xs with
        | [] -> state |> AsyncResult.map List.rev
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

    let traverseResultM f xs = 
        // #if NETSTANDARD2_1
        // let mutable oks = ListCollector()
        // let mutable hasError = false
        // let mutable error = Unchecked.defaultof<_>
        // use i = (xs : seq<_>).GetEnumerator()
        // while not hasError && i.MoveNext() do
        //     match f i.Current with
        //     | Ok o -> oks.Add o
        //     | Error e ->
        //         hasError <- true
        //         error <- Error e
        // if hasError then error
        // else
        //     Ok (oks.Close())
        // #else
        traverseResultM' (Ok []) f xs
        // #endif

    let sequenceResultM xs = traverseResultM id xs

    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (AsyncResult.retn []) f xs

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs


    let rec private traverseResultA' state f xs =
        match xs with
        | [] -> state |> Result.map List.rev |> Result.mapError List.rev
        | x :: xs ->
            let fR : Result<'a, 'b> = f x 

            match state, fR with
            | Ok ys, Ok y -> traverseResultA' (Ok(y :: ys)) f xs
            | Error errs, Error e -> traverseResultA' (Error(e :: errs)) f xs
            | Ok _, Error e -> traverseResultA' (Error [e]) f xs
            | Error e, Ok _ -> traverseResultA' (Error e) f xs

    let rec private traverseAsyncResultA' state f xs =
        match xs with
        | [] -> state |> AsyncResult.map List.rev
        | x :: xs ->
            async {
                let! s = state
                let! fR = f x |> AsyncResult.mapError List.singleton

                match s, fR with
                | Ok ys, Ok y -> return! traverseAsyncResultA' (AsyncResult.retn (y :: ys)) f xs
                | Error errs, Error e -> return! traverseAsyncResultA' (AsyncResult.returnError (errs @ e)) f xs
                | Ok _, Error e
                | Error e, Ok _ -> return! traverseAsyncResultA' (AsyncResult.returnError e) f xs
            }

    let traverseResultA f xs = 
        // #if NETSTANDARD2_1
        // let mutable oks = ListCollector()
        // let mutable hasErrors = false
        // let mutable errors = ListCollector()
        // for x in xs do
        //     match f x with
        //     | Ok o -> 
        //         oks.Add o
        //     | Error e -> 
        //         hasErrors <- true
        //         errors.Add e
        // if hasErrors then Error (errors.Close())
        // else Ok (oks.Close())
        // #else
        traverseResultA' (Ok []) f xs
        // #endif

    let sequenceResultA xs = traverseResultA id xs

    let rec traverseValidationA' state f xs =
        match xs with
        | [] -> state |> Result.map List.rev
        | x :: xs ->
            let fR = f x

            match state, fR with
            | Ok ys, Ok y -> traverseValidationA' (Ok(y :: ys)) f xs
            | Error errs1, Error errs2 -> traverseValidationA' (Error(errs2 @ errs1)) f xs
            | Ok _, Error errs
            | Error errs, Ok _ -> traverseValidationA' (Error errs) f xs

    let traverseValidationA f xs = traverseValidationA' (Ok []) f xs

    let sequenceValidationA xs = traverseValidationA id xs


    let traverseAsyncResultA f xs =
        traverseAsyncResultA' (AsyncResult.retn []) f xs

    let sequenceAsyncResultA xs = traverseAsyncResultA id xs
