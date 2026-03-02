module ArrayTraversalBenchmarks

open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling

// ─── Old O(n²) implementations from Array.fs (pre-optimization) ─────────────
// These use Array.head / Array.skip 1 (creates new n-1 array per step)
// and Array.append [|y|] ys (creates new array per step) → O(n²) total
module private OldArray =

    let rec traverseResultM' (state: Result<'ok array, 'error>) (f: 'a -> Result<'ok, 'error>) xs =
        match xs with
        | [||] -> state |> Result.map Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr
            let res =
                result {
                    let! y = f x
                    let! ys = state
                    return Array.append [| y |] ys
                }
            match res with
            | Ok _ -> traverseResultM' res f xs
            | Error _ -> res

    let traverseResultM f xs = traverseResultM' (Ok [||]) f xs
    let sequenceResultM xs = traverseResultM id xs

    let rec traverseResultA' state f xs =
        match xs with
        | [||] -> state |> Result.eitherMap Array.rev Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr
            match state, f x with
            | Ok ys, Ok y -> traverseResultA' (Ok(Array.append [| y |] ys)) f xs
            | Error errs, Error e -> traverseResultA' (Error(Array.append [| e |] errs)) f xs
            | Ok _, Error e -> traverseResultA' (Error [| e |]) f xs
            | Error e, Ok _ -> traverseResultA' (Error e) f xs

    let traverseResultA f xs = traverseResultA' (Ok [||]) f xs
    let sequenceResultA xs = traverseResultA id xs

    let rec traverseValidationA' state f xs =
        match xs with
        | [||] -> state |> Result.eitherMap Array.rev Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr
            let fR = f x
            match state, fR with
            | Ok ys, Ok y -> traverseValidationA' (Ok(Array.append [| y |] ys)) f xs
            | Error errs1, Error errs2 ->
                let errs = Array.append errs2 errs1
                traverseValidationA' (Error errs) f xs
            | Ok _, Error errs
            | Error errs, Ok _ -> traverseValidationA' (Error errs) f xs

    let traverseValidationA f xs = traverseValidationA' (Ok [||]) f xs
    let sequenceValidationA xs = traverseValidationA id xs

    let rec traverseOptionM' (state: 'ok array option) (f: 'a -> 'ok option) xs =
        match xs with
        | [||] -> state |> Option.map Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr
            let r =
                option {
                    let! y = f x
                    let! ys = state
                    return Array.append [| y |] ys
                }
            match r with
            | Some _ -> traverseOptionM' r f xs
            | None -> r

    let traverseOptionM f xs = traverseOptionM' (Some [||]) f xs
    let sequenceOptionM xs = traverseOptionM id xs

    let rec traverseAsyncResultM'
        (state: Async<Result<'ok array, 'error>>)
        (f: 'a -> Async<Result<'ok, 'error>>)
        xs
        =
        match xs with
        | [||] -> state |> AsyncResult.map Array.rev
        | arr ->
            let x = Array.head arr
            let xs = Array.skip 1 arr
            async {
                let! r =
                    asyncResult {
                        let! ys = state
                        let! y = f x
                        return Array.append [| y |] ys
                    }
                match r with
                | Ok _ -> return! traverseAsyncResultM' (Async.singleton r) f xs
                | Error _ -> return r
            }

    let traverseAsyncResultM f xs = traverseAsyncResultM' (AsyncResult.ok [||]) f xs
    let sequenceAsyncResultM xs = traverseAsyncResultM id xs

// ─── Benchmark classes ────────────────────────────────────────────────────────
// Each class benchmarks one function at all sizes (N = 1, 10, 100, 1000, 10000)
// Baseline = true on Old impl; New calls current library function

[<MemoryDiagnoser>]
type ArrayResultM_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseResultM() =
        OldArray.traverseResultM (fun x -> Ok(x * 2)) (Array.init this.N id)

    [<Benchmark>]
    member this.New_TraverseResultM() =
        Array.traverseResultM (fun x -> Ok(x * 2)) (Array.init this.N id)

[<MemoryDiagnoser>]
type ArrayResultA_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseResultA() =
        OldArray.traverseResultA (fun x -> Ok(x * 2)) (Array.init this.N id)

    [<Benchmark>]
    member this.New_TraverseResultA() =
        Array.traverseResultA (fun x -> Ok(x * 2)) (Array.init this.N id)

[<MemoryDiagnoser>]
type ArrayValidationA_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseValidationA() =
        OldArray.traverseValidationA (fun x -> Ok(x * 2)) (Array.init this.N id)

    [<Benchmark>]
    member this.New_TraverseValidationA() =
        Array.traverseValidationA (fun x -> Ok(x * 2)) (Array.init this.N id)

[<MemoryDiagnoser>]
type ArrayOptionM_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseOptionM() =
        OldArray.traverseOptionM (fun x -> Some(x * 2)) (Array.init this.N id)

    [<Benchmark>]
    member this.New_TraverseOptionM() =
        Array.traverseOptionM (fun x -> Some(x * 2)) (Array.init this.N id)

[<MemoryDiagnoser>]
type ArrayAsyncResultM_Benchmarks() =
    [<Params(1, 10, 100, 1000, 10000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseAsyncResultM() =
        OldArray.traverseAsyncResultM (fun x -> AsyncResult.ok (x * 2)) (Array.init this.N id)
        |> Async.StartImmediateAsTask

    [<Benchmark>]
    member this.New_TraverseAsyncResultM() =
        Array.traverseAsyncResultM (fun x -> AsyncResult.ok (x * 2)) (Array.init this.N id)
        |> Async.StartImmediateAsTask
