namespace FsToolkit.ErrorHandling

open System.Threading.Tasks


[<RequireQualifiedAccess>]
module TaskOption =

    /// <summary>Lifts an item to a Task.</summary>
    /// <param name="x">The item to be the result of the Task.</param>
    /// <returns>A Task with the item as the result.</returns>
    let inline some x = Task.singleton (Some x)
    /// <summary>A Task that yields None.</summary>
    /// <returns>A Task with None as the result.</returns>
    let inline none<'a> : Task<'a option> = Task.singleton None

    let inline map ([<InlineIfLambda>] f) ar = Task.map (Option.map f) ar

    let inline bind ([<InlineIfLambda>] f) (ar: Task<_>) =
        task {
            let! opt = ar

            let t =
                match opt with
                | Some x -> f x
                | None -> none

            return! t
        }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> some (f' x')) x) f

    let inline zip left right =
        Task.zip left right
        |> Task.map (fun (r1, r2) -> Option.zip r1 r2)


    /// <summary>Applies <paramref name="onSome"/> to the input if it is <c>Some</c>, otherwise returns result of running <paramref name="onNone"/>.</summary>
    /// <param name="onSome">The function to apply if <paramref name="input"/> is <c>Some</c>.</param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>None</c>.</param>
    /// <param name="input">The input <c>Task&lt;'input option&gt;</c>.</param>
    /// <returns>The result of applying <paramref name="onSome"/> if the input is <c>Some</c>, else returns result of running <paramref name="onNone"/>.</returns>
    let inline eitherBind
        ([<InlineIfLambda>] onSome: 'input -> 'output)
        ([<InlineIfLambda>] onNone: unit -> 'output)
        (input: Task<'input option>)
        : Task<'output> =
        Task.map (Option.eitherBind onSome onNone) input

    [<System.Obsolete "Please use 1. `Task.bind (Option.eitherBind onSome onNone)` or 2. TaskOption.eitherBind (if possible; different signature) instead">]
    let inline either
        ([<InlineIfLambda>] onSome: 'input -> Task<'output>)
        ([<InlineIfLambda>] onNone: unit -> Task<'output>)
        (input: Task<'input option>)
        : Task<'output> =
        Task.bind (Option.eitherBind onSome onNone) input

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="value">The specified default value.</param>
    /// <param name="taskOption">The input option.</param>
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
    /// <param name="taskOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the result of evaluating <paramref name="defThunk"/>.
    /// </returns>
    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (taskOption: Task<'value option>)
        : Task<'value> =
        taskOption
        |> Task.map (Option.defaultWith defThunk)
