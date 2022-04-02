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
    type CancellableTaskResult<'T,'Error> = CancellationToken -> TaskResult<'T,'Error>


    /// The extra data stored in ResumableStateMachine for tasks
    [<Struct; NoComparison; NoEquality>]
    type CancellableTaskResultStateMachineData<'T, 'Error> =
        [<DefaultValue(false)>]
        val mutable CancellationToken: CancellationToken

        [<DefaultValue(false)>]
        val mutable Result: Result<'T, 'Error>

        [<DefaultValue(false)>]
        val mutable MethodBuilder: CancellableTaskResultMethodBuilder<'T, 'Error>


    and CancellableTaskResultMethodBuilder<'TOverall, 'Error> = AsyncTaskMethodBuilder<Result<'TOverall, 'Error>>
    and CancellableTaskResultStateMachine<'TOverall, 'Error> = ResumableStateMachine<CancellableTaskResultStateMachineData<'TOverall, 'Error>>
    and CancellableTaskResultResumptionFunc<'TOverall, 'Error>= ResumptionFunc<CancellableTaskResultStateMachineData<'TOverall, 'Error>>

    and CancellableTaskResultResumptionDynamicInfo<'TOverall, 'Error> =
        ResumptionDynamicInfo<CancellableTaskResultStateMachineData<'TOverall, 'Error>>

    and CancellableTaskResultCode<'TOverall, 'Error, 'T> = ResumableCode<CancellableTaskResultStateMachineData<'TOverall, 'Error>, 'T>

    type CancellableTaskResultBuilderBase() =


        member inline _.Delay
            (generator: unit -> CancellableTaskResultCode<'TOverall, 'Error, 'T>)
            : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            CancellableTaskResultCode<'TOverall, 'Error, 'T>(fun sm -> (generator ()).Invoke(&sm))

        /// Used to represent no-ops like the implicit empty "else" branch of an "if" expression.
        // [<DefaultValue>]
        member inline _.Zero<'TOverall, 'Error>() : CancellableTaskResultCode<'TOverall, 'Error, unit> = 
            ResumableCode.Zero()

        member inline _.Return(value: 'T) : CancellableTaskResultCode<'T, 'Error, 'T> =
            CancellableTaskResultCode<'T, _, _> (fun sm ->
                sm.Data.Result <- Ok value
                true)

        /// Chains together a step with its following step.
        /// Note that this requires that the first step has no result.
        /// This prevents constructs like `task { return 1; return 2; }`.
        member inline _.Combine
            (
                task1: CancellableTaskResultCode<'TOverall, 'Error, unit>,
                task2: CancellableTaskResultCode<'TOverall, 'Error, 'T>
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            ResumableCode.Combine(task1, task2)

        /// Builds a step that executes the body while the condition predicate is true.
        member inline _.While
            (
                [<InlineIfLambda>] condition: unit -> bool,
                body: CancellableTaskResultCode<'TOverall, 'Error, unit>
            ) : CancellableTaskResultCode<'TOverall, 'Error, unit> =
            ResumableCode.While(condition, body)

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
                ResumableCode<_, _> (fun _sm ->
                    compensation ()
                    true)
            )

        member inline _.For
            (
                sequence: seq<'T>,
                body: 'T -> CancellableTaskResultCode<'TOverall, 'Error, unit>
            ) : CancellableTaskResultCode<'TOverall, 'Error, unit> =
            ResumableCode.For(sequence, body)

        member inline internal this.TryFinallyAsync
            (
                body: CancellableTaskResultCode<'TOverall, 'Error, 'T>,
                compensation: unit -> ValueTask
            ) : CancellableTaskResultCode<'TOverall, 'Error, 'T> =
            ResumableCode.TryFinallyAsync(
                body,
                ResumableCode<_, _> (fun sm ->
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
                            CancellableTaskResultResumptionFunc<'TOverall, 'Error> (fun sm ->
                                awaiter.GetResult() |> ignore
                                true)

                        // shortcut to continue immediately
                        if awaiter.IsCompleted then
                            true
                        else
                            sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)
                            sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                            false)
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
                        ValueTask())
            )

        member inline this.Source(ctr: CancellableTaskResult<'T, 'Error>) : CancellableTaskResult<'T, 'Error> = ctr

        member inline _.Source(result: TaskResult<_, _>) : CancellableTaskResult<_, _> = fun _ -> result 
        member inline _.Source(result: Async<Result<_, _>>) : CancellableTaskResult<_, _> = fun ct -> Async.StartAsTask(result, cancellationToken=ct)
        
        member inline _.Source(result: Async<Choice<_, _>>) : CancellableTaskResult<_, _> = 
            let result = result |> Async.map Result.ofChoice
            fun ct -> 
                Async.StartAsTask(result, cancellationToken=ct)
        member inline _.Source(t: ValueTask<Result<_, _>>) : CancellableTaskResult<'T, 'Error> = fun _ -> task { return! t }

        member inline _.Source(result: Result<_, _>) : CancellableTaskResult<_, _> = fun _ -> Task.singleton result
        member inline _.Source(result: Choice<_, _>) : CancellableTaskResult<_, _> = fun _ -> result |> Result.ofChoice |> Task.singleton 

        // TODO: Use backgroundTask?
        member inline _.Source(result: ColdTask<_>) : CancellableTaskResult<_, _> = 
            fun _ -> 
                // TODO: using `coldTask` results in "internal error: The local field ResumptionDynamicInfo was referenced but not declare" compliation error
                task {
                    let! r = result ()
                    return Ok r
                } 
        // TODO: Use backgroundTask?
        member inline _.Source(result: ColdTask) : CancellableTaskResult<_, _> = 
            fun _ -> 
                // TODO: using `coldTask` results in "internal error: The local field ResumptionDynamicInfo was referenced but not declare" compliation error
                task {
                    let! r = result ()
                    return Ok r
                } 
                
       

          
    type CancellableTaskResultBuilder() =

        inherit CancellableTaskResultBuilderBase()

        // This is the dynamic implementation - this is not used
        // for statically compiled tasks.  An executor (resumptionFuncExecutor) is
        // registered with the state machine, plus the initial resumption.
        // The executor stays constant throughout the execution, it wraps each step
        // of the execution in a try/with.  The resumption is changed at each step
        // to represent the continuation of the computation.
        static member inline RunDynamic(code: CancellableTaskResultCode<'T, 'Error, 'T>) : CancellableTaskResult<'T, 'Error> =

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

                            if step then
                                sm.Data.MethodBuilder.SetResult(sm.Data.Result)
                            else
                                let mutable awaiter =
                                    sm.ResumptionDynamicInfo.ResumptionData :?> ICriticalNotifyCompletion

                                assert not (isNull awaiter)
                                sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)

                        with
                        | exn -> savedExn <- exn
                        // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                        match savedExn with
                        | null -> ()
                        | exn -> sm.Data.MethodBuilder.SetException exn

                    member _.SetStateMachine(sm, state) =
                        sm.Data.MethodBuilder.SetStateMachine(state) }

            fun (ct) ->
                if ct.IsCancellationRequested then
                    Task.FromCanceled<_>(ct)
                else
                    sm.Data.CancellationToken <- ct
                    sm.ResumptionDynamicInfo <- resumptionInfo
                    sm.Data.MethodBuilder <- CancellableTaskResultMethodBuilder<'T, 'Error>.Create ()
                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task

        member inline _.Run(code: CancellableTaskResultCode<'T, 'Error, 'T>) : CancellableTaskResult<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<CancellableTaskResultStateMachineData<'T, 'Error>, CancellableTaskResult<'T, 'Error>>
                    (MoveNextMethodImpl<_> (fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: Exception = null

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if __stack_code_fin then
                                sm.Data.MethodBuilder.SetResult(sm.Data.Result)
                        with
                        | exn -> __stack_exn <- exn
                        // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                        match __stack_exn with
                        | null -> ()
                        | exn -> sm.Data.MethodBuilder.SetException exn
                    //-- RESUMABLE CODE END
                    ))
                    (SetStateMachineMethodImpl<_>(fun sm state -> sm.Data.MethodBuilder.SetStateMachine(state)))
                    (AfterCode<_, _> (fun sm ->
                        let sm = sm

                        fun (ct) ->
                            if ct.IsCancellationRequested then
                                Task.FromCanceled<_>(ct)
                            else
                                let mutable sm = sm
                                sm.Data.CancellationToken <- ct
                                sm.Data.MethodBuilder <- CancellableTaskResultMethodBuilder<'T, 'Error>.Create ()
                                sm.Data.MethodBuilder.Start(&sm)
                                sm.Data.MethodBuilder.Task))
            else
                CancellableTaskResultBuilder.RunDynamic(code)

    type BackgroundCancellableTaskResultBuilder() =

        inherit CancellableTaskResultBuilderBase()

        static member inline RunDynamic(code: CancellableTaskResultCode<'T, 'Error, 'T>) : CancellableTaskResult<'T, 'Error> =
            // backgroundTask { .. } escapes to a background thread where necessary
            // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
            if
                isNull SynchronizationContext.Current
                && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
            then
                CancellableTaskResultBuilder.RunDynamic(code)
            else
                fun (ct) -> Task.Run<Result<'T, 'Error>>((fun () -> CancellableTaskResultBuilder.RunDynamic(code) (ct)), ct)

        //// Same as CancellableTaskResultBuilder.Run except the start is inside Task.Run if necessary
        member inline _.Run(code: CancellableTaskResultCode<'T, 'Error, 'T>) : CancellableTaskResult<'T,'Error> =
            if __useResumableCode then
                __stateMachine<CancellableTaskResultStateMachineData<'T,'Error>, CancellableTaskResult<'T,'Error>>
                    (MoveNextMethodImpl<_> (fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if __stack_code_fin then
                                sm.Data.MethodBuilder.SetResult(sm.Data.Result)
                        with
                        | exn -> sm.Data.MethodBuilder.SetException exn
                    //-- RESUMABLE CODE END
                    ))
                    (SetStateMachineMethodImpl<_>(fun sm state -> sm.Data.MethodBuilder.SetStateMachine(state)))
                    (AfterCode<_, CancellableTaskResult<'T,'Error>> (fun sm ->
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
                                    sm.Data.MethodBuilder <- CancellableTaskResultMethodBuilder<'T,'Error>.Create ()
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
                                            sm.Data.MethodBuilder <- CancellableTaskResultMethodBuilder<'T,'Error>.Create ()
                                            sm.Data.MethodBuilder.Start(&sm)
                                            sm.Data.MethodBuilder.Task),
                                        ct
                                    )))

            else
                BackgroundCancellableTaskResultBuilder.RunDynamic(code)


    [<AutoOpen>]
    module CancellableTaskResultBuilder =

        let cancellableTaskResult = CancellableTaskResultBuilder()
        let backgroundCancellableTaskResult = BackgroundCancellableTaskResultBuilder()

    [<AutoOpen>]
    module LowPriority =
        // Low priority extensions           
        
        type BackgroundCancellableTaskResultBuilder with
            [<NoEagerConstraintApplication>]
            member inline this.Source< ^TaskLike2, ^Awaiter2, 'T, 'Error when ^TaskLike2: (member GetAwaiter : unit -> ^Awaiter2) and ^Awaiter2 :> ICriticalNotifyCompletion and ^Awaiter2: (member get_IsCompleted :
                unit -> bool) and ^Awaiter2: (member GetResult : unit -> 'T)>
                (t: ^TaskLike2)
                : CancellableTaskResult<'T, 'Error> =
                // TODO: Gets errors like "error FS0073: internal error: Undefined or unsolved type variable:  ^Awaiter"
                // this.Run(this.ReturnFrom(t))
                fun _ ->
                    backgroundTask {
                        let! result = t
                        return Ok result
                    }
        type CancellableTaskResultBuilder with
            
            [<NoEagerConstraintApplication>]
            member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error when ^TaskLike: (member GetAwaiter : unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted :
                unit -> bool) and ^Awaiter: (member GetResult : unit -> 'T)>
                (t: ^TaskLike)
                : CancellableTaskResult<'T, 'Error> =
                // TODO: Gets errors like "error FS0073: internal error: Undefined or unsolved type variable:  ^Awaiter"
                // this.Run(this.ReturnFrom(t))
                fun _ ->
                    task {
                        let! result = t
                        return Ok result
                    }

        type CancellableTaskResultBuilderBase with

            [<NoEagerConstraintApplication>]
            static member inline BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error when ^TaskLike: (member GetAwaiter:
                unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
                unit -> bool) and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
                (
                    sm: byref<ResumableStateMachine<CancellableTaskResultStateMachineData<'TOverall,'Error>>>,
                    task: CancellationToken -> ^TaskLike,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : bool =
                sm.Data.CancellationToken.ThrowIfCancellationRequested()

                let mutable awaiter =
                    (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task sm.Data.CancellationToken))

                let cont =
                    (CancellableTaskResultResumptionFunc<'TOverall, 'Error> (fun sm ->
                        let result = (^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>) (awaiter))
                        match result with 
                        | Ok result ->
                            (continuation result).Invoke(&sm)
                        | Error e ->
                            sm.Data.Result <- Error e
                            true
                    ))

                // shortcut to continue immediately
                if (^Awaiter: (member get_IsCompleted: unit -> bool) (awaiter)) then
                    cont.Invoke(&sm)
                else
                    sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)
                    sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                    false

            [<NoEagerConstraintApplication>]
            member inline _.Bind< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error when ^TaskLike: (member GetAwaiter:
                unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
                unit -> bool) and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
                (
                    task: CancellationToken -> ^TaskLike,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall,'Error , 'TResult2>)
                ) : CancellableTaskResultCode<'TOverall, 'Error, 'TResult2> =

                CancellableTaskResultCode<'TOverall, _, _> (fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        sm.Data.CancellationToken.ThrowIfCancellationRequested()
                        // Get an awaiter from the awaitable
                        let mutable awaiter =
                            (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task sm.Data.CancellationToken))

                        let mutable __stack_fin = true

                        if not (^Awaiter: (member get_IsCompleted: unit -> bool) (awaiter)) then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result = (^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>) (awaiter))
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

            // [<NoEagerConstraintApplication>]
            // static member inline BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error when ^TaskLike: (member GetAwaiter:
            //     unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
            //     unit -> bool) and ^Awaiter: (member GetResult: unit -> 'TResult1)>
            //     (
            //         sm: byref<ResumableStateMachine<CancellableTaskResultStateMachineData<'TOverall,'Error>>>,
            //         task: ^TaskLike,
            //         continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
            //     ) : bool =
            //     sm.Data.CancellationToken.ThrowIfCancellationRequested()

            //     let mutable awaiter =
            //         (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task))

            //     let cont =
            //         (CancellableTaskResultResumptionFunc<'TOverall, 'Error> (fun sm ->
            //             let result = (^Awaiter: (member GetResult: unit -> 'TResult1) (awaiter))
                        
            //             (continuation result).Invoke(&sm)
            //         ))

            //     // shortcut to continue immediately
            //     if (^Awaiter: (member get_IsCompleted: unit -> bool) (awaiter)) then
            //         cont.Invoke(&sm)
            //     else
            //         sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)
            //         sm.ResumptionDynamicInfo.ResumptionFunc <- cont
            //         false

            // [<NoEagerConstraintApplication>]
            // member inline this.Bind< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error when ^TaskLike: (member GetAwaiter:
            //     unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
            //     unit -> bool) and ^Awaiter: (member GetResult: unit -> 'TResult1)>
            //     (
            //         task: ^TaskLike,
            //         continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
            //     ) : CancellableTaskResultCode<'TOverall, 'Error, 'TResult2> =
                
                
            //     CancellableTaskResultCode<'TOverall, _, _> (fun sm ->
            //         if __useResumableCode then
            //             //-- RESUMABLE CODE START
            //             sm.Data.CancellationToken.ThrowIfCancellationRequested()
            //             // Get an awaiter from the awaitable
            //             let mutable awaiter =
            //                 (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task))

            //             let mutable __stack_fin = true

            //             if not (^Awaiter: (member get_IsCompleted: unit -> bool) (awaiter)) then
            //                 // This will yield with __stack_yield_fin = false
            //                 // This will resume with __stack_yield_fin = true
            //                 let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
            //                 __stack_fin <- __stack_yield_fin

            //             if __stack_fin then
            //                 let result = (^Awaiter: (member GetResult: unit ->'TResult1) (awaiter))
            //                 (continuation result).Invoke(&sm)

            //             else
            //                 sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
            //                 false
            //         else
                        
            //             CancellableTaskResultBuilderBase.BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error>(
            //                 &sm,
            //                 task,
            //                 continuation
            //             )
            //     //-- RESUMABLE CODE END
            //     )


            // [<NoEagerConstraintApplication>]
            // member inline this.Bind< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall when ^TaskLike: (member GetAwaiter:
            //     unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
            //     unit -> bool) and ^Awaiter: (member GetResult: unit -> 'TResult1)>
            //     (
            //         task: ^TaskLike,
            //         continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
            //     ) : CancellableTaskResultCode<'TOveralll, 'Error, 'TResult2> =
            //     this.Bind((fun (ct: CancellationToken) -> task), continuation)

            // [<NoEagerConstraintApplication>]
            // member inline this.Bind< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall when ^TaskLike: (member GetAwaiter:
            //     unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
            //     unit -> bool) and ^Awaiter: (member GetResult: unit -> 'TResult1)>
            //     (
            //         task: unit -> ^TaskLike,
            //         continuation: ('TResult1 -> CancellableTaskResultCode<'TOveralll, 'Error, 'TResult2>)
            //     ) : CancellableTaskResultCode<'TOveralll, 'Error, 'TResult2> =
            //     this.Bind((fun (ct: CancellationToken) -> task ()), continuation)

            // [<NoEagerConstraintApplication>]
            // member inline this.ReturnFrom< ^TaskLike, ^Awaiter, 'T when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
            //     unit -> bool) and ^Awaiter: (member GetResult: unit -> 'T)>
            //     (task: ^TaskLike)
            //     : CancellableTaskResultCode<'T, 'Error 'T> =

            //     this.Bind(task, (fun v -> this.Return v))


            // [<NoEagerConstraintApplication>]
            // member inline this.ReturnFrom< ^TaskLike, ^Awaiter, 'T when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
            //     unit -> bool) and ^Awaiter: (member GetResult: unit -> 'T)>
            //     (task: unit -> ^TaskLike)
            //     : CancellableTaskResultCode<'T, 'T> =

            //     this.Bind(task, (fun v -> this.Return v))

            // [<NoEagerConstraintApplication>]
            // member inline this.ReturnFrom< ^TaskLike, ^Awaiter, 'T, 'Error when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted:
            //     unit -> bool) and ^Awaiter: (member GetResult: unit -> 'T)>
            //     (task: ^TaskLike)
            //     : CancellableTaskResultCode<'T, 'Error, 'T> =

            //     this.Bind(task, (fun v -> this.Return v))

            member inline _.Using<'Resource, 'TOverall, 'Error, 'T when 'Resource :> IDisposable>
                (
                    resource: 'Resource,
                    body: 'Resource -> CancellableTaskResultCode<'TOverall, 'Error, 'T>
                ) =
                ResumableCode.Using(resource, body)


        

    [<AutoOpen>]
    module HighPriority =
        type Microsoft.FSharp.Control.Async with
            static member inline AwaitCancellableTaskResult([<InlineIfLambda>] t: CancellableTaskResult<'T, 'Error>) =
                async {
                    let! ct = Async.CancellationToken
                    return! t ct |> Async.AwaitTask
                }

            static member inline AsCancellableTaskResult(computation: Async<'T>) =
                fun ct -> Async.StartAsTask(computation, cancellationToken = ct)


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
                    (CancellableTaskResultResumptionFunc<'TOverall, 'Error> (fun sm ->
                        let result = awaiter.GetResult()
                        match result with
                        | Ok result ->
                            (continuation result).Invoke(&sm)
                        | Error e ->
                            sm.Data.Result <- Error e
                            true
                        ))

                // shortcut to continue immediately
                if awaiter.IsCompleted then
                    cont.Invoke(&sm)
                else

                    sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)
                    sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                    false

            member inline _.Bind
                (
                    task: CancellableTaskResult<'TResult1, 'Error>,
                    continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'Error, 'TResult2>)
                ) : CancellableTaskResultCode<'TOverall, 'Error, 'TResult2> =

                CancellableTaskResultCode<'TOverall, 'Error, _> (fun sm ->
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
                            | Ok result ->
                                (continuation result).Invoke(&sm)
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


            member inline this.ReturnFrom(task: CancellableTaskResult<'T, 'Error>) : CancellableTaskResultCode<'T, 'Error, 'T> =
                this.Bind((task), (fun v -> this.Return v))

                

    [<AutoOpen>]
    module MediumPriority =
        open HighPriority
        
        type CancellableTaskResultBuilder with
            
            member inline this.Source(t: Task<'T>) : CancellableTaskResult<'T, 'Error> = 
                fun _ -> 
                    task {
                        let! r = t
                        return Ok r
                    }
            member inline this.Source(t: Task) : CancellableTaskResult<unit, 'Error> =
                fun _ -> 
                    task {
                        do! t
                        return Ok()
                    }
        // Medium priority extensions
        type CancellableTaskResultBuilderBase with



        member inline this.Source(t: ValueTask<'T>) : CancellableTaskResult<'T, 'Error> = fun _ -> t |> Task.mapV Ok

        member inline this.Source(t: ValueTask) : CancellableTaskResult<unit, 'Error> =
            fun _ -> 
                task {
                    do! t
                    return Ok()
                }
        member inline this.Source(t: Async<'T>) : CancellableTaskResult<'T, 'Error> = 
            fun ct -> 
                let t = t |> Async.map Ok
                Async.StartAsTask(t, cancellationToken = ct)
      
        // TODO: Use backgroundTask?
        member inline _.Source(result: CancellableTask<'T>) : CancellableTaskResult<'T, 'Error> = 
            fun ct -> 
                // TODO: using `cancellableTask` results in "internal error: The local field ResumptionDynamicInfo was referenced but not declare" compliation error
                task {
                    let! r = result ct
                    return Ok r
                } 

 
            // member inline this.Bind
            //     (
            //         [<InlineIfLambda>] computation: ColdTask<'TResult1>,
            //         [<InlineIfLambda>] continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'TResult2>)
            //     ) : CancellableTaskResultCode<'TOverall, 'TResult2> =
            //     this.Bind((fun (_: CancellationToken) -> computation ()), continuation)

            // member inline this.ReturnFrom(computation: ColdTask<'T>) : CancellableTaskResultCode<'T, 'T> =
            //     this.ReturnFrom(fun (_: CancellationToken) -> computation ())

            // member inline this.Bind
            //     (
            //         [<InlineIfLambda>] computation: ColdTask,
            //         [<InlineIfLambda>] continuation: (unit -> CancellableTaskResultCode<_, _>)
            //     ) : CancellableTaskResultCode<_, _> =
            //     let foo = fun (_: CancellationToken) -> computation ()
            //     this.Bind(foo, continuation)

            // member inline this.ReturnFrom(computation: ColdTask) : CancellableTaskResultCode<_, _> =
            //     this.ReturnFrom(fun (_: CancellationToken) -> computation ())

            // member inline this.Bind
            //     (
            //         computation: Async<'TResult1>,
            //         [<InlineIfLambda>] continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'TResult2>)
            //     ) : CancellableTaskResultCode<'TOverall, 'TResult2> =
            //     this.Bind(Async.AsCancellableTaskResult computation, continuation)

            // member inline this.ReturnFrom(computation: Async<'T>) : CancellableTaskResultCode<'T, 'T> =
            //     this.ReturnFrom(Async.AsCancellableTaskResult computation)

            // member inline this.Bind
            //     (
            //         computation: Task<'TResult1>,
            //         [<InlineIfLambda>] continuation: ('TResult1 -> CancellableTaskResultCode<'TOverall, 'TResult2>)
            //     ) : CancellableTaskResultCode<'TOverall, 'TResult2> =
            //     this.Bind((fun (_: CancellationToken) -> computation), continuation)

            // member inline this.ReturnFrom(computation: Task<'T>) : CancellableTaskResultCode<'T, 'T> =
            //     this.ReturnFrom(fun (_: CancellationToken) -> computation)





    // [<AutoOpen>]
    // module AsyncExtenions =
    //     type Microsoft.FSharp.Control.AsyncBuilder with
    //         member inline this.Bind
    //             (
    //                 [<InlineIfLambda>] t: CancellableTaskResult<'T>,
    //                 [<InlineIfLambda>] binder: ('T -> Async<'U>)
    //             ) : Async<'U> =
    //             this.Bind(Async.AwaitCancellableTaskResult t, binder)

    //         member inline this.ReturnFrom([<InlineIfLambda>] t: CancellableTaskResult<'T>) : Async<'T> =
    //             this.ReturnFrom(Async.AwaitCancellableTaskResult t)

    //         member inline this.Bind
    //             (
    //                 [<InlineIfLambda>] t: CancellableTaskResult,
    //                 [<InlineIfLambda>] binder: (unit -> Async<'U>)
    //             ) : Async<'U> =
    //             this.Bind(Async.AwaitCancellableTaskResult t, binder)

    //         member inline this.ReturnFrom([<InlineIfLambda>] t: CancellableTaskResult) : Async<unit> =
    //             this.ReturnFrom(Async.AwaitCancellableTaskResult t)

    // There is explicitly no Binds for `CancellableTaskResults` in `Microsoft.FSharp.Control.TaskBuilderBase`.
    // You need to explicitly pass in a `CancellationToken`to start it, you can use `CancellationToken.None`.
    // Reason is I don't want people to assume cancellation is happening without the caller being explicit about where the CancellationToken came from.
    // Similar reasoning for `IcedTasks.ColdTasks.ColdTaskBuilderBase`.

    [<RequireQualifiedAccess>]
    module CancellableTaskResult =
        let getCancellationToken () : CancellableTaskResult<CancellationToken, 'Error> =
            CancellableTaskResultBuilder.cancellableTaskResult.Run(
                CancellableTaskResultCode<_, 'Error, _> (fun sm ->
                    sm.Data.Result <- Ok sm.Data.CancellationToken
                    true)
            )
