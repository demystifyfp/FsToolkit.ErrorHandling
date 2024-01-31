namespace FsToolkit.ErrorHandling

open System.Threading.Tasks


[<RequireQualifiedAccess>]
module Task =
    let inline singleton value =
        value
        |> Task.FromResult

    let inline bind ([<InlineIfLambda>] f: 'a -> Task<'b>) (x: Task<'a>) =
        task {
            let! x = x
            return! f x
        }


    let inline bindV ([<InlineIfLambda>] f: 'a -> Task<'b>) (x: ValueTask<'a>) =
        task {
            let! x = x
            return! f x
        }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> singleton (f' x')) x) f

    let inline map ([<InlineIfLambda>] f) x =
        x
        |> bind (
            f
            >> singleton
        )

    let inline mapV ([<InlineIfLambda>] f) x =
        x
        |> bindV (
            f
            >> singleton
        )

    let inline map2 ([<InlineIfLambda>] f) x y = (apply (apply (singleton f) x) y)

    let inline map3 ([<InlineIfLambda>] f) x y z = apply (map2 f x y) z

    /// Allows us to call `do!` syntax inside a computation expression
    let inline ignore (x: Task<'a>) =
        x
        |> map ignore

    /// Takes two tasks and returns a tuple of the pair
    let zip (a1: Task<_>) (a2: Task<_>) =
        task {
            let! r1 = a1
            let! r2 = a2
            return r1, r2
        }

    let ofUnit (t: Task) = task { return! t }

    /// Creates a `Task` that attempts to execute the provided task,
    /// returning `Choice1Of2` with the result if the task completes
    /// without exceptions, or `Choice2Of2` with the exception if an
    /// exception is thrown.
    let catch (x: Task<_>) =
        task {
            try
                let! r = x
                return Choice1Of2 r
            with e ->
                return Choice2Of2 e
        }
