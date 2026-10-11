namespace FsToolkit.ErrorHandling

open Hopac

[<RequireQualifiedAccess>]
module JobResult =

    let inline ok x =
        Ok x
        |> Job.result

    let inline error x =
        Error x
        |> Job.result

    let inline map ([<InlineIfLambda>] f) jr = Job.map (Result.map f) jr

    let inline map2 ([<InlineIfLambda>] f) xJR yJR = Job.map2 (Result.map2 f) xJR yJR

    let inline map3 ([<InlineIfLambda>] f) xJR yJR zJR = Job.map3 (Result.map3 f) xJR yJR zJR

    let inline mapError ([<InlineIfLambda>] f) jr = Job.map (Result.mapError f) jr

    let inline bind
        ([<InlineIfLambda>] f: 'a -> Job<Result<'c, 'b>>)
        (jr: Job<Result<'a, 'b>>)
        : Job<Result<'c, 'b>> =
        Job.bind (Result.eitherBind f error) jr

    let inline eitherBind
        ([<InlineIfLambda>] onOk: 'input -> 'output)
        ([<InlineIfLambda>] onError: 'inputError -> 'output)
        (input: Job<Result<'input, 'inputError>>)
        : Job<'output> =
        Job.map (Result.eitherBind onOk onError) input

    /// <summary>
    /// Maps the values of an <c>JobResult</c>  to a new <c>JobResult</c>  using the provided functions.
    /// </summary>
    /// <param name="onOk">The function to apply to the 'ok' value of the input <c>JobResult</c>.</param>
    /// <param name="onError">The function to apply to the 'error' value of the input <c>JobResult</c>.</param>
    /// <param name="input">The input <c>JobResult</c> to map.</param>
    /// <returns>A new <c>JobResult</c> with the mapped values.</returns>
    let inline eitherMap
        ([<InlineIfLambda>] onOk: 'okInput -> 'okOutput)
        ([<InlineIfLambda>] onError: 'errorInput -> 'errorOutput)
        (input: Job<Result<'okInput, 'errorInput>>)
        : Job<Result<'okOutput, 'errorOutput>> =
        Job.map (Result.eitherMap onOk onError) input

    let inline ofAsync aAsync =
        aAsync
        |> Job.fromAsync
        |> Job.catch
        |> Job.map Result.ofChoice

    let inline fromTask aTask =
        aTask
        |> Job.fromTask
        |> Job.catch
        |> Job.map Result.ofChoice

    let inline fromUnitTask aTask =
        aTask
        |> Job.fromUnitTask
        |> Job.catch
        |> Job.map Result.ofChoice

    [<System.Obsolete "Please use JobResult.ok instead of singleton">]
    let inline singleton x = ok x

    let inline apply fJR xJR = map2 (fun f x -> f x) fJR xJR

    /// <summary>
    /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise returns <paramref name="ifError"/>
    /// </summary>
    /// <param name="ifError">The value to use if <paramref name="result"/> is <c>Error</c></param>
    /// <param name="result">The input result.</param>
    /// <remarks>
    /// </remarks>
    /// <example>
    /// <code>
    ///     JobResult.error "First" |> JobResult.orElse (JobResult.error "Second") // evaluates to Error ("Second")
    ///     JobResult.error "First" |> JobResult.orElse (JobResult.ok "Second") // evaluates to Ok ("Second")
    ///     JobResult.ok "First" |> JobResult.orElse (JobResult.error "Second") // evaluates to Ok ("First")
    ///     JobResult.ok "First" |> JobResult.orElse (JobResult.ok "Second") // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is Ok, else returns <paramref name="ifError"/>.
    /// </returns>
    let inline orElse (ifError: Job<Result<'ok, 'error2>>) (result: Job<Result<'ok, 'error>>) =
        result
        |> Job.bind (Result.eitherBind ok (fun _ -> ifError))

    /// <summary>
    /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise executes <paramref name="ifErrorFunc"/> and returns the result.
    /// </summary>
    /// <param name="ifErrorFunc">A function that provides an alternate result when evaluated.</param>
    /// <param name="result">The input result.</param>
    /// <remarks>
    /// <paramref name="ifErrorFunc"/>  is not executed unless <paramref name="result"/> is an <c>Error</c>.
    /// </remarks>
    /// <example>
    /// <code>
    ///     JobResult.error "First" |> JobResult.orElseWith (fun _ -> JobResult.error "Second") // evaluates to Error ("Second")
    ///     JobResult.error "First" |> JobResult.orElseWith (fun _ -> JobResult.ok "Second") // evaluates to Ok ("Second")
    ///     JobResult.ok "First" |> JobResult.orElseWith (fun _ -> JobResult.error "Second") // evaluates to Ok ("First")
    ///     JobResult.ok "First" |> JobResult.orElseWith (fun _ -> JobResult.ok "Second") // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is Ok, else the result of executing <paramref name="ifErrorFunc"/>.
    /// </returns>
    let inline orElseWith
        ([<InlineIfLambda>] ifErrorFunc: 'error -> Job<Result<'ok, 'error2>>)
        (result: Job<Result<'ok, 'error>>)
        =
        result
        |> Job.bind (Result.eitherBind ok ifErrorFunc)

    /// Replaces the wrapped value with unit
    let inline ignore<'ok, 'error> (jr: Job<Result<'ok, 'error>>) : Job<Result<unit, 'error>> =
        jr
        |> map ignore<'ok>

    /// Replaces an error value of a job-wrapped result with a custom error
    /// value.
    let inline setError error jobResult =
        jobResult
        |> Job.map (Result.setError error)

    /// Replaces a unit error value of a job-wrapped result with a custom error value.
    /// Safer than setError since you're not losing any information.
    let inline withError error jobResult =
        jobResult
        |> Job.map (Result.withError error)

    /// Extracts the contained value of a job-wrapped result if Ok, otherwise uses ifError.
    let inline defaultValue ifError jobResult =
        jobResult
        |> Job.map (Result.defaultValue ifError)

    /// Extracts the contained value of a job-wrapped result if Error, otherwise uses ifOk.
    let inline defaultError ifOk jobResult =
        jobResult
        |> Job.map (Result.defaultError ifOk)

    /// Extracts the contained value of a job-wrapped result if Ok, otherwise
    /// evaluates ifErrorThunk and uses the result.
    let inline defaultWith ([<InlineIfLambda>] ifErrorThunk: 'error -> 'ok) jobResult =
        jobResult
        |> Job.map (Result.defaultWith ifErrorThunk)

    /// Same as defaultValue for a result where the Ok value is unit. The name
    /// describes better what is actually happening in this case.
    let inline ignoreError<'error> (jobResult: Job<Result<unit, 'error>>) =
        defaultValue () jobResult

    /// If the job-wrapped result is Ok, executes the function on the Ok value.
    /// Passes through the input value.
    let inline tee ([<InlineIfLambda>] f) jobResult =
        jobResult
        |> Job.map (Result.tee f)

    /// If the job-wrapped result is Ok and the predicate returns true, executes
    /// the function on the Ok value. Passes through the input value.
    let inline teeIf ([<InlineIfLambda>] predicate) ([<InlineIfLambda>] f) jobResult =
        jobResult
        |> Job.map (Result.teeIf predicate f)

    /// If the job-wrapped result is Error, executes the function on the Error
    /// value. Passes through the input value.
    let inline teeError ([<InlineIfLambda>] f) jobResult =
        jobResult
        |> Job.map (Result.teeError f)

    /// If the job-wrapped result is Error and the predicate returns true,
    /// executes the function on the Error value. Passes through the input value.
    let inline teeErrorIf ([<InlineIfLambda>] predicate) ([<InlineIfLambda>] f) jobResult =
        jobResult
        |> Job.map (Result.teeErrorIf predicate f)

    /// Takes two results and returns a tuple of the pair
    let inline zip left right =
        Job.zip left right
        |> Job.map (fun (r1, r2) -> Result.zip r1 r2)

    /// Takes two results and returns a tuple of the error pair
    let inline zipError left right =
        Job.zip left right
        |> Job.map (fun (r1, r2) -> Result.zipError r1 r2)

    /// Catches exceptions and maps them to the Error case using the provided function.
    let inline catch f x =
        x
        |> Job.catch
        |> Job.map (
            function
            | Choice1Of2(Ok v) -> Ok v
            | Choice1Of2(Error err) -> Error err
            | Choice2Of2 ex -> Error(f ex)
        )

    /// Lift Job to JobResult
    let inline ofJob x =
        x
        |> Job.map Ok

    /// Lift Result to JobResult
    let inline ofResult (x: Result<_, _>) =
        x
        |> Job.result

    /// Returns the Job-wrapped result if it is Ok and the checker returns a Job-wrapped Ok result or if the Job-wrapped result is Error.
    /// If the checker returns a Job-wrapped Error result, returns the Job-wrapped Error result.
    let inline check
        ([<InlineIfLambda>] checker: 'ok -> Job<Result<unit, 'error>>)
        (x: Job<Result<'ok, 'error>>)
        : Job<Result<'ok, 'error>> =
        bind
            (fun x ->
                checker x
                |> map (fun () -> x)
            )
            x

    /// Bind the JobResult with a synchronous Result-returning function.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: Job<Result<'input, 'error>>)
        : Job<Result<'output, 'error>> =
        Job.map (Result.bind binder) input

    let req reqF errOrF value = bindResult (reqF errOrF) value
    let reqFilter predicate errOrF value = req (Req.filter predicate) errOrF value

    [<System.Obsolete "Please use JobResult.reqFilter predicate error instead of require predicate error">]
    let inline require predicate error result = reqFilter predicate error result

    [<System.Obsolete "Please use Job.req Req.isTrue error instead of requireTrue error">]
    let inline requireTrue error value = Job.req Req.isTrue error value

    [<System.Obsolete "Please use Job.req Req.isFalse error instead of requireFalse error">]
    let inline requireFalse error value = Job.req Req.isFalse error value

    [<System.Obsolete "Please use Job.req Req.some error instead of requireSome error">]
    let inline requireSome error value = Job.req Req.some error value

    [<System.Obsolete "Please use Job.req Req.someWith errorF instead of requireSomeWith errorF">]
    let requireSomeWith errorF value = Job.req Req.someWith errorF value

    [<System.Obsolete "Please use Job.req Req.none error instead of requireNone error">]
    let inline requireNone error value = Job.req Req.none error value

    [<System.Obsolete "Please use Job.req Req.noneWith errorF instead of requireNoneWith errorF">]
    let requireNoneWith errorF value = Job.req Req.noneWith errorF value

    [<System.Obsolete "Please use Job.req Req.valueSome error instead of requireValueSome error">]
    let inline requireValueSome error value = Job.req Req.valueSome error value

    [<System.Obsolete "Please use Job.req Req.valueNone error instead of requireValueNone error">]
    let inline requireValueNone error value = Job.req Req.valueNone error value

    [<System.Obsolete "Please use Job.req (Req.equalTo other) error instead of requireEqualTo other error">]
    let inline requireEqualTo other error value = Job.req (Req.equalTo other) error value

    [<System.Obsolete "Please use JobResult.req (Req.equalTo other) error value instead of requireEqual other value error">]
    let inline requireEqual other value error = req (Req.equalTo other) error value

    [<System.Obsolete "Please use Job.req Req.empty error instead of requireEmpty error">]
    let inline requireEmpty error value = Job.req Req.empty error value

    [<System.Obsolete "Please use Job.req Req.notEmpty error instead of requireNotEmpty error">]
    let inline requireNotEmpty error value = Job.req Req.notEmpty error value

    [<System.Obsolete "Please use Job.req Req.head error instead of requireHead error">]
    let inline requireHead error value = Job.req Req.head error value

    [<System.Obsolete "Please use JobResult.req Req.some error instead of bindRequireSome error">]
    let inline bindRequireSome error x = req Req.some error x

    [<System.Obsolete "Please use JobResult.req Req.none error instead of bindRequireNone error">]
    let inline bindRequireNone error x = req Req.none error x

    [<System.Obsolete "Please use JobResult.req Req.valueSome error instead of bindRequireValueSome error">]
    let inline bindRequireValueSome error x = req Req.valueSome error x

    [<System.Obsolete "Please use JobResult.req Req.valueNone error instead of bindRequireValueNone error">]
    let inline bindRequireValueNone error x = req Req.valueNone error x
