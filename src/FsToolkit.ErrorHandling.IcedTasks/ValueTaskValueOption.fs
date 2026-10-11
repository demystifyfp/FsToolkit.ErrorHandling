namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open IcedTasks

[<RequireQualifiedAccess>]
module ValueTask =

    /// Bind the ValueTask with a synchronous Result-returning function.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: ValueTask<'input>)
        : ValueTask<Result<'output, 'error>> =
        ValueTask.map binder input

    let req reqF errOrF value = bindResult (reqF errOrF) value
    let reqFilter predicate errOrF value = req (Req.filter predicate) errOrF value

[<RequireQualifiedAccess>]
module ValueTaskValueOption =

    let inline map ([<InlineIfLambda>] f) (ar: ValueTask<_ voption>) =
        valueTask {
            let! opt = ar
            return ValueOption.map f opt
        }

    let inline bind ([<InlineIfLambda>] f) (ar: ValueTask<_ voption>) =
        valueTask {
            match! ar with
            | ValueSome x -> return! f x
            | ValueNone -> return ValueNone
        }

    /// <summary>Lifts an item to a ValueTask.</summary>
    /// <param name="x">The item to be the result of the ValueTask.</param>
    /// <returns>A ValueTask with the item as the result.</returns>
    let inline some x = ValueTask.singleton (ValueSome x)
    /// <summary>A ValueTask that yields ValueNone.</summary>
    /// <returns>A ValueTask with ValueNone as the result.</returns>
    let inline none<'a> : ValueTask<'a voption> = ValueTask.singleton ValueNone

    [<System.Obsolete "Please use some instead of singleton">]
    let inline valueSome x = some x

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> some (f' x')) x) f

    let inline zip (left: ValueTask<'a voption>) (right: ValueTask<'b voption>) =
        valueTask {
            let! r1 = left
            let! r2 = right
            return ValueOption.zip r1 r2
        }

    /// <summary>Applies <paramref name="onSome"/> to the input if it is <c>ValueSome</c>, otherwise returns the result of running <paramref name="onNone"/>.</summary>
    /// <param name="onSome">The function to apply if <paramref name="input"/> is <c>ValueSome</c>.</param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>ValueNone</c>.</param>
    /// <param name="input">The input <c>ValueTask&lt;'input voption&gt;</c>.</param>
    /// <returns>The result of applying <paramref name="onSome"/> if the input is <c>ValueSome</c>, else returns the result of running <paramref name="onNone"/>.</returns>
    let inline eitherBind
        ([<InlineIfLambda>] onSome: 'input -> 'output)
        ([<InlineIfLambda>] onNone: unit -> 'output)
        (input: ValueTask<'input voption>)
        : ValueTask<'output> =
        valueTask {
            match! input with
            | ValueSome v -> return onSome v
            | ValueNone -> return onNone ()
        }

    [<System.Obsolete "Please use ValueTaskValueOption.eitherBind or ValueTask.bind instead">]
    let inline either
        ([<InlineIfLambda>] onValueSome: 'input -> ValueTask<'output>)
        ([<InlineIfLambda>] onValueNone: unit -> ValueTask<'output>)
        (input: ValueTask<'input voption>)
        : ValueTask<'output> =
        valueTask {
            match! input with
            | ValueSome v -> return! onValueSome v
            | ValueNone -> return! onValueNone ()
        }

    /// <summary>
    ///  Gets the value of the voption if the voption is <c>ValueSome</c>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="value">The specified default value.</param>
    /// <param name="valueTaskValueOption">The input voption.</param>
    /// <returns>
    /// The voption if the voption is <c>ValueSome</c>, else the default value.
    /// </returns>
    let inline defaultValue (value: 'value) (valueTaskValueOption: ValueTask<'value voption>) =
        valueTask {
            let! opt = valueTaskValueOption
            return ValueOption.defaultValue value opt
        }

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
        valueTask {
            let! opt = valueTaskValueOption
            return ValueOption.defaultWith defThunk opt
        }

    /// Bind the ValueTask&lt;'input voption&gt; with a synchronous Result-returning function.
    /// A ValueNone input will be mapped to Ok ValueNone.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: ValueTask<'input voption>)
        : ValueTask<Result<'output voption, 'error>> =
        ValueTask.bindResult
            (function
            | ValueSome x ->
                binder x
                |> Result.map ValueSome
            | ValueNone -> Ok ValueNone)
            input

    let req reqF errOrF value = bindResult (reqF errOrF) value
    let reqFilter predicate errOrF value = req (Req.filter predicate) errOrF value
