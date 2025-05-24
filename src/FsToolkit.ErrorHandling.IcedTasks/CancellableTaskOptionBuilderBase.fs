namespace FsToolkit.ErrorHandling

open System.Diagnostics


/// Contains methods to build Tasks using the F# computation expression syntax
[<AutoOpen>]
module CancellableTaskOptionBuilderBase =
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


    /// CancellationToken -> Task<Option<'T>>
    type CancellableTaskOption<'T> = CancellableTask<Option<'T>>


    /// The extra data stored in ResumableStateMachine for tasks
    [<Struct; NoComparison; NoEquality>]
    type CancellableTaskOptionBuilderBaseStateMachineData<'T> =
        [<DefaultValue(false)>]
        val mutable CancellationToken: CancellationToken

        [<DefaultValue(false)>]
        val mutable Result: 'T option voption

        [<DefaultValue(false)>]
        val mutable MethodBuilder: AsyncTaskOptionMethodBuilder<'T>

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

    and AsyncTaskOptionMethodBuilder<'TOverall> = AsyncTaskMethodBuilder<'TOverall option>

    /// This is used by the compiler as a template for creating state machine structs
    and CancellableTaskOptionBuilderBaseStateMachine<'TOverall> =
        ResumableStateMachine<CancellableTaskOptionBuilderBaseStateMachineData<'TOverall>>

    /// Represents the runtime continuation of a cancellableTasks state machine created dynamically
    and CancellableTaskOptionBuilderBaseResumptionFunc<'TOverall> =
        ResumptionFunc<CancellableTaskOptionBuilderBaseStateMachineData<'TOverall>>

    /// Represents the runtime continuation of a cancellableTasks state machine created dynamically
    and CancellableTaskOptionBuilderBaseResumptionDynamicInfo<'TOverall> =
        ResumptionDynamicInfo<CancellableTaskOptionBuilderBaseStateMachineData<'TOverall>>

    /// A special compiler-recognised delegate type for specifying blocks of cancellableTasks code with access to the state machine
    and CancellableTaskOptionBuilderBaseCode<'TOverall, 'T> =
        ResumableCode<CancellableTaskOptionBuilderBaseStateMachineData<'TOverall>, 'T>

    /// <summary>
    /// Contains methods to build TaskLikes using the F# computation expression syntax
    /// </summary>
    type CancellableTaskOptionBuilderBase() =
        /// <summary>Creates a CancellableTasks that runs generator</summary>
        /// <param name="generator">The function to run</param>
        /// <returns>A CancellableTasks that runs generator</returns>
        member inline _.Delay
            ([<InlineIfLambda>] generator:
                unit -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>)
            : CancellableTaskOptionBuilderBaseCode<'TOverall, 'T> =
            ResumableCode.Delay(fun () -> generator ())

        /// <summary>Creates A CancellableTasks that just returns ().</summary>
        /// <remarks>
        /// The existence of this method permits the use of empty else branches in the
        /// cancellableTasks { ... } computation expression syntax.
        /// </remarks>
        /// <returns>A CancellableTasks that returns ().</returns>
        [<DefaultValue>]
        member inline _.Zero() : CancellableTaskOptionBuilderBaseCode<'TOverall, unit> =
            CancellableTaskOptionBuilderBaseCode<_, _>(fun sm ->
                sm.Data.Result <- ValueSome(Some Unchecked.defaultof<'TOverall>)
                true
            )

        /// <summary>Creates A Computation that returns the result v.</summary>
        ///
        /// <remarks>A cancellation check is performed when the computation is executed.
        ///
        /// The existence of this method permits the use of return in the
        /// cancellableTasks { ... } computation expression syntax.</remarks>
        ///
        /// <param name="value">The value to return from the computation.</param>
        ///
        /// <returns>A cancellableTasks that returns value when executed.</returns>
        member inline _.Return(value: 'T) : CancellableTaskOptionBuilderBaseCode<'T, 'T> =
            CancellableTaskOptionBuilderBaseCode<'T, 'T>(fun sm ->
                sm.Data.Result <- ValueSome(Some value)
                true
            )

        /// <summary>Creates a CancellableTasks that first runs task1
        /// and then runs computation2, returning the result of computation2.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of expression sequencing in the
        /// cancellableTasks { ... } computation expression syntax.</remarks>
        ///
        /// <param name="task1">The first part of the sequenced computation.</param>
        /// <param name="task2">The second part of the sequenced computation.</param>
        ///
        /// <returns>A CancellableTasks that runs both of the computations sequentially.</returns>
        member inline _.Combine
            (
                task1: CancellableTaskOptionBuilderBaseCode<'TOverall, unit>,
                task2: CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>
            ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'T> =
            ResumableCode.Combine(
                task1,
                CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>(fun sm ->
                    if sm.Data.IsResultNone then true else task2.Invoke(&sm)
                )
            )

        /// <summary>Creates A CancellableTasks that runs computation repeatedly
        /// until guard() becomes false.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of while in the
        /// cancellableTasks { ... } computation expression syntax.</remarks>
        ///
        /// <param name="guard">The function to determine when to stop executing computation.</param>
        /// <param name="computation">The function to be executed.  Equivalent to the body
        /// of a while expression.</param>
        ///
        /// <returns>A CancellableTasks that behaves similarly to a while loop when run.</returns>
        member inline _.While
            (guard: unit -> bool, computation: CancellableTaskOptionBuilderBaseCode<'TOverall, unit>) : CancellableTaskOptionBuilderBaseCode<
                                                                                                            'TOverall,
                                                                                                            unit
                                                                                                         >
            =
            let mutable keepGoing = true

            ResumableCode.While(
                (fun () ->
                    keepGoing
                    && guard ()
                ),
                CancellableTaskOptionBuilderBaseCode<'TOverall, unit>(fun sm ->
                    if sm.Data.IsResultNone then
                        keepGoing <- false
                        true
                    else
                        computation.Invoke(&sm)
                )
            )

        /// <summary>Creates A CancellableTasks that runs computation and returns its result.
        /// If an exception happens then catchHandler(exn) is called and the resulting computation executed instead.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of try/with in the
        /// cancellableTasks { ... } computation expression syntax.</remarks>
        ///
        /// <param name="computation">The input computation.</param>
        /// <param name="catchHandler">The function to run when computation throws an exception.</param>
        ///
        /// <returns>A CancellableTasks that executes computation and calls catchHandler if an
        /// exception is thrown.</returns>
        member inline _.TryWith
            (
                computation: CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>,
                catchHandler: exn -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>
            ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'T> =
            ResumableCode.TryWith(computation, catchHandler)

        /// <summary>Creates A CancellableTasks that runs computation. The action compensation is executed
        /// after computation completes, whether computation exits normally or by an exception. If compensation raises an exception itself
        /// the original exception is discarded and the new exception becomes the overall result of the computation.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of try/finally in the
        /// cancellableTasks { ... } computation expression syntax.</remarks>
        ///
        /// <param name="computation">The input computation.</param>
        /// <param name="compensation">The action to be run after computation completes or raises an
        /// exception (including cancellation).</param>
        ///
        /// <returns>A CancellableTasks that executes computation and compensation afterwards or
        /// when an exception is raised.</returns>
        member inline _.TryFinally
            (
                computation: CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>,
                compensation: unit -> unit
            ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'T> =
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
                            CancellableTaskOptionBuilderBaseStateMachineData<'TOverall>
                         >
                     >,
                [<InlineIfLambda>] getAwaiter: CancellationToken -> 'Awaiter,
                continuation:
                    'TResult1 -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
            ) : bool =
            sm.Data.ThrowIfCancellationRequested()

            let mutable awaiter = getAwaiter sm.Data.CancellationToken

            let cont =
                CancellableTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
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

        /// <summary>Creates A CancellableTask that runs computation, and when
        /// computation generates a result T, runs binder res.</summary>
        ///
        /// <remarks>A cancellation check is performed when the computation is executed.
        ///
        /// The existence of this method permits the use of let! in the
        /// cancellableTask { ... } computation expression syntax.</remarks>
        ///
        /// <param name="getAwaiterTResult">The computation to provide an unbound result.</param>
        /// <param name="continuation">The function to bind the result of computation.</param>
        ///
        /// <returns>A CancellableTask that performs a monadic bind on the result
        /// of computation.</returns>
        [<NoEagerConstraintApplication>]
        member inline _.Bind
            (
                [<InlineIfLambda>] getAwaiterTResult: CancellationToken -> 'Awaiter,
                continuation:
                    'TResult1 -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
            ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2> =

            CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>(fun sm ->
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
                    CancellableTaskOptionBuilderBase.BindDynamic(
                        &sm,
                        getAwaiterTResult,
                        continuation
                    )
            //-- RESUMABLE CODE END
            )


        /// <summary>Creates a CancellableTask that enumerates the sequence seq
        /// on demand and runs body for each element.</summary>
        ///
        /// <remarks>A cancellation check is performed on each iteration of the loop.
        ///
        /// The existence of this method permits the use of for in the
        /// cancellableTask { ... } computation expression syntax.</remarks>
        ///
        /// <param name="sequence">The sequence to enumerate.</param>
        /// <param name="body">A function to take an item from the sequence and create
        /// A CancellableTask.  Can be seen as the body of the for expression.</param>
        ///
        /// <returns>A CancellableTask that will enumerate the sequence and run body
        /// for each element.</returns>
        member inline this.For
            (sequence: seq<'T>, body: 'T -> CancellableTaskOptionBuilderBaseCode<'TOverall, unit>)
            : CancellableTaskOptionBuilderBaseCode<'TOverall, unit> =
            ResumableCode.Using(
                sequence.GetEnumerator(),
                // ... and its body is a while loop that advances the enumerator and runs the body on each element.
                (fun e ->
                    printfn "this.For"

                    this.While(
                        (fun () -> e.MoveNext()),
                        CancellableTaskOptionBuilderBaseCode<'TOverall, unit>(fun sm ->
                            (body e.Current).Invoke(&sm)
                        )
                    )
                )
            )

        /// <summary>Creates A CancellableTask that runs computation. The action compensation is executed
        /// after computation completes, whether computation exits normally or by an exception. If compensation raises an exception itself
        /// the original exception is discarded and the new exception becomes the overall result of the computation.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of try/finally in the
        /// cancellableTask { ... } computation expression syntax.</remarks>
        ///
        /// <param name="computation">The input computation.</param>
        /// <param name="compensation">The action to be run after computation completes or raises an
        /// exception.</param>
        ///
        /// <returns>A CancellableTask that executes computation and compensation afterward or
        /// when an exception is raised.</returns>
        member inline internal this.TryFinallyAsync
            (
                computation: CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>,
                compensation: unit -> 'Awaitable
            ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'T> =
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
                            CancellableTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
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

        /// <summary>Creates A CancellableTask that runs binder(resource).
        /// The action resource.DisposeAsync() is executed as this computation yields its result
        /// or if the CancellableTask exits by an exception or by cancellation.</summary>
        ///
        /// <remarks>
        ///
        /// The existence of this method permits the use of use and use! in the
        /// cancellableTask { ... } computation expression syntax.</remarks>
        ///
        /// <param name="resource">The resource to be used and disposed.</param>
        /// <param name="binder">The function that takes the resource and returns an asynchronous
        /// computation.</param>
        ///
        /// <returns>A CancellableTask that binds and eventually disposes resource.</returns>
        ///
        member inline this.Using
            (
                resource: #IAsyncDisposable,
                binder: #IAsyncDisposable -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>
            ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'T> =
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
            ([<InlineIfLambda>] condition, body: CancellableTaskOptionBuilderBaseCode<_, unit>)
            : CancellableTaskOptionBuilderBaseCode<_, unit> =
            let mutable condition_res = true

            ResumableCode.While(
                (fun () -> condition_res),
                CancellableTaskOptionBuilderBaseCode<_, unit>(fun sm ->
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
                            CancellableTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
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
                body: 'T -> CancellableTaskOptionBuilderBaseCode<_, unit>
            ) : CancellableTaskOptionBuilderBaseCode<_, _> =

            CancellableTaskOptionBuilderBaseCode<_, _>(fun sm ->
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
        type CancellableTaskOptionBuilderBase with


            /// <summary>
            /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
            /// </summary>
            [<NoEagerConstraintApplication>]
            static member inline BindDynamic
                (
                    sm:
                        byref<
                            ResumableStateMachine<
                                CancellableTaskOptionBuilderBaseStateMachineData<'TOverall>
                             >
                         >,
                    [<InlineIfLambda>] getAwaiter: CancellationToken -> 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : bool =
                sm.Data.ThrowIfCancellationRequested()

                let mutable awaiter = getAwaiter sm.Data.CancellationToken

                let cont =
                    CancellableTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
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

            /// <summary>Creates A CancellableTask that runs computation, and when
            /// computation generates a result T, runs binder res.</summary>
            ///
            /// <remarks>A cancellation check is performed when the computation is executed.
            ///
            /// The existence of this method permits the use of let! in the
            /// cancellableTask { ... } computation expression syntax.</remarks>
            ///
            /// <param name="getAwaiterT">The computation to provide an unbound result.</param>
            /// <param name="continuation">The function to bind the result of computation.</param>
            ///
            /// <returns>A CancellableTask that performs a monadic bind on the result
            /// of computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Bind
                (
                    [<InlineIfLambda>] getAwaiterT: CancellationToken -> 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2> =

                CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>(fun sm ->
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
                        CancellableTaskOptionBuilderBase.BindDynamic(
                            &sm,
                            getAwaiterT,
                            continuation
                        )
                //-- RESUMABLE CODE END
                )


            /// <summary>Delegates to the input computation.</summary>
            ///
            /// <remarks>The existence of this method permits the use of return! in the
            /// cancellableTask { ... } computation expression syntax.</remarks>
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
                                CancellableTaskOptionBuilderBaseStateMachineData<'TOverall>
                             >
                         >,
                    awaiter: 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : bool =
                sm.Data.ThrowIfCancellationRequested()
                let mutable awaiter = awaiter

                let cont =
                    CancellableTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
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

            /// <summary>Creates A CancellableTask that runs computation, and when
            /// computation generates a result T, runs binder res.</summary>
            ///
            /// <remarks>A cancellation check is performed when the computation is executed.
            ///
            /// The existence of this method permits the use of let! in the
            /// cancellableTask { ... } computation expression syntax.</remarks>
            ///
            /// <param name="awaiterT">The computation to provide an unbound result.</param>
            /// <param name="continuation">The function to bind the result of computation.</param>
            ///
            /// <returns>A CancellableTask that performs a monadic bind on the result
            /// of computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Bind
                (
                    awaiterT: 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2> =

                CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>(fun sm ->
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
                        CancellableTaskOptionBuilderBase.BindDynamic(&sm, awaiterT, continuation)
                //-- RESUMABLE CODE END
                )

            /// <summary>Delegates to the input computation.</summary>
            ///
            /// <remarks>The existence of this method permits the use of return! in the
            /// task { ... } computation expression syntax.</remarks>
            ///
            /// <param name="awaiterT">The input computation.</param>
            ///
            /// <returns>The input computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline this.ReturnFrom
                (awaiterT: 'Awaiter)
                : CancellableTaskOptionBuilderBaseCode<_, _> =
                this.Bind(awaiterT = awaiterT, continuation = (fun v -> this.Return v))

    /// <exclude/>
    [<AutoOpen>]
    module LowPriority =
        // Low priority extensions
        type CancellableTaskOptionBuilderBase with

            /// <summary>Delegates to the input computation.</summary>
            ///
            /// <remarks>The existence of this method permits the use of return! in the
            /// cancellableTask { ... } computation expression syntax.</remarks>
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
            /// <remarks>This turns a CancellationToken -> 'Awaitable into a CancellationToken -> 'Awaiter.</remarks>
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
                                CancellableTaskOptionBuilderBaseStateMachineData<'TOverall>
                             >
                         >,
                    awaiter: 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : bool =
                sm.Data.ThrowIfCancellationRequested()
                let mutable awaiter = awaiter

                let cont =
                    CancellableTaskOptionBuilderBaseResumptionFunc<'TOverall>(fun sm ->
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

            /// <summary>Creates A CancellableTask that runs computation, and when
            /// computation generates a result T, runs binder res.</summary>
            ///
            /// <remarks>A cancellation check is performed when the computation is executed.
            ///
            /// The existence of this method permits the use of let! in the
            /// cancellableTask { ... } computation expression syntax.</remarks>
            ///
            /// <param name="awaiterTResult">The computation to provide an unbound result.</param>
            /// <param name="continuation">The function to bind the result of computation.</param>
            ///
            /// <returns>A CancellableTask that performs a monadic bind on the result
            /// of computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline _.Bind
                (
                    awaiterTResult: 'Awaiter,
                    continuation:
                        'TResult1 -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>
                ) : CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2> =

                CancellableTaskOptionBuilderBaseCode<'TOverall, 'TResult2>(fun sm ->
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
                        CancellableTaskOptionBuilderBase.BindDynamic(
                            &sm,
                            awaiterTResult,
                            continuation
                        )
                //-- RESUMABLE CODE END
                )

            /// <summary>Delegates to the input computation.</summary>
            ///
            /// <remarks>The existence of this method permits the use of return! in the
            /// task { ... } computation expression syntax.</remarks>
            ///
            /// <param name="awaiterTResult">The input computation.</param>
            ///
            /// <returns>The input computation.</returns>
            [<NoEagerConstraintApplication>]
            member inline this.ReturnFrom
                (awaiterTResult: 'Awaiter)
                : CancellableTaskOptionBuilderBaseCode<_, _> =
                this.Bind(awaiterTResult = awaiterTResult, continuation = (fun v -> this.Return v))

            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This is the identify function.</remarks>
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


            /// <summary>Creates A CancellableTask that runs binder(resource).
            /// The action resource.Dispose() is executed as this computation yields its result
            /// or if the CancellableTask exits by an exception or by cancellation.</summary>
            ///
            /// <remarks>
            ///
            /// The existence of this method permits the use of use and use! in the
            /// cancellableTask { ... } computation expression syntax.</remarks>
            ///
            /// <param name="resource">The resource to be used and disposed.</param>
            /// <param name="binder">The function that takes the resource and returns an asynchronous
            /// computation.</param>
            ///
            /// <returns>A CancellableTask that binds and eventually disposes resource.</returns>
            ///
            member inline _.Using
                (
                    resource: #IDisposable,
                    binder: #IDisposable -> CancellableTaskOptionBuilderBaseCode<'TOverall, 'T>
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

        type CancellableTaskOptionBuilderBase with

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

            static member inline AwaitCancellableTaskOption
                ([<InlineIfLambda>] t: CancellableTaskOption<'T>)
                =
                async {
                    let! ct = Async.CancellationToken

                    return!
                        t ct
                        |> Async.AwaitTask
                }

            static member inline AsCancellableTaskOption(computation: Async<'T>) =
                fun ct -> Async.StartImmediateAsTask(computation, cancellationToken = ct)

        type AsyncEx with

            static member inline AwaitCancellableTaskOption
                ([<InlineIfLambda>] t: CancellableTaskOption<'T>)
                =
                async {
                    let! ct = Async.CancellationToken

                    return!
                        t ct
                        |> Async.AwaitTask
                }

            static member inline AsCancellableTaskOption(computation: Async<'T>) =
                fun ct -> Async.StartImmediateAsTask(computation, cancellationToken = ct)


        type AsyncOptionBuilder with

            member inline this.Source([<InlineIfLambda>] t: CancellableTaskOption<'T>) : Async<_> =
                Async.AwaitCancellableTaskOption t

        // High priority extensions
        type CancellableTaskOptionBuilderBase with

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
            /// <remarks>This turns a ColdTask&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline _.Source(taskOption: TaskOption<'T>) =
                Awaitable.GetTaskAwaiter(taskOption)


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a ColdTask&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline this.Source
                (asyncOption: Async<'T option>)
                : CancellationToken -> TaskAwaiter<'T option> =
                this.Source(Async.AsCancellableTask asyncOption)


            /// <summary>Allows the computation expression to turn other types into CancellationToken -> 'Awaiter</summary>
            ///
            /// <remarks>This turns a ColdTask&lt;'T&gt; into a CancellationToken -> 'Awaiter.</remarks>
            ///
            /// <returns>CancellationToken -> 'Awaiter</returns>
            member inline this.Source(option: Option<'T>) = this.Source(ValueTask<_>(option))
