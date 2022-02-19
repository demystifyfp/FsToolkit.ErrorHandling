namespace FsToolkit.ErrorHandling.Benchmarks 

open System
open BenchmarkDotNet
open BenchmarkDotNet.Attributes
open System.Threading.Tasks
open FsToolkit.ErrorHandling
module TaskResultCE =


    type TaskResultCEInlinedLambdaBuilder() =

        member inline __.Return(value: 'T) : TaskCode<Result<'T, 'TError>,_> = task.Return (Ok value)

        member inline __.ReturnFrom([<InlineIfLambda>] asyncResult: TaskCode<Result<'T, 'TError>,_>) : TaskCode<Result<'T, 'TError>, _> = asyncResult

        member __.Zero() : TaskCode<Result<unit, 'TError>,_> = task.Return <| Ok ()

        member inline __.Bind
            (
                asyncResult: Task<Result<'T, 'TError>>,
                [<InlineIfLambda>] binder: 'T -> TaskCode<Result<'U, 'TError>,_>
            ) : TaskCode<Result<'U, 'TError>,_> =
            let inline binder' r =
                match r with
                | Ok x -> binder x
                | Error x -> task.Return <| Error x

            task.Bind(asyncResult, binder')

        member inline _.Delay([<InlineIfLambda>] generator: unit -> TaskCode<Result<'T, 'TError>,_>) : TaskCode<Result<'T, 'TError>,_> =
            task.Delay generator
        member inline _.Run ([<InlineIfLambda>] foo) = task.Run foo
        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(result: Task<Result<_, _>>) : Task<Result<_, _>> = result
        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source([<InlineIfLambda>] result: TaskCode<Result<_, _>,_>) : TaskCode<Result<_, _>,_> = result


[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module TaskResultCEExtensions =
    open TaskResultCE
    type TaskResultCEInlinedLambdaBuilder with
        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline __.Source(s: #seq<_>) = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>R
        member inline _.Source(result: Result<_, _>) : Task<Result<_, _>> = Task.FromResult result
    let rec fib n = 
        if n < 2L then n
        else fib (n - 1L) + fib (n - 2L)

    let rec afib (n, level) = task {
        if n < 0L then return Error "No"
        elif n < 2L then
            return Ok n
        elif n < level then 
            return Ok (fib  n)
        else
            let n2a = afib (n-2L, level) 
            let! n1 = afib (n-1L, level)
            let! n2 = n2a
            match n1, n2 with
            | Ok n1, Ok n2 -> 
                return Ok (n2 + n1)
            | Error e, _
            | _, Error e -> return Error e
        }
    let taskResultInlinedIfLambda = TaskResultCEInlinedLambdaBuilder()

    [<MemoryDiagnoser>]
    type TaskResult_BindCEBenchmarks () =

        [<Benchmark(Baseline = true)>]
        member this.afib()  =
            afib(10,5) 

        [<Benchmark>]
        member this.Result_Normal_Bind_CE()  = 
            let action () = taskResult {
                let! a = Ok 10
                let! b = Ok 5
                let! c = afib(a, b)
                return c
            }
            action ()
            

        [<Benchmark>]
        member this.Result_Alt_Inlined_Bind_CE ()  = 
            let action () = taskResultInlinedIfLambda {
                let! a = Ok 10
                let! b = Ok 5
                let! c = afib(a, b)
                return c
            }
            action ()