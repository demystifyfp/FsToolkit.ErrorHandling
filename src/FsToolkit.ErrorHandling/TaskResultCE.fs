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

/// Task<Result<'T, 'Error>>
type TaskResult<'T, 'Error> = Task<Result<'T, 'Error>>

[<Struct; NoComparison; NoEquality>]
type TaskResultStateMachineData<'T, 'Error> =

    [<DefaultValue(false)>]
    val mutable Result: Result<'T, 'Error>

    [<DefaultValue(false)>]
    val mutable MethodBuilder: AsyncTaskResultMethodBuilder<'T, 'Error>

    member this.IsResultError = Result.isError this.Result
    member this.IsTaskCompleted = this.MethodBuilder.Task.IsCompleted

and AsyncTaskResultMethodBuilder<'TOverall, 'Error> =
    AsyncTaskMethodBuilder<Result<'TOverall, 'Error>>

and TaskResultStateMachine<'TOverall, 'Error> =
    ResumableStateMachine<TaskResultStateMachineData<'TOverall, 'Error>>

and TaskResultResumptionFunc<'TOverall, 'Error> =
    ResumptionFunc<TaskResultStateMachineData<'TOverall, 'Error>>

and TaskResultResumptionDynamicInfo<'TOverall, 'Error> =
    ResumptionDynamicInfo<TaskResultStateMachineData<'TOverall, 'Error>>

and TaskResultCode<'TOverall, 'Error, 'T> =
    ResumableCode<TaskResultStateMachineData<'TOverall, 'Error>, 'T>


type TaskResultBuilderBase() =
    member inline _.Delay
        (generator: unit -> TaskResultCode<'TOverall, 'Error, 'T>)
        : TaskResultCode<'TOverall, 'Error, 'T> =
        TaskResultCode<'TOverall, 'Error, 'T>(fun sm -> (generator ()).Invoke(&sm))

    /// Used to represent no-ops like the implicit empty "else" branch of an "if" expression.
    [<DefaultValue>]
    member inline _.Zero<'TOverall, 'Error>() : TaskResultCode<'TOverall, 'Error, unit> =
        ResumableCode.Zero()

    member inline _.Return(value: 'T) : TaskResultCode<'T, 'Error, 'T> =
        TaskResultCode<'T, 'Error, _>(fun sm ->
            // printfn "Return Called --> "
            sm.Data.Result <- Ok value
            true
        )


    /// Chains together a step with its following step.
    /// Note that this requires that the first step has no result.
    /// This prevents constructs like `task { return 1; return 2; }`.
    member inline _.Combine
        (
            task1: TaskResultCode<'TOverall, 'Error, unit>,
            task2: TaskResultCode<'TOverall, 'Error, 'T>
        ) : TaskResultCode<'TOverall, 'Error, 'T> =

        ResumableCode.Combine(
            task1,
            TaskResultCode<'TOverall, 'Error, 'T>(fun sm ->
                if sm.Data.IsResultError then true else task2.Invoke(&sm)
            )
        )


    /// Builds a step that executes the body while the condition predicate is true.
    member inline _.While
        ([<InlineIfLambda>] condition: unit -> bool, body: TaskResultCode<'TOverall, 'Error, unit>)
        : TaskResultCode<'TOverall, 'Error, unit> =
        let mutable keepGoing = true

        ResumableCode.While(
            (fun () ->
                keepGoing
                && condition ()
            ),
            TaskResultCode<_, _, _>(fun sm ->
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
            body: TaskResultCode<'TOverall, 'Error, 'T>,
            catch: exn -> TaskResultCode<'TOverall, 'Error, 'T>
        ) : TaskResultCode<'TOverall, 'Error, 'T> =

        // printfn "TryWith Called --> "
        ResumableCode.TryWith(body, catch)

    /// Wraps a step in a try/finally. This catches exceptions both in the evaluation of the function
    /// to retrieve the step, and in the continuation of the step (if any).
    member inline _.TryFinally
        (body: TaskResultCode<'TOverall, 'Error, 'T>, [<InlineIfLambda>] compensation: unit -> unit)
        : TaskResultCode<'TOverall, 'Error, 'T> =

        // printfn "TryFinally Called --> "

        ResumableCode.TryFinally(
            body,
            ResumableCode<_, _>(fun _sm ->
                compensation ()
                true
            )
        )

    member inline this.For
        (sequence: seq<'T>, body: 'T -> TaskResultCode<'TOverall, 'Error, unit>)
        : TaskResultCode<'TOverall, 'Error, unit> =
        // A for loop is just a using statement on the sequence's enumerator...
        ResumableCode.Using(
            sequence.GetEnumerator(),
            // ... and its body is a while loop that advances the enumerator and runs the body on each element.
            (fun e ->
                this.While(
                    (fun () -> e.MoveNext()),
                    TaskResultCode<'TOverall, 'Error, unit>(fun sm -> (body e.Current).Invoke(&sm))
                )
            )
        )

    member inline internal this.TryFinallyAsync
        (body: TaskResultCode<'TOverall, 'Error, 'T>, compensation: unit -> ValueTask)
        : TaskResultCode<'TOverall, 'Error, 'T> =
        ResumableCode.TryFinallyAsync(
            body,
            ResumableCode<_, _>(fun sm ->
                if __useResumableCode then
                    let mutable __stack_condition_fin = true
                    let __stack_vtask = compensation ()

                    if not __stack_vtask.IsCompleted then
                        let mutable awaiter = __stack_vtask.GetAwaiter()
                        // printfn "TryFinallyAsync Before Invoke Task.Status --> %A" sm.Data.MethodBuilder.Task.Status
                        let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                        __stack_condition_fin <- __stack_yield_fin
                        // printfn "TryFinallyAsync Task.Status --> %A" sm.Data.MethodBuilder.Task.Status

                        if not __stack_condition_fin then
                            sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)

                    __stack_condition_fin
                else
                    let vtask = compensation ()
                    let mutable awaiter = vtask.GetAwaiter()

                    let cont =
                        TaskResultResumptionFunc<'TOverall, 'Error>(fun sm ->
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

    member inline this.Using<'Resource, 'TOverall, 'T, 'Error when 'Resource :> IAsyncDisposableNull>
        (resource: 'Resource, body: 'Resource -> TaskResultCode<'TOverall, 'Error, 'T>)
        : TaskResultCode<'TOverall, 'Error, 'T> =
        this.TryFinallyAsync(
            (fun sm -> (body resource).Invoke(&sm)),
            (fun () ->
                if not (isNull (box resource)) then
                    resource.DisposeAsync()
                else
                    ValueTask()
            )
        )


    member inline this.Source(taskResult: TaskResult<'T, 'Error>) : TaskResult<'T, 'Error> =
        taskResult


type TaskResultBuilder() =

    inherit TaskResultBuilderBase()

    // This is the dynamic implementation - this is not used
    // for statically compiled tasks.  An executor (resumptionFuncExecutor) is
    // registered with the state machine, plus the initial resumption.
    // The executor stays constant throughout the execution, it wraps each step
    // of the execution in a try/with.  The resumption is changed at each step
    // to represent the continuation of the computation.
    static member RunDynamic(code: TaskResultCode<'T, 'Error, 'T>) : TaskResult<'T, 'Error> =
        let mutable sm = TaskResultStateMachine<'T, 'Error>()

        let initialResumptionFunc =
            TaskResultResumptionFunc<'T, 'Error>(fun sm -> code.Invoke(&sm))

        let resumptionInfo =
            { new TaskResultResumptionDynamicInfo<_, _>(initialResumptionFunc) with
                member info.MoveNext(sm) =
                    let mutable savedExn = null

                    try
                        sm.ResumptionDynamicInfo.ResumptionData <- null
                        // printfn "RunDynamic BeforeInvoke Data --> %A" sm.Data.Result
                        let step = info.ResumptionFunc.Invoke(&sm)
                        // printfn "RunDynamic AfterInvoke Data --> %A %A" sm.Data.Result sm.Data.MethodBuilder.Task.Status

                        // If the `sm.Data.MethodBuilder` has already been set somewhere else (like While/WhileDynamic), we shouldn't continue
                        if sm.Data.IsTaskCompleted then
                            ()
                        elif step then
                            // printfn "RunDynamic Data --> %A" sm.Data.Result
                            sm.Data.MethodBuilder.SetResult(sm.Data.Result)
                        else
                            match sm.ResumptionDynamicInfo.ResumptionData with
                            | :? ICriticalNotifyCompletion as awaiter ->
                                let mutable awaiter = awaiter
                                sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                            | awaiter -> assert not (isNull awaiter)

                    with exn ->
                        savedExn <- exn
                    // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                    match savedExn with
                    | null -> ()
                    | exn -> sm.Data.MethodBuilder.SetException exn

                member _.SetStateMachine(sm, state) =
                    sm.Data.MethodBuilder.SetStateMachine(state)
            }

        sm.ResumptionDynamicInfo <- resumptionInfo
        sm.Data.MethodBuilder <- AsyncTaskResultMethodBuilder<'T, 'Error>.Create()
        sm.Data.MethodBuilder.Start(&sm)
        sm.Data.MethodBuilder.Task

    member inline _.Run(code: TaskResultCode<'T, 'Error, 'T>) : TaskResult<'T, 'Error> =
        if __useResumableCode then
            __stateMachine<TaskResultStateMachineData<'T, 'Error>, TaskResult<'T, 'Error>>
                (MoveNextMethodImpl<_>(fun sm ->
                    //-- RESUMABLE CODE START
                    __resumeAt sm.ResumptionPoint

                    let mutable __stack_exn: ExceptionNull = null

                    try
                        // printfn "Run BeforeInvoke Task.Status  --> %A" sm.Data.MethodBuilder.Task.Status
                        let __stack_code_fin = code.Invoke(&sm)
                        // printfn "Run Task.Status --> %A" sm.Data.MethodBuilder.Task.Status
                        // If the `sm.Data.MethodBuilder` has already been set somewhere else (like While/WhileDynamic), we shouldn't continue
                        if
                            __stack_code_fin
                            && not sm.Data.IsTaskCompleted
                        then

                            // printfn "Run __stack_code_fin Data  --> %A" sm.Data.Result
                            sm.Data.MethodBuilder.SetResult(sm.Data.Result)
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
                    sm.Data.MethodBuilder <- AsyncTaskResultMethodBuilder<'T, 'Error>.Create()
                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task
                ))
        else
            TaskResultBuilder.RunDynamic(code)

type BackgroundTaskResultBuilder() =

    inherit TaskResultBuilderBase()

    static member RunDynamic(code: TaskResultCode<'T, 'Error, 'T>) : TaskResult<'T, 'Error> =
        // backgroundTask { .. } escapes to a background thread where necessary
        // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
        if
            isNull SynchronizationContext.Current
            && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
        then
            TaskResultBuilder.RunDynamic(code)
        else
            Task.Run<Result<'T, 'Error>>(fun () -> TaskResultBuilder.RunDynamic(code))


    /// Same as TaskBuilder.Run except the start is inside Task.Run if necessary
    member inline _.Run(code: TaskResultCode<'T, 'Error, 'T>) : TaskResult<'T, 'Error> =
        if __useResumableCode then
            __stateMachine<TaskResultStateMachineData<'T, 'Error>, TaskResult<'T, 'Error>>
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
                        sm.Data.MethodBuilder.SetException exn
                //-- RESUMABLE CODE END
                ))
                (SetStateMachineMethodImpl<_>(fun sm state ->
                    sm.Data.MethodBuilder.SetStateMachine(state)
                ))
                (AfterCode<_, TaskResult<'T, 'Error>>(fun sm ->
                    // backgroundTask { .. } escapes to a background thread where necessary
                    // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
                    if
                        isNull SynchronizationContext.Current
                        && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
                    then
                        sm.Data.MethodBuilder <- AsyncTaskResultMethodBuilder<'T, 'Error>.Create()
                        sm.Data.MethodBuilder.Start(&sm)
                        sm.Data.MethodBuilder.Task
                    else
                        let sm = sm // copy contents of state machine so we can capture it

                        Task.Run<Result<'T, 'Error>>(fun () ->
                            let mutable sm = sm // host local mutable copy of contents of state machine on this thread pool thread

                            sm.Data.MethodBuilder <-
                                AsyncTaskResultMethodBuilder<'T, 'Error>.Create()

                            sm.Data.MethodBuilder.Start(&sm)
                            sm.Data.MethodBuilder.Task
                        )
                ))
        else
            BackgroundTaskResultBuilder.RunDynamic(code)

[<AutoOpen>]
module TaskResultBuilder =

    let taskResult = TaskResultBuilder()
    let backgroundTaskResult = BackgroundTaskResultBuilder()


open Microsoft.FSharp.Control
open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators

[<AutoOpen>]
module TaskResultCEExtensionsLowPriority =
    // Low priority extensions
    type TaskResultBuilderBase with


        [<NoEagerConstraintApplication>]
        static member inline BindDynamic<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
            (
                sm: byref<_>,
                awaiter: ^Awaiter,
                continuation: ('TResult1 -> TaskResultCode<'TOverall, 'Error, 'TResult2>)
            ) : bool =

            let mutable awaiter = awaiter

            let cont =
                (TaskResultResumptionFunc<'TOverall, 'Error>(fun sm ->
                    // printfn "ByndDynamic --> %A" sm.Data.Result

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
                sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)
                sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                false

        [<NoEagerConstraintApplication>]
        member inline _.Bind<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Result<'TResult1, 'Error>)>
            (
                awaiter: ^Awaiter,
                continuation: ('TResult1 -> TaskResultCode<'TOverall, 'Error, 'TResult2>)
            ) : TaskResultCode<'TOverall, 'Error, 'TResult2> =

            TaskResultCode<'TOverall, 'Error, _>(fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    // Get an awaiter from the awaitable
                    // printfn "Bynd --> %A" sm.Data.Result

                    let mutable awaiter = awaiter

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
                        // printfn "Bind tasklike --> %A" sm.Data.MethodBuilder.Task.Status
                        sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                        false
                else
                    TaskResultBuilderBase.BindDynamic<
                        'TResult1,
                        'TResult2,
                        ^Awaiter,
                        'TOverall,
                        'Error
                     >(
                        &sm,
                        awaiter,
                        continuation
                    )
            //-- RESUMABLE CODE END
            )

        [<NoEagerConstraintApplication>]
        member inline this.ReturnFrom< ^TaskLike, ^Awaiter, 'T, 'Error
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
            (awaiter: ^Awaiter)
            : TaskResultCode<'T, 'Error, 'T> =

            this.Bind(awaiter, (fun v -> this.Return v))


        [<NoEagerConstraintApplication>]
        member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
            (t: ^Awaiter)
            : ^Awaiter =
            t


        [<NoEagerConstraintApplication>]
        member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
            when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Result<'T, 'Error>)>
            (t: ^TaskLike)
            : ^Awaiter =
            (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) (t))

        [<NoEagerConstraintApplication>]
        member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
            when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'T)>
            (t: ^TaskLike)
            : TaskResult<'T, 'Error> =
            task {
                let! r = t
                return Ok r
            }

        member inline _.Using<'Resource, 'TOverall, 'T, 'Error when 'Resource :> IDisposableNull>
            (resource: 'Resource, body: 'Resource -> TaskResultCode<'TOverall, 'Error, 'T>)
            =
            ResumableCode.Using(resource, body)

[<AutoOpen>]
module TaskResultCEExtensionsHighPriority =
    // High priority extensions
    type TaskResultBuilderBase with


        member inline this.Bind
            (
                task: TaskResult<'TResult1, 'Error>,
                continuation: ('TResult1 -> TaskResultCode<'TOverall, 'Error, 'TResult2>)
            ) : TaskResultCode<'TOverall, 'Error, 'TResult2> =
            this.Bind(task.GetAwaiter(), continuation)

        member inline this.ReturnFrom
            (task: TaskResult<'T, 'Error>)
            : TaskResultCode<'T, 'Error, 'T> =
            this.Bind(task.GetAwaiter(), (fun v -> this.Return v))

        member inline this.BindReturn(x: TaskResult<'T, 'Error>, f) =
            this.Bind(x.GetAwaiter(), (fun x -> this.Return(f x)))

        member inline _.MergeSources(t1: TaskResult<'T, 'Error>, t2: TaskResult<'T1, 'Error>) =
            TaskResult.zip t1 t2

        member inline _.Source(s: #seq<_>) = s

[<AutoOpen>]
module TaskResultCEExtensionsMediumPriority =

    // Medium priority extensions
    type TaskResultBuilderBase with

        member inline this.Source(t: Task<'T>) : TaskResult<'T, 'Error> =
            t
            |> Task.map Ok

        member inline this.Source(computation: Async<'T>) : TaskResult<'T, 'Error> =
            computation
            |> Async.map Ok
            |> Async.StartImmediateAsTask

[<AutoOpen>]
module TaskResultCEExtensionsHighPriority2 =

    // Medium priority extensions
    type TaskResultBuilderBase with


        member inline _.Source(result: Async<Result<_, _>>) : Task<Result<_, _>> =
            result
            |> Async.StartImmediateAsTask

        member inline _.Source(t: ValueTask<Result<_, _>>) : Task<Result<_, _>> = task { return! t }

        member inline _.Source(result: Result<_, _>) : Task<Result<_, _>> = Task.singleton result

        member inline _.Source(result: Choice<_, _>) : Task<Result<_, _>> =
            result
            |> Result.ofChoice
            |> Task.singleton
