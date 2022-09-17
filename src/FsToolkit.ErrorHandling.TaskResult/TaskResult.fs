namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
#if NETSTANDARD2_0
open FSharp.Control.Tasks.Affine
#endif

[<RequireQualifiedAccess>]
module TaskResult =

    let inline map ([<InlineIfLambda>] f) tr = Task.map (Result.map f) tr

    let inline mapError ([<InlineIfLambda>] f) tr = Task.map (Result.mapError f) tr

    let inline bind ([<InlineIfLambda>] f) (tr: Task<_>) =
        tr
        |> Task.bind (
            Result.either
                f
                (Error
                 >> Task.singleton)
        )

    let inline foldResult ([<InlineIfLambda>] onSuccess) ([<InlineIfLambda>] onError) tr =
        Task.map (Result.fold onSuccess onError) tr

    let inline ofAsync aAsync =
        aAsync
        |> Async.Catch
        |> Async.StartAsTask
        |> Task.map Result.ofChoice

    let inline retn x =
        Ok x
        |> Task.singleton

    let inline ok x = retn x

    let inline returnError x =
        Error x
        |> Task.singleton

    let inline error x = returnError x

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
        |> Task.bind (Result.either ok (fun _ -> ifError))

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
        |> Task.bind (Result.either ok ifErrorFunc)

    /// Replaces the wrapped value with unit
    let inline ignore<'ok, 'error> (tr: Task<Result<'ok, 'error>>) =
        tr
        |> map ignore<'ok>

    /// Returns the specified error if the task-wrapped value is false.
    let inline requireTrue error value =
        value
        |> Task.map (Result.requireTrue error)

    /// Returns the specified error if the task-wrapped value is true.
    let inline requireFalse error value =
        value
        |> Task.map (Result.requireFalse error)

    // Converts an task-wrapped Option to a Result, using the given error if None.
    let inline requireSome error option =
        option
        |> Task.map (Result.requireSome error)

    // Converts an task-wrapped Option to a Result, using the given error if Some.
    let inline requireNone error option =
        option
        |> Task.map (Result.requireNone error)

    /// Returns Ok if the task-wrapped value and the provided value are equal, or the specified error if not.
    let inline requireEqual x1 x2 error =
        x2
        |> Task.map (fun x2' -> Result.requireEqual x1 x2' error)

    /// Returns Ok if the two values are equal, or the specified error if not.
    let inline requireEqualTo other error this =
        this
        |> Task.map (Result.requireEqualTo other error)

    /// Returns Ok if the task-wrapped sequence is empty, or the specified error if not.
    let inline requireEmpty error xs =
        xs
        |> Task.map (Result.requireEmpty error)

    /// Returns Ok if the task-wrapped sequence is not-empty, or the specified error if not.
    let inline requireNotEmpty error xs =
        xs
        |> Task.map (Result.requireNotEmpty error)

    /// Returns the first item of the task-wrapped sequence if it exists, or the specified
    /// error if the sequence is empty
    let inline requireHead error xs =
        xs
        |> Task.map (Result.requireHead error)

    /// Replaces an error value of an task-wrapped result with a custom error
    /// value.
    let inline setError error taskResult =
        taskResult
        |> Task.map (Result.setError error)

    /// Replaces a unit error value of an task-wrapped result with a custom
    /// error value. Safer than setError since you're not losing any information.
    let inline withError error taskResult =
        taskResult
        |> Task.map (Result.withError error)

    /// Extracts the contained value of an task-wrapped result if Ok, otherwise
    /// uses ifError.
    let inline defaultValue ifError taskResult =
        taskResult
        |> Task.map (Result.defaultValue ifError)

    /// Extracts the contained value of an task-wrapped result if Error, otherwise
    /// uses ifOk.
    let inline defaultError ifOk taskResult =
        taskResult
        |> Task.map (Result.defaultError ifOk)

    /// Extracts the contained value of an task-wrapped result if Ok, otherwise
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
    let inline zip x1 x2 =
        Task.zip x1 x2
        |> Task.map (fun (r1, r2) -> Result.zip r1 r2)

    /// Takes two results and returns a tuple of the error pair
    let inline zipError x1 x2 =
        Task.zip x1 x2
        |> Task.map (fun (r1, r2) -> Result.zipError r1 r2)

    /// Catches exceptions and maps them to the Error case using the provided function.
    let inline catch ([<InlineIfLambda>] f) x =
        x
        |> Task.catch
        |> Task.map (
            function
            | Choice1Of2 (Ok v) -> Ok v
            | Choice1Of2 (Error err) -> Error err
            | Choice2Of2 ex -> Error(f ex)
        )

    /// Lift Task to TaskResult
    let inline ofTask x =
        x
        |> Task.map Ok

    /// Lift Result to TaskResult
    let inline ofResult (x: Result<_, _>) =
        x
        |> Task.singleton
