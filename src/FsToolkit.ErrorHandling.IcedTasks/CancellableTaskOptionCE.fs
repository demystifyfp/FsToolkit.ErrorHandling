namespace FsToolkit.ErrorHandling

/// Contains methods to build CancellableTasks using the F# computation expression syntax
[<AutoOpen>]
module CancellableTaskOptionCE =

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
    type CancellableTaskOptionBuilder() =

        inherit CancellableTaskOptionBuilderBase()

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
            (code: CancellableTaskOptionBuilderBaseCode<'T, 'T>)
            : CancellableTaskOption<'T> =

            let mutable sm = CancellableTaskOptionBuilderBaseStateMachine<'T>()

            let initialResumptionFunc =
                CancellableTaskOptionBuilderBaseResumptionFunc<'T>(fun sm -> code.Invoke(&sm))

            let resumptionInfo =
                { new CancellableTaskOptionBuilderBaseResumptionDynamicInfo<'T>(initialResumptionFunc) with
                    member info.MoveNext(sm) =
                        let mutable savedExn = null

                        try
                            sm.ResumptionDynamicInfo.ResumptionData <- null
                            let step = info.ResumptionFunc.Invoke(&sm)

                            if step then
                                sm.Data.SetResult()
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

            fun ct ->
                if ct.IsCancellationRequested then
                    Task.FromCanceled<_>(ct)
                else
                    sm.Data.CancellationToken <- ct
                    sm.ResumptionDynamicInfo <- resumptionInfo
                    sm.Data.MethodBuilder <- AsyncTaskMethodBuilder<Option<'T>>.Create()
                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task


        /// Hosts the task code in a state machine and starts the task.
        member inline _.Run
            (code: CancellableTaskOptionBuilderBaseCode<'T, 'T>)
            : CancellableTaskOption<'T> =
            if __useResumableCode then
                __stateMachine<
                    CancellableTaskOptionBuilderBaseStateMachineData<'T>,
                    CancellableTaskOption<'T>
                 >
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: ExceptionNull = null

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if __stack_code_fin then
                                sm.Data.SetResult()
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

                        fun ct ->
                            if ct.IsCancellationRequested then
                                Task.FromCanceled<_>(ct)
                            else
                                let mutable sm = sm
                                sm.Data.CancellationToken <- ct

                                sm.Data.MethodBuilder <-
                                    AsyncTaskMethodBuilder<Option<'T>>.Create()

                                sm.Data.MethodBuilder.Start(&sm)
                                sm.Data.MethodBuilder.Task
                    ))
            else
                CancellableTaskOptionBuilder.RunDynamic(code)

        /// Specify a Source of CancellationToken -> Task<_> on the real type to allow type inference to work
        member inline _.Source
            (x: CancellationToken -> Task<_>)
            : CancellationToken -> Awaiter<TaskAwaiter<_>, _> =
            fun ct -> Awaitable.GetTaskAwaiter(x ct)

    /// Contains methods to build CancellableTasks using the F# computation expression syntax
    type BackgroundCancellableTaskOptionBuilder() =

        inherit CancellableTaskOptionBuilderBase()

        /// <summary>
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        /// </summary>
        static member inline RunDynamic
            (code: CancellableTaskOptionBuilderBaseCode<'T, 'T>)
            : CancellableTaskOption<'T> =
            // backgroundTask { .. } escapes to a background thread where necessary
            // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
            if
                isNull SynchronizationContext.Current
                && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
            then
                CancellableTaskOptionBuilder.RunDynamic(code)
            else
                fun ct ->
                    Task.Run<Option<'T>>(
                        (fun () -> CancellableTaskOptionBuilder.RunDynamic code ct),
                        ct
                    )

        /// <summary>
        /// Hosts the task code in a state machine and starts the task, executing in the ThreadPool using Task.Run
        /// </summary>
        member inline _.Run
            (code: CancellableTaskOptionBuilderBaseCode<'T, 'T>)
            : CancellableTaskOption<'T> =
            if __useResumableCode then
                __stateMachine<
                    CancellableTaskOptionBuilderBaseStateMachineData<'T>,
                    CancellableTaskOption<'T>
                 >
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: ExceptionNull = null

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if __stack_code_fin then
                                sm.Data.MethodBuilder.SetResult(sm.Data.Result.Value)
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
                    (AfterCode<_, CancellableTaskOption<'T>>(fun sm ->
                        // backgroundTask { .. } escapes to a background thread where necessary
                        // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
                        if
                            isNull SynchronizationContext.Current
                            && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
                        then
                            let mutable sm = sm

                            fun ct ->
                                if ct.IsCancellationRequested then
                                    Task.FromCanceled<_>(ct)
                                else
                                    sm.Data.CancellationToken <- ct

                                    sm.Data.MethodBuilder <-
                                        AsyncTaskMethodBuilder<Option<'T>>.Create()

                                    sm.Data.MethodBuilder.Start(&sm)
                                    sm.Data.MethodBuilder.Task
                        else
                            let sm = sm // copy contents of state machine so we can capture it

                            fun (ct) ->
                                if ct.IsCancellationRequested then
                                    Task.FromCanceled<_>(ct)
                                else
                                    Task.Run<Option<'T>>(
                                        (fun () ->
                                            let mutable sm = sm // host local mutable copy of contents of state machine on this thread pool thread
                                            sm.Data.CancellationToken <- ct

                                            sm.Data.MethodBuilder <-
                                                AsyncTaskMethodBuilder<Option<'T>>.Create()

                                            sm.Data.MethodBuilder.Start(&sm)
                                            sm.Data.MethodBuilder.Task
                                        ),
                                        ct
                                    )
                    ))

            else
                BackgroundCancellableTaskOptionBuilder.RunDynamic(code)

    /// Contains the cancellableTask computation expression builder.
    [<AutoOpen>]
    module CancellableTaskOptionBuilder =

        /// <summary>
        /// Builds a cancellableTask using computation expression syntax.
        /// </summary>
        let cancellableTaskOption = CancellableTaskOptionBuilder()

        /// <summary>
        /// Builds a cancellableTask using computation expression syntax which switches to execute on a background thread if not already doing so.
        /// </summary>
        let backgroundCancellableTaskOption = BackgroundCancellableTaskOptionBuilder()


[<RequireQualifiedAccess>]
module CancellableTaskOption =
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
    /// <param name="x">The item to be the result of the CancellableTask.</param>
    /// <returns>A CancellableTask with the item as the result.</returns>
    let inline some x = cancellableTask { return Some x }


    /// <summary>Allows chaining of CancellableTasks.</summary>
    /// <param name="binder">The continuation.</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the binder.</returns>
    let inline bind
        ([<InlineIfLambda>] binder: 'input -> CancellableTaskOption<'output>)
        ([<InlineIfLambda>] cTask: CancellableTaskOption<'input>)
        =
        cancellableTaskOption {
            let! cResult = cTask
            return! binder cResult
        }

    /// <summary>Allows chaining of CancellableTasks.</summary>
    /// <param name="mapper">The continuation.</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the mapper wrapped in a CancellableTasks.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'input -> 'output)
        ([<InlineIfLambda>] cTask: CancellableTaskOption<'input>)
        =
        cancellableTaskOption {
            let! cResult = cTask
            return mapper cResult
        }

    /// <summary>Allows chaining of CancellableTasks.</summary>
    /// <param name="applicable">A function wrapped in a CancellableTasks</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the applicable.</returns>
    let inline apply
        ([<InlineIfLambda>] applicable: CancellableTaskOption<'input -> 'output>)
        ([<InlineIfLambda>] cTask: CancellableTaskOption<'input>)
        =
        cancellableTaskOption {
            let! (applier: 'input -> 'output) = applicable
            let! (cResult: 'input) = cTask
            return applier cResult
        }

    /// <summary>Takes two CancellableTasks, starts them serially in order of left to right, and returns a tuple of the pair.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A tuple of the parameters passed in</returns>
    let inline zip
        ([<InlineIfLambda>] left: CancellableTaskOption<'left>)
        ([<InlineIfLambda>] right: CancellableTaskOption<'right>)
        =
        cancellableTaskOption {
            let! r1 = left
            let! r2 = right
            return r1, r2
        }

    /// <summary>Takes two CancellableTask, starts them concurrently, and returns a tuple of the pair.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A tuple of the parameters passed in.</returns>
    let inline parallelZip
        ([<InlineIfLambda>] left: CancellableTaskOption<'left>)
        ([<InlineIfLambda>] right: CancellableTaskOption<'right>)
        =
        cancellableTask {
            let! ct = getCancellationToken ()
            let r1 = left ct
            let r2 = right ct
            let! r1 = r1
            let! r2 = r2
            return Option.zip r1 r2
        }

    /// <summary>
    /// Returns result of running <paramref name="onSome"/> if it is <c>Some</c>, otherwise returns result of running <paramref name="onNone"/>
    /// </summary>
    /// <param name="onSome">The function to run if <paramref name="input"/> is <c>Some</c></param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>None</c></param>
    /// <param name="input">The input option.</param>
    /// <returns>
    /// The result of running <paramref name="onSome"/> if the input is <c>Some</c>, else returns result of running <paramref name="onNone"/>.
    /// </returns>
    let inline either
        ([<InlineIfLambda>] onSome: 'input -> CancellableTask<'output>)
        ([<InlineIfLambda>] onNone: unit -> CancellableTask<'output>)
        (input: CancellableTask<'input option>)
        : CancellableTask<'output> =
        input
        |> CancellableTask.bind (
            function
            | Some v -> onSome v
            | None -> onNone ()
        )

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="value">The specified default value.</param>
    /// <param name="cancellableTaskOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the default value.
    /// </returns>
    let inline defaultValue
        (value: 'value)
        (cancellableTaskOption: CancellableTask<'value option>)
        =
        cancellableTaskOption
        |> CancellableTask.map (Option.defaultValue value)

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise evaluates <paramref name="defThunk"/> and returns the result.
    /// </summary>
    /// <param name="defThunk">A thunk that provides a default value when evaluated.</param>
    /// <param name="cancellableTaskOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the result of evaluating <paramref name="defThunk"/>.
    /// </returns>
    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (cancellableTaskOption: CancellableTask<'value option>)
        : CancellableTask<'value> =
        cancellableTaskOption
        |> CancellableTask.map (Option.defaultWith defThunk)
