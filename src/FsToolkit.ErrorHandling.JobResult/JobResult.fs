namespace FsToolkit.ErrorHandling

open Hopac
open Hopac.Infixes

[<RequireQualifiedAccess>]
module JobResult =

    let inline map ([<InlineIfLambda>] f) jr = Job.map (Result.map f) jr

    let inline mapError ([<InlineIfLambda>] f) jr = Job.map (Result.mapError f) jr

    let inline bind ([<InlineIfLambda>] f: 'a -> Job<Result<'c, 'b>>) (jr: Job<Result<'a, 'b>>) =
        Job.bind
            (Result.either
                f
                (Error
                 >> Job.result))
            jr

    let inline foldResult ([<InlineIfLambda>] onSuccess) ([<InlineIfLambda>] onError) jr =
        Job.map (Result.fold onSuccess onError) jr

    let inline eitherMap ([<InlineIfLambda>] onSuccess) ([<InlineIfLambda>] onError) jr =
        Job.map (Result.eitherMap onSuccess onError) jr

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

    let inline retn x =
        Ok x
        |> Job.result

    let inline ok x = retn x

    let inline returnError x =
        Error x
        |> Job.result

    let inline error x = returnError x

    let inline map2 ([<InlineIfLambda>] f) xJR yJR = Job.map2 (Result.map2 f) xJR yJR

    let inline map3 ([<InlineIfLambda>] f) xJR yJR zJR = Job.map3 (Result.map3 f) xJR yJR zJR

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
    let inline ignore<'ok, 'error> (jr: Job<Result<'ok, 'error>>) =
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

    // Converts an job-wrapped Option to a Result, using the given error if None.
    let inline requireSome error option =
        option
        |> Job.map (Result.requireSome error)

    // Converts an job-wrapped Option to a Result, using the given error if Some.
    let inline requireNone error option =
        option
        |> Job.map (Result.requireNone error)

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

    /// Replaces an error value of an job-wrapped result with a custom error
    /// value.
    let inline setError error jobResult =
        jobResult
        |> Job.map (Result.setError error)

    /// Replaces a unit error value of an job-wrapped result with a custom
    /// error value. Safer than setError since you're not losing any information.
    let inline withError error jobResult =
        jobResult
        |> Job.map (Result.withError error)

    /// Extracts the contained value of an job-wrapped result if Ok, otherwise
    /// uses ifError.
    let inline defaultValue ifError jobResult =
        jobResult
        |> Job.map (Result.defaultValue ifError)

    /// Extracts the contained value of an job-wrapped result if Error, otherwise
    /// uses ifOk.
    let inline defaultError ifOk jobResult =
        jobResult
        |> Job.map (Result.defaultError ifOk)

    /// Extracts the contained value of an job-wrapped result if Ok, otherwise
    /// evaluates ifErrorThunk and uses the result.
    let inline defaultWith ifErrorThunk jobResult =
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
            | Choice1Of2 (Ok v) -> Ok v
            | Choice1Of2 (Error err) -> Error err
            | Choice2Of2 ex -> Error(f ex)
        )

    /// Lift Job to JobResult
    let inline ofJob x =
        x
        |> Job.map Ok

    /// Lift Result to JobResult
    let inline ofResult (x: Result<_, _>) =
        x
        |> Job.singleton

    /// Bind the JobResult and requireSome on the inner option value.
    let inline bindRequireSome error x =
        x
        |> bind (
            Result.requireSome error
            >> Job.singleton
        )

    /// Bind the JobResult and requireNone on the inner option value.
    let inline bindRequireNone error x =
        x
        |> bind (
            Result.requireNone error
            >> Job.singleton
        )
