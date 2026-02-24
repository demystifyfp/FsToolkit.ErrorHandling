namespace FsToolkit.ErrorHandling

open System.Threading.Tasks


[<RequireQualifiedAccess>]
module ValueTaskValueOption =

    let inline map ([<InlineIfLambda>] f) (ar: ValueTask<_ voption>) =
        ValueTask<_ voption>(
            task {
                let! opt = ar
                return ValueOption.map f opt
            }
        )

    let inline bind ([<InlineIfLambda>] f) (ar: ValueTask<_ voption>) =
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

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> valueSome (f' x')) x) f

    let inline zip (x1: ValueTask<'a voption>) (x2: ValueTask<'b voption>) =
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
        ValueTask<'value>(
            task {
                let! opt = valueTaskValueOption
                return ValueOption.defaultWith defThunk opt
            }
        )
