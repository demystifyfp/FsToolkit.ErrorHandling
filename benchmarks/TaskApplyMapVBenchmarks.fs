namespace Benchmarks

open System.Threading.Tasks
open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling

/// Old Task.apply implementation: bind (fun f' -> bind (fun x' -> singleton (f' x')) x) f
/// This creates multiple closure allocations from nested lambdas capturing f, x, and the intermediate f'.
module OldTaskApply =
    let singleton (value: 'T) : Task<'T> = Task.FromResult value

    let bind (f: 'a -> Task<'b>) (x: Task<'a>) =
        task {
            let! x = x
            return! f x
        }

    let apply f x =
        bind (fun f' -> bind (fun x' -> singleton (f' x')) x) f

/// Old Task.mapV implementation: x |> bindV (f >> singleton)
/// This creates a function composition allocation (f >> Task.FromResult)
/// and goes through Bind + ReturnFrom instead of BindReturn.
module OldTaskMapV =
    let singleton (value: 'T) : Task<'T> = Task.FromResult value

    let bindV (f: 'a -> Task<'b>) (x: ValueTask<'a>) =
        task {
            let! x = x
            return! f x
        }

    let mapV (f: 'a -> 'b) (x: ValueTask<'a>) =
        x
        |> bindV (
            f
            >> singleton
        )


[<MemoryDiagnoser>]
type TaskApply_Benchmarks() =
    let completedFuncTask: Task<int -> int> = Task.FromResult(fun x -> x + 1)
    let completedValueTask: Task<int> = Task.FromResult 42

    [<Benchmark(Baseline = true)>]
    member _.Old_TaskApply() =
        (OldTaskApply.apply completedFuncTask completedValueTask).Result

    [<Benchmark>]
    member _.New_TaskApply() =
        (Task.apply completedFuncTask completedValueTask).Result


[<MemoryDiagnoser>]
type TaskMapV_Benchmarks() =
    let completedValueTask = ValueTask<int>(42)

    [<Benchmark(Baseline = true)>]
    member _.Old_TaskMapV() =
        (OldTaskMapV.mapV (fun x -> x + 1) completedValueTask).Result

    [<Benchmark>]
    member _.New_TaskMapV() =
        (Task.mapV (fun x -> x + 1) completedValueTask).Result
