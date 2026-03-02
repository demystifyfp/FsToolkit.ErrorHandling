module CEWhileBenchmarks

open System.Threading.Tasks
open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling

// ─── Old recursive While: current AsyncOptionCE implementation ─────────────────
// Creates a new Bind (async wrapper) on EVERY loop iteration → allocation per step
module private OldWhile =

    type AsyncOptionBuilderOld() =
        member _.Return(x: 'T) : Async<'T option> = async.Return(Some x)
        member _.ReturnFrom(x: Async<'T option>) : Async<'T option> = x
        member _.Zero() : Async<unit option> = async.Return(Some())

        member _.Bind(x: Async<'T option>, f: 'T -> Async<'U option>) : Async<'U option> =
            async {
                let! r = x

                match r with
                | Some v -> return! f v
                | None -> return None
            }

        member _.Delay(f: unit -> Async<'T option>) : Async<'T option> = async.Delay(f)

        // OLD: Creates a new Bind wrapper per iteration — allocates per loop step
        member this.While
            (guard: unit -> bool, computation: Async<unit option>)
            : Async<unit option> =
            if not (guard ()) then
                this.Zero()
            else
                this.Bind(computation, (fun () -> this.While(guard, computation)))

    let asyncOptionOld = AsyncOptionBuilderOld()

// ─── New iterative While: proposed fix ────────────────────────────────────────
// Uses explicit async { while ... } loop — single Async allocation regardless of n
module private NewWhile =

    type AsyncOptionBuilderNew() =
        member _.Return(x: 'T) : Async<'T option> = async.Return(Some x)
        member _.ReturnFrom(x: Async<'T option>) : Async<'T option> = x
        member _.Zero() : Async<unit option> = async.Return(Some())

        member _.Bind(x: Async<'T option>, f: 'T -> Async<'U option>) : Async<'U option> =
            async {
                let! r = x

                match r with
                | Some v -> return! f v
                | None -> return None
            }

        member _.Delay(f: unit -> Async<'T option>) : Async<'T option> = async.Delay(f)

        // NEW: Iterative loop — single async workflow, no per-iteration allocation
        member _.While(guard: unit -> bool, computation: Async<unit option>) : Async<unit option> =
            async {
                let mutable keepGoing = guard ()
                let mutable result: unit option = Some()

                while keepGoing
                      && result.IsSome do
                    let! r = computation
                    result <- r

                    if r.IsSome then
                        keepGoing <- guard ()

                return result
            }

    let asyncOptionNew = AsyncOptionBuilderNew()

// ─── Benchmark: While loop with N iterations ─────────────────────────────────
// Compares old recursive While vs new iterative While
// Higher N = more pronounced allocation difference

[<MemoryDiagnoser>]
type AsyncOptionWhile_Benchmarks() =
    [<Params(1, 10, 100, 1000)>]
    member val N = 0 with get, set

    [<Benchmark(Baseline = true)>]
    member this.Old_RecursiveWhile() =
        let mutable i = 0

        OldWhile.asyncOptionOld {
            while i < this.N do
                i <- i + 1
        }
        |> Async.StartImmediateAsTask

    [<Benchmark>]
    member this.New_IterativeWhile() =
        let mutable i = 0

        NewWhile.asyncOptionNew {
            while i < this.N do
                i <- i + 1
        }
        |> Async.StartImmediateAsTask

// ─── Benchmark: While loop with library asyncOption ───────────────────────────
// Benchmarks actual asyncOption CE (current recursive implementation)
// vs asyncResult CE (already uses mutable-ref approach)
// to show the gap that the fix closes

[<MemoryDiagnoser>]
type AsyncCE_While_Comparison_Benchmarks() =
    [<Params(1, 10, 100, 1000)>]
    member val N = 0 with get, set

    // Current asyncOption.While = recursive (allocates per iteration)
    [<Benchmark(Baseline = true)>]
    member this.AsyncOption_CurrentImpl() =
        let mutable i = 0

        asyncOption {
            while i < this.N do
                i <- i + 1

            return i
        }
        |> Async.StartImmediateAsTask

    // asyncResult.While already uses mutable-ref approach (better)
    [<Benchmark>]
    member this.AsyncResult_MutableRefImpl() =
        let mutable i = 0

        asyncResult {
            while i < this.N do
                i <- i + 1

            return i
        }
        |> Async.StartImmediateAsTask
