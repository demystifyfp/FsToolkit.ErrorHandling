namespace FsToolkit.ErrorHandling


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
    open FsToolkit.ErrorHandling
    open IcedTasks

    /// CancellationToken -> Task<Result<'T, 'Error>>
    type CancellableTaskValidation<'T, 'Error> = CancellableTask<Result<'T, 'Error list>>

    /// The extra data stored in ResumableStateMachine for tasks
    [<Struct; NoComparison; NoEquality>]
    type CancellableTaskValidationStateMachineData<'T, 'Error> =
        [<DefaultValue(false)>]
        val mutable CancellationToken: CancellationToken

        [<DefaultValue(false)>]
        val mutable Result: Result<'T, 'Error list>

        [<DefaultValue(false)>]
        val mutable MethodBuilder: CancellableTaskValidationMethodBuilder<'T, 'Error>


        member inline this.IsResultError = Result.isError this.Result
        member inline this.IsTaskCompleted = this.MethodBuilder.Task.IsCompleted

        member inline this.ThrowIfCancellationRequested() =
            this.CancellationToken.ThrowIfCancellationRequested()

    and CancellableTaskValidationMethodBuilder<'TOverall, 'Error> =
        AsyncTaskMethodBuilder<Result<'TOverall, 'Error list>>

    and CancellableTaskValidationStateMachine<'TOverall, 'Error> =
        ResumableStateMachine<CancellableTaskValidationStateMachineData<'TOverall, 'Error>>

    and CancellableTaskValidationResumptionFunc<'TOverall, 'Error> =
        ResumptionFunc<CancellableTaskValidationStateMachineData<'TOverall, 'Error>>

    and CancellableTaskValidationResumptionDynamicInfo<'TOverall, 'Error> =
        ResumptionDynamicInfo<CancellableTaskValidationStateMachineData<'TOverall, 'Error>>

    and CancellableTaskValidationCode<'TOverall, 'Error, 'T> =
        ResumableCode<CancellableTaskValidationStateMachineData<'TOverall, 'Error>, 'T>

    type CancellableTaskValidationBuilderBase() =

        member inline _.Delay
            (generator: unit -> CancellableTaskValidationCode<'TOverall, 'Error, 'T>)
            : CancellableTaskValidationCode<'TOverall, 'Error, 'T> =
            CancellableTaskValidationCode<'TOverall, 'Error, 'T>(fun sm ->
                sm.Data.ThrowIfCancellationRequested()
                (generator ()).Invoke(&sm)
            )

        /// Used to represent no-ops like the implicit empty "else" branch of an "if" expression.
        [<DefaultValue>]
        member inline _.Zero<'TOverall, 'Error>
            ()
            : CancellableTaskValidationCode<'TOverall, 'Error, unit> =
            ResumableCode.Zero()

        member inline _.Return(value: 'T) : CancellableTaskValidationCode<'T, 'Error, 'T> =
            CancellableTaskValidationCode<'T, _, _>(fun sm ->
                sm.Data.ThrowIfCancellationRequested()
                sm.Data.Result <- Ok value
                true
            )


        /// Chains together a step with its following step.
        /// Note that this requires that the first step has no result.
        /// This prevents constructs like `task { return 1; return 2; }`.
        member inline _.Combine
            (
                task1: CancellableTaskValidationCode<'TOverall, 'Error, unit>,
                task2: CancellableTaskValidationCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskValidationCode<'TOverall, 'Error, 'T> =
            ResumableCode.Combine(
                CancellableTaskValidationCode(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()
                    task1.Invoke(&sm)
                ),
                CancellableTaskValidationCode<'TOverall, 'Error, 'T>(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()
                    if sm.Data.IsResultError then true else task2.Invoke(&sm)
                )
            )


        /// Builds a step that executes the body while the condition predicate is true.
        member inline _.While
            (
                [<InlineIfLambda>] condition: unit -> bool,
                body: CancellableTaskValidationCode<'TOverall, 'Error, unit>
            ) : CancellableTaskValidationCode<'TOverall, 'Error, unit> =
            let mutable keepGoing = true

            ResumableCode.While(
                (fun () ->
                    keepGoing
                    && condition ()
                ),
                CancellableTaskValidationCode<_, _, _>(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()

                    if sm.Data.IsResultError then
                        keepGoing <- false
                        sm.Data.MethodBuilder.SetResult sm.Data.Result
                        true
                    else
                        body.Invoke(&sm)
                )
            )

        /// Wraps a step in a try/with. This catches exceptions both in the evaluation of the function
        /// to retrieve the step, and in the continuation of the step (if any).
        member inline _.TryWith
            (
                computation: CancellableTaskValidationCode<'TOverall, 'Error, 'T>,
                catchHandler: exn -> CancellableTaskValidationCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskValidationCode<'TOverall, 'Error, 'T> =
            ResumableCode.TryWith(
                CancellableTaskValidationCode(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()
                    computation.Invoke(&sm)
                ),
                catchHandler
            )

        /// Wraps a step in a try/finally. This catches exceptions both in the evaluation of the function
        /// to retrieve the step, and in the continuation of the step (if any).
        member inline _.TryFinally
            (
                computation: CancellableTaskValidationCode<'TOverall, 'Error, 'T>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : CancellableTaskValidationCode<'TOverall, 'Error, 'T> =
            ResumableCode.TryFinally(

                CancellableTaskValidationCode(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()
                    computation.Invoke(&sm)
                ),
                ResumableCode<_, _>(fun _ ->
                    compensation ()
                    true
                )
            )

        member inline this.For
            (
                sequence: seq<'T>,
                body: 'T -> CancellableTaskValidationCode<'TOverall, 'Error, unit>
            ) : CancellableTaskValidationCode<'TOverall, 'Error, unit> =
            ResumableCode.Using(
                sequence.GetEnumerator(),
                // ... and its body is a while loop that advances the enumerator and runs the body on each element.
                (fun e ->
                    this.While(
                        (fun () -> e.MoveNext()),
                        CancellableTaskValidationCode<'TOverall, 'Error, unit>(fun sm ->
                            sm.Data.ThrowIfCancellationRequested()
                            (body e.Current).Invoke(&sm)
                        )
                    )
                )
            )

        member inline internal this.TryFinallyAsync
            (
                body: CancellableTaskValidationCode<'TOverall, 'Error, 'T>,
                compensation: unit -> ValueTask
            ) : CancellableTaskValidationCode<'TOverall, 'Error, 'T> =
            ResumableCode.TryFinallyAsync(
                body,
                ResumableCode<_, _>(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()

                    if __useResumableCode then
                        let mutable __stack_condition_fin = true
                        let __stack_vtask = compensation ()

                        if not __stack_vtask.IsCompleted then
                            let mutable awaiter = __stack_vtask.GetAwaiter()
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_condition_fin <- __stack_yield_fin

                            if not __stack_condition_fin then
                                sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)

                        __stack_condition_fin
                    else
                        let vtask = compensation ()
                        let mutable awaiter = vtask.GetAwaiter()

                        let cont =
                            CancellableTaskValidationResumptionFunc<'TOverall, 'Error>(fun sm ->
                                awaiter.GetResult()
                                |> ignore

                                true
                            )

                        // shortcut to continue immediately
                        if awaiter.IsCompleted then
                            true
                        else
                            sm.ResumptionDynamicInfo.ResumptionData <-
                                (awaiter :> ICriticalNotifyCompletion)

                            sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                            false
                )
            )

        member inline this.Using<'Resource, 'TOverall, 'Error, 'T when 'Resource :> IAsyncDisposable>
            (
                resource: 'Resource,
                body: 'Resource -> CancellableTaskValidationCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskValidationCode<'TOverall, 'Error, 'T> =
            this.TryFinallyAsync(
                (fun sm -> (body resource).Invoke(&sm)),
                (fun () ->
                    if not (isNull (box resource)) then
                        resource.DisposeAsync()
                    else
                        ValueTask()
                )
            )

        member inline this.Source
            (ctr: CancellableTaskValidation<'T, 'Error>)
            : CancellableTaskValidation<'T, 'Error> =
            ctr

        member inline this.Source(xs: #seq<_>) = xs

        member inline _.Source(result: TaskResult<_, _>) : CancellableTaskValidation<_, _> =
            cancellableTask { return! result }

        member inline _.Source(result: Async<Result<_, _>>) : CancellableTaskValidation<_, _> =
            cancellableTask { return! result }

        member inline this.Source(result: Async<Choice<_, _>>) : CancellableTaskValidation<_, _> =
            result
            |> Async.map Result.ofChoice
            |> this.Source

        member inline _.Source(t: ValueTask<Result<_, _>>) : CancellableTaskValidation<'T, 'Error> =
            cancellableTask { return! t }


        member inline this.Source(result: Choice<_, _>) : CancellableTaskValidation<_, _> =
            result
            |> Result.ofChoice
            |> Validation.ofResult
            |> CancellableTask.singleton

        member inline _.Source(result: Validation<_, _>) : CancellableTaskValidation<_, _> =
            result
            |> CancellableTask.singleton


    type CancellableTaskValidationBuilder() =

        inherit CancellableTaskValidationBuilderBase()

        // This is the dynamic implementation - this is not used
        // for statically compiled tasks.  An executor (resumptionFuncExecutor) is
        // registered with the state machine, plus the initial resumption.
        // The executor stays constant throughout the execution, it wraps each step
        // of the execution in a try/with.  The resumption is changed at each step
        // to represent the continuation of the computation.
        static member inline RunDynamic
            (code: CancellableTaskValidationCode<'T, 'Error, 'T>)
            : CancellableTaskValidation<'T, 'Error> =

            let mutable sm = CancellableTaskValidationStateMachine<'T, 'Error>()

            let initialResumptionFunc =
                CancellableTaskValidationResumptionFunc<'T, 'Error>(fun sm -> code.Invoke(&sm))

            let resumptionInfo =
                { new CancellableTaskValidationResumptionDynamicInfo<'T, 'Error>(initialResumptionFunc) with
                    member info.MoveNext(sm) =
                        let mutable savedExn = null

                        try
                            sm.ResumptionDynamicInfo.ResumptionData <- null
                            let step = info.ResumptionFunc.Invoke(&sm)

                            if sm.Data.IsTaskCompleted then
                                ()
                            elif step then
                                sm.Data.MethodBuilder.SetResult(sm.Data.Result)
                            else
                                let mutable awaiter =
                                    sm.ResumptionDynamicInfo.ResumptionData
                                    :?> ICriticalNotifyCompletion

                                assert not (isNull awaiter)
                                sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)

                        with exn ->
                            savedExn <- exn
                        // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                        match savedExn with
                        | null -> ()
                        | exn ->
                            // printfn "%A" exn
                            sm.Data.MethodBuilder.SetException exn

                    member _.SetStateMachine(sm, state) =
                        sm.Data.MethodBuilder.SetStateMachine(state)
                }

            fun (ct) ->
                if ct.IsCancellationRequested then
                    Task.FromCanceled<_>(ct)
                else
                    sm.Data.CancellationToken <- ct
                    sm.ResumptionDynamicInfo <- resumptionInfo

                    sm.Data.MethodBuilder <-
                        CancellableTaskValidationMethodBuilder<'T, 'Error>.Create()

                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task

        member inline _.Run
            (code: CancellableTaskValidationCode<'T, 'Error, 'T>)
            : CancellableTaskValidation<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<CancellableTaskValidationStateMachineData<'T, 'Error>, CancellableTaskValidation<'T, 'Error>>
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: Exception = null

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if
                                __stack_code_fin
                                && not sm.Data.IsTaskCompleted
                            then
                                sm.Data.MethodBuilder.SetResult(sm.Data.Result)
                        with exn ->
                            __stack_exn <- exn
                        // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                        match __stack_exn with
                        | null -> ()
                        | exn ->
                            // printfn "%A" exn
                            sm.Data.MethodBuilder.SetException exn
                    //-- RESUMABLE CODE END
                    ))
                    (SetStateMachineMethodImpl<_>(fun sm state ->
                        sm.Data.MethodBuilder.SetStateMachine(state)
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
                                    CancellableTaskValidationMethodBuilder<'T, 'Error>.Create()

                                sm.Data.MethodBuilder.Start(&sm)
                                sm.Data.MethodBuilder.Task
                    ))
            else
                CancellableTaskValidationBuilder.RunDynamic(code)

    type BackgroundCancellableTaskValidationBuilder() =

        inherit CancellableTaskValidationBuilderBase()

        static member inline RunDynamic
            (code: CancellableTaskValidationCode<'T, 'Error, 'T>)
            : CancellableTaskValidation<'T, 'Error> =
            // backgroundTask { .. } escapes to a background thread where necessary
            // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
            if
                isNull SynchronizationContext.Current
                && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
            then
                CancellableTaskValidationBuilder.RunDynamic(code)
            else
                fun (ct) ->
                    Task.Run<Result<'T, 'Error list>>(
                        (fun () -> CancellableTaskValidationBuilder.RunDynamic (code) (ct)),
                        ct
                    )

        /// Same as CancellableTaskValidationBuilder.Run except the start is inside Task.Run if necessary
        member inline _.Run
            (code: CancellableTaskValidationCode<'T, 'Error, 'T>)
            : CancellableTaskValidation<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<CancellableTaskValidationStateMachineData<'T, 'Error>, CancellableTaskValidation<'T, 'Error>>
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if
                                __stack_code_fin
                                && not sm.Data.IsTaskCompleted
                            then
                                sm.Data.MethodBuilder.SetResult(sm.Data.Result)
                        with exn ->

                            // printfn "%A" exn
                            sm.Data.MethodBuilder.SetException exn
                    //-- RESUMABLE CODE END
                    ))
                    (SetStateMachineMethodImpl<_>(fun sm state ->
                        sm.Data.MethodBuilder.SetStateMachine(state)
                    ))
                    (AfterCode<_, CancellableTaskValidation<'T, 'Error>>(fun sm ->
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
                                        CancellableTaskValidationMethodBuilder<'T, 'Error>.Create()

                                    sm.Data.MethodBuilder.Start(&sm)
                                    sm.Data.MethodBuilder.Task
                        else
                            let sm = sm // copy contents of state machine so we can capture it

                            fun (ct) ->
                                if ct.IsCancellationRequested then
                                    Task.FromCanceled<_>(ct)
                                else
                                    Task.Run<Result<'T, 'Error list>>(
                                        (fun () ->
                                            let mutable sm = sm // host local mutable copy of contents of state machine on this thread pool thread
                                            sm.Data.CancellationToken <- ct

                                            sm.Data.MethodBuilder <-
                                                CancellableTaskValidationMethodBuilder<'T, 'Error>
                                                    .Create()

                                            sm.Data.MethodBuilder.Start(&sm)
                                            sm.Data.MethodBuilder.Task
                                        ),
                                        ct
                                    )
                    ))
            else
                BackgroundCancellableTaskValidationBuilder.RunDynamic(code)


    [<AutoOpen>]
    module CancellableTaskValidationBuilder =

        let cancellableTaskValidation = CancellableTaskValidationBuilder()

        let backgroundCancellableTaskValidation =
            BackgroundCancellableTaskValidationBuilder()

    [<AutoOpen>]
    module LowPriority =
        // Low priority extensions
        type CancellableTaskValidationBuilderBase with

            [<NoEagerConstraintApplication>]
            static member inline BindDynamic<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
                (
                    sm:
                        byref<ResumableStateMachine<CancellableTaskValidationStateMachineData<'TOverall, 'Error>>>,
                    getAwaiter: CancellationToken -> ^Awaiter,
                    continuation:
                        ('TResult1 -> CancellableTaskValidationCode<'TOverall, 'Error, 'TResult2>)
                ) : bool =
                sm.Data.CancellationToken.ThrowIfCancellationRequested()

                let mutable awaiter = getAwaiter sm.Data.CancellationToken

                let cont =
                    (CancellableTaskValidationResumptionFunc<'TOverall, 'Error>(fun sm ->
                        let result =
                            (^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>) (awaiter))

                        match result with
                        | Ok result -> (continuation result).Invoke(&sm)
                        | Error e ->
                            sm.Data.Result <- Error e
                            true
                    ))

                // shortcut to continue immediately
                if (^Awaiter: (member get_IsCompleted: unit -> bool) (awaiter)) then
                    cont.Invoke(&sm)
                else
                    sm.ResumptionDynamicInfo.ResumptionData <-
                        (awaiter :> ICriticalNotifyCompletion)

                    sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                    false

            [<NoEagerConstraintApplication>]
            member inline _.Bind<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
                (
                    getAwaiter: CancellationToken -> ^Awaiter,
                    continuation:
                        ('TResult1 -> CancellableTaskValidationCode<'TOverall, 'Error, 'TResult2>)
                ) : CancellableTaskValidationCode<'TOverall, 'Error, 'TResult2> =

                CancellableTaskValidationCode<'TOverall, _, _>(fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        sm.Data.CancellationToken.ThrowIfCancellationRequested()
                        // Get an awaiter from the awaitable
                        let mutable awaiter = getAwaiter sm.Data.CancellationToken

                        let mutable __stack_fin = true

                        if not (^Awaiter: (member get_IsCompleted: unit -> bool) (awaiter)) then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result =
                                (^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>) (awaiter))

                            match result with
                            | Ok result -> (continuation result).Invoke(&sm)
                            | Error e ->
                                sm.Data.Result <- Error e
                                true
                        else
                            sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                            false
                    else
                        CancellableTaskValidationBuilderBase.BindDynamic<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error>(
                            &sm,
                            getAwaiter,
                            continuation
                        )
                //-- RESUMABLE CODE END
                )

            [<NoEagerConstraintApplication>]
            member inline this.ReturnFrom<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
                (getAwaiter: CancellationToken -> ^Awaiter)
                : CancellableTaskValidationCode<'TResult1, 'Error, 'TResult1> =

                this.Bind(getAwaiter, (fun v -> this.Return v))

            // [<NoEagerConstraintApplication>]
            // member inline this.BindReturn<'TResult1, 'TResult2, 'Awaiter, 'TOverall, 'Error
            //     when ^Awaiter :> ICriticalNotifyCompletion
            //     and ^Awaiter: (member get_IsCompleted: unit -> bool)
            //     and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
            //     (
            //         getAwaiter: CancellationToken -> 'Awaiter,
            //         f: 'TResult1 -> 'TResult2
            //     ) : CancellableTaskValidationCode<'TResult2, 'Error, 'TResult2> =
            //     this.Bind((fun ct -> getAwaiter ct), (fun v -> this.Return(f v)))

            [<NoEagerConstraintApplication>]
            member inline _.Source<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
                (getAwaiter: CancellationToken -> ^Awaiter)
                : CancellationToken -> ^Awaiter =
                getAwaiter

            [<NoEagerConstraintApplication>]
            member inline _.Source< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
                (task: ^TaskLike)
                : CancellationToken -> ^Awaiter =
                (fun (ct: CancellationToken) ->
                    (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task))
                )

            [<NoEagerConstraintApplication>]
            member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (t: ^TaskLike)
                : CancellableTaskValidation<'T, 'Error> =

                cancellableTask {
                    let! r = t
                    return Ok r
                }


            [<NoEagerConstraintApplication>]
            member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (t: unit -> ^TaskLike)
                : CancellableTaskValidation<'T, 'Error> =

                cancellableTask {
                    let! r = t
                    return Ok r
                }


            [<NoEagerConstraintApplication>]
            member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (t: CancellationToken -> ^TaskLike)
                : CancellableTaskValidation<'T, 'Error> =

                cancellableTask {
                    let! r = t
                    return Ok r
                }

            member inline _.Using<'Resource, 'TOverall, 'Error, 'T when 'Resource :> IDisposable>
                (
                    resource: 'Resource,
                    binder: 'Resource -> CancellableTaskValidationCode<'TOverall, 'Error, 'T>
                ) =
                ResumableCode.Using(
                    resource,
                    fun resource ->
                        CancellableTaskValidationCode<'TOverall, 'Error, 'T>(fun sm ->
                            sm.Data.ThrowIfCancellationRequested()
                            (binder resource).Invoke(&sm)
                        )
                )

            member inline _.Source(result: Validation<_, _>) : CancellableTaskValidation<_, _> =
                CancellableTask.singleton result

    [<AutoOpen>]
    module HighPriority =
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

        type CancellableTaskValidationBuilderBase with

            member inline this.Bind
                (
                    task: CancellableTaskValidation<'TResult1, 'Error>,
                    continuation:
                        ('TResult1 -> CancellableTaskValidationCode<'TOverall, 'Error, 'TResult2>)
                ) : CancellableTaskValidationCode<'TOverall, 'Error, 'TResult2> =
                let x = fun ct -> (task ct).GetAwaiter()
                this.Bind(getAwaiter = x, continuation = continuation)

            // [<NoEagerConstraintApplication>]
            // member inline this.BindReturn<'TResult1, 'TResult2, 'Awaiter, 'TOverall, 'Error
            //     when ^Awaiter :> ICriticalNotifyCompletion
            //     and ^Awaiter: (member get_IsCompleted: unit -> bool)
            //     and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
            //     (
            //         getAwaiter: CancellationToken -> 'Awaiter,
            //         f: 'TResult1 -> 'TResult2
            //     ) : CancellableTaskValidationCode<'TResult2, 'Error, 'TResult2> =
            //     this.Bind((fun ct -> getAwaiter ct), (fun v -> this.Return(f v)))

            member inline this.ReturnFrom
                (task: CancellableTaskValidation<'T, 'Error>)
                : CancellableTaskValidationCode<'T, 'Error, 'T> =
                this.Bind(task, (fun v -> this.Return v))

    [<AutoOpen>]
    module MediumPriority =
        open HighPriority

        type CancellableTaskValidationBuilder with

            member inline _.Source(t: Task<'T>) : CancellableTaskValidation<'T, 'Error> =
                cancellableTask {
                    let! r = t
                    return Ok r
                }

            member inline _.Source
                (result: CancellableTask<'T>)
                : CancellableTaskValidation<'T, 'Error> =
                cancellableTask {
                    let! r = result
                    return Ok r
                }

            member inline _.Source
                (result: CancellableTask)
                : CancellableTaskValidation<unit, 'Error> =
                cancellableTask {
                    let! r = result
                    return Ok r
                }

            member inline _.Source(result: ColdTask<_>) : CancellableTaskValidation<_, _> =
                cancellableTask {
                    let! r = result
                    return Ok r
                }

            member inline _.Source(result: ColdTask) : CancellableTaskValidation<_, _> =
                cancellableTask {
                    let! r = result
                    return Ok r
                }

            member inline _.Source(t: Async<'T>) : CancellableTaskValidation<'T, 'Error> =
                cancellableTask {
                    let! r = t
                    return Ok r
                }

            member inline _.Source
                (t: Result<'ok, 'error>)
                : CancellableTaskValidation<'ok, 'error> =
                cancellableTask {
                    return
                        t
                        |> Validation.ofResult
                }


    [<AutoOpen>]
    module AsyncExtensions =
        type Microsoft.FSharp.Control.AsyncBuilder with

            member inline this.Bind
                (
                    [<InlineIfLambda>] t: CancellableTaskValidation<'T, 'Error>,
                    [<InlineIfLambda>] binder: (_ -> Async<_>)
                ) : Async<_> =
                this.Bind(Async.AwaitCancellableTaskValidation t, binder)

            member inline this.ReturnFrom
                ([<InlineIfLambda>] t: CancellableTaskValidation<'T, 'Error>)
                =
                this.ReturnFrom(Async.AwaitCancellableTaskValidation t)


        type FsToolkit.ErrorHandling.AsyncResultCE.AsyncResultBuilder with

            member inline this.Source
                ([<InlineIfLambda>] t: CancellableTaskValidation<'T, 'Error>)
                : Async<_> =
                Async.AwaitCancellableTaskValidation t

    [<RequireQualifiedAccess>]
    module CancellableTaskValidation =
        let getCancellationToken () : CancellableTaskValidation<CancellationToken, 'Error> =
            CancellableTaskValidationBuilder.cancellableTaskValidation.Run(
                CancellableTaskValidationCode<_, 'Error, _>(fun sm ->
                    sm.Data.Result <- Ok sm.Data.CancellationToken
                    true
                )
            )

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

        let inline ofResult (result: Result<'ok, 'error>) : CancellableTaskValidation<'ok, 'error> =
            let x = Result.mapError List.singleton result
            fun _ -> Task.FromResult(x)

        let inline ofChoice (choice: Choice<'ok, 'error>) : CancellableTaskValidation<'ok, 'error> =
            match choice with
            | Choice1Of2 x -> ok x
            | Choice2Of2 x -> error x

        let inline retn (value: 'ok) : CancellableTaskValidation<'ok, 'error> = ok value

        /// <summary>Allows chaining of CancellableTaskValidation.</summary>
        /// <param name="binder">The continuation.</param>
        /// <param name="cTask">The value.</param>
        /// <returns>The result of the binder.</returns>
        let inline bind
            ([<InlineIfLambda>] binder: 'input -> CancellableTaskValidation<'output, 'error>)
            ([<InlineIfLambda>] cTask: CancellableTaskValidation<'input, 'error>)
            =
            cancellableTask {
                let! cResult = cTask

                match cResult with
                | Ok x -> return! binder x
                | Error e -> return Error e
            }

        let inline mapError
            ([<InlineIfLambda>] errorMapper: 'errorInput -> 'errorOutput)
            (input: CancellableTaskValidation<'ok, 'errorInput>)
            : CancellableTaskValidation<'ok, 'errorOutput> =
            cancellableTask {
                let! input = input
                return Result.mapError (List.map errorMapper) input
            }

        let inline mapErrors
            ([<InlineIfLambda>] errorMapper: 'errorInput list -> 'errorOutput list)
            (input: CancellableTaskValidation<'ok, 'errorInput>)
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
            (ifError: CancellableTaskValidation<'input, 'errorOutput>)
            (cTask: CancellableTaskValidation<'input, 'errorInput>)
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
            (cTask: CancellableTaskValidation<'input, 'errorInput>)
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
            =
            cancellableTask {
                let! r1 = left
                let! r2 = right

                return
                    match r1, r2 with
                    | Ok x1res, Ok x2res -> Ok(x1res, x2res)
                    | Error e, Ok _ -> Error e
                    | Ok _, Error e -> Error e
                    | Error e1, Error e2 -> Error(e1 @ e2)
            }

        /// <summary>Takes two CancellableTaskValidation, starts them concurrently, and returns a tuple of the pair.</summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>A tuple of the parameters passed in.</returns>
        let inline parallelZip
            ([<InlineIfLambda>] left: CancellableTaskValidation<'left, 'error>)
            ([<InlineIfLambda>] right: CancellableTaskValidation<'right, 'error>)
            =
            cancellableTask {
                let! ct' = getCancellationToken ()

                match ct' with
                | Ok ct ->
                    return!
                        cancellableTask {
                            let! r1 = left ct
                            let! r2 = right ct

                            return
                                match r1, r2 with
                                | Ok x1res, Ok x2res -> Ok(x1res, x2res)
                                | Error e, Ok _ -> Error e
                                | Ok _, Error e -> Error e
                                | Error e1, Error e2 -> Error(e1 @ e2)
                        }
                | Error e -> return Error e
            }

    [<AutoOpen>]
    module JimmyToldMeTo =

        type CancellableTaskResultBuilderBase with

            member inline this.MergeSources
                (
                    left: CancellableTaskValidation<'left, 'error>,
                    right: CancellableTaskValidation<'right, 'error>
                ) : CancellableTaskValidation<'left * 'right, 'error> =
                cancellableTask {
                    let! ct = CancellableTask.getCancellationToken ()
                    let r1 = left ct
                    let r2 = right ct
                    let! r1' = r1
                    let! r2' = r2

                    return
                        match r1', r2' with
                        | Ok x1res, Ok x2res -> Ok(x1res, x2res)
                        | Error e, Ok _ -> Error e
                        | Ok _, Error e -> Error e
                        | Error e1, Error e2 -> Error(e1 @ e2)
                }
