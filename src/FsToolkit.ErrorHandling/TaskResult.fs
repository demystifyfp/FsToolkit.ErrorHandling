namespace FsToolkit.ErrorHandling

open System.Threading.Tasks

[<RequireQualifiedAccess>]
module TaskResult =

    let inline ok x = Task.singleton (Ok x)

    let inline error x = Task.singleton (Error x)

    let inline map ([<InlineIfLambda>] f) tr = Task.map (Result.map f) tr

    let inline mapError ([<InlineIfLambda>] f) tr = Task.map (Result.mapError f) tr

    let inline ofAsync aAsync =
        aAsync
        |> Async.Catch
        |> Async.StartImmediateAsTask
        |> Task.map Result.ofChoice

    let inline bind ([<InlineIfLambda>] f) (tr: Task<_>) =
        Task.bind (Result.eitherBind f error) tr

    let inline eitherBind
        ([<InlineIfLambda>] onOk: 'input -> 'output)
        ([<InlineIfLambda>] onError: 'inputError -> 'output)
        (input: Task<Result<'input, 'inputError>>)
        : Task<'output> =
        Task.map (Result.eitherBind onOk onError) input

    [<System.Obsolete "Please use eitherBind instead of foldResult (renamed to align with Result naming)">]
    let inline foldResult
        ([<InlineIfLambda>] onOk: 'input -> 'output)
        ([<InlineIfLambda>] onError: 'inputError -> 'output)
        (input: Task<Result<'input, 'inputError>>)
        : Task<'output> =
        eitherBind onOk onError input

    /// <summary>
    /// Maps the values of an <c>TaskResult</c>  to a new <c>TaskResult</c>  using the provided functions.
    /// </summary>
    /// <param name="onOk">The function to apply to the 'ok' value of the input <c>TaskResult</c>.</param>
    /// <param name="onError">The function to apply to the 'error' value of the input <c>TaskResult</c>.</param>
    /// <param name="input">The input <c>TaskResult</c> to map.</param>
    /// <returns>A new <c>TaskResult</c> with the mapped values.</returns>
    let inline eitherMap
        ([<InlineIfLambda>] onOk: 'okInput -> 'okOutput)
        ([<InlineIfLambda>] onError: 'errorInput -> 'errorOutput)
        (input: Task<Result<'okInput, 'errorInput>>)
        : Task<Result<'okOutput, 'errorOutput>> =
        Task.map (Result.eitherMap onOk onError) input

    let inline map2 ([<InlineIfLambda>] f) xTR yTR = Task.map2 (Result.map2 f) xTR yTR

    let inline map3 ([<InlineIfLambda>] f) xTR yTR zTR = Task.map3 (Result.map3 f) xTR yTR zTR

    let inline apply fTR xTR = map2 (fun f x -> f x) fTR xTR

    /// <summary>
    /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise returns <paramref name="ifError"/>
    /// </summary>
    /// <param name="ifError">The value to use if <paramref name="result"/> is <c>Error</c></param>
    /// <param name="result">The input result.</param>
    /// <remarks>
    /// </remarks>
    /// <example>
    /// <code>
    ///     TaskResult.error "First" |> TaskResult.orElse (TaskResult.error "Second") // evaluates to Error ("Second")
    ///     TaskResult.error "First" |> TaskResult.orElse (TaskResult.ok "Second") // evaluates to Ok ("Second")
    ///     TaskResult.ok "First" |> TaskResult.orElse (TaskResult.error "Second") // evaluates to Ok ("First")
    ///     TaskResult.ok "First" |> TaskResult.orElse (TaskResult.ok "Second") // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is Ok, else returns <paramref name="ifError"/>.
    /// </returns>
    let inline orElse (ifError: Task<Result<'ok, 'error2>>) (result: Task<Result<'ok, 'error>>) =
        result
        |> Task.bind (Result.eitherBind ok (fun _ -> ifError))

    /// <summary>
    /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise executes <paramref name="ifErrorFunc"/> and returns the result.
    /// </summary>
    /// <param name="ifErrorFunc">A function that provides an alternate result when evaluated.</param>
    /// <param name="result">The input result.</param>
    /// <remarks>
    /// <paramref name="ifErrorFunc"/> is not executed unless <paramref name="result"/> is an <c>Error</c>.
    /// </remarks>
    /// <example>
    /// <code>
    ///     TaskResult.error "First" |> TaskResult.orElseWith (fun _ -> TaskResult.error "Second") // evaluates to Error ("Second")
    ///     TaskResult.error "First" |> TaskResult.orElseWith (fun _ -> TaskResult.ok "Second") // evaluates to Ok ("Second")
    ///     TaskResult.ok "First" |> TaskResult.orElseWith (fun _ -> TaskResult.error "Second") // evaluates to Ok ("First")
    ///     TaskResult.ok "First" |> TaskResult.orElseWith (fun _ -> TaskResult.ok "Second") // evaluates to Ok ("First")
    /// </code>
    /// </example>
    /// <returns>
    /// The result if the result is Ok, else the result of executing <paramref name="ifErrorFunc"/>.
    /// </returns>
    let inline orElseWith
        ([<InlineIfLambda>] ifErrorFunc: 'error -> Task<Result<'ok, 'error2>>)
        (result: Task<Result<'ok, 'error>>)
        =
        result
        |> Task.bind (Result.eitherBind ok ifErrorFunc)

    /// Replaces the wrapped value with unit
    let inline ignore<'ok, 'error> (tr: Task<Result<'ok, 'error>>) =
        tr
        |> map ignore<'ok>

    /// Replaces an error value of a task-wrapped result with a custom error
    /// value.
    let inline setError error taskResult =
        taskResult
        |> Task.map (Result.setError error)

    /// Replaces a unit error value of a task-wrapped result with a custom
    /// error value. Safer than setError since you're not losing any information.
    let inline withError error taskResult =
        taskResult
        |> Task.map (Result.withError error)

    /// Extracts the contained value of a task-wrapped result if Ok, otherwise
    /// uses ifError.
    let inline defaultValue ifError taskResult =
        taskResult
        |> Task.map (Result.defaultValue ifError)

    /// Extracts the contained value of a task-wrapped result if Error, otherwise
    /// uses ifOk.
    let inline defaultError ifOk taskResult =
        taskResult
        |> Task.map (Result.defaultError ifOk)

    /// Extracts the contained value of a task-wrapped result if Ok, otherwise
    /// evaluates ifErrorThunk and uses the result.
    let inline defaultWith ifErrorThunk taskResult =
        taskResult
        |> Task.map (Result.defaultWith ifErrorThunk)

    /// Same as defaultValue for a result where the Ok value is unit. The name
    /// describes better what is actually happening in this case.
    let inline ignoreError<'error> (taskResult: Task<Result<unit, 'error>>) =
        defaultValue () taskResult

    /// If the task-wrapped result is Ok, executes the function on the Ok value.
    /// Passes through the input value.
    let inline tee ([<InlineIfLambda>] f) taskResult =
        taskResult
        |> Task.map (Result.tee f)

    /// If the task-wrapped result is Ok and the predicate returns true, executes
    /// the function on the Ok value. Passes through the input value.
    let inline teeIf ([<InlineIfLambda>] predicate) ([<InlineIfLambda>] f) taskResult =
        taskResult
        |> Task.map (Result.teeIf predicate f)

    /// If the task-wrapped result is Error, executes the function on the Error
    /// value. Passes through the input value.
    let inline teeError ([<InlineIfLambda>] f) taskResult =
        taskResult
        |> Task.map (Result.teeError f)

    /// If the task-wrapped result is Error and the predicate returns true,
    /// executes the function on the Error value. Passes through the input value.
    let inline teeErrorIf predicate ([<InlineIfLambda>] f) taskResult =
        taskResult
        |> Task.map (Result.teeErrorIf predicate f)

    /// Takes two results and returns a tuple of the pair
    let inline zip left right =
        Task.zip left right
        |> Task.map (fun (r1, r2) -> Result.zip r1 r2)

    /// Takes two results and returns a tuple of the error pair
    let inline zipError left right =
        Task.zip left right
        |> Task.map (fun (r1, r2) -> Result.zipError r1 r2)

    /// Catches exceptions and maps them to the Error case using the provided function.
    let inline catch ([<InlineIfLambda>] f) x =
        x
        |> Task.catch
        |> Task.map (
            function
            | Choice1Of2(Ok v) -> Ok v
            | Choice1Of2(Error err) -> Error err
            | Choice2Of2 ex -> Error(f ex)
        )

    /// <summary>
    /// Lifts a <c>Task&lt;'ok&gt;</c> into a <c>Task&lt;Result&lt;'ok, 'error&gt;&gt;</c> by wrapping the value in <c>Ok</c>.
    /// Any exceptions thrown by the task will not be caught and will propagate as-is.
    /// To catch exceptions and map them to the <c>Error</c> case, use <see cref="ofCatchTask"/>.
    /// </summary>
    /// <param name="x">The task to lift.</param>
    /// <returns>A task containing <c>Ok</c> of the task's result value.</returns>
    let inline ofTask x =
        x
        |> Task.map Ok

    /// <summary>
    /// Lifts a <c>Task&lt;'ok&gt;</c> into a <c>Task&lt;Result&lt;'ok, exn&gt;&gt;</c>, catching any exceptions
    /// thrown by the task and wrapping them in <c>Error</c>. If the task completes successfully,
    /// the result is wrapped in <c>Ok</c>.
    /// </summary>
    /// <param name="x">The task to lift.</param>
    /// <returns>
    /// A task containing <c>Ok</c> of the result value if the task succeeds, or
    /// <c>Error</c> of the exception if the task throws.
    /// </returns>
    /// <example>
    /// <code>
    ///     TaskResult.ofCatchTask (task { return 42 })
    ///     // Returns: task { return Ok 42 }
    ///
    ///     TaskResult.ofCatchTask (task { failwith "something went wrong" })
    ///     // Returns: task { return Error (System.Exception("something went wrong")) }
    /// </code>
    /// </example>
    let inline ofCatchTask (x: Task<'ok>) : Task<Result<'ok, exn>> =
        x
        |> Task.catch
        |> Task.map Result.ofChoice

    /// Lift Result to TaskResult
    let inline ofResult (x: Result<_, _>) =
        x
        |> Task.singleton

    /// Returns the task-wrapped result if it is Ok and the checkFunc returns a task-wrapped Ok result or if the task-wrapped result is Error.
    /// If the checkFunc returns a task-wrapped Error result, returns the task-wrapped Error result.
    let inline check
        ([<InlineIfLambda>] checker: 'ok -> Task<Result<unit, 'error>>)
        (x: Task<Result<'ok, 'error>>)
        : Task<Result<'ok, 'error>> =
        bind
            (fun x ->
                checker x
                |> map (fun () -> x)
            )
            x

    /// Bind the TaskResult with a synchronous Result-returning function
    let inline bindResult
        ([<InlineIfLambda>] reqF: 'input -> Result<'output, 'error>)
        (input: Task<Result<'input, 'error>>)
        : Task<Result<'output, 'error>> =
        Task.bindResult (Result.bind reqF) input

    let inline req ([<InlineIfLambda>] reqF) errOrF value = bindResult (reqF errOrF) value
    let inline reqFilter predicate error value = req (Req.filter predicate) error value

    [<System.Obsolete "Please use TaskResult.reqFilter predicate error instead of require predicate error">]
    let inline require predicate error result = reqFilter predicate error result

    [<System.Obsolete "Please use TaskResult.req (Req.equalTo other) error value instead of requireEqual other value error">]
    let inline requireEqual other value error = req (Req.equalTo other) error value

    [<System.Obsolete "Please use Task.req Req.isTrue error instead of requireTrue error">]
    let inline requireTrue error value = Task.req Req.isTrue error value

    [<System.Obsolete "Please use Task.req Req.isTrueWith errorF instead of requireTrueWith errorF">]
    let inline requireTrueWith errorF value = Task.req Req.isTrueWith errorF value

    [<System.Obsolete "Please use Task.req Req.isFalse error instead of requireFalse error">]
    let inline requireFalse error value = Task.req Req.isFalse error value

    [<System.Obsolete "Please use Task.req Req.isFalseWith errorF instead of requireFalseWith errorF">]
    let inline requireFalseWith errorF value = Task.req Req.isFalseWith errorF value

    [<System.Obsolete "Please use Task.req Req.some error instead of requireSome error">]
    let inline requireSome error value = Task.req Req.some error value

    [<System.Obsolete "Please use Task.req Req.someWith errorF instead of requireSomeWith errorF">]
    let inline requireSomeWith errorF value = Task.req Req.someWith errorF value

    [<System.Obsolete "Please use Task.req Req.none error instead of requireNone error">]
    let inline requireNone error value = Task.req Req.none error value

    [<System.Obsolete "Please use Task.req Req.noneWith errorF instead of requireNoneWith errorF">]
    let inline requireNoneWith errorF value = Task.req Req.noneWith errorF value

    [<System.Obsolete "Please use Task.req Req.valueSome error instead of requireValueSome error">]
    let inline requireValueSome error value = Task.req Req.valueSome error value

    [<System.Obsolete "Please use Task.req Req.valueSomeWith errorF instead of requireValueSomeWith errorF">]
    let inline requireValueSomeWith errorF value = Task.req Req.valueSomeWith errorF value

    [<System.Obsolete "Please use Task.req Req.valueNone error instead of requireValueNone error">]
    let inline requireValueNone error value = Task.req Req.valueNone error value

    [<System.Obsolete "Please use Task.req Req.valueNoneWith errorF instead of requireValueNoneWith errorF">]
    let inline requireValueNoneWith errorF value = Task.req Req.valueNoneWith errorF value

    [<System.Obsolete "Please use Task.req (Req.equalTo other) error instead of requireEqualTo other error">]
    let inline requireEqualTo other error value =
        Task.req (Req.equalTo other) error value

    [<System.Obsolete "Please use Task.req Req.empty error instead of requireEmpty error">]
    let inline requireEmpty error value = Task.req Req.empty error value

    [<System.Obsolete "Please use Task.req Req.notEmpty error instead of requireNotEmpty error">]
    let inline requireNotEmpty error value = Task.req Req.notEmpty error value

    [<System.Obsolete "Please use Task.req Req.head error instead of requireHead error">]
    let inline requireHead error value = Task.req Req.head error value

    [<System.Obsolete "Please use TaskResult.req Req.some error instead of bindRequireSome error">]
    let inline bindRequireSome error x = req Req.some error x

    [<System.Obsolete "Please use TaskResult.req Req.none error instead of bindRequireNone error">]
    let inline bindRequireNone error x = req Req.none error x

    [<System.Obsolete "Please use TaskResult.req Req.someWith errorF instead of bindRequireSomeWith errorF">]
    let inline bindRequireSomeWith errorF x = req Req.someWith errorF x

    [<System.Obsolete "Please use TaskResult.req Req.noneWith errorF instead of bindRequireNoneWith errorF">]
    let inline bindRequireNoneWith errorF x = req Req.noneWith errorF x

    [<System.Obsolete "Please use TaskResult.req Req.valueSome error instead of bindRequireValueSome error">]
    let inline bindRequireValueSome error x = req Req.valueSome error x

    [<System.Obsolete "Please use TaskResult.req Req.valueNone error instead of bindRequireValueNone error">]
    let inline bindRequireValueNone error x = req Req.valueNone error x

    [<System.Obsolete "Please use TaskResult.req Req.valueSomeWith errorF instead of bindRequireValueSomeWith errorF">]
    let inline bindRequireValueSomeWith errorF x = req Req.valueSomeWith errorF x

    [<System.Obsolete "Please use TaskResult.req Req.valueNoneWith errorF instead of bindRequireValueNoneWith errorF">]
    let inline bindRequireValueNoneWith errorF x = req Req.valueNoneWith errorF x

    [<System.Obsolete "Please use TaskResult.req Req.isTrue error instead of bindRequireTrue error">]
    let inline bindRequireTrue error x = req Req.isTrue error x

    [<System.Obsolete "Please use TaskResult.req Req.isTrueWith errorF instead of bindRequireTrueWith errorF">]
    let inline bindRequireTrueWith errorF x = req Req.isTrueWith errorF x

    [<System.Obsolete "Please use TaskResult.req Req.isFalse error instead of bindRequireFalse error">]
    let inline bindRequireFalse error x = req Req.isFalse error x

    [<System.Obsolete "Please use TaskResult.req Req.isFalseWith errorF instead of bindRequireFalseWith errorF">]
    let inline bindRequireFalseWith errorF x = req Req.isFalseWith errorF x

    [<System.Obsolete "Please use TaskResult.req Req.notNull error instead of bindRequireNotNull error">]
    let inline bindRequireNotNull error x = req Req.notNull error x

    [<System.Obsolete "Please use TaskResult.req (Req.equalTo other) error instead of bindRequireEqual other error">]
    let inline bindRequireEqual y error x = req (Req.equalTo y) error x

    [<System.Obsolete "Please use TaskResult.req Req.empty error instead of bindRequireEmpty error">]
    let inline bindRequireEmpty error x = req Req.empty error x

    [<System.Obsolete "Please use TaskResult.req Req.notEmpty error instead of bindRequireNotEmpty error">]
    let inline bindRequireNotEmpty error x = req Req.notEmpty error x

    [<System.Obsolete "Please use TaskResult.req Req.head error instead of bindRequireHead error">]
    let inline bindRequireHead error x = req Req.head error x
