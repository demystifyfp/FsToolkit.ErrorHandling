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
        Job.bind (Result.either f error) jr

    let inline either
        ([<InlineIfLambda>] onSuccess: 'a -> 'b)
        ([<InlineIfLambda>] onError: 'c -> 'b)
        (jr: Job<Result<'a, 'c>>)
        : Job<'b> =
        Job.map (Result.either onSuccess onError) jr

    /// <summary>
    /// Maps the values of an <c>JobResult</c>  to a new <c>JobResult</c>  using the provided functions.
    /// </summary>
    /// <param name="onOk">The function to apply to the 'ok' value of the input <c>JobResult</c>.</param>
    /// <param name="onError">The function to apply to the 'error' value of the input <c>JobResult</c>.</param>
    /// <param name="input">The input <c>AsyncResult</c> to map.</param>
    /// <returns>A new <c>AsyncResult</c> with the mapped values.</returns>
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
        |> Job.bind (Result.either ok (fun _ -> ifError))

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
        |> Job.bind (Result.either ok ifErrorFunc)

    /// Replaces the wrapped value with unit
    let inline ignore<'ok, 'error> (jr: Job<Result<'ok, 'error>>) : Job<Result<unit, 'error>> =
        jr
        |> map ignore<'ok>

    /// Returns the specified error if the job-wrapped value is false.
    let inline requireTrue error value =
        value
        |> Job.map (Result.requireTrue error)

    /// Returns the specified error if the job-wrapped value is true.
    let inline requireFalse error value =
        value
        |> Job.map (Result.requireFalse error)

    // Converts a job-wrapped Option to a Result, using the given error if None.
    let inline requireSome error option =
        option
        |> Job.map (Result.requireSome error)

    // Converts a job-wrapped Option to a Result, using the given error factory if None.
    let inline requireSomeWith ([<InlineIfLambda>] errorFactory: unit -> 'error) option =
        option
        |> Job.map (Result.requireSomeWith errorFactory)

    // Converts a job-wrapped Option to a Result, using the given error if Some.
    let inline requireNone error option =
        option
        |> Job.map (Result.requireNone error)

    // Converts a job-wrapped Option to a Result, using the given error factory if Some.
    let inline requireNoneWith ([<InlineIfLambda>] errorFactory: unit -> 'error) option =
        option
        |> Job.map (Result.requireNoneWith errorFactory)

    // Converts a job-wrapped ValueOption to a Result, using the given error if ValueNone.
    let inline requireValueSome error voption =
        voption
        |> Job.map (Result.requireValueSome error)

    // Converts a job-wrapped ValueOption to a Result, using the given error if ValueSome.
    let inline requireValueNone error voption =
        voption
        |> Job.map (Result.requireValueNone error)

    /// Returns Ok if the job-wrapped value and the provided value are equal, or the specified error if not.
    let inline requireEqual x1 x2 error =
        x2
        |> Job.map (fun x2' -> Result.requireEqual x1 x2' error)

    /// Returns Ok if the two values are equal, or the specified error if not.
    let inline requireEqualTo other error this =
        this
        |> Job.map (Result.requireEqualTo other error)

    /// Returns Ok if the job-wrapped sequence is empty, or the specified error if not.
    let inline requireEmpty error xs =
        xs
        |> Job.map (Result.requireEmpty error)

    /// Returns Ok if the job-wrapped sequence is not-empty, or the specified error if not.
    let inline requireNotEmpty error xs =
        xs
        |> Job.map (Result.requireNotEmpty error)

    /// Returns the first item of the job-wrapped sequence if it exists, or the specified
    /// error if the sequence is empty
    let inline requireHead error xs =
        xs
        |> Job.map (Result.requireHead error)

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
    let inline zip j1 j2 =
        Job.zip j1 j2
        |> Job.map (fun (r1, r2) -> Result.zip r1 r2)

    /// Takes two results and returns a tuple of the error pair
    let inline zipError j1 j2 =
        Job.zip j1 j2
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

    /// Bind the JobResult with a synchronous Result-returning function.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: Job<Result<'input, 'error>>)
        : Job<Result<'output, 'error>> =
        Job.map (Result.bind binder) input

    /// Bind the JobResult and requireSome on the inner option value.
    let inline bindRequireSome error x = bindResult (Result.requireSome error) x

    /// Bind the JobResult and requireNone on the inner option value.
    let inline bindRequireNone error x = bindResult (Result.requireNone error) x

    /// Bind the JobResult and requireValueSome on the inner voption value.
    let inline bindRequireValueSome error x =
        bindResult (Result.requireValueSome error) x

    /// Bind the JobResult and requireValueNone on the inner voption value.
    let inline bindRequireValueNone error x =
        bindResult (Result.requireValueNone error) x
