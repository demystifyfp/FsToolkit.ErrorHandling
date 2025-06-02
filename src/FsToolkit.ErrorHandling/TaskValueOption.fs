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
                | ValueNone -> task { return ValueNone }

            return! t
        }

    let inline valueSome x = task { return ValueSome x }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> valueSome (f' x')) x) f

    let inline zip x1 x2 =
        Task.zip x1 x2
        |> Task.map (fun (r1, r2) -> ValueOption.zip r1 r2)


    /// <summary>
    /// Returns result of running <paramref name="onValueSome"/> if it is <c>ValueSome</c>, otherwise returns result of running <paramref name="onValueNone"/>
    /// </summary>
    /// <param name="onValueSome">The function to run if <paramref name="input"/> is <c>ValueSome</c></param>
    /// <param name="onValueNone">The function to run if <paramref name="input"/> is <c>ValueNone</c></param>
    /// <param name="input">The input voption.</param>
    /// <returns>
    /// The result of running <paramref name="onValueSome"/> if the input is <c>ValueSome</c>, else returns result of running <paramref name="onValueNone"/>.
    /// </returns>
    let inline either
        ([<InlineIfLambda>] onValueSome: 'input -> Task<'output>)
        ([<InlineIfLambda>] onValueNone: unit -> Task<'output>)
        (input: Task<'input voption>)
        : Task<'output> =
        input
        |> Task.bind (
            function
            | ValueSome v -> onValueSome v
            | ValueNone -> onValueNone ()
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
