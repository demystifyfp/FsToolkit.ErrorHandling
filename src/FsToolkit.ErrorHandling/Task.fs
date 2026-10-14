namespace FsToolkit.ErrorHandling

open System.Threading.Tasks

[<RequireQualifiedAccess>]
module Task =

#if !NET10_0_OR_GREATER
    /// <summary>Creates a task that returns the given value.</summary>
    [<System.Obsolete("Use the built in FSharp.Core Task.result instead")>]
    let inline singleton value =
        value
        |> Task.FromResult
#endif

    let inline bindV ([<InlineIfLambda>] f: 'a -> Task<'b>) (x: ValueTask<'a>) =
        task {
            let! x = x
            return! f x
        }

    let inline apply f x =
        task {
            let! f' = f
            let! x' = x
            return f' x'
        }

    let inline mapV ([<InlineIfLambda>] f) x =
        x
        |> bindV (
            f
            >> Task.result
        )

    let inline map2 ([<InlineIfLambda>] f) x y =
        task {
            let! x' = x
            let! y' = y
            return f x' y'
        }

    let inline map3 ([<InlineIfLambda>] f) x y z =
        task {
            let! x' = x
            let! y' = y
            let! z' = z
            return f x' y' z'
        }

    /// Takes two tasks and returns a tuple of the pair
    let zip (a1: Task<_>) (a2: Task<_>) =
        task {
            let! r1 = a1
            let! r2 = a2
            return r1, r2
        }

    let ofUnit (t: Task) = task { return! t }

    /// Bind the Task with a synchronous Result-returning function.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: Task<'input>)
        : Task<Result<'output, 'error>> =
        Task.map binder input

    let inline req ([<InlineIfLambda>] reqF) errOrF value = bindResult (reqF errOrF) value
    let inline reqFilter predicate error value = req (Req.filter predicate) error value
