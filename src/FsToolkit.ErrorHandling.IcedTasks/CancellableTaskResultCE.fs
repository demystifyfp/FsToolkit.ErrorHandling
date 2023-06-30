namespace FsToolkit.ErrorHandling


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
    open FsToolkit.ErrorHandling
    open IcedTasks

    /// CancellationToken -> Task<Result<'T, 'Error>>
    type CancellableTaskResult<'T, 'Error> = CancellableTask<Result<'T, 'Error>>


    /// The extra data stored in ResumableStateMachine for tasks
    [<Struct; NoComparison; NoEquality>]
    type CancellableTaskResultStateMachineData<'T, 'Error> =
        [<DefaultValue(false)>]
        val mutable CancellationToken: CancellationToken

        [<DefaultValue(false)>]
        val mutable Result: Result<'T, 'Error>

        [<DefaultValue(false)>]
        val mutable MethodBuilder: CancellableTaskResultMethodBuilder<'T, 'Error>


        member inline this.IsResultError = Result.isError this.Result
        member inline this.IsTaskCompleted = this.MethodBuilder.Task.IsCompleted

        member inline this.ThrowIfCancellationRequested() =
            this.CancellationToken.ThrowIfCancellationRequested()

    and CancellableTaskResultMethodBuilder<'TOverall, 'Error> =
        AsyncTaskMethodBuilder<Result<'TOverall, 'Error>>

    and CancellableTaskResultStateMachine<'TOverall, 'Error> =
        ResumableStateMachine<CancellableTaskResultStateMachineData<'TOverall, 'Error>>

    and CancellableTaskResultResumptionFunc<'TOverall, 'Error> =
        ResumptionFunc<CancellableTaskResultStateMachineData<'TOverall, 'Error>>

    and CancellableTaskResultResumptionDynamicInfo<'TOverall, 'Error> =
        ResumptionDynamicInfo<CancellableTaskResultStateMachineData<'TOverall, 'Error>>

    and CancellableTaskResultCode<'TOverall, 'Error, 'T> =
        ResumableCode<CancellableTaskResultStateMachineData<'TOverall, 'Error>, 'T>

    type CancellableTaskResultBuilderBase() =

        member inline _.Delay
            (generator: unit -> CancellableTaskResultCode<'TOverall, 'Error, 'T>)
            : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            CancellableTaskResultCode<'TOverall, 'Error, 'T>(fun sm ->
                sm.Data.ThrowIfCancellationRequested()
                (generator ()).Invoke(&sm)
            )

        /// Used to represent no-ops like the implicit empty "else" branch of an "if" expression.
        [<DefaultValue>]
        member inline _.Zero<'TOverall, 'Error>
            ()
            : CancellableTaskResultCode<'TOverall, 'Error, unit> =
            ResumableCode.Zero()

        member inline _.Return(value: 'T) : CancellableTaskResultCode<'T, 'Error, 'T> =
            CancellableTaskResultCode<'T, _, _>(fun sm ->
                sm.Data.ThrowIfCancellationRequested()
                sm.Data.Result <- Ok value
                true
            )


        /// Chains together a step with its following step.
        /// Note that this requires that the first step has no result.
        /// This prevents constructs like `task { return 1; return 2; }`.
        member inline _.Combine
            (
                task1: CancellableTaskResultCode<'TOverall, 'Error, unit>,
                task2: CancellableTaskResultCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            ResumableCode.Combine(
                CancellableTaskResultCode(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()
                    task1.Invoke(&sm)
                ),
                CancellableTaskResultCode<'TOverall, 'Error, 'T>(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()
                    if sm.Data.IsResultError then true else task2.Invoke(&sm)
                )
            )


        /// Builds a step that executes the body while the condition predicate is true.
        member inline _.While
            (
                [<InlineIfLambda>] condition: unit -> bool,
                body: CancellableTaskResultCode<'TOverall, 'Error, unit>
            ) : CancellableTaskResultCode<'TOverall, 'Error, unit> =
            let mutable keepGoing = true

            ResumableCode.While(
                (fun () ->
                    keepGoing
                    && condition ()
                ),
                CancellableTaskResultCode<_, _, _>(fun sm ->
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
                computation: CancellableTaskResultCode<'TOverall, 'Error, 'T>,
                catchHandler: exn -> CancellableTaskResultCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            ResumableCode.TryWith(
                CancellableTaskResultCode(fun sm ->
                    sm.Data.ThrowIfCancellationRequested()
                    computation.Invoke(&sm)
                ),
                catchHandler
            )

        /// Wraps a step in a try/finally. This catches exceptions both in the evaluation of the function
        /// to retrieve the step, and in the continuation of the step (if any).
        member inline _.TryFinally
            (
                computation: CancellableTaskResultCode<'TOverall, 'Error, 'T>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            ResumableCode.TryFinally(

                CancellableTaskResultCode(fun sm ->
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
                body: 'T -> CancellableTaskResultCode<'TOverall, 'Error, unit>
            ) : CancellableTaskResultCode<'TOverall, 'Error, unit> =
            ResumableCode.Using(
                sequence.GetEnumerator(),
                // ... and its body is a while loop that advances the enumerator and runs the body on each element.
                (fun e ->
                    this.While(
                        (fun () -> e.MoveNext()),
                        CancellableTaskResultCode<'TOverall, 'Error, unit>(fun sm ->
                            sm.Data.ThrowIfCancellationRequested()
                            (body e.Current).Invoke(&sm)
                        )
                    )
                )
            )

        member inline internal this.TryFinallyAsync
            (
                body: CancellableTaskResultCode<'TOverall, 'Error, 'T>,
                compensation: unit -> ValueTask
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
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
                            CancellableTaskResultResumptionFunc<'TOverall, 'Error>(fun sm ->
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
                body: 'Resource -> CancellableTaskResultCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
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
            (ctr: CancellableTaskResult<'T, 'Error>)
            : CancellableTaskResult<'T, 'Error> =
            ctr

        member inline this.Source(xs: #seq<_>) = xs

        member inline _.Source(result: TaskResult<_, _>) : CancellableTaskResult<_, _> =
            cancellableTask { return! result }

        member inline _.Source(result: Async<Result<_, _>>) : CancellableTaskResult<_, _> =
            cancellableTask { return! result }

        member inline this.Source(result: Async<Choice<_, _>>) : CancellableTaskResult<_, _> =
            result
            |> Async.map Result.ofChoice
            |> this.Source

        member inline _.Source(t: ValueTask<Result<_, _>>) : CancellableTaskResult<'T, 'Error> =
            cancellableTask { return! t }

        member inline _.Source(result: Result<_, _>) : CancellableTaskResult<_, _> =
            CancellableTask.singleton result

        member inline this.Source(result: Choice<_, _>) : CancellableTaskResult<_, _> =
            result
            |> Result.ofChoice
            |> this.Source


    type CancellableTaskResultBuilder() =

        inherit CancellableTaskResultBuilderBase()

        // This is the dynamic implementation - this is not used
        // for statically compiled tasks.  An executor (resumptionFuncExecutor) is
        // registered with the state machine, plus the initial resumption.
        // The executor stays constant throughout the execution, it wraps each step
        // of the execution in a try/with.  The resumption is changed at each step
        // to represent the continuation of the computation.
        static member inline RunDynamic
            (code: CancellableTaskResultCode<'T, 'Error, 'T>)
            : CancellableTaskResult<'T, 'Error> =

            let mutable sm = CancellableTaskResultStateMachine<'T, 'Error>()

            let initialResumptionFunc =
                CancellableTaskResultResumptionFunc<'T, 'Error>(fun sm -> code.Invoke(&sm))

            let resumptionInfo =
                { new CancellableTaskResultResumptionDynamicInfo<'T, 'Error>(initialResumptionFunc) with
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

                    sm.Data.MethodBuilder <- CancellableTaskResultMethodBuilder<'T, 'Error>.Create()

                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task

        member inline _.Run
            (code: CancellableTaskResultCode<'T, 'Error, 'T>)
            : CancellableTaskResult<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<CancellableTaskResultStateMachineData<'T, 'Error>, CancellableTaskResult<'T, 'Error>>
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
                                    CancellableTaskResultMethodBuilder<'T, 'Error>.Create()

                                sm.Data.MethodBuilder.Start(&sm)
                                sm.Data.MethodBuilder.Task
                    ))
            else
                CancellableTaskResultBuilder.RunDynamic(code)

    type BackgroundCancellableTaskResultBuilder() =

        inherit CancellableTaskResultBuilderBase()

        static member inline RunDynamic
            (code: CancellableTaskResultCode<'T, 'Error, 'T>)
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

        /// Same as CancellableTaskResultBuilder.Run except the start is inside Task.Run if necessary
        member inline _.Run
            (code: CancellableTaskResultCode<'T, 'Error, 'T>)
            : CancellableTaskResult<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<CancellableTaskResultStateMachineData<'T, 'Error>, CancellableTaskResult<'T, 'Error>>
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
                                        CancellableTaskResultMethodBuilder<'T, 'Error>.Create()

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
                                                CancellableTaskResultMethodBuilder<'T, 'Error>
                                                    .Create()

                                            sm.Data.MethodBuilder.Start(&sm)
                                            sm.Data.MethodBuilder.Task
                                        ),
                                        ct
                                    )
                    ))
            else
                BackgroundCancellableTaskResultBuilder.RunDynamic(code)


    [<AutoOpen>]
    module CancellableTaskResultBuilder =

        let cancellableTaskResult = CancellableTaskResultBuilder()
        let backgroundCancellableTaskResult = BackgroundCancellableTaskResultBuilder()

    [<AutoOpen>]
    module LowPriority =
        // Low priority extensions
        type CancellableTaskResultBuilderBase with

            [<NoEagerConstraintApplication>]
            static member inline BindDynamic<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
                (
                    sm:
                        byref<ResumableStateMachine<CancellableTaskResultStateMachineData<'TOverall, 'Error>>>,
                    getAwaiter: CancellationToken -> ^Awaiter,
                    continuation:
                        ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : bool =
                sm.Data.CancellationToken.ThrowIfCancellationRequested()

                let mutable awaiter = getAwaiter sm.Data.CancellationToken

                let cont =
                    (CancellableTaskResultResumptionFunc<'TOverall, 'Error>(fun sm ->
                        let result =
                            (^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>) (awaiter))

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
                and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
                (
                    getAwaiter: CancellationToken -> ^Awaiter,
                    continuation:
                        ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : CancellableTaskResultCode<'TOverall, 'Error, 'TResult2> =

                CancellableTaskResultCode<'TOverall, _, _>(fun sm ->
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
                                (^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>) (awaiter))

                            match result with
                            | Ok result -> (continuation result).Invoke(&sm)
                            | Error e ->
                                sm.Data.Result <- Error e
                                true
                        else
                            sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                            false
                    else
                        CancellableTaskResultBuilderBase.BindDynamic<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error>(
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
                and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
                (getAwaiter: CancellationToken -> ^Awaiter)
                : CancellableTaskResultCode<'TResult1, 'Error, 'TResult1> =

                this.Bind(getAwaiter, (fun v -> this.Return v))


            [<NoEagerConstraintApplication>]
            member inline _.Source<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
                (getAwaiter: CancellationToken -> ^Awaiter)
                : CancellationToken -> ^Awaiter =
                getAwaiter

            [<NoEagerConstraintApplication>]
            member inline _.Source< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
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
                : CancellableTaskResult<'T, 'Error> =

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
                : CancellableTaskResult<'T, 'Error> =

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
                : CancellableTaskResult<'T, 'Error> =

                cancellableTask {
                    let! r = t
                    return Ok r
                }

            member inline _.Using<'Resource, 'TOverall, 'Error, 'T when 'Resource :> IDisposable>
                (
                    resource: 'Resource,
                    binder: 'Resource -> CancellableTaskResultCode<'TOverall, 'Error, 'T>
                ) =
                ResumableCode.Using(
                    resource,
                    fun resource ->
                        CancellableTaskResultCode<'TOverall, 'Error, 'T>(fun sm ->
                            sm.Data.ThrowIfCancellationRequested()
                            (binder resource).Invoke(&sm)
                        )
                )

    [<AutoOpen>]
    module HighPriority =
        type Microsoft.FSharp.Control.Async with

            static member inline AwaitCancellableTaskResult
                ([<InlineIfLambda>] t: CancellableTaskResult<'T, 'Error>)
                =
                async {
                    let! ct = Async.CancellationToken

                    return!
                        t ct
                        |> Async.AwaitTask
                }

            static member inline AsCancellableTaskResult(computation: Async<'T>) =
                fun ct -> Async.StartImmediateAsTask(computation, cancellationToken = ct)

        type CancellableTaskResultBuilderBase with

            member inline this.Bind
                (
                    task: CancellableTaskResult<'TResult1, 'Error>,
                    continuation:
                        ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : CancellableTaskResultCode<'TOverall, 'Error, 'TResult2> =
                let x = fun ct -> (task ct).GetAwaiter()
                this.Bind(x, continuation)

            member inline this.ReturnFrom
                (task: CancellableTaskResult<'T, 'Error>)
                : CancellableTaskResultCode<'T, 'Error, 'T> =
                this.Bind(task, (fun v -> this.Return v))


    [<AutoOpen>]
    module MediumPriority =
        open HighPriority

        type CancellableTaskResultBuilder with

            member inline _.Source(t: Task<'T>) =
                cancellableTask {
                    let! r = t
                    return Ok r
                }

            member inline _.Source(result: CancellableTask<'T>) =
                cancellableTask {
                    let! r = result
                    return Ok r
                }

            member inline _.Source(result: CancellableTask) : CancellableTaskResult<unit, 'Error> =
                cancellableTask {
                    let! r = result
                    return Ok r
                }

            member inline _.Source(result: ColdTask<_>) : CancellableTaskResult<_, _> =
                cancellableTask {
                    let! r = result
                    return Ok r
                }

            member inline _.Source(result: ColdTask) : CancellableTaskResult<_, _> =
                cancellableTask {
                    let! r = result
                    return Ok r
                }

            member inline _.Source(t: Async<'T>) : CancellableTaskResult<'T, 'Error> =
                cancellableTask {
                    let! r = t
                    return Ok r
                }


    [<AutoOpen>]
    module AsyncExtenions =
        type Microsoft.FSharp.Control.AsyncBuilder with

            member inline this.Bind
                (
                    [<InlineIfLambda>] t: CancellableTaskResult<'T, 'Error>,
                    [<InlineIfLambda>] binder: (_ -> Async<_>)
                ) : Async<_> =
                this.Bind(Async.AwaitCancellableTaskResult t, binder)

            member inline this.ReturnFrom([<InlineIfLambda>] t: CancellableTaskResult<'T, 'Error>) =
                this.ReturnFrom(Async.AwaitCancellableTaskResult t)


        type FsToolkit.ErrorHandling.AsyncResultCE.AsyncResultBuilder with

            member inline this.Source
                ([<InlineIfLambda>] t: CancellableTaskResult<'T, 'Error>)
                : Async<_> =
                Async.AwaitCancellableTaskResult t

    // There is explicitly no Binds for `CancellableTaskResults` in `Microsoft.FSharp.Control.TaskBuilderBase`.
    // You need to explicitly pass in a `CancellationToken`to start it, you can use `CancellationToken.None`.
    // Reason is I don't want people to assume cancellation is happening without the caller being explicit about where the CancellationToken came from.

    [<RequireQualifiedAccess>]
    module CancellableTaskResult =
        let getCancellationToken () : CancellableTaskResult<CancellationToken, 'Error> =
            CancellableTaskResultBuilder.cancellableTaskResult.Run(
                CancellableTaskResultCode<_, 'Error, _>(fun sm ->
                    sm.Data.Result <- Ok sm.Data.CancellationToken
                    true
                )
            )

        /// <summary>Lifts an item to a CancellableTaskResult.</summary>
        /// <param name="item">The item to be the result of the CancellableTaskResult.</param>
        /// <returns>A CancellableTaskResult with the item as the result.</returns>
        let inline singleton (item: 'item) : CancellableTaskResult<'item, 'Error> =
            fun _ -> Task.FromResult(Ok item)


        /// <summary>Allows chaining of CancellableTaskResult.</summary>
        /// <param name="binder">The continuation.</param>
        /// <param name="cTask">The value.</param>
        /// <returns>The result of the binder.</returns>
        let inline bind
            ([<InlineIfLambda>] binder: 'input -> CancellableTaskResult<'output, 'error>)
            ([<InlineIfLambda>] cTask: CancellableTaskResult<'input, 'error>)
            =
            cancellableTaskResult {
                let! cResult = cTask
                return! binder cResult
            }

        /// <summary>Allows chaining of CancellableTaskResult.</summary>
        /// <param name="mapper">The continuation.</param>
        /// <param name="cTask">The value.</param>
        /// <returns>The result of the mapper wrapped in a CancellableTaskResult.</returns>
        let inline map
            ([<InlineIfLambda>] mapper: 'input -> 'output)
            ([<InlineIfLambda>] cTask: CancellableTaskResult<'input, 'error>)
            =
            cancellableTaskResult {
                let! cResult = cTask
                return mapper cResult
            }

        /// <summary>Allows chaining of CancellableTaskResult.</summary>
        /// <param name="applicable">A function wrapped in a CancellableTaskResult</param>
        /// <param name="cTask">The value.</param>
        /// <returns>The result of the applicable.</returns>
        let inline apply
            ([<InlineIfLambda>] applicable: CancellableTaskResult<'input -> 'output, 'error>)
            ([<InlineIfLambda>] cTask: CancellableTaskResult<'input, 'error>)
            =
            cancellableTaskResult {
                let! applier = applicable
                let! cResult = cTask
                return applier cResult
            }

        /// <summary>Takes two CancellableTaskResult, starts them serially in order of left to right, and returns a tuple of the pair.</summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>A tuple of the parameters passed in</returns>
        let inline zip
            ([<InlineIfLambda>] left: CancellableTaskResult<'left, 'error>)
            ([<InlineIfLambda>] right: CancellableTaskResult<'right, 'error>)
            =
            cancellableTaskResult {
                let! r1 = left
                let! r2 = right
                return r1, r2
            }

        /// <summary>Takes two CancellableTaskResult, starts them concurrently, and returns a tuple of the pair.</summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>A tuple of the parameters passed in.</returns>
        let inline parallelZip
            ([<InlineIfLambda>] left: CancellableTaskResult<'left, 'error>)
            ([<InlineIfLambda>] right: CancellableTaskResult<'right, 'error>)
            =
            cancellableTaskResult {
                let! ct = getCancellationToken ()
                let r1 = left ct
                let r2 = right ct
                let! r1 = r1
                let! r2 = r2
                return r1, r2
            }
