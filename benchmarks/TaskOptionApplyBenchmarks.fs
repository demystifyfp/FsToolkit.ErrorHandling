namespace Benchmarks

open System.Threading.Tasks
open BenchmarkDotNet.Attributes
open FsToolkit.ErrorHandling

/// Old TaskOption.apply: nested bind closures creating 3 task state machines
module OldTaskOption =
    let bind (f: 'a -> Task<'b option>) (ar: Task<'a option>) =
        task {
            let! opt = ar

            let t =
                match opt with
                | Some x -> f x
                | None -> task { return None }

            return! t
        }

    let some x = task { return Some x }

    let apply f x =
        bind (fun f' -> bind (fun x' -> some (f' x')) x) f


/// Old TaskResult.bindRequireSome: bind (f >> Task.singleton) pattern
module OldTaskResult =
    let singleton (value: 'T) : Task<'T> = Task.FromResult value

    let bind (f: 'a -> Task<Result<'b, 'c>>) (tr: Task<Result<'a, 'c>>) =
        task {
            let! result = tr

            match result with
            | Ok x -> return! f x
            | Error e -> return Error e
        }

    let bindRequireSome error x =
        x
        |> bind (
            Result.requireSome error
            >> singleton
        )


[<MemoryDiagnoser>]
type TaskOptionApply_Benchmarks() =
    let completedFuncTask: Task<(int -> int) option> =
        Task.FromResult(Some(fun x -> x + 1))

    let completedValueTask: Task<int option> = Task.FromResult(Some 42)
    let completedNoneFunc: Task<(int -> int) option> = Task.FromResult None

    [<Benchmark(Baseline = true)>]
    member _.Old_TaskOptionApply() =
        (OldTaskOption.apply completedFuncTask completedValueTask).Result

    [<Benchmark>]
    member _.New_TaskOptionApply() =
        (TaskOption.apply completedFuncTask completedValueTask).Result

    [<Benchmark>]
    member _.Old_TaskOptionApply_None() =
        (OldTaskOption.apply completedNoneFunc completedValueTask).Result

    [<Benchmark>]
    member _.New_TaskOptionApply_None() =
        (TaskOption.apply completedNoneFunc completedValueTask).Result


[<MemoryDiagnoser>]
type TaskResultBindRequire_Benchmarks() =
    let completedOkSome: Task<Result<int option, string>> = Task.FromResult(Ok(Some 42))
    let completedOkNone: Task<Result<int option, string>> = Task.FromResult(Ok None)
    let completedError: Task<Result<int option, string>> = Task.FromResult(Error "err")

    [<Benchmark(Baseline = true)>]
    member _.Old_BindRequireSome_OkSome() =
        (OldTaskResult.bindRequireSome "missing" completedOkSome).Result

    [<Benchmark>]
    member _.New_BindRequireSome_OkSome() =
        (TaskResult.bindRequireSome "missing" completedOkSome).Result

    [<Benchmark>]
    member _.Old_BindRequireSome_Error() =
        (OldTaskResult.bindRequireSome "missing" completedError).Result

    [<Benchmark>]
    member _.New_BindRequireSome_Error() =
        (TaskResult.bindRequireSome "missing" completedError).Result
