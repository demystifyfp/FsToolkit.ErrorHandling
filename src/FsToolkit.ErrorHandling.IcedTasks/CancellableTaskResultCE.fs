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


        member this.IsResultError = Result.isError this.Result
        member this.IsTaskCompleted = this.MethodBuilder.Task.IsCompleted


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


    module CancellableTaskResultBuilderBase =

        let rec WhileDynamic
            (
                sm: byref<CancellableTaskResultStateMachine<_, _>>,
                condition: unit -> bool,
                body: CancellableTaskResultCode<_, _, _>
            ) : bool =
            if condition () then
                if body.Invoke(&sm) then
                    if sm.Data.IsResultError then
                        // Set the result now to allow short-circuiting of the rest of the CE.
                        // Run/RunDynamic will skip setting the result if it's already been set.
                        // Combine/CombineDynamic will not continue if the result has been set.
                        sm.Data.MethodBuilder.SetResult sm.Data.Result
                        true
                    else
                        WhileDynamic(&sm, condition, body)
                else
                    let rf = sm.ResumptionDynamicInfo.ResumptionFunc

                    sm.ResumptionDynamicInfo.ResumptionFunc <-
                        (CancellableTaskResultResumptionFunc<_, _>(fun sm ->
                            WhileBodyDynamicAux(&sm, condition, body, rf)
                        ))

                    false
            else
                true

        and WhileBodyDynamicAux
            (
                sm: byref<CancellableTaskResultStateMachine<_, _>>,
                condition: unit -> bool,
                body: CancellableTaskResultCode<_, _, _>,
                rf: CancellableTaskResultResumptionFunc<_, _>
            ) : bool =
            if rf.Invoke(&sm) then
                if sm.Data.IsResultError then
                    // Set the result now to allow short-circuiting of the rest of the CE.
                    // Run/RunDynamic will skip setting the result if it's already been set.
                    // Combine/CombineDynamic will not continue if the result has been set.
                    sm.Data.MethodBuilder.SetResult sm.Data.Result
                    true
                else
                    WhileDynamic(&sm, condition, body)
            else
                let rf = sm.ResumptionDynamicInfo.ResumptionFunc

                sm.ResumptionDynamicInfo.ResumptionFunc <-
                    (CancellableTaskResultResumptionFunc<_, _>(fun sm ->
                        WhileBodyDynamicAux(&sm, condition, body, rf)
                    ))

                false

    type CancellableTaskResultBuilderBase() =


        member inline _.Delay
            (generator: unit -> CancellableTaskResultCode<'TOverall, 'Error, 'T>)
            : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            CancellableTaskResultCode<'TOverall, 'Error, 'T>(fun sm -> (generator ()).Invoke(&sm))

        /// Used to represent no-ops like the implicit empty "else" branch of an "if" expression.
        // [<DefaultValue>]
        member inline _.Zero<'TOverall, 'Error>
            ()
            : CancellableTaskResultCode<'TOverall, 'Error, unit> =
            ResumableCode.Zero()

        member inline _.Return(value: 'T) : CancellableTaskResultCode<'T, 'Error, 'T> =
            CancellableTaskResultCode<'T, _, _>(fun sm ->
                sm.Data.Result <- Ok value
                true
            )


        static member inline CombineDynamic
            (
                sm: byref<CancellableTaskResultStateMachine<_, _>>,
                task1: CancellableTaskResultCode<'TOverall, 'Error, unit>,
                task2: CancellableTaskResultCode<'TOverall, 'Error, 'T>
            ) : bool =
            let shouldContinue = task1.Invoke(&sm)

            if sm.Data.IsTaskCompleted then
                true
            elif shouldContinue then
                task2.Invoke(&sm)
            else
                let rec resume (mf: CancellableTaskResultResumptionFunc<_, _>) =
                    CancellableTaskResultResumptionFunc<_, _>(fun sm ->
                        let shouldContinue = mf.Invoke(&sm)

                        if sm.Data.IsTaskCompleted then
                            true
                        elif shouldContinue then
                            task2.Invoke(&sm)
                        else
                            sm.ResumptionDynamicInfo.ResumptionFunc <-
                                (resume (sm.ResumptionDynamicInfo.ResumptionFunc))

                            false
                    )

                sm.ResumptionDynamicInfo.ResumptionFunc <-
                    (resume (sm.ResumptionDynamicInfo.ResumptionFunc))

                false

        /// Chains together a step with its following step.
        /// Note that this requires that the first step has no result.
        /// This prevents constructs like `task { return 1; return 2; }`.
        member inline _.Combine
            (
                task1: CancellableTaskResultCode<'TOverall, 'Error, unit>,
                task2: CancellableTaskResultCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =

            CancellableTaskResultCode<'TOverall, 'Error, 'T>(fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    // NOTE: The code for code1 may contain await points! Resuming may branch directly
                    // into this code!
                    // printfn "Combine Called Before Invoke --> "
                    let __stack_fin = task1.Invoke(&sm)
                    // printfn "Combine Called After Invoke --> %A " sm.Data.MethodBuilder.Task.Status

                    if sm.Data.IsTaskCompleted then true
                    elif __stack_fin then task2.Invoke(&sm)
                    else false
                else
                    CancellableTaskResultBuilderBase.CombineDynamic(&sm, task1, task2)
            )


        /// Builds a step that executes the body while the condition predicate is true.
        member inline _.While
            (
                [<InlineIfLambda>] condition: unit -> bool,
                body: CancellableTaskResultCode<'TOverall, 'Error, unit>
            ) : CancellableTaskResultCode<'TOverall, 'Error, unit> =
            CancellableTaskResultCode<'TOverall, 'Error, unit>(fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    let mutable __stack_go = true

                    while __stack_go
                          && not sm.Data.IsResultError
                          && condition () do
                        // NOTE: The body of the state machine code for 'while' may contain await points, so resuming
                        // the code will branch directly into the expanded 'body', branching directly into the while loop
                        let __stack_body_fin = body.Invoke(&sm)
                        // printfn "While After Invoke --> %A" sm.Data.Result
                        // If the body completed, we go back around the loop (__stack_go = true)
                        // If the body yielded, we yield (__stack_go = false)
                        __stack_go <- __stack_body_fin

                    if sm.Data.IsResultError then
                        // Set the result now to allow short-circuiting of the rest of the CE.
                        // Run/RunDynamic will skip setting the result if it's already been set.
                        // Combine/CombineDynamic will not continue if the result has been set.
                        sm.Data.MethodBuilder.SetResult sm.Data.Result

                    __stack_go
                //-- RESUMABLE CODE END
                else
                    // failwith "no dynamic yet"
                    CancellableTaskResultBuilderBase.WhileDynamic(&sm, condition, body)
            )

        /// Wraps a step in a try/with. This catches exceptions both in the evaluation of the function
        /// to retrieve the step, and in the continuation of the step (if any).
        member inline _.TryWith
            (
                body: CancellableTaskResultCode<'TOverall, 'Error, 'T>,
                catch: exn -> CancellableTaskResultCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            ResumableCode.TryWith(body, catch)

        /// Wraps a step in a try/finally. This catches exceptions both in the evaluation of the function
        /// to retrieve the step, and in the continuation of the step (if any).
        member inline _.TryFinally
            (
                body: CancellableTaskResultCode<'TOverall, 'Error, 'T>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            ResumableCode.TryFinally(
                body,
                ResumableCode<_, _>(fun _sm ->
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
        // member inline this.Source(ctr: CancellableTaskResultCode<'T,'Error,'T>) : CancellableTaskResultCode<'T,'Error,'T> = ctr


        member inline _.Source(result: TaskResult<_, _>) : CancellableTaskResult<_, _> =
            fun _ -> result

        member inline _.Source(result: Async<Result<_, _>>) : CancellableTaskResult<_, _> =
            fun ct -> Async.StartAsTask(result, cancellationToken = ct)

        member inline _.Source(result: Async<Choice<_, _>>) : CancellableTaskResult<_, _> =
            let result =
                result
                |> Async.map Result.ofChoice

            fun ct -> Async.StartAsTask(result, cancellationToken = ct)

        member inline _.Source(t: ValueTask<Result<_, _>>) : CancellableTaskResult<'T, 'Error> =
            fun _ -> task { return! t }

        member inline _.Source(result: Result<_, _>) : CancellableTaskResult<_, _> =
            fun _ -> Task.singleton result

        member inline _.Source(result: Choice<_, _>) : CancellableTaskResult<_, _> =
            fun _ ->
                result
                |> Result.ofChoice
                |> Task.singleton

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

                    sm.Data.MethodBuilder <-
                        CancellableTaskResultMethodBuilder<'T, 'Error>.Create ()

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
                                    CancellableTaskResultMethodBuilder<'T, 'Error>.Create ()

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

        //// Same as CancellableTaskResultBuilder.Run except the start is inside Task.Run if necessary
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
                                        CancellableTaskResultMethodBuilder<'T, 'Error>.Create ()

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
                                                CancellableTaskResultMethodBuilder<'T, 'Error>.Create
                                                    ()

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
            static member inline BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
                (
                    sm: byref<ResumableStateMachine<CancellableTaskResultStateMachineData<'TOverall, 'Error>>>,
                    task: CancellationToken -> ^TaskLike,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : bool =
                sm.Data.CancellationToken.ThrowIfCancellationRequested()

                let mutable awaiter =
                    (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task
                                                                           sm.Data.CancellationToken))

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
            member inline _.Bind< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
                (
                    task: CancellationToken -> ^TaskLike,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : CancellableTaskResultCode<'TOverall, 'Error, 'TResult2> =

                CancellableTaskResultCode<'TOverall, _, _>(fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        sm.Data.CancellationToken.ThrowIfCancellationRequested()
                        // Get an awaiter from the awaitable
                        let mutable awaiter =
                            (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task
                                                                                   sm.Data.CancellationToken))

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
                        CancellableTaskResultBuilderBase.BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error>(
                            &sm,
                            task,
                            continuation
                        )
                //-- RESUMABLE CODE END
                )

            [<NoEagerConstraintApplication>]
            static member inline BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'TResult1)>
                (
                    sm: byref<ResumableStateMachine<CancellableTaskResultStateMachineData<'TOverall, 'Error>>>,
                    task: ^TaskLike,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : bool =
                sm.Data.CancellationToken.ThrowIfCancellationRequested()

                let mutable awaiter = (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task))

                let cont =
                    (CancellableTaskResultResumptionFunc<'TOverall, 'Error>(fun sm ->
                        let result = (^Awaiter: (member GetResult: unit -> 'TResult1) (awaiter))

                        (continuation result).Invoke(&sm)
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
            member inline this.Bind< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'TResult1)>
                (
                    task: ^TaskLike,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : CancellableTaskResultCode<'TOverall, 'Error, 'TResult2> =


                CancellableTaskResultCode<'TOverall, _, _>(fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        sm.Data.CancellationToken.ThrowIfCancellationRequested()
                        // Get an awaiter from the awaitable
                        let mutable awaiter =
                            (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task))

                        let mutable __stack_fin = true

                        if not (^Awaiter: (member get_IsCompleted: unit -> bool) (awaiter)) then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result = (^Awaiter: (member GetResult: unit -> 'TResult1) (awaiter))

                            (continuation result).Invoke(&sm)

                        else
                            sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                            false

                    else

                        CancellableTaskResultBuilderBase.BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error>(
                            &sm,
                            task,
                            continuation
                        )
                //-- RESUMABLE CODE END
                )

            [<NoEagerConstraintApplication>]
            member inline this.ReturnFrom< ^TaskLike, ^Awaiter, 'T, 'Error
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (task: ^TaskLike)
                : CancellableTaskResultCode<'T, 'Error, 'T> =

                this.Bind(task, (fun v -> this.Return v))

            member inline _.Using<'Resource, 'TOverall, 'Error, 'T when 'Resource :> IDisposable>
                (
                    resource: 'Resource,
                    body: 'Resource -> CancellableTaskResultCode<'TOverall, 'Error, 'T>
                ) =
                ResumableCode.Using(resource, body)


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
                fun ct -> Async.StartAsTask(computation, cancellationToken = ct)


        type CancellableTaskResultBuilder with

            [<NoEagerConstraintApplication>]
            member inline this.Source< ^TaskLike, ^Awaiter, 'T
                when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit -> 'T)>
                (t: ^TaskLike)
                : CancellableTaskResult<_, _> =
                // : CancellableTaskResultCode<_,_,_> =
                // TODO: Gets errors like "error FS0073: internal error: Undefined or unsolved type variable:  ^Awaiter"
                // this.Run(this.ReturnFrom(t))
                // this.ReturnFrom(t)
                fun _ -> task {
                    let! r = t
                    return Ok r
                }

        // High priority extensions
        type CancellableTaskResultBuilderBase with

            static member inline BindDynamic
                (
                    sm: byref<ResumableStateMachine<CancellableTaskResultStateMachineData<'TOverall, 'Error>>>,
                    task: CancellableTaskResult<'TResult1, 'Error>,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : bool =
                let mutable awaiter =
                    let ct = sm.Data.CancellationToken

                    if ct.IsCancellationRequested then
                        Task.FromCanceled<_>(ct).GetAwaiter()
                    else
                        task(ct).GetAwaiter()

                let cont =
                    (CancellableTaskResultResumptionFunc<'TOverall, 'Error>(fun sm ->
                        let result = awaiter.GetResult()

                        match result with
                        | Ok result -> (continuation result).Invoke(&sm)
                        | Error e ->
                            sm.Data.Result <- Error e
                            true
                    ))

                // shortcut to continue immediately
                if awaiter.IsCompleted then
                    cont.Invoke(&sm)
                else

                    sm.ResumptionDynamicInfo.ResumptionData <-
                        (awaiter :> ICriticalNotifyCompletion)

                    sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                    false

            member inline _.Bind
                (
                    task: CancellableTaskResult<'TResult1, 'Error>,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : CancellableTaskResultCode<'TOverall, 'Error, 'TResult2> =

                CancellableTaskResultCode<'TOverall, 'Error, _>(fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        // Get an awaiter from the task
                        let mutable awaiter =
                            let ct = sm.Data.CancellationToken

                            if ct.IsCancellationRequested then
                                Task.FromCanceled<_>(ct).GetAwaiter()
                            else
                                task(ct).GetAwaiter()

                        let mutable __stack_fin = true

                        if not awaiter.IsCompleted then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result = awaiter.GetResult()

                            match result with
                            | Ok result -> (continuation result).Invoke(&sm)
                            | Error e ->
                                sm.Data.Result <- Error e
                                true
                        else
                            sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                            false
                    else
                        CancellableTaskResultBuilderBase.BindDynamic(&sm, task, continuation)
                //-- RESUMABLE CODE END
                )


            member inline this.ReturnFrom
                (task: CancellableTaskResult<'T, 'Error>)
                : CancellableTaskResultCode<'T, 'Error, 'T> =
                this.Bind((task), (fun v -> this.Return v))


    [<AutoOpen>]
    module MediumPriority =
        open HighPriority

        type CancellableTaskResultBuilder with

            member inline this.Source
                (t: Task<'T>)
                // : CancellableTaskResult<'T, 'Error> =
                =
                // this.ReturnFrom(t)
                fun _ -> task {
                    let! r = t
                    return Ok r
                }

            member inline _.Source
                (result: CancellableTask<'T>)
                : CancellableTaskResult<'T, 'Error> =
                cancellableTask {
                    let! r = result
                    return Ok r
                }


            member inline _.Source(result: CancellableTask) : CancellableTaskResult<unit, 'Error> = cancellableTask {
                let! r = result
                return Ok r
            }

            member inline _.Source(result: ColdTask<_>) : CancellableTaskResult<_, _> =
                fun _ ->
                // TODO: using `coldTask` results in "internal error: The local field ResumptionDynamicInfo was referenced but not declare" compliation error
                task {
                    let! r = result ()
                    return Ok r
                }

            member inline _.Source(result: ColdTask) : CancellableTaskResult<_, _> =
                fun _ ->
                // TODO: using `coldTask` results in "internal error: The local field ResumptionDynamicInfo was referenced but not declare" compliation error
                task {
                    let! r = result ()
                    return Ok r
                }

        // Medium priority extensions
        type CancellableTaskResultBuilderBase with

            member inline this.Source(t: Async<'T>) : CancellableTaskResult<'T, 'Error> =
                fun ct ->
                    let t =
                        t
                        |> Async.map Ok

                    Async.StartAsTask(t, cancellationToken = ct)


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
