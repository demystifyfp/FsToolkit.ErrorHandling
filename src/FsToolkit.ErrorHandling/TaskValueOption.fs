namespace FsToolkit.ErrorHandling

open System.Threading.Tasks


[<RequireQualifiedAccess>]
module TaskValueOption =

    let inline map ([<InlineIfLambda>] f) ar = Task.map (ValueOption.map f) ar

    let inline bind ([<InlineIfLambda>] f) (ar: Task<_>) =
        task {
            let! opt = ar

            let t =
                match opt with
                | ValueSome x -> f x
                | ValueNone -> Task.singleton ValueNone

            return! t
        }

    let inline valueSome x = Task.singleton (ValueSome x)

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> valueSome (f' x')) x) f

    let inline zip left right =
        Task.zip left right
        |> Task.map (fun (r1, r2) -> ValueOption.zip r1 r2)


    /// <summary>Applies <paramref name="onSome"/> to the input if it is <c>ValueSome</c>, otherwise returns result of running <paramref name="onNone"/>.</summary>
    /// <param name="onSome">The function to apply if <paramref name="input"/> is <c>ValueSome</c>.</param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>ValueNone</c>.</param>
    /// <param name="input">The input <c>Task&lt;'input voption&gt;</c>.</param>/
    /// <returns>The result of applying <paramref name="onSome"/> if the input is <c>ValueSome</c>, else returns result of running <paramref name="onNone"/>.</returns>
    let inline either
        ([<InlineIfLambda>] onSome: 'input -> 'output)
        ([<InlineIfLambda>] onNone: unit -> 'output)
        (input: Task<'input voption>)
        : Task<'output> =
        input
        |> Task.map (
            function
            | ValueSome v -> onSome v
            | ValueNone -> onNone ()
        )

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
