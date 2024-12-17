namespace FsToolkit.ErrorHandling

open System.Threading.Tasks


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

    let inline singleton x = task { return Some x }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> singleton (f' x')) x) f

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

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="value">The specified default value.</param>
    /// <param name="asyncOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the default value.
    /// </returns>
    let inline defaultValue (value: 'value) (taskOption: Task<'value option>) =
        taskOption
        |> Task.map (Option.defaultValue value)

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise evaluates <paramref name="defThunk"/> and returns the result.
    /// </summary>
    /// <param name="defThunk">A thunk that provides a default value when evaluated.</param>
    /// <param name="asyncOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the result of evaluating <paramref name="defThunk"/>.
    /// </returns>
    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (taskOption: Task<'value option>)
        : Task<'value> =
        taskOption
        |> Task.map (Option.defaultWith defThunk)
