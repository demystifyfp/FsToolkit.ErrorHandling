namespace FsToolkit.ErrorHandling


// What's going on here?
//
// F# method overload resolution  has some weird quirks we're taking advantage of to allow
// for binding (`let!/do!/return!`) many various types (Such as Task/Async/Result/Validation)
// in a computation expression. .The gist is, any member methods attached to the type itself
// (the Builder object) will be preferred above all else when selection overloads to resolve. It
// will then use the  the most recent Extension Methods that have been opened. The way we structure
// these overloads is to provide the most "concrete" overloads first, and then the more generic
// ones later. For example, `Validation` is defined as a `Result<'T, 'Error list>`, but we also
// want to be able to bind to `Result` itself and create a list of errors from it.  So we need to
// have a `Validation` member method in a higher module, and then a `Result` member method
// somewhere lower. Another example is `Task<Result<'T, 'Error>>` vs `Task<'T>`. We want to be able
// to bind to both, so we need to have a `Task<Result<'T, 'Error>>` member method in a higher
//  module, and then a `Task<'T>` member method somewhere lower.

// NoEagerConstraintApplication also changes behavior of SRTP methods, read the
// TaskBuilder RFC for more info.

// The reason we do AutoOpens here instead of using the attribute on the module itself
// is because it may restrict how the implementation is relying on other sections, such as
// The MediumPriority module may use something from the HighPriority module. If we put the
// HighPriority module after the MediumPriority module it will fail to compile. So we don't want
// the order of the code itself to determine the priority, this allows us to control that ordering
// more explicitly.
//
// Additional readings:
// - [F# Computation Expression Method Overload Resolution Ordering](https://gist.github.com/TheAngryByrd/c8b9c8ebcda3bb162f425bfb281d2e2b)
// - [F# RFC FS-1097 - Task builder](https://github.com/fsharp/fslang-design/blob/main/FSharp-6.0/FS-1097-task-builder.md#feature-noeagerconstraintapplicationattribute)
// - ["Most concrete" tiebreaker for generic overloads](https://github.com/fsharp/fslang-suggestions/issues/905)


//     [<assembly: AutoOpen("FsToolkit.ErrorHandling.CancellableTaskValidationCE.LowestPriority")>]
//     [<assembly: AutoOpen("FsToolkit.ErrorHandling.CancellableTaskValidationCE.LowerPriority")>]
//     [<assembly: AutoOpen("FsToolkit.ErrorHandling.CancellableTaskValidationCE.LowerPriority2")>]
//     [<assembly: AutoOpen("FsToolkit.ErrorHandling.CancellableTaskValidationCE.LowPriority")>]
//     [<assembly: AutoOpen("FsToolkit.ErrorHandling.CancellableTaskValidationCE.MediumPriority")>]
//     [<assembly: AutoOpen("FsToolkit.ErrorHandling.CancellableTaskValidationCE.HighPriority")>]
//     [<assembly: AutoOpen("FsToolkit.ErrorHandling.CancellableTaskValidationCE.AsyncExtensions")>]
//     [<assembly: AutoOpen("FsToolkit.ErrorHandling.CancellableTaskValidationCE.CancellableTaskValidationBuilderModule")>]
//     do ()


/// Contains methods to build CancellableTasks using the F# computation expression syntax
[<AutoOpen>]
module CancellableTaskValidationCE =

    open System
    open System.Runtime.CompilerServices
    open System.Threading
    open System.Threading.Tasks
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.CompilerServices
    open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Collections
    open IcedTasks


    /// CancellationToken -> Task<Result<'T, 'Error list>>
    type CancellableTaskValidation<'T, 'Error> = CancellableTask<Validation<'T, 'Error>>


    /// Contains methods to build CancellableTasks using the F# computation expression syntax
    type CancellableTaskValidationBuilder() =

        inherit CancellableTaskResultBuilderBase()

        // This is the dynamic implementation - this is not used
        // for statically compiled tasks.  An executor (resumptionFuncExecutor) is
        // registered with the state machine, plus the initial resumption.
        // The executor stays constant throughout the execution, it wraps each step
        // of the execution in a try/with.  The resumption is changed at each step
        // to represent the continuation of the computation.
        /// <summary>
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        /// </summary>
        static member inline RunDynamic
            (code:
                CancellableTaskResultBuilderBaseCode<'T, 'T, _, AsyncTaskMethodBuilder<Validation<'T, 'Error>>>)
            : CancellableTaskValidation<'T, 'Error> =

            let mutable sm = CancellableTaskResultBuilderBaseStateMachine<'T, _, _>()

            let initialResumptionFunc =
                CancellableTaskResultBuilderBaseResumptionFunc<'T, _, _>(fun sm -> code.Invoke(&sm))

            let resumptionInfo =
                { new CancellableTaskResultBuilderBaseResumptionDynamicInfo<'T, _, _>(initialResumptionFunc) with
                    member info.MoveNext(sm) =
                        let mutable savedExn = null

                        try
                            sm.ResumptionDynamicInfo.ResumptionData <- null
                            let step = info.ResumptionFunc.Invoke(&sm)

                            if step then
                                sm.Data.MethodBuilder.SetResult sm.Data.Result
                            // MethodBuilder.SetResult(&sm.Data.MethodBuilder, sm.Data.Result)
                            else
                                let mutable awaiter =
                                    sm.ResumptionDynamicInfo.ResumptionData
                                    :?> ICriticalNotifyCompletion

                                assert not (isNull awaiter)

                                MethodBuilder.AwaitUnsafeOnCompleted(
                                    &sm.Data.MethodBuilder,
                                    &awaiter,
                                    &sm
                                )

                        with exn ->
                            savedExn <- exn
                        // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                        match savedExn with
                        | null -> ()
                        | exn -> MethodBuilder.SetException(&sm.Data.MethodBuilder, exn)

                    member _.SetStateMachine(sm, state) =
                        MethodBuilder.SetStateMachine(&sm.Data.MethodBuilder, state)
                }

            fun (ct) ->
                if ct.IsCancellationRequested then
                    Task.FromCanceled<_>(ct)
                else
                    sm.Data.CancellationToken <- ct
                    sm.ResumptionDynamicInfo <- resumptionInfo
                    sm.Data.MethodBuilder <- AsyncTaskMethodBuilder<Validation<'T, 'Error>>.Create()
                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task


        /// Hosts the task code in a state machine and starts the task.
        member inline _.Run
            (code: CancellableTaskResultBuilderBaseCode<'T, 'T, _, _>)
            : CancellableTaskValidation<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<CancellableTaskResultBuilderBaseStateMachineData<'T, _, AsyncTaskMethodBuilder<Validation<'T, 'Error>>>, CancellableTaskValidation<'T, 'Error>>
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: Exception = null

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if __stack_code_fin then
                                sm.Data.MethodBuilder.SetResult sm.Data.Result
                        // MethodBuilder.SetResult(&sm.Data.MethodBuilder, sm.Data.Result)
                        with exn ->
                            __stack_exn <- exn
                        // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                        match __stack_exn with
                        | null -> ()
                        | exn -> MethodBuilder.SetException(&sm.Data.MethodBuilder, exn)
                    //-- RESUMABLE CODE END
                    ))
                    (SetStateMachineMethodImpl<_>(fun sm state ->
                        MethodBuilder.SetStateMachine(&sm.Data.MethodBuilder, state)
                    ))
                    (AfterCode<_, _>(fun sm ->
                        let sm = sm

                        fun (ct) ->
                            if ct.IsCancellationRequested then
                                Task.FromCanceled<_>(ct)
                            else
                                let mutable sm = sm
                                sm.Data.CancellationToken <- ct

                                sm.Data.MethodBuilder <-
                                    AsyncTaskMethodBuilder<Validation<'T, 'Error>>.Create()

                                sm.Data.MethodBuilder.Start(&sm)
                                sm.Data.MethodBuilder.Task
                    ))
            else
                failwith "LOL"
        // CancellableTaskResultBuilder.RunDynamic(code)

        /// Specify a Source of CancellationToken -> Task<_> on the real type to allow type inference to work
        member inline _.Source
            (x: CancellableTaskValidation<_, _>)
            : CancellationToken -> Awaiter<TaskAwaiter<_>, _> =
            fun ct -> Awaitable.GetTaskAwaiter(x ct)


    /// Contains methods to build CancellableTasks using the F# computation expression syntax
    type BackgroundCancellableTaskResultBuilder() =

        inherit CancellableTaskResultBuilderBase()

        /// <summary>
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        /// </summary>
        static member inline RunDynamic
            (code: CancellableTaskResultBuilderBaseCode<'T, 'T, 'Error, _>)
            : CancellableTaskResult<'T, 'Error> =
            // backgroundTask { .. } escapes to a background thread where necessary
            // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
            if
                isNull SynchronizationContext.Current
                && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
            then
                CancellableTaskResultBuilder.RunDynamic(code)
            else
                fun (ct) ->
                    Task.Run<Result<'T, 'Error>>(
                        (fun () -> CancellableTaskResultBuilder.RunDynamic (code) (ct)),
                        ct
                    )

        /// <summary>
        /// Hosts the task code in a state machine and starts the task, executing in the ThreadPool using Task.Run
        /// </summary>
        member inline _.Run
            (code: CancellableTaskResultBuilderBaseCode<'T, 'T, 'Error, _>)
            : CancellableTaskResult<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<CancellableTaskResultBuilderBaseStateMachineData<'T, 'Error, _>, CancellableTaskResult<'T, 'Error>>
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: Exception = null

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if __stack_code_fin then
                                MethodBuilder.SetResult(&sm.Data.MethodBuilder, sm.Data.Result)
                        with exn ->
                            __stack_exn <- exn
                        // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                        match __stack_exn with
                        | null -> ()
                        | exn -> MethodBuilder.SetException(&sm.Data.MethodBuilder, exn)
                    //-- RESUMABLE CODE END
                    ))
                    (SetStateMachineMethodImpl<_>(fun sm state ->
                        MethodBuilder.SetStateMachine(&sm.Data.MethodBuilder, state)
                    ))
                    (AfterCode<_, CancellableTaskResult<'T, 'Error>>(fun sm ->
                        // backgroundTask { .. } escapes to a background thread where necessary
                        // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
                        if
                            isNull SynchronizationContext.Current
                            && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
                        then
                            let mutable sm = sm

                            fun (ct) ->
                                if ct.IsCancellationRequested then
                                    Task.FromCanceled<_>(ct)
                                else
                                    sm.Data.CancellationToken <- ct

                                    sm.Data.MethodBuilder <-
                                        AsyncTaskMethodBuilder<Result<'T, 'Error>>.Create()

                                    sm.Data.MethodBuilder.Start(&sm)
                                    sm.Data.MethodBuilder.Task
                        else
                            let sm = sm // copy contents of state machine so we can capture it

                            fun (ct) ->
                                if ct.IsCancellationRequested then
                                    Task.FromCanceled<_>(ct)
                                else
                                    Task.Run<Result<'T, 'Error>>(
                                        (fun () ->
                                            let mutable sm = sm // host local mutable copy of contents of state machine on this thread pool thread
                                            sm.Data.CancellationToken <- ct

                                            sm.Data.MethodBuilder <-
                                                AsyncTaskMethodBuilder<Result<'T, 'Error>>
                                                    .Create()

                                            sm.Data.MethodBuilder.Start(&sm)
                                            sm.Data.MethodBuilder.Task
                                        ),
                                        ct
                                    )
                    ))

            else
                BackgroundCancellableTaskResultBuilder.RunDynamic(code)

    /// Contains the cancellableTask computation expression builder.
    [<AutoOpen>]
    module CancellableTaskResultBuilder =

        /// <summary>
        /// Builds a cancellableTask using computation expression syntax.
        /// </summary>
        let cancellableTaskValidation = CancellableTaskValidationBuilder()

        /// <summary>
        /// Builds a cancellableTask using computation expression syntax which switches to execute on a background thread if not already doing so.
        /// </summary>
        let backgroundCancellableTaskResult = BackgroundCancellableTaskResultBuilder()


/// <summary>
/// A set of extension methods making it possible to bind against <see cref='T:IcedTasks.CancellableTasks.CancellableTask`1'/> in async computations.
/// </summary>
[<AutoOpen>]
module AsyncExtensions =
    open IcedTasks

    type AsyncExBuilder with

        member inline this.Source([<InlineIfLambda>] t: CancellableTask<'T>) : Async<'T> =
            AsyncEx.AwaitCancellableTask t

        member inline this.Source([<InlineIfLambda>] t: CancellableTask) : Async<unit> =
            AsyncEx.AwaitCancellableTask t

    type Microsoft.FSharp.Control.AsyncBuilder with

        member inline this.Bind
            (
                [<InlineIfLambda>] t: CancellableTask<'T>,
                [<InlineIfLambda>] binder: ('T -> Async<'U>)
            ) : Async<'U> =
            this.Bind(Async.AwaitCancellableTask t, binder)

        member inline this.ReturnFrom([<InlineIfLambda>] t: CancellableTask<'T>) : Async<'T> =
            this.ReturnFrom(Async.AwaitCancellableTask t)

        member inline this.Bind
            (
                [<InlineIfLambda>] t: CancellableTask,
                [<InlineIfLambda>] binder: (unit -> Async<'U>)
            ) : Async<'U> =
            this.Bind(Async.AwaitCancellableTask t, binder)

        member inline this.ReturnFrom([<InlineIfLambda>] t: CancellableTask) : Async<unit> =
            this.ReturnFrom(Async.AwaitCancellableTask t)


    type Microsoft.FSharp.Control.Async with

        static member inline AwaitCancellableTaskValidation
            ([<InlineIfLambda>] t: CancellableTaskValidation<'T, 'Error>)
            =
            async {
                let! ct = Async.CancellationToken

                return!
                    t ct
                    |> Async.AwaitTask
            }

        static member inline AsCancellableTaskValidation(computation: Async<'T>) =
            fun ct -> Async.StartImmediateAsTask(computation, cancellationToken = ct)


    type FsToolkit.ErrorHandling.AsyncValidationCE.AsyncValidationBuilder with

        member inline this.Source
            ([<InlineIfLambda>] t: CancellableTaskValidation<'T, 'Error>)
            : Async<_> =
            Async.AwaitCancellableTaskValidation t

// There is explicitly no Binds for `CancellableTasks` in `Microsoft.FSharp.Control.TaskBuilderBase`.
// You need to explicitly pass in a `CancellationToken`to start it, you can use `CancellationToken.None`.
// Reason is I don't want people to assume cancellation is happening without the caller being explicit about where the CancellationToken came from.
// Similar reasoning for `IcedTasks.ColdTasks.ColdTaskBuilderBase`.

/// Contains a set of standard functional helper function

[<RequireQualifiedAccess>]
module CancellableTaskValidation =
    open System.Threading.Tasks
    open System.Threading
    open IcedTasks

    /// <summary>Gets the default cancellation token for executing computations.</summary>
    ///
    /// <returns>The default CancellationToken.</returns>
    ///
    /// <category index="3">Cancellation and Exceptions</category>
    ///
    /// <example id="default-cancellation-token-1">
    /// <code lang="F#">
    /// use tokenSource = new CancellationTokenSource()
    /// let primes = [ 2; 3; 5; 7; 11 ]
    /// for i in primes do
    ///     let computation =
    ///         cancellableTask {
    ///             let! cancellationToken = CancellableTask.getCancellationToken()
    ///             do! Task.Delay(i * 1000, cancellationToken)
    ///             printfn $"{i}"
    ///         }
    ///     computation tokenSource.Token |> ignore
    /// Thread.Sleep(6000)
    /// tokenSource.Cancel()
    /// printfn "Tasks Finished"
    /// </code>
    /// This will print "2" 2 seconds from start, "3" 3 seconds from start, "5" 5 seconds from start, cease computation and then
    /// followed by "Tasks Finished".
    /// </example>
    let inline getCancellationToken () =
        fun (ct: CancellationToken) -> ValueTask<CancellationToken> ct

    /// <summary>Lifts an item to a CancellableTask.</summary>
    /// <param name="item">The item to be the result of the CancellableTask.</param>
    /// <returns>A CancellableTask with the item as the result.</returns>
    let inline singleton (item: 'item) : CancellableTaskValidation<'item, 'Error> =
        fun _ -> Task.FromResult(Ok item)

    /// <summary>Allows chaining of CancellableTasks.</summary>
    /// <param name="binder">The continuation.</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the binder.</returns>
    let inline bind
        ([<InlineIfLambda>] binder: 'input -> CancellableTaskValidation<'output, 'Error>)
        ([<InlineIfLambda>] cTask: CancellableTaskValidation<'input, 'Error>)
        =
        cancellableTaskValidation {
            let! cResult = cTask
            return! binder cResult
        }


    let inline ofResult (result: Result<'ok, 'error>) : CancellableTaskValidation<'ok, 'error> =
        let x = Result.mapError List.singleton result
        fun _ -> Task.FromResult(x)

    /// <summary>Lifts an item to a CancellableTaskValidation.</summary>
    /// <param name="item">The item to be the ok result of the CancellableTaskValidation.</param>
    /// <returns>A CancellableTaskValidation with the item as the result.</returns>
    let inline ok (item: 'ok) : CancellableTaskValidation<'ok, 'error> =
        fun _ -> Task.FromResult(Ok item)

    /// <summary>Lifts an item to a CancellableTaskValidation.</summary>
    /// <param name="error">The item to be the error result of the CancellableTaskValidation.</param>
    /// <returns>A CancellableTaskValidation with the item as the result.</returns>
    let inline error (error: 'error) : CancellableTaskValidation<'ok, 'error> =
        fun _ -> Task.FromResult(Error [ error ])


    let inline ofChoice (choice: Choice<'ok, 'error>) : CancellableTaskValidation<'ok, 'error> =
        match choice with
        | Choice1Of2 x -> ok x
        | Choice2Of2 x -> error x

    let inline retn (value: 'ok) : CancellableTaskValidation<'ok, 'error> = ok value


    let inline mapError
        ([<InlineIfLambda>] errorMapper: 'errorInput -> 'errorOutput)
        ([<InlineIfLambda>] input: CancellableTaskValidation<'ok, 'errorInput>)
        : CancellableTaskValidation<'ok, 'errorOutput> =
        cancellableTask {
            let! input = input
            return Result.mapError (List.map errorMapper) input
        }

    let inline mapErrors
        ([<InlineIfLambda>] errorMapper: 'errorInput list -> 'errorOutput list)
        ([<InlineIfLambda>] input: CancellableTaskValidation<'ok, 'errorInput>)
        : CancellableTaskValidation<'ok, 'errorOutput> =
        cancellableTask {
            let! input = input
            return Result.mapError errorMapper input
        }

    /// <summary>Allows chaining of CancellableTaskValidation.</summary>
    /// <param name="mapper">The continuation.</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the mapper wrapped in a CancellableTaskValidation.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'input -> 'output)
        ([<InlineIfLambda>] cTask: CancellableTaskValidation<'input, 'error>)
        : CancellableTaskValidation<'output, 'error> =
        cancellableTask {
            let! cResult = cTask
            return Result.map mapper cResult
        }

    /// <summary>Allows chaining of CancellableTaskValidation.</summary>
    /// <param name="mapper">The continuation.</param>
    /// <param name="cTask1">The 1st value.</param>
    /// <param name="cTask2">The 2nd value.</param>
    /// <returns>The result of the mapper wrapped in a CancellableTaskValidation.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'input1 -> 'input2 -> 'output)
        ([<InlineIfLambda>] cTask1: CancellableTaskValidation<'input1, 'error>)
        ([<InlineIfLambda>] cTask2: CancellableTaskValidation<'input2, 'error>)
        : CancellableTaskValidation<'output, 'error> =
        cancellableTask {
            let! cResult1 = cTask1
            let! cResult2 = cTask2

            return
                match cResult1, cResult2 with
                | Ok x, Ok y -> Ok(mapper x y)
                | Ok _, Error errs -> Error errs
                | Error errs, Ok _ -> Error errs
                | Error errs1, Error errs2 ->
                    Error(
                        errs1
                        @ errs2
                    )
        }

    /// <summary>Allows chaining of CancellableTaskValidation.</summary>
    /// <param name="mapper">The continuation.</param>
    /// <param name="cTask1">The 1st value.</param>
    /// <param name="cTask2">The 2nd value.</param>
    /// <param name="cTask3">The 2nd value.</param>
    /// <returns>The result of the mapper wrapped in a CancellableTaskValidation.</returns>
    let inline map3
        ([<InlineIfLambda>] mapper: 'input1 -> 'input2 -> 'input3 -> 'output)
        ([<InlineIfLambda>] cTask1: CancellableTaskValidation<'input1, 'error>)
        ([<InlineIfLambda>] cTask2: CancellableTaskValidation<'input2, 'error>)
        ([<InlineIfLambda>] cTask3: CancellableTaskValidation<'input3, 'error>)
        : CancellableTaskValidation<'output, 'error> =
        cancellableTask {
            let! cResult1 = cTask1
            let! cResult2 = cTask2
            let! cResult3 = cTask3

            return
                match cResult1, cResult2, cResult3 with
                | Ok x, Ok y, Ok z -> Ok(mapper x y z)
                | Error errs, Ok _, Ok _ -> Error errs
                | Ok _, Error errs, Ok _ -> Error errs
                | Ok _, Ok _, Error errs -> Error errs
                | Error errs1, Error errs2, Ok _ ->
                    Error(
                        errs1
                        @ errs2
                    )
                | Ok _, Error errs1, Error errs2 ->
                    Error(
                        errs1
                        @ errs2
                    )
                | Error errs1, Ok _, Error errs2 ->
                    Error(
                        errs1
                        @ errs2
                    )
                | Error errs1, Error errs2, Error errs3 ->
                    Error(
                        errs1
                        @ errs2
                        @ errs3
                    )
        }

    /// <summary>Allows chaining of CancellableTaskValidation.</summary>
    /// <param name="applicable">A function wrapped in a CancellableTaskValidation</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the applicable.</returns>
    let inline apply
        ([<InlineIfLambda>] applicable: CancellableTaskValidation<'input -> 'output, 'error>)
        ([<InlineIfLambda>] cTask: CancellableTaskValidation<'input, 'error>)
        : CancellableTaskValidation<'output, 'error> =
        cancellableTask {
            let! applier = applicable
            let! cResult = cTask

            return
                match applier, cResult with
                | Ok f, Ok x -> Ok(f x)
                | Error errs, Ok _
                | Ok _, Error errs -> Error errs
                | Error errs1, Error errs2 ->
                    Error(
                        errs1
                        @ errs2
                    )
        }

    let inline orElse
        ([<InlineIfLambda>] ifError: CancellableTaskValidation<'input, 'errorOutput>)
        ([<InlineIfLambda>] cTask: CancellableTaskValidation<'input, 'errorInput>)
        : CancellableTaskValidation<'input, 'errorOutput> =
        cancellableTask {
            let! result = cTask

            return!
                result
                |> Result.either ok (fun _ -> ifError)
        }

    let inline orElseWith
        ([<InlineIfLambda>] ifErrorFunc:
            'errorInput list -> CancellableTaskValidation<'input, 'errorOutput>)
        ([<InlineIfLambda>] cTask: CancellableTaskValidation<'input, 'errorInput>)
        : CancellableTaskValidation<'input, 'errorOutput> =
        cancellableTask {
            let! result = cTask

            return!
                match result with
                | Ok x -> ok x
                | Error err -> ifErrorFunc err
        }

    /// <summary>Takes two CancellableTaskValidation, starts them serially in order of left to right, and returns a tuple of the pair.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A tuple of the parameters passed in</returns>
    let inline zip
        ([<InlineIfLambda>] left: CancellableTaskValidation<'left, 'error>)
        ([<InlineIfLambda>] right: CancellableTaskValidation<'right, 'error>)
        : CancellableTaskValidation<('left * 'right), 'error> =
        cancellableTask {
            let! r1 = left
            let! r2 = right

            return Validation.zip r1 r2
        }

    /// <summary>Takes two CancellableTaskValidation, starts them concurrently, and returns a tuple of the pair.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A tuple of the parameters passed in.</returns>
    let inline parallelZip
        ([<InlineIfLambda>] left: CancellableTaskValidation<'left, 'error>)
        ([<InlineIfLambda>] right: CancellableTaskValidation<'right, 'error>)
        : CancellableTaskValidation<('left * 'right), 'error> =
        cancellableTask {
            let! ct = CancellableTask.getCancellationToken ()
            let left = left ct
            let right = right ct
            let! r1 = left
            let! r2 = right

            return Validation.zip r1 r2

        }


[<AutoOpen>]
module CancellableTaskResultBuilderPriority1 =
    open System.Threading.Tasks

    type CancellableTaskValidationBuilder with

        member inline this.Source(result: Result<'T, 'Error>) =
            this.Source(ValueTask<_>(Validation.ofResult result))


        member inline this.Source(choice: Choice<'T, 'Error>) =
            this.Source(ValueTask<_>(Validation.ofChoice choice))


[<AutoOpen>]
module CancellableTaskResultBuilderPriority2 =
    open System.Threading.Tasks

    type CancellableTaskValidationBuilder with

        member inline this.Source(result: Validation<'T, 'Error>) =
            this.Source(ValueTask<_>(result))

open IcedTasks
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core.CompilerServices


[<AutoOpen>]
module CTVMergeSourcesExtensionsCT1CT2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                [<InlineIfLambda>] left: CancellationToken -> 'Awaiter1,
                [<InlineIfLambda>] right: CancellationToken -> 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let left = left ct
                            let right = right ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip (Ok leftR) (Ok rightR))
                                    )
                            ))
                    )
                )
            )

[<AutoOpen>]
module CTVMergeSourcesExtensionsCV1CT2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                [<InlineIfLambda>] left: CancellationToken -> 'Awaiter1,
                [<InlineIfLambda>] right: CancellationToken -> 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let left = left ct
                            let right = right ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip leftR (Ok rightR))
                                    )
                            ))
                    )
                )
            )


[<AutoOpen>]
module CTVMergeSourcesExtensionsCT1CV2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                [<InlineIfLambda>] left: CancellationToken -> 'Awaiter1,
                [<InlineIfLambda>] right: CancellationToken -> 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let left = left ct
                            let right = right ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip (Ok leftR) rightR)
                                    )
                            ))
                    )
                )
            )


[<AutoOpen>]
module CTVMergeSourcesExtensionsCV1CV2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                [<InlineIfLambda>] left: CancellationToken -> 'Awaiter1,
                [<InlineIfLambda>] right: CancellationToken -> 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let left = left ct
                            let right = right ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip leftR rightR)
                                    )
                            ))
                    )
                )
            )


[<AutoOpen>]
module CTVMergeSourcesExtensionsCT1T2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                [<InlineIfLambda>] left: CancellationToken -> 'Awaiter1,
                right: 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let left = left ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip (Ok leftR) (Ok rightR))
                                    )
                            ))
                    )
                )
            )

[<AutoOpen>]
module CTVMergeSourcesExtensionsCV1T2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                [<InlineIfLambda>] left: CancellationToken -> 'Awaiter1,
                right: 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let left = left ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip leftR (Ok rightR))
                                    )
                            ))
                    )
                )
            )


[<AutoOpen>]
module CTVMergeSourcesExtensionsCT1TV2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                [<InlineIfLambda>] left: CancellationToken -> 'Awaiter1,
                right: 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let left = left ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip (Ok leftR) rightR)
                                    )
                            ))
                    )
                )
            )


[<AutoOpen>]
module CTVMergeSourcesExtensionsCV1TV2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                [<InlineIfLambda>] left: CancellationToken -> 'Awaiter1,
                right: 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let left = left ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip leftR (rightR))
                                    )
                            ))
                    )
                )
            )

[<AutoOpen>]
module CTVMergeSourcesExtensionsT1CT2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                left: 'Awaiter1,
                [<InlineIfLambda>] right: CancellationToken -> 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let right = right ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip (Ok leftR) (Ok rightR))
                                    )
                            ))
                    )
                )
            )

[<AutoOpen>]
module CTVMergeSourcesExtensionsTV1CT2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                left: 'Awaiter1,
                [<InlineIfLambda>] right: CancellationToken -> 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let right = right ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip leftR (Ok rightR))
                                    )
                            ))
                    )
                )
            )


[<AutoOpen>]
module CTVMergeSourcesExtensionsT1CV2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                left: 'Awaiter1,
                [<InlineIfLambda>] right: CancellationToken -> 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let right = right ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip (Ok leftR) rightR)
                                    )
                            ))
                    )
                )
            )

[<AutoOpen>]
module CTVMergeSourcesExtensionsTV1CV2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources
            (
                left: 'Awaiter1,
                [<InlineIfLambda>] right: CancellationToken -> 'Awaiter2
            ) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            let right = right ct

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip leftR rightR)
                                    )
                            ))
                    )
                )
            )


[<AutoOpen>]
module CTVMergeSourcesExtensionsT1T2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources(left: 'Awaiter1, right: 'Awaiter2) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip (Ok leftR) (Ok rightR))
                                    )
                            ))
                    )
                )
            )

[<AutoOpen>]
module CTVMergeSourcesExtensionsTV1T2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources(left: 'Awaiter1, right: 'Awaiter2) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip leftR (Ok rightR))
                                    )
                            ))
                    )
                )
            )


[<AutoOpen>]
module CTVMergeSourcesExtensionsT1TV2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources(left: 'Awaiter1, right: 'Awaiter2) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->
                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip (Ok leftR) rightR)
                                    )
                            ))
                    )
                )
            )

[<AutoOpen>]
module CTVMergeSourcesExtensionsTV1TV2 =

    type CancellableTaskValidationBuilder with

        [<NoEagerConstraintApplication>]
        member inline this.MergeSources(left: 'Awaiter1, right: 'Awaiter2) =
            this.Source(
                cancellableTask.Run(
                    cancellableTask.Bind(
                        (fun ct -> cancellableTask.Source(ValueTask<_> ct)),
                        fun ct ->

                            (cancellableTask.Bind(
                                left,
                                fun leftR ->
                                    cancellableTask.BindReturn(
                                        right,
                                        (fun rightR -> Validation.zip leftR rightR)
                                    )
                            ))
                    )
                )
            )
