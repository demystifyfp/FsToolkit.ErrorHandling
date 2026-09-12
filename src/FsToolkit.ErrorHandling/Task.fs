namespace FsToolkit.ErrorHandling

open System.Threading.Tasks

// FsToolkit v5 had singleton, map, bind, catch and ignore with the following diffs:
// - result replaces singleton (Obsoleted)
// - catch yields Result (was choice; source+binary breaking change)
// - ignore gains [<RequiresExplicitTypeArguments>] (source breaking change)

[<RequireQualifiedAccess>]
module Task =

#if !NET_10_0_OR_GREATER
    /// <summary>Creates a task that returns the given value.</summary>
    [<System.Obsolete("Use the built in FSharp.Core Task.result instead")>]
    let inline singleton value =
        value
        |> Task.FromResult
#endif

    // NOTE FSharp.Core Task.bind is available in versions >= 11 intrinsically (via auto-opened Microsoft.FSharp.Control.Task module).
    // Alternately, to avail of the shimmed version in this library, `open FsToolkit.ErrorHandling`

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

    // NOTE FSharp.Core Task.map is available in versions >= 11 intrinsically (via auto-opened Microsoft.FSharp.Control.Task module).
    // Alternately, to avail of the shimmed version in this library, `open FsToolkit.ErrorHandling`

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

    // NOTE FSharp.Core Task.ignore is available in versions >= 11 intrinsically (via auto-opened Microsoft.FSharp.Control.Task module).
    // Alternately, to avail of the shimmed version in this library, `open FsToolkit.ErrorHandling`
    // NOTE Breaking change vs V5: [<RequiresExplicitTypeArguments>] has been added, so source changes may be required.

    /// Takes two tasks and returns a tuple of the pair
    let zip (a1: Task<_>) (a2: Task<_>) =
        task {
            let! r1 = a1
            let! r2 = a2
            return r1, r2
        }

    let ofUnit (t: Task) = task { return! t }

    // NOTE FSharp.Core Task.catch is available in versions >= 11 intrinsically (via auto-opened Microsoft.FSharp.Control.Task module).
    // Alternately, to avail of the shimmed version in this library, `open FsToolkit.ErrorHandling`
    // NOTE Breaking change vs V5: the signature has changed to Task<Result<'T, exn>> (was Task<Choice<'T, exn>>)
