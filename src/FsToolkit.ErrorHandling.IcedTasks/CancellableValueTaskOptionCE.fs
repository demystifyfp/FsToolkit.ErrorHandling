namespace FsToolkit.ErrorHandling


/// Contains methods to build CancellableValueTasks using the F# computation expression syntax
[<AutoOpen>]
module CancellableValueTaskOptionCE =
    open System
    open System.Runtime.CompilerServices
    open System.Threading
    open System.Threading.Tasks
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.CompilerServices
    open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Collections
    open System.Collections.Generic
    open IcedTasks

    /// CancellationToken -> ValueTask<Option<'T>>
    type CancellableValueTaskOption<'T> = CancellableValueTask<Option<'T>>

    /// The extra data stored in ResumableStateMachine for tasks
    [<Struct; NoComparison; NoEquality>]
    type CancellableValueTaskOptionBuilderBaseStateMachineData<'T> =
        [<DefaultValue(false)>]
        val mutable CancellationToken: CancellationToken

        [<DefaultValue(false)>]
        val mutable Result: 'T option voption

        [<DefaultValue(false)>]
        val mutable MethodBuilder: AsyncValueTaskOptionMethodBuilder<'T>

        member inline this.IsResultNone =
            match this.Result with
            | ValueNone -> false
            | ValueSome(None) -> true
            | ValueSome _ -> false

        member inline this.SetResult() =
            match this.Result with
            | ValueNone -> this.MethodBuilder.SetResult None
            | ValueSome x -> this.MethodBuilder.SetResult x

        /// <summary>Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</summary>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        member inline this.ThrowIfCancellationRequested() =
            this.CancellationToken.ThrowIfCancellationRequested()

    and AsyncValueTaskOptionMethodBuilder<'TOverall> = AsyncValueTaskMethodBuilder<'TOverall option>

    /// This is used by the compiler as a template for creating state machine structs
    and CancellableValueTaskOptionBuilderBaseStateMachine<'TOverall> =
        ResumableStateMachine<CancellableValueTaskOptionBuilderBaseStateMachineData<'TOverall>>

    /// Represents the runtime continuation of a cancellableValueTasks state machine created dynamically
    and CancellableValueTaskOptionBuilderBaseResumptionFunc<'TOverall> =
        ResumptionFunc<CancellableValueTaskOptionBuilderBaseStateMachineData<'TOverall>>

    /// Represents the runtime continuation of a cancellableValueTasks state machine created dynamically
    and CancellableValueTaskOptionBuilderBaseResumptionDynamicInfo<'TOverall> =
        ResumptionDynamicInfo<CancellableValueTaskOptionBuilderBaseStateMachineData<'TOverall>>

    /// A special compiler-recognised delegate type for specifying blocks of cancellableValueTasks code with access to the state machine
    and CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T> =
        ResumableCode<CancellableValueTaskOptionBuilderBaseStateMachineData<'TOverall>, 'T>

    /// <summary>
    /// Contains methods to build TaskLikes using the F# computation expression syntax
    /// </summary>
    type CancellableValueTaskOptionBuilderBase() =
        /// <summary>Creates a CancellableValueTasks that runs generator</summary>
        /// <param name="generator">The function to run</param>
        /// <returns>A CancellableValueTasks that runs generator</returns>
        member inline _.Delay
            ([<InlineIfLambda>] generator:
                unit -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>)
            : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T> =
            ResumableCode.Delay(fun () -> generator ())

        /// <summary>Creates A CancellableValueTasks that just returns ().</summary>
        /// <remarks>
        /// The existence of this method permits the use of empty else branches in the
        /// cancellableValueTaskOption { ... } computation expression syntax.
        /// </remarks>
        /// <returns>A CancellableValueTasks that returns ().</returns>
        [<DefaultValue>]
        member inline _.Zero() : CancellableValueTaskOptionBuilderBaseCode<'TOverall, unit> =
            CancellableValueTaskOptionBuilderBaseCode<_, _>(fun sm ->
                sm.Data.Result <- ValueSome(Some Unchecked.defaultof<'TOverall>)
                true
            )

        /// <summary>Creates A Computation that returns the result v.</summary>
        ///
        /// <remarks>A cancellation check is performed when the computation is executed.
        ///
        /// The existence of this method permits the use of return in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="value">The value to return from the computation.</param>
        ///
        /// <returns>A cancellableValueTasks that returns value when executed.</returns>
        member inline _.Return(value: 'T) : CancellableValueTaskOptionBuilderBaseCode<'T, 'T> =
            CancellableValueTaskOptionBuilderBaseCode<'T, 'T>(fun sm ->
                sm.Data.Result <- ValueSome(Some value)
                true
            )

        /// <summary>Creates a CancellableValueTasks that first runs task1
        /// and then runs computation2, returning the result of computation2.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of expression sequencing in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="task1">The first part of the sequenced computation.</param>
        /// <param name="task2">The second part of the sequenced computation.</param>
        ///
        /// <returns>A CancellableValueTasks that runs both of the computations sequentially.</returns>
        member inline _.Combine
            (
                task1: CancellableValueTaskOptionBuilderBaseCode<'TOverall, unit>,
                task2: CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>
            ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T> =
            ResumableCode.Combine(
                task1,
                CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>(fun sm ->
                    if sm.Data.IsResultNone then true else task2.Invoke(&sm)
                )
            )

        /// <summary>Creates A CancellableValueTasks that runs computation repeatedly
        /// until guard() becomes false.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of while in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="guard">The function to determine when to stop executing computation.</param>
        /// <param name="computation">The function to be executed.  Equivalent to the body
        /// of a while expression.</param>
        ///
        /// <returns>A CancellableValueTasks that behaves similarly to a while loop when run.</returns>
        member inline _.While
            (
                guard: unit -> bool,
                computation: CancellableValueTaskOptionBuilderBaseCode<'TOverall, unit>
            ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, unit> =
            let mutable keepGoing = true

            ResumableCode.While(
                (fun () ->
                    keepGoing
                    && guard ()
                ),
                CancellableValueTaskOptionBuilderBaseCode<'TOverall, unit>(fun sm ->
                    if sm.Data.IsResultNone then
                        keepGoing <- false
                        true
                    else
                        computation.Invoke(&sm)
                )
            )

        /// <summary>Creates A CancellableValueTasks that runs computation and returns its result.
        /// If an exception happens then catchHandler(exn) is called and the resulting computation executed instead.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of try/with in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="computation">The input computation.</param>
        /// <param name="catchHandler">The function to run when computation throws an exception.</param>
        ///
        /// <returns>A CancellableValueTasks that executes computation and calls catchHandler if an
        /// exception is thrown.</returns>
        member inline _.TryWith
            (
                computation: CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>,
                catchHandler: exn -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>
            ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T> =
            ResumableCode.TryWith(computation, catchHandler)

        /// <summary>Creates A CancellableValueTasks that runs computation. The action compensation is executed
        /// after computation completes, whether computation exits normally or by an exception. If compensation raises an exception itself
        /// the original exception is discarded and the new exception becomes the overall result of the computation.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of try/finally in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="computation">The input computation.</param>
        /// <param name="compensation">The action to be run after computation completes or raises an
        /// exception (including cancellation).</param>
        ///
        /// <returns>A CancellableValueTasks that executes computation and compensation afterwards or
        /// when an exception is raised.</returns>
        member inline _.TryFinally
            (
                computation: CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>,
                compensation: unit -> unit
            ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T> =
            ResumableCode.TryFinally(
                computation,
                ResumableCode<_, _>(fun _ ->
                    compensation ()
                    true
                )
            )


        /// <summary>
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        /// </summary>
        [<NoEagerConstraintApplication>]
        static member inline BindDynamic
            (
                sm:
                    byref<
                        ResumableStateMachine<
                            CancellableValueTaskOptionBuilderBaseStateMachineData<'TOverall>
                         >
                     >,
                [<InlineIfLambda>] getAwaiter: CancellationToken -> 'Awaiter,
                continuation:
                    'TResult1 -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
            ) : bool =
            sm.Data.ThrowIfCancellationRequested()

            let mutable awaiter = getAwaiter sm.Data.CancellationToken

            let cont =
                CancellableValueTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
                    let result = Awaiter.GetResult awaiter

                    match result with
                    | Some result -> (continuation result).Invoke(&sm)
                    | None ->
                        sm.Data.Result <- ValueSome None
                        true
                )

            // shortcut to continue immediately
            if Awaiter.IsCompleted awaiter then
                cont.Invoke(&sm)
            else
                sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)

                sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                false

        /// <summary>Creates A CancellableValueTask that runs computation, and when
        /// computation generates a result T, runs binder res.</summary>
        ///
        /// <remarks>A cancellation check is performed when the computation is executed.
        ///
        /// The existence of this method permits the use of let! in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="getAwaiterTResult">The computation to provide an unbound result.</param>
        /// <param name="continuation">The function to bind the result of computation.</param>
        ///
        /// <returns>A CancellableValueTask that performs a monadic bind on the result
        /// of computation.</returns>
        [<NoEagerConstraintApplication>]
        member inline _.Bind
            (
                [<InlineIfLambda>] getAwaiterTResult: CancellationToken -> 'Awaiter,
                continuation:
                    'TResult1 -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
            ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2> =

            CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>(fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    sm.Data.ThrowIfCancellationRequested()
                    // Get an awaiter from the Awaiter
                    let mutable awaiter = getAwaiterTResult sm.Data.CancellationToken

                    let mutable __stack_fin = true

                    if not (Awaiter.IsCompleted awaiter) then
                        // This will yield with __stack_yield_fin = false
                        // This will resume with __stack_yield_fin = true
                        let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                        __stack_fin <- __stack_yield_fin

                    if __stack_fin then
                        let result = Awaiter.GetResult awaiter

                        match result with
                        | Some result -> (continuation result).Invoke(&sm)
                        | None ->
                            sm.Data.Result <- ValueSome None
                            true
                    else
                        let mutable awaiter = awaiter :> ICriticalNotifyCompletion

                        MethodBuilder.AwaitUnsafeOnCompleted(&sm.Data.MethodBuilder, &awaiter, &sm)

                        false
                else
                    CancellableValueTaskOptionBuilderBase.BindDynamic(
                        &sm,
                        getAwaiterTResult,
                        continuation
                    )
            //-- RESUMABLE CODE END
            )


        /// <summary>Creates a CancellableValueTask that enumerates the sequence seq
        /// on demand and runs body for each element.</summary>
        ///
        /// <remarks>A cancellation check is performed on each iteration of the loop.
        ///
        /// The existence of this method permits the use of for in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="sequence">The sequence to enumerate.</param>
        /// <param name="body">A function to take an item from the sequence and create
        /// A CancellableValueTask.  Can be seen as the body of the for expression.</param>
        ///
        /// <returns>A CancellableValueTask that will enumerate the sequence and run body
        /// for each element.</returns>
        member inline this.For
            (
                sequence: seq<'T>,
                body: 'T -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, unit>
            ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, unit> =
            ResumableCode.Using(
                sequence.GetEnumerator(),
                // ... and its body is a while loop that advances the enumerator and runs the body on each element.
                (fun e ->
                    this.While(
                        (fun () -> e.MoveNext()),
                        CancellableValueTaskOptionBuilderBaseCode<'TOverall, unit>(fun sm ->
                            (body e.Current).Invoke(&sm)
                        )
                    )
                )
            )

        /// <summary>Creates A CancellableValueTask that runs computation. The action compensation is executed
        /// after computation completes, whether computation exits normally or by an exception. If compensation raises an exception itself
        /// the original exception is discarded and the new exception becomes the overall result of the computation.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of try/finally in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="computation">The input computation.</param>
        /// <param name="compensation">The action to be run after computation completes or raises an
        /// exception.</param>
        ///
        /// <returns>A CancellableValueTask that executes computation and compensation afterward or
        /// when an exception is raised.</returns>
        member inline internal this.TryFinallyAsync
            (
                computation: CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>,
                compensation: unit -> 'Awaitable
            ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T> =
            ResumableCode.TryFinallyAsync(
                computation,
                ResumableCode<_, _>(fun sm ->

                    if __useResumableCode then
                        let mutable __stack_condition_fin = true
                        let mutable awaiter = compensation ()

                        if not (Awaiter.IsCompleted awaiter) then
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_condition_fin <- __stack_yield_fin

                        if __stack_condition_fin then
                            Awaiter.GetResult awaiter
                        else

                            let mutable awaiter = awaiter :> ICriticalNotifyCompletion

                            MethodBuilder.AwaitUnsafeOnCompleted(
                                &sm.Data.MethodBuilder,
                                &awaiter,
                                &sm
                            )

                        __stack_condition_fin
                    else
                        let mutable awaiter = compensation ()

                        let cont =
                            CancellableValueTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
                                Awaiter.GetResult awaiter
                                true
                            )

                        // shortcut to continue immediately
                        if Awaiter.IsCompleted awaiter then
                            cont.Invoke(&sm)
                        else
                            sm.ResumptionDynamicInfo.ResumptionData <-
                                (awaiter :> ICriticalNotifyCompletion)

                            sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                            false
                )
            )

        /// <summary>Creates A CancellableValueTask that runs binder(resource).
        /// The action resource.DisposeAsync() is executed as this computation yields its result
        /// or if the CancellableValueTask exits by an exception or by cancellation.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of use and use! in the
        /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
        ///
        /// <param name="resource">The resource to be used and disposed.</param>
        /// <param name="binder">The function that takes the resource and returns an asynchronous
        /// computation.</param>
        ///
        /// <returns>A CancellableValueTask that binds and eventually disposes resource.</returns>
        ///
        member inline this.Using
            (
                resource: #IAsyncDisposable,
                binder:
                    #IAsyncDisposable -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>
            ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T> =
            this.TryFinallyAsync(
                (fun sm -> (binder resource).Invoke(&sm)),
                (fun () ->
                    if not (isNull (box resource)) then
                        resource.DisposeAsync()
                        |> Awaitable.GetAwaiter
                    else
                        ValueTask()
                        |> Awaitable.GetAwaiter
                )
            )


        member inline internal _.WhileAsync
            ([<InlineIfLambda>] condition, body: CancellableValueTaskOptionBuilderBaseCode<_, unit>)
            : CancellableValueTaskOptionBuilderBaseCode<_, unit> =
            let mutable condition_res = true

            ResumableCode.While(
                (fun () -> condition_res),
                CancellableValueTaskOptionBuilderBaseCode<_, unit>(fun sm ->
                    if __useResumableCode then

                        let mutable __stack_condition_fin = true
                        let mutable awaiter = condition ()

                        if Awaiter.IsCompleted awaiter then

                            __stack_condition_fin <- true

                            condition_res <- Awaiter.GetResult awaiter
                        else

                            // This will yield with __stack_fin = false
                            // This will resume with __stack_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_condition_fin <- __stack_yield_fin

                            if __stack_condition_fin then
                                condition_res <- Awaiter.GetResult awaiter


                        if __stack_condition_fin then

                            if condition_res then body.Invoke(&sm) else true
                        else
                            let mutable awaiter = awaiter :> ICriticalNotifyCompletion

                            MethodBuilder.AwaitUnsafeOnCompleted(
                                &sm.Data.MethodBuilder,
                                &awaiter,
                                &sm
                            )

                            false
                    else

                        let mutable awaiter = condition ()

                        let cont =
                            CancellableValueTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
                                condition_res <- Awaiter.GetResult awaiter
                                if condition_res then body.Invoke(&sm) else true
                            )

                        if Awaiter.IsCompleted awaiter then
                            cont.Invoke(&sm)
                        else
                            sm.ResumptionDynamicInfo.ResumptionData <-
                                (awaiter :> ICriticalNotifyCompletion)

                            sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                            false
                )
            )

        member inline this.For
            (
                source: #IAsyncEnumerable<'T>,
                body: 'T -> CancellableValueTaskOptionBuilderBaseCode<_, unit>
            ) : CancellableValueTaskOptionBuilderBaseCode<_, _> =

            CancellableValueTaskOptionBuilderBaseCode<_, _>(fun sm ->
                this
                    .Using(
                        source.GetAsyncEnumerator sm.Data.CancellationToken,
                        (fun (e: IAsyncEnumerator<'T>) ->
                            this.WhileAsync(
                                (fun () -> Awaitable.GetAwaiter(e.MoveNextAsync())),
                                (fun sm -> (body e.Current).Invoke(&sm))
                            )
                        )

                    )
                    .Invoke(&sm)
            )


    /// <exclude/>
    [<AutoOpen>]
    module LowPriority2 =
        // Low priority extensions
        type CancellableValueTaskOptionBuilderBase with


            /// <summary>
            /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
            /// </summary>
            [<NoEagerConstraintApplication>]
            static member inline BindDynamic
                (
                    sm:
                        byref<
                            ResumableStateMachine<
                                CancellableValueTaskOptionBuilderBaseStateMachineData<'TOverall>
                             >
                         >,
                    [<InlineIfLambda>] getAwaiter: CancellationToken -> 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : bool =
                sm.Data.ThrowIfCancellationRequested()

                let mutable awaiter = getAwaiter sm.Data.CancellationToken

                let cont =
                    CancellableValueTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
                        let result = Awaiter.GetResult awaiter

                        (continuation result).Invoke(&sm)
                    )

                // shortcut to continue immediately
                if Awaiter.IsCompleted awaiter then
                    cont.Invoke(&sm)
                else
                    sm.ResumptionDynamicInfo.ResumptionData <-
                        (awaiter :> ICriticalNotifyCompletion)

                    sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                    false

            /// <summary>Creates A CancellableValueTask that runs computation, and when
            /// computation generates a result T, runs binder res.</summary>
            ///
            /// <remarks>A cancellation check is performed when the computation is executed.
            ///
            /// The existence of this method permits the use of let! in the
            /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
            ///
            /// <param name="getAwaiterT">The computation to provide an unbound result.</param>
            /// <param name="continuation">The function to bind the result of computation.</param>
            ///
            /// <returns>A CancellableValueTask that performs a monadic bind on the result
            /// of computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Bind
                (
                    [<InlineIfLambda>] getAwaiterT: CancellationToken -> 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2> =

                CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>(fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        sm.Data.ThrowIfCancellationRequested()
                        // Get an awaiter from the Awaiter
                        let mutable awaiter = getAwaiterT sm.Data.CancellationToken

                        let mutable __stack_fin = true

                        if not (Awaiter.IsCompleted awaiter) then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result = Awaiter.GetResult awaiter

                            (continuation result).Invoke(&sm)
                        else
                            let mutable awaiter = awaiter :> ICriticalNotifyCompletion

                            MethodBuilder.AwaitUnsafeOnCompleted(
                                &sm.Data.MethodBuilder,
                                &awaiter,
                                &sm
                            )

                            false
                    else
                        CancellableValueTaskOptionBuilderBase.BindDynamic(
                            &sm,
                            getAwaiterT,
                            continuation
                        )
                //-- RESUMABLE CODE END
                )


            /// <summary>Delegates to the input computation.</summary>
            ///
            /// <remarks>The existence of this method permits the use of return! in the
            /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
            ///
            /// <param name="getAwaiterT">The input computation.</param>
            ///
            /// <returns>The input computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline this.ReturnFrom
                ([<InlineIfLambda>] getAwaiterT: CancellationToken -> 'Awaiter)
                =
                this.Bind(
                    getAwaiterT = (fun ct -> getAwaiterT ct),
                    continuation = (fun v -> this.Return v)
                )

            /// <summary>
            /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
            /// </summary>
            [<NoEagerConstraintApplication>]
            static member inline BindDynamic
                (
                    sm:
                        byref<
                            ResumableStateMachine<
                                CancellableValueTaskOptionBuilderBaseStateMachineData<'TOverall>
                             >
                         >,
                    awaiter: 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : bool =
                sm.Data.ThrowIfCancellationRequested()
                let mutable awaiter = awaiter

                let cont =
                    CancellableValueTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
                        let result = Awaiter.GetResult awaiter

                        (continuation result).Invoke(&sm)
                    )

                // shortcut to continue immediately
                if Awaiter.IsCompleted awaiter then
                    cont.Invoke(&sm)
                else
                    sm.ResumptionDynamicInfo.ResumptionData <-
                        (awaiter :> ICriticalNotifyCompletion)

                    sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                    false

            /// <summary>Creates A CancellableValueTask that runs computation, and when
            /// computation generates a result T, runs binder res.</summary>
            ///
            /// <remarks>A cancellation check is performed when the computation is executed.
            ///
            /// The existence of this method permits the use of let! in the
            /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
            ///
            /// <param name="awaiterT">The computation to provide an unbound result.</param>
            /// <param name="continuation">The function to bind the result of computation.</param>
            ///
            /// <returns>A CancellableValueTask that performs a monadic bind on the result
            /// of computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Bind
                (
                    awaiterT: 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2> =

                CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>(fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        sm.Data.ThrowIfCancellationRequested()
                        // Get an awaiter from the Awaiter
                        let mutable awaiter = awaiterT

                        let mutable __stack_fin = true

                        if not (Awaiter.IsCompleted awaiter) then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result = Awaiter.GetResult awaiter

                            (continuation result).Invoke(&sm)
                        else
                            let mutable awaiter = awaiter :> ICriticalNotifyCompletion

                            MethodBuilder.AwaitUnsafeOnCompleted(
                                &sm.Data.MethodBuilder,
                                &awaiter,
                                &sm
                            )

                            false
                    else
                        CancellableValueTaskOptionBuilderBase.BindDynamic(
                            &sm,
                            awaiterT,
                            continuation
                        )
                //-- RESUMABLE CODE END
                )

            /// <summary>Delegates to the input computation.</summary>
            ///
            /// <remarks>The existence of this method permits the use of return! in the
            /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
            ///
            /// <param name="awaiterT">The input computation.</param>
            ///
            /// <returns>The input computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline this.ReturnFrom
                (awaiterT: 'Awaiter)
                : CancellableValueTaskOptionBuilderBaseCode<_, _> =
                this.Bind(awaiterT = awaiterT, continuation = (fun v -> this.Return v))

    /// <exclude/>
    [<AutoOpen>]
    module LowPriority =
        // Low priority extensions
        type CancellableValueTaskOptionBuilderBase with

            /// <summary>Delegates to the input computation.</summary>
            ///
            /// <remarks>The existence of this method permits the use of return! in the
            /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
            ///
            /// <param name="getAwaiterTResult">The input computation.</param>
            ///
            /// <returns>The input computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline this.ReturnFrom
                ([<InlineIfLambda>] getAwaiterTResult: CancellationToken -> 'Awaiter)
                =
                this.Bind(
                    getAwaiterTResult = (fun ct -> getAwaiterTResult ct),
                    continuation = (fun v -> this.Return v)
                )

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a CancellationToken -> 'Awaiter into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'TResult1, 'Awaiter, 'TOverall
                when Awaitable<'Awaitable, 'Awaiter, 'TResult1>>
                ([<InlineIfLambda>] cancellableAwaiter: CancellationToken -> 'Awaiter)
                : CancellationToken -> 'Awaiter =
                (fun ct -> cancellableAwaiter ct)


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a CancellationToken -> 'Awaitable into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'TResult1, 'Awaiter, 'TOverall
                when Awaitable<'Awaitable, 'Awaiter, 'TResult1>>
                ([<InlineIfLambda>] cancellableAwaitable: CancellationToken -> 'Awaitable)
                : CancellationToken -> 'Awaiter =
                (fun ct -> Awaitable.GetAwaiter(cancellableAwaitable ct))


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a unit -> 'Awaitable into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'TResult1, 'Awaiter, 'TOverall
                when Awaitable<'Awaitable, 'Awaiter, 'TResult1>>
                ([<InlineIfLambda>] coldAwaitable: unit -> 'Awaitable)
                : CancellationToken -> 'Awaiter =
                (fun ct -> Awaitable.GetAwaiter(coldAwaitable ()))

            /// <summary>
            /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
            /// </summary>
            [<NoEagerConstraintApplication>]
            static member inline BindDynamic
                (
                    sm:
                        byref<
                            ResumableStateMachine<
                                CancellableValueTaskOptionBuilderBaseStateMachineData<'TOverall>
                             >
                         >,
                    awaiter: 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : bool =
                sm.Data.ThrowIfCancellationRequested()
                let mutable awaiter = awaiter

                let cont =
                    CancellableValueTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
                        let result = Awaiter.GetResult awaiter

                        match result with
                        | Some result -> (continuation result).Invoke(&sm)
                        | None ->
                            sm.Data.Result <- ValueSome None
                            true
                    )

                // shortcut to continue immediately
                if Awaiter.IsCompleted awaiter then
                    cont.Invoke(&sm)
                else
                    sm.ResumptionDynamicInfo.ResumptionData <-
                        (awaiter :> ICriticalNotifyCompletion)

                    sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                    false

            /// <summary>Creates A CancellableValueTask that runs computation, and when
            /// computation generates a result T, runs binder res.</summary>
            ///
            /// <remarks>A cancellation check is performed when the computation is executed.
            ///
            /// The existence of this method permits the use of let! in the
            /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
            ///
            /// <param name="awaiterTResult">The computation to provide an unbound result.</param>
            /// <param name="continuation">The function to bind the result of computation.</param>
            ///
            /// <returns>A CancellableValueTask that performs a monadic bind on the result
            /// of computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Bind
                (
                    awaiterTResult: 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2> =

                CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'TResult2>(fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        sm.Data.ThrowIfCancellationRequested()
                        // Get an awaiter from the Awaiter
                        let mutable awaiter = awaiterTResult

                        let mutable __stack_fin = true

                        if not (Awaiter.IsCompleted awaiter) then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result = Awaiter.GetResult awaiter

                            match result with
                            | Some result -> (continuation result).Invoke(&sm)
                            | None ->
                                sm.Data.Result <- ValueSome None
                                true
                        else
                            let mutable awaiter = awaiter :> ICriticalNotifyCompletion

                            MethodBuilder.AwaitUnsafeOnCompleted(
                                &sm.Data.MethodBuilder,
                                &awaiter,
                                &sm
                            )

                            false
                    else
                        CancellableValueTaskOptionBuilderBase.BindDynamic(
                            &sm,
                            awaiterTResult,
                            continuation
                        )
                //-- RESUMABLE CODE END
                )

            /// <summary>Delegates to the input computation.</summary>
            ///
            /// <remarks>The existence of this method permits the use of return! in the
            /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
            ///
            /// <param name="awaiterTResult">The input computation.</param>
            ///
            /// <returns>The input computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline this.ReturnFrom
                (awaiterTResult: 'Awaiter)
                : CancellableValueTaskOptionBuilderBaseCode<_, _> =
                this.Bind(awaiterTResult = awaiterTResult, continuation = (fun v -> this.Return v))

            /// <summary>Allows the computation expression to turn other types into 'Awaiter</summary>
            ///
            /// <remarks>This is the identity function.</remarks>
            ///
            /// <returns>'Awaiter</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Source<'TResult1, 'TResult2, 'Awaiter, 'TOverall
                when Awaiter<'Awaiter, 'TResult1>>
                (awaiter: 'Awaiter)
                : 'Awaiter =
                awaiter


            /// <summary>Allows the computation expression to turn other types into 'Awaiter</summary>
            ///
            /// <remarks>This turns a ^Awaitable into a 'Awaiter.</remarks>
            ///
            /// <returns>'Awaiter</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Source<'Awaitable, 'TResult1, 'TResult2, 'Awaiter, 'TOverall
                when Awaitable<'Awaitable, 'Awaiter, 'TResult1>>
                (task: 'Awaitable)
                : 'Awaiter =
                Awaitable.GetAwaiter task


            /// <summary>Creates A CancellableValueTask that runs binder(resource).
            /// The action resource.Dispose() is executed as this computation yields its result
            /// or if the CancellableValueTask exits by an exception or by cancellation.</summary>
            ///
            /// <remarks>
            ///
            /// The existence of this method permits the use of use and use! in the
            /// cancellableValueTaskOption { ... } computation expression syntax.</remarks>
            ///
            /// <param name="resource">The resource to be used and disposed.</param>
            /// <param name="binder">The function that takes the resource and returns an asynchronous
            /// computation.</param>
            ///
            /// <returns>A CancellableValueTask that binds and eventually disposes resource.</returns>
            ///
            member inline _.Using
                (
                    resource: #IDisposable,
                    binder: #IDisposable -> CancellableValueTaskOptionBuilderBaseCode<'TOverall, 'T>
                ) =
                ResumableCode.Using(resource, binder)


            /// <summary>Allows the computation expression to turn other types into other types</summary>
            ///
            /// <remarks>This is the identify function for For binds.</remarks>
            ///
            /// <returns>IEnumerable</returns>
            member inline _.Source(s: #seq<_>) : #seq<_> = s


    [<AutoOpen>]
    module MedHighPriority =

        type CancellableValueTaskOptionBuilderBase with

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a Task&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>'Awaiter</returns>
            member inline _.Source(taskAwaiter: TaskAwaiter<'T>) : Awaiter<TaskAwaiter<'T>, 'T> =
                taskAwaiter

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a Task&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>'Awaiter</returns>
            member inline _.Source(taskT: Task<'T>) : Awaiter<TaskAwaiter<'T>, 'T> =
                Awaitable.GetTaskAwaiter taskT

            member inline _.Source
                (cancellableTask: CancellationToken -> Task<_>)
                : CancellationToken -> Awaiter<TaskAwaiter<_>, _> =
                fun ct -> Awaitable.GetTaskAwaiter(cancellableTask ct)


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a Async&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline this.Source(computation: Async<'T>) =
                this.Source(Async.AsCancellableTask(computation))


    /// <exclude/>
    [<AutoOpen>]
    module HighPriority =


        type Microsoft.FSharp.Control.Async with

            /// <summary>Returns an asynchronous computation that will wait for the given <see cref="CancellableValueTaskOption{T}"/>.</summary>
            static member inline AwaitCancellableValueTaskOption
                ([<InlineIfLambda>] t: CancellableValueTaskOption<'T>)
                =
                async {
                    let! ct = Async.CancellationToken

                    return!
                        t ct
                        |> (fun vt -> vt.AsTask())
                        |> Async.AwaitTask
                }

            /// <summary>Creates a <see cref="CancellableValueTaskOption{T}"/> from an asynchronous computation.</summary>
            /// <param name="computation">The async computation to convert.</param>
            /// <returns>A <see cref="CancellableValueTaskOption{T}"/> that wraps the result in Some.</returns>
            static member inline AsCancellableValueTaskOption(computation: Async<'T>) =
                fun ct ->
                    ValueTask<'T option>(
                        Async.StartImmediateAsTask(computation, cancellationToken = ct)
                        |> Task.map Some
                    )

        type AsyncEx with

            /// <summary>Returns an asynchronous computation that will wait for the given <see cref="CancellableValueTaskOption{T}"/>.</summary>
            static member inline AwaitCancellableValueTaskOption
                ([<InlineIfLambda>] t: CancellableValueTaskOption<'T>)
                =
                async {
                    let! ct = Async.CancellationToken

                    return!
                        t ct
                        |> (fun vt -> vt.AsTask())
                        |> Async.AwaitTask
                }

            /// <summary>Creates a <see cref="CancellableValueTaskOption{T}"/> from an asynchronous computation.</summary>
            /// <param name="computation">The async computation to convert.</param>
            /// <returns>A <see cref="CancellableValueTaskOption{T}"/> that wraps the result in Some.</returns>
            static member inline AsCancellableValueTaskOption(computation: Async<'T>) =
                fun ct ->
                    ValueTask<'T option>(
                        Async.StartImmediateAsTask(computation, cancellationToken = ct)
                        |> Task.map Some
                    )

        type AsyncOptionBuilder with

            /// <summary>Allows binding a <see cref="CancellableValueTaskOption{T}"/> in an asyncOption computation expression.</summary>
            member inline this.Source
                ([<InlineIfLambda>] t: CancellableValueTaskOption<'T>)
                : Async<_> =
                Async.AwaitCancellableValueTaskOption t

        // High priority extensions
        type CancellableValueTaskOptionBuilderBase with

            /// <summary>Allows the computation expression to turn other types into other types</summary>
            ///
            /// <remarks>This is the identify function for For binds.</remarks>
            ///
            /// <returns>IEnumerable</returns>
            member inline _.Source(s: #IAsyncEnumerable<_>) = s


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a ColdTask&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline _.Source
                ([<InlineIfLambda>] task: unit -> TaskAwaiter<'T>)
                : CancellationToken -> Awaiter<TaskAwaiter<'T>, 'T> =
                (fun (ct: CancellationToken) -> (task ()))

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a ColdTask&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline _.Source
                ([<InlineIfLambda>] task: unit -> Task<'T>)
                : CancellationToken -> Awaiter<TaskAwaiter<'T>, 'T> =
                (fun (ct: CancellationToken) -> Awaitable.GetTaskAwaiter(task ()))

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a CancellableTask&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline _.Source
                ([<InlineIfLambda>] cancellableTaskAwaiter: CancellationToken -> TaskAwaiter<'T>)
                : CancellationToken -> Awaiter<TaskAwaiter<'T>, 'T> =
                (fun ct -> (cancellableTaskAwaiter ct))

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a CancellableTask&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline _.Source
                ([<InlineIfLambda>] task: CancellationToken -> Task<'T>)
                : CancellationToken -> Awaiter<TaskAwaiter<'T>, 'T> =
                (fun ct -> Awaitable.GetTaskAwaiter(task ct))


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a TaskOption&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>'Awaiter</returns>
            member inline _.Source(taskOption: TaskOption<'T>) =
                Awaitable.GetTaskAwaiter(taskOption)


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns an AsyncOption&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline this.Source
                (asyncOption: Async<'T option>)
                : CancellationToken -> TaskAwaiter<'T option> =
                this.Source(Async.AsCancellableTask asyncOption)


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns an Option&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline this.Source(option: Option<'T>) = this.Source(ValueTask<_>(option))

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a CancellableValueTask&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline _.Source
                ([<InlineIfLambda>] cancellableValueTask: CancellationToken -> ValueTask<'T>)
                : CancellationToken -> Awaiter<ValueTaskAwaiter<'T>, 'T> =
                (fun ct -> (cancellableValueTask ct).GetAwaiter())

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a CancellableValueTaskOption&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline _.Source
                ([<InlineIfLambda>] cancellableValueTaskOption: CancellableValueTaskOption<'T>)
                : CancellationToken -> Awaiter<ValueTaskAwaiter<'T option>, 'T option> =
                (fun ct -> (cancellableValueTaskOption ct).GetAwaiter())

    /// Contains methods to build CancellableValueTasks using the F# computation expression syntax
    type CancellableValueTaskOptionBuilder() =

        inherit CancellableValueTaskOptionBuilderBase()

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
            (code: CancellableValueTaskOptionBuilderBaseCode<'T, 'T>)
            : CancellableValueTaskOption<'T> =

            let mutable sm = CancellableValueTaskOptionBuilderBaseStateMachine<'T>()

            let initialResumptionFunc =
                CancellableValueTaskOptionBuilderBaseResumptionFunc<'T>(fun sm -> code.Invoke(&sm))

            let resumptionInfo =
                { new CancellableValueTaskOptionBuilderBaseResumptionDynamicInfo<'T>(initialResumptionFunc) with
                    member info.MoveNext(sm) =
                        let mutable savedExn = null

                        try
                            sm.ResumptionDynamicInfo.ResumptionData <- null
                            let step = info.ResumptionFunc.Invoke(&sm)

                            if step then
                                sm.Data.SetResult()
                            else
                                match sm.ResumptionDynamicInfo.ResumptionData with
                                | :? ICriticalNotifyCompletion as awaiter ->
                                    let mutable awaiter = awaiter

                                    MethodBuilder.AwaitUnsafeOnCompleted(
                                        &sm.Data.MethodBuilder,
                                        &awaiter,
                                        &sm
                                    )
                                | awaiter -> assert not (isNull awaiter)

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
                    ValueTask<'T option>(Task.FromCanceled<'T option>(ct))
                else
                    sm.Data.CancellationToken <- ct
                    sm.ResumptionDynamicInfo <- resumptionInfo
                    sm.Data.MethodBuilder <- AsyncValueTaskOptionMethodBuilder<'T>.Create()
                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task


        /// Hosts the task code in a state machine and starts the task.
        member inline _.Run
            (code: CancellableValueTaskOptionBuilderBaseCode<'T, 'T>)
            : CancellableValueTaskOption<'T> =
            if __useResumableCode then
                __stateMachine<
                    CancellableValueTaskOptionBuilderBaseStateMachineData<'T>,
                    CancellableValueTaskOption<'T>
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
                                ValueTask<'T option>(Task.FromCanceled<'T option>(ct))
                            else
                                let mutable sm = sm
                                sm.Data.CancellationToken <- ct

                                sm.Data.MethodBuilder <-
                                    AsyncValueTaskOptionMethodBuilder<'T>.Create()

                                sm.Data.MethodBuilder.Start(&sm)
                                sm.Data.MethodBuilder.Task
                    ))
            else
                CancellableValueTaskOptionBuilder.RunDynamic(code)

        /// Specify a Source of CancellationToken -> ValueTask<_> on the real type to allow type inference to work
        member inline _.Source
            (x: CancellationToken -> ValueTask<_>)
            : CancellationToken -> Awaiter<ValueTaskAwaiter<_>, _> =
            fun ct -> (x ct).GetAwaiter()

    /// Contains methods to build CancellableValueTasks using the F# computation expression syntax
    type BackgroundCancellableValueTaskOptionBuilder() =

        inherit CancellableValueTaskOptionBuilderBase()

        /// <summary>
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        /// </summary>
        static member inline RunDynamic
            (code: CancellableValueTaskOptionBuilderBaseCode<'T, 'T>)
            : CancellableValueTaskOption<'T> =
            // backgroundTask { .. } escapes to a background thread where necessary
            // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
            if
                isNull SynchronizationContext.Current
                && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
            then
                CancellableValueTaskOptionBuilder.RunDynamic(code)
            else
                fun ct ->
                    ValueTask<'T option>(
                        Task.Run<'T option>(
                            (fun () ->
                                (CancellableValueTaskOptionBuilder.RunDynamic code ct).AsTask()
                            ),
                            ct
                        )
                    )

        /// <summary>
        /// Hosts the task code in a state machine and starts the task, executing in the ThreadPool using Task.Run
        /// </summary>
        member inline _.Run
            (code: CancellableValueTaskOptionBuilderBaseCode<'T, 'T>)
            : CancellableValueTaskOption<'T> =
            if __useResumableCode then
                __stateMachine<
                    CancellableValueTaskOptionBuilderBaseStateMachineData<'T>,
                    CancellableValueTaskOption<'T>
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
                    (AfterCode<_, CancellableValueTaskOption<'T>>(fun sm ->
                        // backgroundTask { .. } escapes to a background thread where necessary
                        // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
                        if
                            isNull SynchronizationContext.Current
                            && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
                        then
                            let mutable sm = sm

                            fun ct ->
                                if ct.IsCancellationRequested then
                                    ValueTask<'T option>(Task.FromCanceled<'T option>(ct))
                                else
                                    sm.Data.CancellationToken <- ct

                                    sm.Data.MethodBuilder <-
                                        AsyncValueTaskOptionMethodBuilder<'T>.Create()

                                    sm.Data.MethodBuilder.Start(&sm)
                                    sm.Data.MethodBuilder.Task
                        else
                            let sm = sm // copy contents of state machine so we can capture it

                            fun (ct) ->
                                if ct.IsCancellationRequested then
                                    ValueTask<'T option>(Task.FromCanceled<'T option>(ct))
                                else
                                    ValueTask<'T option>(
                                        Task.Run<'T option>(
                                            (fun () ->
                                                let mutable sm = sm // host local mutable copy of contents of state machine on this thread pool thread
                                                sm.Data.CancellationToken <- ct

                                                sm.Data.MethodBuilder <-
                                                    AsyncValueTaskOptionMethodBuilder<'T>.Create()

                                                sm.Data.MethodBuilder.Start(&sm)
                                                sm.Data.MethodBuilder.Task.AsTask()
                                            ),
                                            ct
                                        )
                                    )
                    ))

            else
                BackgroundCancellableValueTaskOptionBuilder.RunDynamic(code)

    /// Contains the cancellableValueTaskOption computation expression builder.
    [<AutoOpen>]
    module CancellableValueTaskOptionBuilder =

        /// <summary>
        /// Builds a cancellableValueTaskOption using computation expression syntax.
        /// </summary>
        let cancellableValueTaskOption = CancellableValueTaskOptionBuilder()

        /// <summary>
        /// Builds a cancellableValueTaskOption using computation expression syntax which switches to execute on a background thread if not already doing so.
        /// </summary>
        let backgroundCancellableValueTaskOption =
            BackgroundCancellableValueTaskOptionBuilder()


[<RequireQualifiedAccess>]
module CancellableValueTaskOption =
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
    ///         cancellableValueTask {
    ///             let! cancellationToken = CancellableValueTask.getCancellationToken()
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

    /// <summary>Lifts an item to a CancellableValueTaskOption.</summary>
    /// <param name="x">The item to be the result of the CancellableValueTaskOption.</param>
    /// <returns>A CancellableValueTaskOption with the item as the result.</returns>
    let inline some x : CancellableValueTaskOption<'a> = fun _ -> ValueTask<'a option>(Some x)


    /// <summary>Allows chaining of CancellableValueTaskOptions.</summary>
    /// <param name="binder">The continuation.</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the binder.</returns>
    let inline bind
        ([<InlineIfLambda>] binder: 'input -> CancellableValueTaskOption<'output>)
        ([<InlineIfLambda>] cTask: CancellableValueTaskOption<'input>)
        =
        cancellableValueTaskOption {
            let! cResult = cTask
            return! binder cResult
        }

    /// <summary>Allows chaining of CancellableValueTaskOptions.</summary>
    /// <param name="mapper">The continuation.</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the mapper wrapped in a CancellableValueTaskOption.</returns>
    let inline map
        ([<InlineIfLambda>] mapper: 'input -> 'output)
        ([<InlineIfLambda>] cTask: CancellableValueTaskOption<'input>)
        =
        cancellableValueTaskOption {
            let! cResult = cTask
            return mapper cResult
        }

    /// <summary>Allows chaining of CancellableValueTaskOptions.</summary>
    /// <param name="applicable">A function wrapped in a CancellableValueTaskOption</param>
    /// <param name="cTask">The value.</param>
    /// <returns>The result of the applicable.</returns>
    let inline apply
        ([<InlineIfLambda>] applicable: CancellableValueTaskOption<'input -> 'output>)
        ([<InlineIfLambda>] cTask: CancellableValueTaskOption<'input>)
        =
        cancellableValueTaskOption {
            let! (applier: 'input -> 'output) = applicable
            let! (cResult: 'input) = cTask
            return applier cResult
        }

    /// <summary>Takes two CancellableValueTaskOptions, starts them serially in order of left to right, and returns a tuple of the pair.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A tuple of the parameters passed in</returns>
    let inline zip
        ([<InlineIfLambda>] left: CancellableValueTaskOption<'left>)
        ([<InlineIfLambda>] right: CancellableValueTaskOption<'right>)
        =
        cancellableValueTaskOption {
            let! r1 = left
            let! r2 = right
            return r1, r2
        }

    /// <summary>Takes two CancellableValueTaskOptions, starts them concurrently, and returns a tuple of the pair.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A tuple of the parameters passed in.</returns>
    let inline parallelZip
        ([<InlineIfLambda>] left: CancellableValueTaskOption<'left>)
        ([<InlineIfLambda>] right: CancellableValueTaskOption<'right>)
        =
        cancellableValueTask {
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
        ([<InlineIfLambda>] onSome: 'input -> CancellableValueTask<'output>)
        ([<InlineIfLambda>] onNone: unit -> CancellableValueTask<'output>)
        (input: CancellableValueTask<'input option>)
        : CancellableValueTask<'output> =
        input
        |> CancellableValueTask.bind (
            function
            | Some v -> onSome v
            | None -> onNone ()
        )

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise returns the specified default value.
    /// </summary>
    /// <param name="value">The specified default value.</param>
    /// <param name="cancellableValueTaskOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the default value.
    /// </returns>
    let inline defaultValue
        (value: 'value)
        (cancellableValueTaskOption: CancellableValueTask<'value option>)
        =
        cancellableValueTaskOption
        |> CancellableValueTask.map (Option.defaultValue value)

    /// <summary>
    ///  Gets the value of the option if the option is <c>Some</c>, otherwise evaluates <paramref name="defThunk"/> and returns the result.
    /// </summary>
    /// <param name="defThunk">A thunk that provides a default value when evaluated.</param>
    /// <param name="cancellableValueTaskOption">The input option.</param>
    /// <returns>
    /// The option if the option is <c>Some</c>, else the result of evaluating <paramref name="defThunk"/>.
    /// </returns>
    let inline defaultWith
        ([<InlineIfLambda>] defThunk: unit -> 'value)
        (cancellableValueTaskOption: CancellableValueTask<'value option>)
        : CancellableValueTask<'value> =
        cancellableValueTaskOption
        |> CancellableValueTask.map (Option.defaultWith defThunk)
