namespace FsToolkit.ErrorHandling

open System.Threading.Tasks


[<RequireQualifiedAccess>]
module ValueTaskValueOption =

    let inline map ([<InlineIfLambda>] f) (ar: ValueTask<_ voption>) =
        if ar.IsCompletedSuccessfully then
            ValueTask<_ voption>(ValueOption.map f ar.Result)
        else
            ValueTask<_ voption>(
                task {
                    let! opt = ar
                    return ValueOption.map f opt
                }
            )

    let inline bind ([<InlineIfLambda>] f) (ar: ValueTask<_ voption>) =
        if ar.IsCompletedSuccessfully then
            match ar.Result with
            | ValueSome x -> f x
            | ValueNone -> ValueTask<_ voption>(ValueNone)
        else
            ValueTask<_ voption>(
                task {
                    let! opt = ar

                    let t =
                        match opt with
                        | ValueSome x -> f x
                        | ValueNone -> ValueTask<_ voption>(ValueNone)

                    return! t
                }
            )

    let inline valueSome x = ValueTask<_ voption>(ValueSome x)

    let inline apply (f: ValueTask<('a -> 'b) voption>) (x: ValueTask<'a voption>) =
        if f.IsCompletedSuccessfully then
            match f.Result with
            | ValueSome f' ->
                if x.IsCompletedSuccessfully then
                    ValueTask<_ voption>(ValueOption.map f' x.Result)
                else
                    ValueTask<_ voption>(
                        task {
                            let! xOpt = x
                            return ValueOption.map f' xOpt
                        }
                    )
            | ValueNone -> ValueTask<_ voption>(ValueNone)
        else
            ValueTask<_ voption>(
                task {
                    let! fOpt = f

                    match fOpt with
                    | ValueSome f' ->
                        let! xOpt = x
                        return ValueOption.map f' xOpt
                    | ValueNone -> return ValueNone
                }
            )

    let inline zip (x1: ValueTask<'a voption>) (x2: ValueTask<'b voption>) =
        if
            x1.IsCompletedSuccessfully
            && x2.IsCompletedSuccessfully
        then
            ValueTask<('a * 'b) voption>(ValueOption.zip x1.Result x2.Result)
        else
            ValueTask<('a * 'b) voption>(
                task {
                    let! r1 = x1
                    let! r2 = x2
                    return ValueOption.zip r1 r2
                }
            )

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
        ([<InlineIfLambda>] onValueSome: 'input -> ValueTask<'output>)
        ([<InlineIfLambda>] onValueNone: unit -> ValueTask<'output>)
        (input: ValueTask<'input voption>)
        : ValueTask<'output> =
        if input.IsCompletedSuccessfully then
            match input.Result with
            | ValueSome v -> onValueSome v
            | ValueNone -> onValueNone ()
        else
            ValueTask<'output>(
                task {
                    let! opt = input

                    match opt with
                    | ValueSome v -> return! onValueSome v
                    | ValueNone -> return! onValueNone ()
                }
            )

    /// <summary>
    ///  Gets the value of the voption if the voption is <c>ValueSome</c>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="value">The specified default value.</param>
    /// <param name="valueTaskValueOption">The input voption.</param>
    /// <returns>
    /// The voption if the voption is <c>ValueSome</c>, else the default value.
    /// </returns>
    let inline defaultValue (value: 'value) (valueTaskValueOption: ValueTask<'value voption>) =
        if valueTaskValueOption.IsCompletedSuccessfully then
            ValueTask<'value>(ValueOption.defaultValue value valueTaskValueOption.Result)
        else
            ValueTask<'value>(
                task {
                    let! opt = valueTaskValueOption
                    return ValueOption.defaultValue value opt
                }
            )

    /// <summary>
    ///  Gets the value of the voption if the voption is <c>ValueSome</c>, otherwise evaluates <paramref name="defThunk"/> and returns the result.
    /// </summary>
    /// <param name="defThunk">A thunk that provides a default value when evaluated.</param>
    /// <param name="valueTaskValueOption">The input voption.</param>
    /// <returns>
    /// The voption if the voption is <c>ValueSome</c>, else the result of evaluating <paramref name="defThunk"/>.
    /// </returns>
    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (valueTaskValueOption: ValueTask<'value voption>)
        : ValueTask<'value> =
        if valueTaskValueOption.IsCompletedSuccessfully then
            ValueTask<'value>(ValueOption.defaultWith defThunk valueTaskValueOption.Result)
        else
            ValueTask<'value>(
                task {
                    let! opt = valueTaskValueOption
                    return ValueOption.defaultWith defThunk opt
                }
            )
