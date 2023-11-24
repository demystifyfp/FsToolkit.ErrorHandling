namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
#if NETSTANDARD2_0
open FSharp.Control.Tasks.Affine
#endif

[<RequireQualifiedAccess>]
module TaskOption =

    let inline map ([<InlineIfLambda>] f) ar = Task.map (Option.map f) ar

    let inline bind ([<InlineIfLambda>] f) (ar: Task<_>) =
        task {
            let! opt = ar

            let t =
                match opt with
                | Some x -> f x
                | None -> task { return None }

            return! t
        }

    let inline retn x = task { return Some x }
    let inline some x = task { return Some x }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> retn (f' x')) x) f

    let inline zip x1 x2 =
        Task.zip x1 x2
        |> Task.map (fun (r1, r2) -> Option.zip r1 r2)


    /// <summary>
    /// Returns result of running <paramref name="onSome"/> if it is <c>Some</c>, otherwise returns result of running <paramref name="onNone"/>
    /// </summary>
    /// <param name="onSome">The function to run if <paramref name="input"/> is <c>Some</c></param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>None</c></param>
    /// <param name="input">The input option.</param>
    /// <returns>
    /// The result of running <paramref name="onSome"/> if the input is <c>Some</c>, else returns result of running <paramref name="onNone"/>.
    /// </returns>
    let inline either
        ([<InlineIfLambda>] onSome: 'input -> Task<'output>)
        ([<InlineIfLambda>] onNone: unit -> Task<'output>)
        (input: Task<'input option>)
        : Task<'output> =
        input
        |> Task.bind (
            function
            | Some v -> onSome v
            | None -> onNone ()
        )

    let inline defaultValue (value: 'value) (taskOption: Task<'value option>) =
        taskOption
        |> Task.map (Option.defaultValue value)

    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (taskOption: Task<'value option>)
        : Task<'value> =
        taskOption
        |> Task.map (Option.defaultWith defThunk)
