namespace FsToolkit.ErrorHandling

/// Contains methods to build CancellableTasks using the F# computation expression syntax
[<AutoOpen>]
module CancellableTaskResultCE =

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


    /// Contains methods to build CancellableTasks using the F# computation expression syntax
    type CancellableTaskResultBuilder() =

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
            (code: CancellableTaskResultBuilderBaseCode<'T, 'T, 'Error, _>)
            : CancellableTaskResult<'T, 'Error> =

            let mutable sm = CancellableTaskResultBuilderBaseStateMachine<'T, 'Error, _>()

            let initialResumptionFunc =
                CancellableTaskResultBuilderBaseResumptionFunc<'T, 'Error, _>(fun sm ->
                    code.Invoke(&sm)
                )

            let resumptionInfo =
                { new CancellableTaskResultBuilderBaseResumptionDynamicInfo<'T, 'Error, _>(initialResumptionFunc) with
                    member info.MoveNext(sm) =
                        let mutable savedExn = null

                        try
                            sm.ResumptionDynamicInfo.ResumptionData <- null
                            let step = info.ResumptionFunc.Invoke(&sm)

                            if step then
                                MethodBuilder.SetResult(&sm.Data.MethodBuilder, sm.Data.Result)
                            else
                                let mutable awaiter =
                                    sm.ResumptionDynamicInfo.ResumptionData
                                    :?> ICriticalNotifyCompletion

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
                    sm.Data.MethodBuilder <- AsyncTaskMethodBuilder<Result<'T, 'Error>>.Create()
                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task


        /// Hosts the task code in a state machine and starts the task.
        member inline _.Run
            (code: CancellableTaskResultBuilderBaseCode<'T, 'T, 'Error, _>)
            : CancellableTaskResult<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<
                    CancellableTaskResultBuilderBaseStateMachineData<'T, 'Error, _>,
                    CancellableTaskResult<'T, 'Error>
                 >
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: ExceptionNull = null

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
                    (AfterCode<_, _>(fun sm ->
                        let sm = sm

                        fun (ct) ->
                            if ct.IsCancellationRequested then
                                Task.FromCanceled<_>(ct)
                            else
                                let mutable sm = sm
                                sm.Data.CancellationToken <- ct

                                sm.Data.MethodBuilder <-
                                    AsyncTaskMethodBuilder<Result<'T, 'Error>>.Create()

                                sm.Data.MethodBuilder.Start(&sm)
                                sm.Data.MethodBuilder.Task
                    ))
            else
                CancellableTaskResultBuilder.RunDynamic(code)

        /// Specify a Source of CancellationToken -> Task<_> on the real type to allow type inference to work
        member inline _.Source
            (x: CancellationToken -> Task<_>)
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
                __stateMachine<
                    CancellableTaskResultBuilderBaseStateMachineData<'T, 'Error, _>,
                    CancellableTaskResult<'T, 'Error>
                 >
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: ExceptionNull = null

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
        let cancellableTaskResult = CancellableTaskResultBuilder()

        /// <summary>
        /// Builds a cancellableTask using computation expression syntax which switches to execute on a background thread if not already doing so.
        /// </summary>
        let backgroundCancellableTaskResult = BackgroundCancellableTaskResultBuilder()


[<RequireQualifiedAccess>]
module CancellableTaskResult =
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
    let inline singleton (item: 'item) : CancellableTaskResult<'item, 'Error> =
        fun _ -> Task.FromResult(Ok item)


    /// <summary>Allows chaining of CancellableTasks.</summary>
    /// <param name="binder">The continuation.</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the binder.</returns>
    let inline bind
        ([<InlineIfLambda>] binder: 'input -> CancellableTaskResult<'output, 'Error>)
        ([<InlineIfLambda>] cTask: CancellableTaskResult<'input, 'Error>)
        =
        cancellableTaskResult {
            let! cResult = cTask
            return! binder cResult
        }

    /// <summary>Allows chaining of CancellableTasks.</summary>
    /// <param name="mapper">The continuation.</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the mapper wrapped in a CancellableTasks.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'input -> 'output)
        ([<InlineIfLambda>] cTask: CancellableTaskResult<'input, 'Error>)
        =
        cancellableTaskResult {
            let! cResult = cTask
            return mapper cResult
        }

    /// <summary>Allows chaining of CancellableTasks.</summary>
    /// <param name="applicable">A function wrapped in a CancellableTasks</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the applicable.</returns>
    let inline apply
        ([<InlineIfLambda>] applicable: CancellableTaskResult<'input -> 'output, 'Error>)
        ([<InlineIfLambda>] cTask: CancellableTaskResult<'input, 'Error>)
        =
        cancellableTaskResult {
            let! (applier: 'input -> 'output) = applicable
            let! (cResult: 'input) = cTask
            return applier cResult
        }

    /// <summary>Takes two CancellableTasks, starts them serially in order of left to right, and returns a tuple of the pair.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A tuple of the parameters passed in</returns>
    let inline zip
        ([<InlineIfLambda>] left: CancellableTaskResult<'left, 'Error>)
        ([<InlineIfLambda>] right: CancellableTaskResult<'right, 'Error>)
        =
        cancellableTaskResult {
            let! r1 = left
            let! r2 = right
            return r1, r2
        }

    /// <summary>Takes two CancellableTask, starts them concurrently, and returns a tuple of the pair.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A tuple of the parameters passed in.</returns>
    let inline parallelZip
        ([<InlineIfLambda>] left: CancellableTaskResult<'left, 'Error>)
        ([<InlineIfLambda>] right: CancellableTaskResult<'right, 'Error>)
        =
        cancellableTask {
            let! ct = getCancellationToken ()
            let r1 = left ct
            let r2 = right ct
            let! r1 = r1
            let! r2 = r2
            return Result.zip r1 r2
        }
