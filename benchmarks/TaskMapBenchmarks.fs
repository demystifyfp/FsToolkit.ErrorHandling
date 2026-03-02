namespace Benchmarks

open System.Threading.Tasks
open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling

/// Old Task.map implementation: bind (f >> singleton)
/// This creates a function composition allocation (f >> Task.FromResult)
/// and goes through Bind + ReturnFrom instead of BindReturn.
module OldTask =
    let singleton (value: 'T) : Task<'T> = Task.FromResult value

    let bind (f: 'a -> Task<'b>) (x: Task<'a>) =
        task {
            let! x = x
            return! f x
        }

    let map (f: 'a -> 'b) (x: Task<'a>) : Task<'b> =
        x
        |> bind (
            f
            >> singleton
        )


/// Old List.traverseTaskResultM' implementation with List.toArray conversion
module OldListTask =
    let traverseTaskResultM' (f: 'c -> Task<Result<'a, 'b>>) (xs: 'c list) =
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


/// New List.traverseTaskResultM' without List.toArray — direct list traversal
module NewListTask =
    let traverseTaskResultM' (f: 'c -> Task<Result<'a, 'b>>) (xs: 'c list) =
        let mutable state = Ok []

        task {
            let mutable remaining = xs

            while state
                  |> Result.isOk
                  && not remaining.IsEmpty do
                let! r = f remaining.Head
                remaining <- remaining.Tail

                match (r, state) with
                | Ok y, Ok ys -> state <- Ok(y :: ys)
                | Error e, _ -> state <- Error e
                | _, _ -> ()

            return
                state
                |> Result.map List.rev
        }


[<MemoryDiagnoser>]
type TaskMap_Benchmarks() =
    let completedTask = Task.FromResult 42

    [<Benchmark(Baseline = true)>]
    member _.Old_TaskMap() =
        (OldTask.map (fun x -> x + 1) completedTask).Result

    [<Benchmark>]
    member _.New_TaskMap() =
        (Task.map (fun x -> x + 1) completedTask).Result


[<MemoryDiagnoser>]
type ListTraverseTaskResultM_Benchmarks() =
    [<Params(1, 10, 100, 1000)>]
    member val N = 0 with get, set

    member this.InputList = List.init this.N id

    [<Benchmark(Baseline = true)>]
    member this.Old_TraverseTaskResultM() =
        (OldListTask.traverseTaskResultM'
            (fun x -> Task.FromResult(Ok(x * 2): Result<int, string>))
            this.InputList)
            .Result

    [<Benchmark>]
    member this.New_TraverseTaskResultM() =
        (NewListTask.traverseTaskResultM'
            (fun x -> Task.FromResult(Ok(x * 2): Result<int, string>))
            this.InputList)
            .Result
