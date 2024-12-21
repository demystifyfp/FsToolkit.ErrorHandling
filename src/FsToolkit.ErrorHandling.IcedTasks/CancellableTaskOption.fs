namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks


open System
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Control
open Microsoft.FSharp.Collections
open IcedTasks

/// CancellationToken -> ValueTask<'T voption>
type CancellableValueTaskValueOption<'T> = CancellationToken -> ValueTask<'T voption>

[<Struct; NoComparison; NoEquality>]
type TaskOptionStateMachineData<'T> =
    [<DefaultValue(false)>]
    val mutable CancellationToken: CancellationToken

    [<DefaultValue(false)>]
    val mutable Result: 'T voption voption

    [<DefaultValue(false)>]
    val mutable MethodBuilder: AsyncTaskOptionMethodBuilder<'T>

    member this.IsResultNone =
        match this.Result with
        | ValueNone -> false
        | ValueSome(ValueNone) -> true
        | ValueSome _ -> false

    member this.SetResult() =
        match this.Result with
        | ValueNone -> this.MethodBuilder.SetResult ValueNone
        | ValueSome x -> this.MethodBuilder.SetResult x


    member this.IsTaskCompleted = this.MethodBuilder.Task.IsCompleted
#if NET6_0_OR_GREATER
and AsyncTaskOptionMethodBuilder<'TOverall> = PoolingAsyncValueTaskMethodBuilder<'TOverall voption>
#else
and AsyncTaskOptionMethodBuilder<'TOverall> = AsyncValueTaskMethodBuilder<'TOverall voption>
#endif
and TaskOptionStateMachine<'TOverall> = ResumableStateMachine<TaskOptionStateMachineData<'TOverall>>
and TaskOptionResumptionFunc<'TOverall> = ResumptionFunc<TaskOptionStateMachineData<'TOverall>>

and TaskOptionResumptionDynamicInfo<'TOverall> =
    ResumptionDynamicInfo<TaskOptionStateMachineData<'TOverall>>

and TaskOptionCode<'TOverall, 'T> = ResumableCode<TaskOptionStateMachineData<'TOverall>, 'T>

type ValueTaskValueOptionBuilderBase() =

    member inline _.Delay
        (generator: unit -> TaskOptionCode<'TOverall, 'T>)
        : TaskOptionCode<'TOverall, 'T> =
        TaskOptionCode<'TOverall, 'T>(fun sm -> (generator ()).Invoke(&sm))

    /// Used to represent no-ops like the implicit empty "else" branch of an "if" expression.
    [<DefaultValue>]
    member inline _.Zero<'TOverall>() : TaskOptionCode<'TOverall, unit> =
        TaskOptionCode<_, _>(fun sm ->
            sm.Data.Result <- ValueSome(ValueSome Unchecked.defaultof<'TOverall>)
            true
        )

    member inline _.Return(value: 'T) : TaskOptionCode<'T, 'T> =
        TaskOptionCode<'T, _>(fun sm ->
            sm.Data.Result <- ValueSome(ValueSome value)
            true
        )

    /// Chains together a step with its following step.
    /// Note that this requires that the first step has no result.
    /// This prevents constructs like `task { return 1; return 2; }`.
    member inline _.Combine
        (task1: TaskOptionCode<'TOverall, unit>, task2: TaskOptionCode<'TOverall, 'T>)
        : TaskOptionCode<'TOverall, 'T> =

        ResumableCode.Combine(
            task1,
            TaskOptionCode<'TOverall, 'T>(fun sm ->
                if sm.Data.IsResultNone then true else task2.Invoke(&sm)
            )
        )

    /// Builds a step that executes the body while the condition predicate is true.
    member inline _.While
        ([<InlineIfLambda>] condition: unit -> bool, body: TaskOptionCode<'TOverall, unit>)
        : TaskOptionCode<'TOverall, unit> =
        let mutable keepGoing = true

        ResumableCode.While(
            (fun () ->
                keepGoing
                && condition ()
            ),
            TaskOptionCode<_, _>(fun sm ->
                if sm.Data.IsResultNone then
                    keepGoing <- false
                    sm.Data.SetResult()
                    true
                else
                    body.Invoke(&sm)
            )
        )


    /// Wraps a step in a try/with. This catches exceptions both in the evaluation of the function
    /// to retrieve the step, and in the continuation of the step (if any).
    member inline _.TryWith
        (body: TaskOptionCode<'TOverall, 'T>, catch: exn -> TaskOptionCode<'TOverall, 'T>)
        : TaskOptionCode<'TOverall, 'T> =
        ResumableCode.TryWith(body, catch)

    /// Wraps a step in a try/finally. This catches exceptions both in the evaluation of the function
    /// to retrieve the step, and in the continuation of the step (if any).
    member inline _.TryFinally
        (body: TaskOptionCode<'TOverall, 'T>, [<InlineIfLambda>] compensation: unit -> unit)
        : TaskOptionCode<'TOverall, 'T> =
        ResumableCode.TryFinally(
            body,
            ResumableCode<_, _>(fun _sm ->
                compensation ()
                true
            )
        )

    member inline this.For
        (sequence: seq<'T>, body: 'T -> TaskOptionCode<'TOverall, unit>)
        : TaskOptionCode<'TOverall, unit> =
        ResumableCode.Using(
            sequence.GetEnumerator(),
            // ... and its body is a while loop that advances the enumerator and runs the body on each element.
            (fun e ->
                this.While(
                    (fun () -> e.MoveNext()),
                    TaskOptionCode<'TOverall, unit>(fun sm -> (body e.Current).Invoke(&sm))
                )
            )
        )

    member inline internal this.TryFinallyAsync
        (body: TaskOptionCode<'TOverall, 'T>, compensation: unit -> ValueTask)
        : TaskOptionCode<'TOverall, 'T> =
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
                        TaskOptionResumptionFunc<'TOverall>(fun sm ->
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

    member inline this.Using<'Resource, 'TOverall, 'T when 'Resource :> IAsyncDisposable>
        (resource: 'Resource, body: 'Resource -> TaskOptionCode<'TOverall, 'T>)
        : TaskOptionCode<'TOverall, 'T> =
        this.TryFinallyAsync(
            (fun sm -> (body resource).Invoke(&sm)),
            (fun () ->
                if not (isNull (box resource)) then
                    resource.DisposeAsync()
                else
                    ValueTask()
            )
        )

    member inline this.Source(computation: Async<'T option>) : CancellableValueTaskValueOption<'T> =
        cancellableValueTask {
            let! r = computation

            return
                r
                |> Option.toValueOption
        }


    member inline this.Source
        (taskOption: CancellableValueTaskValueOption<'T>)
        : CancellableValueTaskValueOption<'T> =
        taskOption

// member inline this.Source(taskOption: ValueTask<'T option>) : CancellableTaskOption<'T> =
//     taskOption.AsTask()


type ValueTaskValueOptionBuilder() =

    inherit ValueTaskValueOptionBuilderBase()

    // This is the dynamic implementation - this is not used
    // for statically compiled tasks.  An executor (resumptionFuncExecutor) is
    // registered with the state machine, plus the initial resumption.
    // The executor stays constant throughout the execution, it wraps each step
    // of the execution in a try/with.  The resumption is changed at each step
    // to represent the continuation of the computation.
    static member RunDynamic(code: TaskOptionCode<'T, 'T>) : CancellableValueTaskValueOption<'T> =
        let mutable sm = TaskOptionStateMachine<'T>()

        let initialResumptionFunc = TaskOptionResumptionFunc<'T>(fun sm -> code.Invoke(&sm))

        let resumptionInfo =
            { new TaskOptionResumptionDynamicInfo<_>(initialResumptionFunc) with
                member info.MoveNext(sm) =
                    let mutable savedExn = null

                    try
                        sm.ResumptionDynamicInfo.ResumptionData <- null
                        let step = info.ResumptionFunc.Invoke(&sm)

                        // If the `sm.Data.MethodBuilder` has already been set somewhere else (like While/WhileDynamic), we shouldn't continue
                        if sm.Data.IsTaskCompleted then
                            ()

                        if step then
                            sm.Data.SetResult()
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
                    | exn -> sm.Data.MethodBuilder.SetException exn

                member _.SetStateMachine(sm, state) =
                    sm.Data.MethodBuilder.SetStateMachine(state)
            }

        fun ct ->
            if ct.IsCancellationRequested then
                ValueTask<_> ValueNone
            else
                sm.Data.CancellationToken <- ct
                sm.ResumptionDynamicInfo <- resumptionInfo
                sm.Data.MethodBuilder <- AsyncTaskOptionMethodBuilder<'T>.Create()
                sm.Data.MethodBuilder.Start(&sm)
                sm.Data.MethodBuilder.Task


    member inline _.Run(code: TaskOptionCode<'T, 'T>) : CancellableValueTaskValueOption<'T> =
        if __useResumableCode then
            __stateMachine<TaskOptionStateMachineData<'T>, CancellableValueTaskValueOption<'T>>
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
                            sm.Data.SetResult()
                    with exn ->
                        __stack_exn <- exn
                    // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                    match __stack_exn with
                    | null -> ()
                    | exn -> sm.Data.MethodBuilder.SetException exn
                //-- RESUMABLE CODE END
                ))
                (SetStateMachineMethodImpl<_>(fun sm state ->
                    sm.Data.MethodBuilder.SetStateMachine(state)
                ))
                (AfterCode<_, _>(fun sm ->
                    let sm = sm

                    fun ct ->
                        if ct.IsCancellationRequested then
                            ValueTask<_> ValueNone
                        else
                            let mutable sm = sm
                            sm.Data.CancellationToken <- ct
                            sm.Data.MethodBuilder <- AsyncTaskOptionMethodBuilder<'T>.Create()
                            sm.Data.MethodBuilder.Start(&sm)
                            sm.Data.MethodBuilder.Task
                ))
        else
            ValueTaskValueOptionBuilder.RunDynamic(code)


[<AutoOpen>]
module TaskOptionBuilder =

    let vTask_vOption = ValueTaskValueOptionBuilder()

type TaskOptionBuilder() =

    inherit ValueTaskValueOptionBuilderBase()

    member inline _.Run(code: TaskOptionCode<'T, 'T>) =
        cancellableTask {
            let! r = vTask_vOption.Run(code)
            return ValueOption.toOption r
        }


// type BackgroundTaskOptionBuilder() =

//     inherit TaskOptionBuilderBase()

//     static member RunDynamic(code: TaskOptionCode<'T, 'T>) : CancellableTaskOption<'T> =
//         // backgroundTask { .. } escapes to a background thread where necessary
//         // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
//         if
//             isNull SynchronizationContext.Current
//             && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
//         then
//             TaskOptionBuilder.RunDynamic(code)
//         else
//             Task.Run<'T option>(fun () -> TaskOptionBuilder.RunDynamic(code))


//     //// Same as TaskBuilder.Run except the start is inside Task.Run if necessary
//     member inline _.Run(code: TaskOptionCode<'T, 'T>) : CancellableTaskOption<'T> =
//         if __useResumableCode then
//             __stateMachine<TaskOptionStateMachineData<'T>, CancellableTaskOption<'T>>
//                 (MoveNextMethodImpl<_>(fun sm ->
//                     //-- RESUMABLE CODE START
//                     __resumeAt sm.ResumptionPoint

//                     try
//                         let __stack_code_fin = code.Invoke(&sm)

//                         if
//                             __stack_code_fin
//                             && not sm.Data.IsTaskCompleted
//                         then
//                             sm.Data.MethodBuilder.SetResult(sm.Data.Result.Value)
//                     with exn ->
//                         sm.Data.MethodBuilder.SetException exn
//                 //-- RESUMABLE CODE END
//                 ))
//                 (SetStateMachineMethodImpl<_>(fun sm state ->
//                     sm.Data.MethodBuilder.SetStateMachine(state)
//                 ))
//                 (AfterCode<_, CancellableTaskOption<'T>>(fun sm ->
//                     // backgroundTask { .. } escapes to a background thread where necessary
//                     // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
//                     if
//                         isNull SynchronizationContext.Current
//                         && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
//                     then
//                         sm.Data.MethodBuilder <- AsyncTaskOptionMethodBuilder<'T>.Create()
//                         sm.Data.MethodBuilder.Start(&sm)
//                         sm.Data.MethodBuilder.Task
//                     else
//                         let sm = sm // copy contents of state machine so we can capture it

//                         Task.Run<'T option>(fun () ->
//                             let mutable sm = sm // host local mutable copy of contents of state machine on this thread pool thread
//                             sm.Data.MethodBuilder <- AsyncTaskOptionMethodBuilder<'T>.Create()
//                             sm.Data.MethodBuilder.Start(&sm)
//                             sm.Data.MethodBuilder.Task
//                         )
//                 ))
//         else
//             BackgroundTaskOptionBuilder.RunDynamic(code)

// [<AutoOpen>]
// module TaskOptionBuilder =

//     let taskOption = ValueTaskValueOptionBuilder()
// let backgroundTaskOption = BackgroundTaskOptionBuilder()


[<AutoOpen>]
module TaskOptionCEExtensionsLowPriority =
    // Low priority extensions
    type ValueTaskValueOptionBuilderBase with

        [<NoEagerConstraintApplication>]
        static member inline BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall
            when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'TResult1 option)>
            (
                sm: byref<_>,
                task: ^TaskLike,
                continuation: ('TResult1 -> TaskOptionCode<'TOverall, 'TResult2>)
            ) : bool =

            let mutable awaiter = (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task))

            let cont =
                (TaskOptionResumptionFunc<'TOverall>(fun sm ->
                    let result = (^Awaiter: (member GetResult: unit -> 'TResult1 option) (awaiter))

                    match result with
                    | Some result -> (continuation result).Invoke(&sm)
                    | None ->
                        sm.Data.Result <- ValueSome ValueNone
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
        member inline _.Bind< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall
            when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'TResult1 option)>
            (task: ^TaskLike, continuation: ('TResult1 -> TaskOptionCode<'TOverall, 'TResult2>))
            : TaskOptionCode<'TOverall, 'TResult2> =

            TaskOptionCode<'TOverall, _>(fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    // Get an awaiter from the awaitable
                    let mutable awaiter = (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (task))

                    let mutable __stack_fin = true

                    if not (^Awaiter: (member get_IsCompleted: unit -> bool) (awaiter)) then
                        // This will yield with __stack_yield_fin = false
                        // This will resume with __stack_yield_fin = true
                        let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                        __stack_fin <- __stack_yield_fin

                    if __stack_fin then
                        let result =
                            (^Awaiter: (member GetResult: unit -> 'TResult1 option) (awaiter))

                        match result with
                        | Some result -> (continuation result).Invoke(&sm)
                        | None ->
                            sm.Data.Result <- ValueSome ValueNone
                            true
                    else
                        sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                        false
                else
                    ValueTaskValueOptionBuilderBase.BindDynamic<
                        ^TaskLike,
                        'TResult1,
                        'TResult2,
                        ^Awaiter,
                        'TOverall
                     >(
                        &sm,
                        task,
                        continuation
                    )
            //-- RESUMABLE CODE END
            )

        [<NoEagerConstraintApplication>]
        member inline this.ReturnFrom< ^TaskLike, ^Awaiter, 'T
            when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'T option)>
            (task: ^TaskLike)
            : TaskOptionCode<'T, 'T> =

            this.Bind(task, (fun v -> this.Return v))

        [<NoEagerConstraintApplication>]
        member inline this.Source< ^TaskLike, ^Awaiter, 'T
            when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'T)>
            (t: ^TaskLike)
            : CancellableValueTaskValueOption<'T> =
            fun ct ->
                valueTask {
                    let! r = t
                    return ValueSome r
                }

        member inline _.Using<'Resource, 'TOverall, 'T when 'Resource :> IDisposable>
            (resource: 'Resource, body: 'Resource -> TaskOptionCode<'TOverall, 'T>)
            =
            ResumableCode.Using(resource, body)

[<AutoOpen>]
module TaskOptionCEExtensionsHighPriority =
    // High priority extensions
    type ValueTaskValueOptionBuilderBase with

        static member BindDynamic
            (
                sm: byref<TaskOptionStateMachine<_>>,
                task: CancellableValueTaskValueOption<'TResult1>,
                continuation: ('TResult1 -> TaskOptionCode<'TOverall, 'TResult2>)
            ) : bool =
            let mutable awaiter = (task sm.Data.CancellationToken).GetAwaiter()

            let cont =
                (TaskOptionResumptionFunc<'TOverall>(fun sm ->
                    let result = awaiter.GetResult()

                    match result with
                    | ValueSome result -> (continuation result).Invoke(&sm)
                    | ValueNone ->
                        sm.Data.Result <- ValueSome ValueNone
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
                task: CancellableValueTaskValueOption<'TResult1>,
                continuation: ('TResult1 -> TaskOptionCode<'TOverall, 'TResult2>)
            ) : TaskOptionCode<'TOverall, 'TResult2> =

            TaskOptionCode<'TOverall, _>(fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    // Get an awaiter from the task
                    let mutable awaiter = (task sm.Data.CancellationToken).GetAwaiter()

                    let mutable __stack_fin = true

                    if not awaiter.IsCompleted then
                        // This will yield with __stack_yield_fin = false
                        // This will resume with __stack_yield_fin = true
                        let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                        __stack_fin <- __stack_yield_fin

                    if __stack_fin then
                        let result = awaiter.GetResult()

                        match result with
                        | ValueSome result -> (continuation result).Invoke(&sm)
                        | ValueNone ->
                            sm.Data.Result <- ValueSome ValueNone
                            true

                    else
                        sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                        false

                else
                    ValueTaskValueOptionBuilderBase.BindDynamic(&sm, task, continuation)
            //-- RESUMABLE CODE END
            )

        member inline this.BindReturn
            (x: CancellableValueTaskValueOption<'T>, [<InlineIfLambda>] f)
            =
            this.Bind(x, (fun x -> this.Return(f x)))

        // member inline _.MergeSources(t1: CancellableTaskOption<'T>, t2: CancellableTaskOption<'T1>) = TaskOption.zip t1 t2

        member inline this.ReturnFrom
            (task: CancellableValueTaskValueOption<'T>)
            : TaskOptionCode<'T, 'T> =
            this.Bind(task, (fun v -> this.Return v))

        member inline _.Source(s: #seq<_>) = s

// [<AutoOpen>]
// module TaskOptionCEExtensionsMediumPriority =

//     // Medium priority extensions
//     type TaskOptionBuilderBase with

//         member inline this.Source(t: Task<'T>) : CancellableTaskOption<'T> =
//             t
//             |> Task.map Some

//         member inline this.Source(t: Task) : CancellableTaskOption<unit> =
//             task {
//                 do! t
//                 return Some()
//             }

//         member inline this.Source(t: ValueTask<'T>) : CancellableTaskOption<'T> =
//             t
//             |> Task.mapV Some

//         member inline this.Source(t: ValueTask) : CancellableTaskOption<unit> =
//             task {
//                 do! t
//                 return Some()
//             }

//         member inline this.Source(opt: Option<'T>) : CancellableTaskOption<'T> = Task.FromResult opt

//         member inline this.Source(computation: Async<'T>) : CancellableTaskOption<'T> =
//             computation
//             |> Async.map Some
//             |> Async.StartImmediateAsTask
