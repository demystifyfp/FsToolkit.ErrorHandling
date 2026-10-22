namespace FsToolkit.ErrorHandling

open System.Threading.Tasks


[<RequireQualifiedAccess>]
module TaskValueOption =

    /// <summary>Lifts an item to a Task.</summary>
    /// <param name="x">The item to be the result of the Task.</param>
    /// <returns>A Task with the item as the result.</returns>
    let inline some x = Task.result (ValueSome x)
    /// <summary>A Task that yields ValueNone.</summary>
    /// <returns>A Task with ValueNone as the result.</returns>
    let inline none<'a> : Task<'a voption> = Task.result ValueNone

    [<System.Obsolete "Please use some instead of valueSome">]
    let inline valueSome x = some x

    let inline map ([<InlineIfLambda>] f) ar = Task.map (ValueOption.map f) ar

    let inline bind ([<InlineIfLambda>] f) (ar: Task<_>) =
        task {
            match! ar with
            | ValueSome x -> return! f x
            | ValueNone -> return ValueNone
        }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> some (f' x')) x) f

    let inline zip left right =
        Task.zip left right
        |> Task.map (fun (r1, r2) -> ValueOption.zip r1 r2)

    /// <summary>Applies <paramref name="onSome"/> to the input if it is <c>ValueSome</c>, otherwise returns result of running <paramref name="onNone"/>.</summary>
    /// <param name="onSome">The function to apply if <paramref name="input"/> is <c>ValueSome</c>.</param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>ValueNone</c>.</param>
    /// <param name="input">The input <c>Task&lt;'input voption&gt;</c>.</param>
    /// <returns>The result of applying <paramref name="onSome"/> if the input is <c>ValueSome</c>, else returns result of running <paramref name="onNone"/>.</returns>
    let inline eitherBind
        ([<InlineIfLambda>] onSome: 'input -> 'output)
        ([<InlineIfLambda>] onNone: unit -> 'output)
        (input: Task<'input voption>)
        : Task<'output> =
        Task.map (ValueOption.eitherBind onSome onNone) input

    [<System.Obsolete "Please use 1. `Task.bind (ValueOption.eitherBind onSome onNone)` or 2. TaskValueOption.eitherBind (if possible; different signature) instead">]
    let inline either
        ([<InlineIfLambda>] onSome: 'input -> Task<'output>)
        ([<InlineIfLambda>] onNone: unit -> Task<'output>)
        (input: Task<'input voption>)
        : Task<'output> =
        Task.bind (ValueOption.eitherBind onSome onNone) input

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="value">The specified default value.</param>
    /// <param name="taskValueOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the default value.
    /// </returns>
    let inline defaultValue (value: 'value) (taskValueOption: Task<'value voption>) =
        taskValueOption
        |> Task.map (ValueOption.defaultValue value)

    /// <summary>
    ///  Gets the value of the voption if the voption is <c>ValueSome</c>, otherwise evaluates <paramref name="defThunk"/> and returns the result.
    /// </summary>
    /// <param name="defThunk">A thunk that provides a default value when evaluated.</param>
    /// <param name="taskValueOption">The input voption.</param>
    /// <returns>
    /// The voption if the option is <c>ValueSome</c>, else the result of evaluating <paramref name="defThunk"/>.
    /// </returns>
    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (taskValueOption: Task<'value voption>)
        : Task<'value> =
        taskValueOption
        |> Task.map (ValueOption.defaultWith defThunk)
