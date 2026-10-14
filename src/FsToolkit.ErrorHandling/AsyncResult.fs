namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
#if !FABLE_COMPILER
open System.Runtime.ExceptionServices
#endif

[<RequireQualifiedAccess>]
module AsyncResult =

    let inline ok (value: 'ok) : Async<Result<'ok, 'error>> = Async.result (Ok value)

    let inline error (error: 'error) : Async<Result<'ok, 'error>> = Async.result (Error error)

    let inline map
        ([<InlineIfLambda>] mapper: 'input -> 'output)
        (input: Async<Result<'input, 'error>>)
        : Async<Result<'output, 'error>> =
        Async.map (Result.map mapper) input

    let inline mapError
        ([<InlineIfLambda>] mapper: 'inputError -> 'outputError)
        (input: Async<Result<'ok, 'inputError>>)
        : Async<Result<'ok, 'outputError>> =
        Async.map (Result.mapError mapper) input

    let inline bind
        ([<InlineIfLambda>] binder: 'input -> Async<Result<'output, 'error>>)
        (input: Async<Result<'input, 'error>>)
        : Async<Result<'output, 'error>> =
        Async.bind (Result.eitherBind binder error) input

    let inline eitherBind
        ([<InlineIfLambda>] onOk: 'input -> 'output)
        ([<InlineIfLambda>] onError: 'inputError -> 'output)
        (input: Async<Result<'input, 'inputError>>)
        : Async<'output> =
        Async.map (Result.eitherBind onOk onError) input

    [<System.Obsolete "Please use eitherBind instead of foldResult (renamed to align with Result naming)">]
    let inline foldResult
        ([<InlineIfLambda>] onOk: 'input -> 'output)
        ([<InlineIfLambda>] onError: 'inputError -> 'output)
        (input: Async<Result<'input, 'inputError>>)
        : Async<'output> =
        eitherBind onOk onError input

    /// <summary>
    /// Maps the values of an <c>AsyncResult</c>  to a new <c>AsyncResult</c>  using the provided functions.
    ///
    /// Documentation is found here: <href>https://demystifyfp.gitbook.io/fstoolkit-errorhandling/fstoolkit.errorhandling/asyncResult/eitherMap</href>
    /// </summary>
    /// <param name="onOk">The function to apply to the 'ok' value of the input <c>AsyncResult</c>.</param>
    /// <param name="onError">The function to apply to the 'error' value of the input <c>AsyncResult</c>.</param>
    /// <param name="input">The input <c>AsyncResult</c> to map.</param>
    /// <returns>A new <c>AsyncResult</c> with the mapped values.</returns>
    let inline eitherMap
        ([<InlineIfLambda>] onOk: 'okInput -> 'okOutput)
        ([<InlineIfLambda>] onError: 'errorInput -> 'errorOutput)
        (input: Async<Result<'okInput, 'errorInput>>)
        : Async<Result<'okOutput, 'errorOutput>> =
        Async.map (Result.eitherMap onOk onError) input

    [<System.Obsolete "Please use Async.map Ok instead of AsyncResult.ofAsync (Consider Async.catch/catchWith if you want to trap exceptions as Error)">]
    let inline ofAsync (async: Async<'ok>) : Async<Result<'ok, 'error>> = Async.map Ok async

    [<System.Obsolete "Please use Async.result instead of AsyncResult.ofResult">]
    let inline ofResult (x: Result<'ok, 'error>) : Async<Result<'ok, 'error>> = Async.result x
#if !FABLE_COMPILER
    [<System.Obsolete "Please use Async.Await >> Async.catch instead of AsyncResult.ofTask">]
    let inline ofTask (task: Task<'input>) : Async<Result<'input, exn>> =
        Async.Await task
        |> Async.catch

    [<System.Obsolete "Please use Async.Await >> Async.catch instead of AsyncResult.ofTaskAction">]
    let inline ofTaskAction (task: Task) : Async<Result<unit, exn>> =
        Async.Await task
        |> Async.catch
#endif

    let inline map2
        ([<InlineIfLambda>] mapper: 'input1 -> 'input2 -> 'output)
        (input1: Async<Result<'input1, 'error>>)
        (input2: Async<Result<'input2, 'error>>)
        : Async<Result<'output, 'error>> =
        Async.map2 (Result.map2 mapper) input1 input2

    let inline map3
        ([<InlineIfLambda>] mapper: 'input1 -> 'input2 -> 'input3 -> 'output)
        (input1: Async<Result<'input1, 'error>>)
        (input2: Async<Result<'input2, 'error>>)
        (input3: Async<Result<'input3, 'error>>)
        : Async<Result<'output, 'error>> =
        Async.map3 (Result.map3 mapper) input1 input2 input3

    let inline apply
        (applier: Async<Result<'input -> 'output, 'error>>)
        (input: Async<Result<'input, 'error>>)
        : Async<Result<'output, 'error>> =
        map2 (fun f x -> f x) applier input


    /// <summary>
    /// Returns <paramref name="input"/> if it is <c>Ok</c>, otherwise returns <paramref name="ifError"/>
    /// </summary>
    /// <param name="ifError">The value to use if <paramref name="input"/> is <c>Error</c></param>
    /// <param name="input">The input result.</param>
    /// <remarks>
    /// </remarks>
    /// <example>
    /// <code>
    ///     AsyncResult.error "First" |> AsyncResult.orElse (AsyncResult.error "Second") // evaluates to Error ("Second")
    ///     AsyncResult.error "First" |> AsyncResult.orElse (AsyncResult.ok "Second") // evaluates to Ok ("Second")
    ///     AsyncResult.ok "First" |> AsyncResult.orElse (AsyncResult.error "Second") // evaluates to Ok ("First")
    ///     AsyncResult.ok "First" |> AsyncResult.orElse (AsyncResult.ok "Second") // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is Ok, else returns <paramref name="ifError"/>.
    /// </returns>
    let inline orElse
        (ifError: Async<Result<'ok, 'errorOutput>>)
        (input: Async<Result<'ok, 'errorInput>>)
        : Async<Result<'ok, 'errorOutput>> =
        Async.bind (Result.eitherBind ok (fun _ -> ifError)) input

    /// <summary>
    /// Returns <paramref name="input"/> if it is <c>Ok</c>, otherwise executes <paramref name="ifErrorFunc"/> and returns the result.
    /// </summary>
    /// <param name="ifErrorFunc">A function that provides an alternate result when evaluated.</param>
    /// <param name="input">The input result.</param>
    /// <remarks>
    /// <paramref name="ifErrorFunc"/>  is not executed unless <paramref name="input"/> is an <c>Error</c>.
    /// </remarks>
    /// <example>
    /// <code>
    ///     AsyncResult.error "First" |> AsyncResult.orElseWith (fun _ -> AsyncResult.error "Second") // evaluates to Error ("Second")
    ///     AsyncResult.error "First" |> AsyncResult.orElseWith (fun _ -> AsyncResult.ok "Second") // evaluates to Ok ("Second")
    ///     AsyncResult.ok "First" |> AsyncResult.orElseWith (fun _ -> AsyncResult.error "Second") // evaluates to Ok ("First")
    ///     AsyncResult.ok "First" |> AsyncResult.orElseWith (fun _ -> AsyncResult.ok "Second") // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is Ok, else the result of executing <paramref name="ifErrorFunc"/>.
    /// </returns>
    let inline orElseWith
        ([<InlineIfLambda>] ifErrorFunc: 'errorInput -> Async<Result<'ok, 'errorOutput>>)
        (input: Async<Result<'ok, 'errorInput>>)
        : Async<Result<'ok, 'errorOutput>> =
        Async.bind (Result.eitherBind ok ifErrorFunc) input

    /// Replaces the wrapped value with unit
    let inline ignore<'ok, 'error>
        (value: Async<Result<'ok, 'error>>)
        : Async<Result<unit, 'error>> =
        value
        |> map ignore<'ok>

    /// Replaces an error value of an async-wrapped result with a custom error
    /// value.
    let inline setError
        (error: 'errorOutput)
        (asyncResult: Async<Result<'ok, 'errorInput>>)
        : Async<Result<'ok, 'errorOutput>> =
        asyncResult
        |> Async.map (Result.setError error)

    /// Replaces a unit error value of an async-wrapped result with a custom
    /// error value. Safer than setError since you're not losing any information.
    let inline withError
        (error: 'errorOutput)
        (asyncResult: Async<Result<'ok, unit>>)
        : Async<Result<'ok, 'errorOutput>> =
        asyncResult
        |> Async.map (Result.withError error)

    /// Extracts the contained value of an async-wrapped result if Ok, otherwise
    /// uses ifError.
    let inline defaultValue (ifError: 'ok) (asyncResult: Async<Result<'ok, 'error>>) : Async<'ok> =
        asyncResult
        |> Async.map (Result.defaultValue ifError)

    /// Extracts the contained value of an async-wrapped result if Error, otherwise
    /// uses ifOk.
    let inline defaultError
        (ifOk: 'error)
        (asyncResult: Async<Result<'ok, 'error>>)
        : Async<'error> =
        asyncResult
        |> Async.map (Result.defaultError ifOk)

    /// Extracts the contained value of an async-wrapped result if Ok, otherwise
    /// evaluates ifErrorThunk and uses the result.
    let inline defaultWith
        ([<InlineIfLambda>] ifErrorThunk: 'error -> 'ok)
        (asyncResult: Async<Result<'ok, 'error>>)
        : Async<'ok> =
        asyncResult
        |> Async.map (Result.defaultWith ifErrorThunk)

    /// Same as defaultValue for a result where the Ok value is unit. The name
    /// describes better what is actually happening in this case.
    let inline ignoreError<'error> (result: Async<Result<unit, 'error>>) : Async<unit> =
        defaultValue () result

    /// If the async-wrapped result is Ok, executes the function on the Ok value.
    /// Passes through the input value.
    let inline tee
        ([<InlineIfLambda>] inspector: 'ok -> unit)
        (asyncResult: Async<Result<'ok, 'error>>)
        : Async<Result<'ok, 'error>> =
        asyncResult
        |> Async.map (Result.tee inspector)

    /// If the async-wrapped result is Ok and the predicate returns true, executes
    /// the function on the Ok value. Passes through the input value.
    let inline teeIf
        ([<InlineIfLambda>] predicate: 'ok -> bool)
        ([<InlineIfLambda>] inspector: 'ok -> unit)
        (asyncResult: Async<Result<'ok, 'error>>)
        : Async<Result<'ok, 'error>> =
        asyncResult
        |> Async.map (Result.teeIf predicate inspector)

    /// If the async-wrapped result is Error, executes the function on the Error
    /// value. Passes through the input value.
    let inline teeError
        ([<InlineIfLambda>] teeFunction: 'error -> unit)
        (asyncResult: Async<Result<'ok, 'error>>)
        : Async<Result<'ok, 'error>> =
        asyncResult
        |> Async.map (Result.teeError teeFunction)

    /// If the async-wrapped result is Error and the predicate returns true,
    /// executes the function on the Error value. Passes through the input value.
    let inline teeErrorIf
        ([<InlineIfLambda>] predicate: 'error -> bool)
        ([<InlineIfLambda>] teeFunction: 'error -> unit)
        (asyncResult: Async<Result<'ok, 'error>>)
        : Async<Result<'ok, 'error>> =
        asyncResult
        |> Async.map (Result.teeErrorIf predicate teeFunction)


    /// Takes two results and returns a tuple of the pair
    let inline zip
        (left: Async<Result<'leftOk, 'error>>)
        (right: Async<Result<'rightOk, 'error>>)
        : Async<Result<'leftOk * 'rightOk, 'error>> =
        Async.zip left right
        |> Async.map (fun (r1, r2) -> Result.zip r1 r2)

    /// Takes two results and returns a tuple of the error pair
    let inline zipError
        (left: Async<Result<'ok, 'leftError>>)
        (right: Async<Result<'ok, 'rightError>>)
        : Async<Result<'ok, 'leftError * 'rightError>> =
        Async.zip left right
        |> Async.map (fun (r1, r2) -> Result.zipError r1 r2)

    /// Catches exceptions and maps them to the Error case using the provided function.
    let inline catchWith
        ([<InlineIfLambda>] exnMapper: exn -> 'error)
        (input: Async<Result<'ok, 'error>>)
        : Async<Result<'ok, 'error>> =
        input
        |> Async.catchWith (fun exn -> Error(exnMapper exn))

    [<System.Obsolete "Please use AsyncResult.catchWith instead of AsyncResult.catch">]
    let inline catch
        ([<InlineIfLambda>] exnMapper: exn -> 'error)
        (input: Async<Result<'ok, 'error>>)
        : Async<Result<'ok, 'error>> =
        catchWith exnMapper input

    /// Gets the value in the Ok case or re-raises the exception in the Error case
    let inline getOrReraise (input: Async<Result<'ok, exn>>) : Async<'ok> =
        async {
            match! input with
            | Ok a -> return a
            | Error exn ->
#if FABLE_COMPILER
                return raise exn
#else
                ExceptionDispatchInfo.Capture(exn).Throw()
                return Unchecked.defaultof<_>
#endif
        }

    /// Returns the async-wrapped result if it is Ok and the checker returns a async-wrapped Ok result or if the async-wrapped result is Error.
    /// If the checker returns a async-wrapped Error result, returns the async-wrapped Error result.
    let inline check
        ([<InlineIfLambda>] checker: 'ok -> Async<Result<unit, 'error>>)
        (x: Async<Result<'ok, 'error>>)
        : Async<Result<'ok, 'error>> =
        x
        |> bind (fun x ->
            checker x
            |> map (fun () -> x)
        )

    /// Bind the AsyncResult with a synchronous Result-returning function
    let inline bindResult
        ([<InlineIfLambda>] reqF: 'input -> Result<'output, 'error>)
        (input: Async<Result<'input, 'error>>)
        : Async<Result<'output, 'error>> =
        Async.bindResult (Result.bind reqF) input

    let inline req ([<InlineIfLambda>] reqF) errOrF value = bindResult (reqF errOrF) value
    let inline reqFilter predicate error value = req (Req.filter predicate) error value

    [<System.Obsolete "Please use AsyncResult.reqFilter predicate error instead of require predicate error">]
    let inline require predicate error result = reqFilter predicate error result

    [<System.Obsolete "Please use AsyncResult.req (Req.equalTo other) error value instead of requireEqual other value error">]
    let inline requireEqual other value error = req (Req.equalTo other) error value

    [<System.Obsolete "Please use Async.req Req.isTrue error instead of requireTrue error">]
    let inline requireTrue error value = Async.req Req.isTrue error value

    [<System.Obsolete "Please use Async.req Req.isTrueWith errorF instead of requireTrueWith errorF">]
    let inline requireTrueWith errorF value = Async.req Req.isTrueWith errorF value

    [<System.Obsolete "Please use Async.req Req.isFalse error instead of requireFalse error">]
    let inline requireFalse error value = Async.req Req.isFalse error value

    [<System.Obsolete "Please use Async.req Req.isFalseWith errorF instead of requireFalseWith errorF">]
    let inline requireFalseWith errorF value = Async.req Req.isFalseWith errorF value

    [<System.Obsolete "Please use Async.req Req.some error instead of requireSome error">]
    let inline requireSome error value = Async.req Req.some error value

    [<System.Obsolete "Please use Async.req Req.someWith errorF instead of requireSomeWith errorF">]
    let inline requireSomeWith errorF value = Async.req Req.someWith errorF value

    [<System.Obsolete "Please use Async.req Req.none error instead of requireNone error">]
    let inline requireNone error value = Async.req Req.none error value

    [<System.Obsolete "Please use Async.req Req.noneWith errorF instead of requireNoneWith errorF">]
    let inline requireNoneWith errorF value = Async.req Req.noneWith errorF value

    [<System.Obsolete "Please use Async.req Req.valueSome error instead of requireValueSome error">]
    let inline requireValueSome error value = Async.req Req.valueSome error value

    [<System.Obsolete "Please use Async.req Req.valueSomeWith errorF instead of requireValueSomeWith errorF">]
    let inline requireValueSomeWith errorF value =
        Async.req Req.valueSomeWith errorF value

    [<System.Obsolete "Please use Async.req Req.valueNone error instead of requireValueNone error">]
    let inline requireValueNone error value = Async.req Req.valueNone error value

    [<System.Obsolete "Please use Async.req Req.valueNoneWith errorF instead of requireValueNoneWith errorF">]
    let inline requireValueNoneWith errorF value =
        Async.req Req.valueNoneWith errorF value

    [<System.Obsolete "Please use Async.req (Req.equalTo other) error instead of requireEqualTo other error">]
    let inline requireEqualTo other error value =
        Async.req (Req.equalTo other) error value

    [<System.Obsolete "Please use Async.req Req.empty error instead of requireEmpty error">]
    let inline requireEmpty error value = Async.req Req.empty error value

    [<System.Obsolete "Please use Async.req Req.notEmpty error instead of requireNotEmpty error">]
    let inline requireNotEmpty error value = Async.req Req.notEmpty error value

    [<System.Obsolete "Please use Async.req Req.head error instead of requireHead error">]
    let inline requireHead error value = Async.req Req.head error value

    [<System.Obsolete "Please use AsyncResult.req Req.some error instead of bindRequireSome error">]
    let inline bindRequireSome error x = req Req.some error x

    [<System.Obsolete "Please use AsyncResult.req Req.none error instead of bindRequireNone error">]
    let inline bindRequireNone error x = req Req.none error x

    [<System.Obsolete "Please use AsyncResult.req Req.someWith errorF instead of bindRequireSomeWith errorF">]
    let inline bindRequireSomeWith errorF x = req Req.someWith errorF x

    [<System.Obsolete "Please use AsyncResult.req Req.noneWith errorF instead of bindRequireNoneWith errorF">]
    let inline bindRequireNoneWith errorF x = req Req.noneWith errorF x

    [<System.Obsolete "Please use AsyncResult.req Req.valueSome error instead of bindRequireValueSome error">]
    let inline bindRequireValueSome error x = req Req.valueSome error x

    [<System.Obsolete "Please use AsyncResult.req Req.valueNone error instead of bindRequireValueNone error">]
    let inline bindRequireValueNone error x = req Req.valueNone error x

    [<System.Obsolete "Please use AsyncResult.req Req.valueSomeWith errorF instead of bindRequireValueSomeWith errorF">]
    let inline bindRequireValueSomeWith errorF x = req Req.valueSomeWith errorF x

    [<System.Obsolete "Please use AsyncResult.req Req.valueNoneWith errorF instead of bindRequireValueNoneWith errorF">]
    let inline bindRequireValueNoneWith errorF x = req Req.valueNoneWith errorF x

    [<System.Obsolete "Please use AsyncResult.req Req.isTrue error instead of bindRequireTrue error">]
    let inline bindRequireTrue error x = req Req.isTrue error x

    [<System.Obsolete "Please use AsyncResult.req Req.isTrueWith errorF instead of bindRequireTrueWith errorF">]
    let inline bindRequireTrueWith errorF x = req Req.isTrueWith errorF x

    [<System.Obsolete "Please use AsyncResult.req Req.isFalse error instead of bindRequireFalse error">]
    let inline bindRequireFalse error x = req Req.isFalse error x

    [<System.Obsolete "Please use AsyncResult.req Req.isFalseWith errorF instead of bindRequireFalseWith errorF">]
    let inline bindRequireFalseWith errorF x = req Req.isFalseWith errorF x

    [<System.Obsolete "Please use AsyncResult.req Req.notNull error instead of bindRequireNotNull error">]
    let inline bindRequireNotNull error x = req Req.notNull error x

    [<System.Obsolete "Please use AsyncResult.req Req.equalTo other error instead of bindRequireEqual other error">]
    let inline bindRequireEqual y error x = req (Req.equalTo y) error x

    [<System.Obsolete "Please use AsyncResult.req Req.empty error instead of bindRequireEmpty error">]
    let inline bindRequireEmpty error x = req Req.empty error x

    [<System.Obsolete "Please use AsyncResult.req Req.notEmpty error instead of bindRequireNotEmpty error">]
    let inline bindRequireNotEmpty error x = req Req.notEmpty error x

    [<System.Obsolete "Please use AsyncResult.req Req.head error instead of bindRequireHead error">]
    let inline bindRequireHead error x = req Req.head error x
