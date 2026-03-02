module ListTraversalBenchmarks

open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling

// ─── Old cons+rev implementations from List.fs (pre-optimization) ────────────
// These build the result list in reverse via (::) cons, then call List.rev
// to restore order. List.rev allocates n additional cons cells = extra O(n) copy.
module private OldList =

    let rec traverseResultM' (state: Result<'ok list, 'error>) (f: 'a -> Result<'ok, 'error>) xs =
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

    let traverseResultM f xs = traverseResultM' (Ok []) f xs
    let sequenceResultM xs = traverseResultM id xs

    let rec traverseResultA' state f xs =
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

    let traverseResultA f xs = traverseResultA' (Ok []) f xs
    let sequenceResultA xs = traverseResultA id xs

    let rec traverseValidationA' state f xs =
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

    let rec traverseOptionM' (state: 'ok list option) (f: 'a -> 'ok option) xs =
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

    let traverseOptionM f xs = traverseOptionM' (Some []) f xs
    let sequenceOptionM xs = traverseOptionM id xs

    let rec traverseAsyncResultM'
        (state: Async<Result<'ok list, 'error>>)
        (f: 'a -> Async<Result<'ok, 'error>>)
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

    let traverseAsyncResultM f xs =
        traverseAsyncResultM' (AsyncResult.ok []) f xs

    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

    let rec traverseAsyncResultA' state f xs =
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

    let traverseAsyncResultA f xs =
        traverseAsyncResultA' (AsyncResult.ok []) f xs

    let sequenceAsyncResultA xs = traverseAsyncResultA id xs

// ─── Benchmark classes ────────────────────────────────────────────────────────

[<MemoryDiagnoser>]
type ListResultM_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseResultM() =
        OldList.traverseResultM (fun x -> (Ok(x * 2): Result<int, string>)) (List.init this.N id)

    [<Benchmark>]
    member this.New_TraverseResultM() =
        List.traverseResultM (fun x -> (Ok(x * 2): Result<int, string>)) (List.init this.N id)

[<MemoryDiagnoser>]
type ListResultA_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseResultA() =
        OldList.traverseResultA (fun x -> (Ok(x * 2): Result<int, string>)) (List.init this.N id)

    [<Benchmark>]
    member this.New_TraverseResultA() =
        List.traverseResultA (fun x -> (Ok(x * 2): Result<int, string>)) (List.init this.N id)

[<MemoryDiagnoser>]
type ListValidationA_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseValidationA() =
        OldList.traverseValidationA
            (fun x -> (Ok(x * 2): Result<int, string list>))
            (List.init this.N id)

    [<Benchmark>]
    member this.New_TraverseValidationA() =
        List.traverseValidationA
            (fun x -> (Ok(x * 2): Result<int, string list>))
            (List.init this.N id)

[<MemoryDiagnoser>]
type ListOptionM_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseOptionM() =
        OldList.traverseOptionM (fun x -> Some(x * 2)) (List.init this.N id)

    [<Benchmark>]
    member this.New_TraverseOptionM() =
        List.traverseOptionM (fun x -> Some(x * 2)) (List.init this.N id)

[<MemoryDiagnoser>]
type ListAsyncResultM_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseAsyncResultM() =
        OldList.traverseAsyncResultM
            (fun x -> (AsyncResult.ok (x * 2): Async<Result<int, string>>))
            (List.init this.N id)
        |> Async.StartImmediateAsTask

    [<Benchmark>]
    member this.New_TraverseAsyncResultM() =
        List.traverseAsyncResultM
            (fun x -> (AsyncResult.ok (x * 2): Async<Result<int, string>>))
            (List.init this.N id)
        |> Async.StartImmediateAsTask

[<MemoryDiagnoser>]
type ListAsyncResultA_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseAsyncResultA() =
        OldList.traverseAsyncResultA
            (fun x -> (AsyncResult.ok (x * 2): Async<Result<int, string>>))
            (List.init this.N id)
        |> Async.StartImmediateAsTask

    [<Benchmark>]
    member this.New_TraverseAsyncResultA() =
        List.traverseAsyncResultA
            (fun x -> (AsyncResult.ok (x * 2): Async<Result<int, string>>))
            (List.init this.N id)
        |> Async.StartImmediateAsTask
