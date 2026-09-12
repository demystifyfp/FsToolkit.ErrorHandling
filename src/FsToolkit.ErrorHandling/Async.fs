namespace FsToolkit.ErrorHandling

/// <summary>
/// Helper functions for working with the <c>Async</c> type.
/// </summary>
[<RequireQualifiedAccess>]
module Async =

#if !NET_10_0_OR_GREATER
    /// <summary>
    /// Converts a value to an <c>Async</c> value
    /// </summary>
    /// <param name="value">The value to convert to an <c>Async</c> value.</param>
    /// <returns>The <c>Async</c> value.</returns>
    [<System.Obsolete("Use the built in FSharp.Core Async.result instead")>]
    let inline singleton (value: 'value) : Async<'value> = Async.result value
#endif

    /// <summary>
    /// Takes a transformation function and applies it to the value of an <c>Async</c> value.
    /// </summary>
    /// <param name="binder">The function to bind over the <c>Async</c> value.</param>
    /// <param name="input">The <c>Async</c> value to bind over.</param>
    /// <returns>The result of binding the function over the <c>Async</c> value.</returns>
    let inline bind
        ([<InlineIfLambda>] binder: 'input -> Async<'output>)
        (input: Async<'input>)
        : Async<'output> =
        async.Bind(input, binder)

    /// <summary>
    /// Applies an <c>Async</c> function to an <c>Async</c> value.
    /// </summary>
    /// <param name="applier">The <c>Async</c> function to apply.</param>
    /// <param name="input">The <c>Async</c> value to apply the function to.</param>
    /// <returns>The result of applying the function to the value.</returns>
    let inline apply (applier: Async<'input -> 'output>) (input: Async<'input>) : Async<'output> =
        bind (fun f' -> bind (fun x' -> Async.result (f' x')) input) applier

    /// <summary>
    /// Applies a transformation to the value of an <c>Async</c> value to a new <c>Async</c> value using the provided function.
    /// </summary>
    /// <param name="mapper">The function to apply to the value of the <c>Async</c> value.</param>
    /// <param name="input">The <c>Async</c> value to transform.</param>
    /// <returns>The transformed <c>Async</c> value.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'input -> 'output)
        (input: Async<'input>)
        : Async<'output> =
        bind
            (fun x' ->
                mapper x'
                |> Async.result
            )
            input

    /// <summary>
    /// Applies a transformation to the values of two <c>Async</c> values to a new <c>Async</c> value using the provided function.
    /// </summary>
    /// <param name="mapper">The function to apply to the values of the <c>Async</c> values.</param>
    /// <param name="input1">The first <c>Async</c> value to transform.</param>
    /// <param name="input2">The second <c>Async</c> value to transform.</param>
    /// <returns>The transformed <c>Async</c> value.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'input1 -> 'input2 -> 'output)
        (input1: Async<'input1>)
        (input2: Async<'input2>)
        : Async<'output> =
        bind
            (fun x ->
                bind
                    (fun y ->
                        mapper x y
                        |> Async.result
                    )
                    input2
            )
            input1

    /// <summary>
    /// Applies a transformation to the values of three <c>Async</c> values to a new <c>Async</c> value using the provided function.
    /// </summary>
    /// <param name="mapper">The function to apply to the values of the <c>Async</c> values.</param>
    /// <param name="input1">The first <c>Async</c> value to transform.</param>
    /// <param name="input2">The second <c>Async</c> value to transform.</param>
    /// <param name="input3">The third <c>Async</c> value to transform.</param>
    /// <returns>The transformed <c>Async</c> value.</returns>
    let inline map3
        ([<InlineIfLambda>] mapper: 'input1 -> 'input2 -> 'input3 -> 'output)
        (input1: Async<'input1>)
        (input2: Async<'input2>)
        (input3: Async<'input3>)
        : Async<'output> =
        bind
            (fun x ->
                bind
                    (fun y ->
                        bind
                            (fun z ->
                                mapper x y z
                                |> Async.result
                            )
                            input3
                    )
                    input2
            )
            input1

    /// <summary>
    /// Takes two asyncs and returns a tuple of the pair
    /// </summary>
    /// <param name="left">The first async value.</param>
    /// <param name="right">The second async value.</param>
    /// <returns>The tuple of the pair.</returns>
    let inline zip (left: Async<'left>) (right: Async<'right>) : Async<'left * 'right> =
        bind (fun l -> bind (fun r -> Async.result (l, r)) right) left

    /// <summary>
    /// Executes two asyncs concurrently <see cref='M:Microsoft.FSharp.Control.FSharpAsync.Parallel``1'/> and returns a mapping of the values
    /// </summary>
    /// <param name="mapper">The function to apply to the values of the <c>Async</c> values.</param>
    /// <param name="input1">The first <c>Async</c> to execute</param>
    /// <param name="input2">The second <c>Async</c> to execute</param>
    /// <returns>The transformed <c>Async</c> value.</returns>
    let inline parallelMap2
        ([<InlineIfLambda>] mapper: 'input1 -> 'input2 -> 'output)
        (input1: Async<'input1>)
        (input2: Async<'input2>)
        : Async<'output> =

        Async.Parallel(
            [|
                map box input1
                map box input2
            |]
        )
        |> map (fun results ->
            let a = unbox<'input1> results[0]
            let b = unbox<'input2> results[1]
            mapper a b
        )

    /// <summary>
    /// Executes three asyncs concurrently <see cref='M:Microsoft.FSharp.Control.FSharpAsync.Parallel``1'/> and returns a mapping of the values
    /// </summary>
    /// <param name="mapper">The function to apply to the values of the <c>Async</c> values.</param>
    /// <param name="input1">The first <c>Async</c> to execute</param>
    /// <param name="input2">The second <c>Async</c> to execute</param>
    /// <param name="input3">The third <c>Async</c> value to transform.</param>
    /// <returns>The transformed <c>Async</c> value.</returns>
    let inline parallelMap3
        ([<InlineIfLambda>] mapper: 'input1 -> 'input2 -> 'input3 -> 'output)
        (input1: Async<'input1>)
        (input2: Async<'input2>)
        (input3: Async<'input3>)
        : Async<'output> =
        Async.Parallel(
            [|
                map box input1
                map box input2
                map box input3
            |]
        )
        |> map (fun results ->
            let a = unbox<'input1> results[0]
            let b = unbox<'input2> results[1]
            let c = unbox<'input3> results[2]
            mapper a b c
        )

/// <summary>
/// Operators for working with the <c>Async</c> type.
/// </summary>
module AsyncOperators =

    /// <summary>
    /// Shorthand for <c>Async.map</c>
    /// </summary>
    /// <param name="mapper">The function to map over the <c>Async</c> value.</param>
    /// <param name="input">The <c>Async</c> value to map over.</param>
    /// <returns>The result of mapping the function over the <c>Async</c> value.</returns>
    let inline (<!>)
        ([<InlineIfLambda>] mapper: 'input -> 'output)
        (input: Async<'input>)
        : Async<'output> =
        Async.map mapper input

    /// <summary>
    /// Shorthand for <c>Async.apply</c>
    /// </summary>
    /// <param name="applier">The <c>Async</c> function to apply.</param>
    /// <param name="input">The <c>Async</c> value to apply the function to.</param>
    /// <returns>The result of applying the function to the value.</returns>
    let inline (<*>) (applier: Async<'input -> 'output>) (input: Async<'input>) : Async<'output> =
        Async.apply applier input

    /// <summary>
    /// Shorthand for <c>Async.bind</c>
    /// </summary>
    /// <param name="input">The <c>Async</c> value to bind over.</param>
    /// <param name="binder">The function to bind over the <c>Async</c> value.</param>
    /// <returns>The result of binding the function over the <c>Async</c> value.</returns>
    let inline (>>=)
        (input: Async<'input>)
        ([<InlineIfLambda>] binder: 'input -> Async<'output>)
        : Async<'output> =
        Async.bind binder input


[<AutoOpen>]
module AsyncExt =
    open System

    type Microsoft.FSharp.Control.Async with

        static member TryFinallyAsync(comp: Async<'T>, deferred) : Async<'T> =

            let finish (compResult, deferredResult) (cont, (econt: exn -> unit), ccont) =
                match (compResult, deferredResult) with
                | (Choice1Of3 x, Choice1Of3()) -> cont x
                | (Choice2Of3 compExn, Choice1Of3()) -> econt compExn
                | (Choice3Of3 compExn, Choice1Of3()) -> ccont compExn
                | (Choice1Of3 _, Choice2Of3 deferredExn) -> econt deferredExn
                | (Choice2Of3 compExn, Choice2Of3 deferredExn) ->
                    econt
                    <| new AggregateException(compExn, deferredExn)
                | (Choice3Of3 compExn, Choice2Of3 deferredExn) -> econt deferredExn
                | (_, Choice3Of3 deferredExn) ->
                    econt
                    <| new Exception("Unexpected cancellation.", deferredExn)

            let startDeferred compResult (cont, econt, ccont) =
                Async.StartWithContinuations(
                    deferred,
                    (fun () -> finish (compResult, Choice1Of3()) (cont, econt, ccont)),
                    (fun exn -> finish (compResult, Choice2Of3 exn) (cont, econt, ccont)),
                    (fun exn -> finish (compResult, Choice3Of3 exn) (cont, econt, ccont))
                )

            let startComp ct (cont, econt, ccont) =
                Async.StartWithContinuations(
                    comp,
                    (fun x -> startDeferred (Choice1Of3(x)) (cont, econt, ccont)),
                    (fun exn -> startDeferred (Choice2Of3 exn) (cont, econt, ccont)),
                    (fun exn -> startDeferred (Choice3Of3 exn) (cont, econt, ccont)),
                    ct
                )

            async {
                let! ct = Async.CancellationToken
                return! Async.FromContinuations(startComp ct)
            }
