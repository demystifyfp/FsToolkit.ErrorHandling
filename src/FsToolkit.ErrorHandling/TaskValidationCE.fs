namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System.Runtime.CompilerServices
open System.Threading
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Control
open Microsoft.FSharp.Collections

[<Struct; NoComparison; NoEquality>]
type TaskValidationStateMachineData<'T, 'Error> =

    [<DefaultValue(false)>]
    val mutable Validation: Validation<'T, 'Error>

    [<DefaultValue(false)>]
    val mutable MethodBuilder: AsyncTaskValidationMethodBuilder<'T, 'Error>

    member this.IsValidationError = Result.isError this.Validation
    member this.IsTaskCompleted = this.MethodBuilder.Task.IsCompleted

and AsyncTaskValidationMethodBuilder<'TOverall, 'Error> =
    AsyncTaskMethodBuilder<Validation<'TOverall, 'Error>>

and TaskValidationStateMachine<'TOverall, 'Error> =
    ResumableStateMachine<TaskValidationStateMachineData<'TOverall, 'Error>>

and TaskValidationResumptionFunc<'TOverall, 'Error> =
    ResumptionFunc<TaskValidationStateMachineData<'TOverall, 'Error>>

and TaskValidationResumptionDynamicInfo<'TOverall, 'Error> =
    ResumptionDynamicInfo<TaskValidationStateMachineData<'TOverall, 'Error>>

and TaskValidationCode<'TOverall, 'Error, 'T> =
    ResumableCode<TaskValidationStateMachineData<'TOverall, 'Error>, 'T>


type TaskValidationBuilderBase() =
    member inline _.Delay
        (generator: unit -> TaskValidationCode<'TOverall, 'Error, 'T>)
        : TaskValidationCode<'TOverall, 'Error, 'T> =
        TaskValidationCode<'TOverall, 'Error, 'T>(fun sm -> (generator ()).Invoke(&sm))

    /// Used to represent no-ops like the implicit empty "else" branch of an "if" expression.
    [<DefaultValue>]
    member inline _.Zero<'TOverall, 'Error>() : TaskValidationCode<'TOverall, 'Error, unit> =
        ResumableCode.Zero()

    member inline _.Return(value: 'T) : TaskValidationCode<'T, 'Error, 'T> =
        TaskValidationCode<'T, 'Error, _>(fun sm ->
            sm.Data.Validation <- Ok value
            true
        )

    /// Chains together a step with its following step.
    /// Note that this requires that the first step has no result.
    /// This prevents constructs like `task { return 1; return 2; }`.
    member inline _.Combine
        (
            task1: TaskValidationCode<'TOverall, 'Error, unit>,
            task2: TaskValidationCode<'TOverall, 'Error, 'T>
        ) : TaskValidationCode<'TOverall, 'Error, 'T> =

        ResumableCode.Combine(
            task1,
            TaskValidationCode<'TOverall, 'Error, 'T>(fun sm ->
                if sm.Data.IsValidationError then
                    true
                else
                    task2.Invoke(&sm)
            )
        )


    /// Builds a step that executes the body while the condition predicate is true.
    member inline _.While
        (
            [<InlineIfLambda>] condition: unit -> bool,
            body: TaskValidationCode<'TOverall, 'Error, unit>
        ) : TaskValidationCode<'TOverall, 'Error, unit> =
        let mutable keepGoing = true

        ResumableCode.While(
            (fun () ->
                keepGoing
                && condition ()
            ),
            TaskValidationCode<_, _, _>(fun sm ->
                if sm.Data.IsValidationError then
                    keepGoing <- false
                    sm.Data.MethodBuilder.SetResult sm.Data.Validation
                    true
                else
                    body.Invoke(&sm)
            )
        )

    /// Wraps a step in a try/with. This catches exceptions both in the evaluation of the function
    /// to retrieve the step, and in the continuation of the step (if any).
    member inline _.TryWith
        (
            body: TaskValidationCode<'TOverall, 'Error, 'T>,
            catch: exn -> TaskValidationCode<'TOverall, 'Error, 'T>
        ) : TaskValidationCode<'TOverall, 'Error, 'T> =

        ResumableCode.TryWith(body, catch)

    /// Wraps a step in a try/finally. This catches exceptions both in the evaluation of the function
    /// to retrieve the step, and in the continuation of the step (if any).
    member inline _.TryFinally
        (
            body: TaskValidationCode<'TOverall, 'Error, 'T>,
            [<InlineIfLambda>] compensation: unit -> unit
        ) : TaskValidationCode<'TOverall, 'Error, 'T> =

        ResumableCode.TryFinally(
            body,
            ResumableCode<_, _>(fun _sm ->
                compensation ()
                true
            )
        )

    member inline this.For
        (sequence: seq<'T>, body: 'T -> TaskValidationCode<'TOverall, 'Error, unit>)
        : TaskValidationCode<'TOverall, 'Error, unit> =
        // A for loop is just a using statement on the sequence's enumerator...
        ResumableCode.Using(
            sequence.GetEnumerator(),
            // ... and its body is a while loop that advances the enumerator and runs the body on each element.
            (fun e ->
                this.While(
                    (fun () -> e.MoveNext()),
                    TaskValidationCode<'TOverall, 'Error, unit>(fun sm ->
                        (body e.Current).Invoke(&sm)
                    )
                )
            )
        )

    member inline internal this.TryFinallyAsync
        (body: TaskValidationCode<'TOverall, 'Error, 'T>, compensation: unit -> ValueTask)
        : TaskValidationCode<'TOverall, 'Error, 'T> =
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
                        TaskValidationResumptionFunc<'TOverall, 'Error>(fun sm ->
                            awaiter.GetResult()

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
        (resource: 'Resource, body: 'Resource -> TaskValidationCode<'TOverall, 'Error, 'T>)
        : TaskValidationCode<'TOverall, 'Error, 'T> =
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
        (taskValidation: TaskValidation<'T, 'Error>)
        : TaskValidation<'T, 'Error> =
        taskValidation


type TaskValidationBuilder() =

    inherit TaskValidationBuilderBase()

    // This is the dynamic implementation - this is not used
    // for statically compiled tasks.  An executor (resumptionFuncExecutor) is
    // registered with the state machine, plus the initial resumption.
    // The executor stays constant throughout the execution, it wraps each step
    // of the execution in a try/with.  The resumption is changed at each step
    // to represent the continuation of the computation.
    static member RunDynamic
        (code: TaskValidationCode<'T, 'Error, 'T>)
        : TaskValidation<'T, 'Error> =
        let mutable sm = TaskValidationStateMachine<'T, 'Error>()

        let initialResumptionFunc =
            TaskValidationResumptionFunc<'T, 'Error>(fun sm -> code.Invoke(&sm))

        let resumptionInfo =
            { new TaskValidationResumptionDynamicInfo<_, _>(initialResumptionFunc) with
                member info.MoveNext(sm) =
                    let mutable savedExn = null

                    try
                        sm.ResumptionDynamicInfo.ResumptionData <- null
                        let step = info.ResumptionFunc.Invoke(&sm)

                        // If the `sm.Data.MethodBuilder` has already been set somewhere else (like While/WhileDynamic), we shouldn't continue
                        if sm.Data.IsTaskCompleted then
                            ()
                        elif step then
                            sm.Data.MethodBuilder.SetResult(sm.Data.Validation)
                        else
                            let mutable awaiter =
                                sm.ResumptionDynamicInfo.ResumptionData
                                :?> ICriticalNotifyCompletion

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

        sm.ResumptionDynamicInfo <- resumptionInfo
        sm.Data.MethodBuilder <- AsyncTaskValidationMethodBuilder<'T, 'Error>.Create()
        sm.Data.MethodBuilder.Start(&sm)
        sm.Data.MethodBuilder.Task

    member inline _.Run(code: TaskValidationCode<'T, 'Error, 'T>) : TaskValidation<'T, 'Error> =
        if __useResumableCode then
            __stateMachine<TaskValidationStateMachineData<'T, 'Error>, TaskValidation<'T, 'Error>>
                (MoveNextMethodImpl<_>(fun sm ->
                    //-- RESUMABLE CODE START
                    __resumeAt sm.ResumptionPoint

                    let mutable __stack_exn: ExceptionNull = null

                    try
                        let __stack_code_fin = code.Invoke(&sm)
                        // If the `sm.Data.MethodBuilder` has already been set somewhere else (like While/WhileDynamic), we shouldn't continue
                        if
                            __stack_code_fin
                            && not sm.Data.IsTaskCompleted
                        then
                            sm.Data.MethodBuilder.SetResult(sm.Data.Validation)
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
                    sm.Data.MethodBuilder <- AsyncTaskValidationMethodBuilder<'T, 'Error>.Create()
                    sm.Data.MethodBuilder.Start(&sm)
                    sm.Data.MethodBuilder.Task
                ))
        else
            TaskValidationBuilder.RunDynamic(code)

type BackgroundTaskValidationBuilder() =

    inherit TaskValidationBuilderBase()

    static member RunDynamic
        (code: TaskValidationCode<'T, 'Error, 'T>)
        : TaskValidation<'T, 'Error> =
        // backgroundTask { .. } escapes to a background thread where necessary
        // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
        if
            isNull SynchronizationContext.Current
            && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
        then
            TaskValidationBuilder.RunDynamic(code)
        else
            Task.Run<Validation<'T, 'Error>>(fun () -> TaskValidationBuilder.RunDynamic(code))


    /// Same as TaskBuilder.Run except the start is inside Task.Run if necessary
    member inline _.Run(code: TaskValidationCode<'T, 'Error, 'T>) : TaskValidation<'T, 'Error> =
        if __useResumableCode then
            __stateMachine<TaskValidationStateMachineData<'T, 'Error>, TaskValidation<'T, 'Error>>
                (MoveNextMethodImpl<_>(fun sm ->
                    //-- RESUMABLE CODE START
                    __resumeAt sm.ResumptionPoint

                    try
                        let __stack_code_fin = code.Invoke(&sm)

                        if
                            __stack_code_fin
                            && not sm.Data.IsTaskCompleted
                        then
                            sm.Data.MethodBuilder.SetResult(sm.Data.Validation)
                    with exn ->
                        sm.Data.MethodBuilder.SetException exn
                //-- RESUMABLE CODE END
                ))
                (SetStateMachineMethodImpl<_>(fun sm state ->
                    sm.Data.MethodBuilder.SetStateMachine(state)
                ))
                (AfterCode<_, TaskValidation<'T, 'Error>>(fun sm ->
                    // backgroundTask { .. } escapes to a background thread where necessary
                    // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
                    if
                        isNull SynchronizationContext.Current
                        && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
                    then
                        sm.Data.MethodBuilder <-
                            AsyncTaskValidationMethodBuilder<'T, 'Error>.Create()

                        sm.Data.MethodBuilder.Start(&sm)
                        sm.Data.MethodBuilder.Task
                    else
                        let sm = sm // copy contents of state machine so we can capture it

                        Task.Run<Validation<'T, 'Error>>(fun () ->
                            let mutable sm = sm // host local mutable copy of contents of state machine on this thread pool thread

                            sm.Data.MethodBuilder <-
                                AsyncTaskValidationMethodBuilder<'T, 'Error>.Create()

                            sm.Data.MethodBuilder.Start(&sm)
                            sm.Data.MethodBuilder.Task
                        )
                ))
        else
            BackgroundTaskValidationBuilder.RunDynamic(code)

[<AutoOpen>]
module TaskValidationBuilder =

    let taskValidation = TaskValidationBuilder()
    let backgroundTaskValidation = BackgroundTaskValidationBuilder()

[<AutoOpen>]
module TaskValidationCEExtensionsLowPriority =
    // Low priority extensions
    type TaskValidationBuilderBase with


        [<NoEagerConstraintApplication>]
        static member inline BindDynamic<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
            (
                sm: byref<_>,
                awaiter: ^Awaiter,
                continuation: 'TResult1 -> TaskValidationCode<'TOverall, 'Error, 'TResult2>
            ) : bool =

            let mutable awaiter = awaiter

            let cont =
                TaskValidationResumptionFunc<'TOverall, 'Error>(fun sm ->

                    let result =
                        (^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>) awaiter)

                    match result with
                    | Ok result -> (continuation result).Invoke(&sm)
                    | Error e ->
                        sm.Data.Validation <- Error e
                        true

                )

            // shortcut to continue immediately
            if (^Awaiter: (member get_IsCompleted: unit -> bool) awaiter) then
                cont.Invoke(&sm)
            else
                sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)
                sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                false

        [<NoEagerConstraintApplication>]
        member inline _.Bind<'TResult1, 'TResult2, ^Awaiter, 'TOverall, 'Error
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>)>
            (
                awaiter: ^Awaiter,
                continuation: 'TResult1 -> TaskValidationCode<'TOverall, 'Error, 'TResult2>
            ) : TaskValidationCode<'TOverall, 'Error, 'TResult2> =

            TaskValidationCode<'TOverall, 'Error, _>(fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    // Get an awaiter from the awaitable

                    let mutable awaiter = awaiter

                    let mutable __stack_fin = true

                    if not (^Awaiter: (member get_IsCompleted: unit -> bool) awaiter) then
                        // This will yield with __stack_yield_fin = false
                        // This will resume with __stack_yield_fin = true
                        let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                        __stack_fin <- __stack_yield_fin

                    if __stack_fin then
                        let result =
                            (^Awaiter: (member GetResult: unit -> Validation<'TResult1, 'Error>) awaiter)

                        match result with
                        | Ok result -> (continuation result).Invoke(&sm)
                        | Error e ->
                            sm.Data.Validation <- Error e
                            true
                    else
                        sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                        false
                else
                    TaskValidationBuilderBase.BindDynamic<
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
            and ^Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
            (awaiter: ^Awaiter)
            : TaskValidationCode<'T, 'Error, 'T> =

            this.Bind(awaiter, (fun v -> this.Return v))


        [<NoEagerConstraintApplication>]
        member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
            when ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
            (t: ^Awaiter)
            : ^Awaiter =
            t


        [<NoEagerConstraintApplication>]
        member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
            when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> Validation<'T, 'Error>)>
            (t: ^TaskLike)
            : ^Awaiter =
            (^TaskLike: (member GetAwaiter: unit -> ^Awaiter) t)

        [<NoEagerConstraintApplication>]
        member inline this.Source< ^TaskLike, ^Awaiter, 'T, 'Error
            when ^TaskLike: (member GetAwaiter: unit -> ^Awaiter)
            and ^Awaiter :> ICriticalNotifyCompletion
            and ^Awaiter: (member get_IsCompleted: unit -> bool)
            and ^Awaiter: (member GetResult: unit -> 'T)>
            (t: ^TaskLike)
            : TaskValidation<'T, 'Error> =
            task {
                let! r = t
                return Ok r
            }

        member inline _.Using<'Resource, 'TOverall, 'T, 'Error when 'Resource :> IDisposableNull>
            (resource: 'Resource, body: 'Resource -> TaskValidationCode<'TOverall, 'Error, 'T>)
            =
            ResumableCode.Using(resource, body)

[<AutoOpen>]
module TaskValidationCEExtensionsHighPriority =
    // High priority extensions
    type TaskValidationBuilderBase with


        member inline this.Bind
            (
                task: TaskValidation<'TResult1, 'Error>,
                continuation: ('TResult1 -> TaskValidationCode<'TOverall, 'Error, 'TResult2>)
            ) : TaskValidationCode<'TOverall, 'Error, 'TResult2> =
            this.Bind(task.GetAwaiter(), continuation)

        member inline this.ReturnFrom
            (task: TaskValidation<'T, 'Error>)
            : TaskValidationCode<'T, 'Error, 'T> =
            this.Bind(task.GetAwaiter(), (fun v -> this.Return v))

        member inline this.BindReturn(x: TaskValidation<'T, 'Error>, f) =
            this.Bind(x.GetAwaiter(), (fun x -> this.Return(f x)))

        member inline _.MergeSources
            (t1: TaskValidation<'T, 'Error>, t2: TaskValidation<'T1, 'Error>)
            =
            TaskValidation.zip t1 t2

        member inline _.Source(s: #seq<_>) = s

[<AutoOpen>]
module TaskValidationCEExtensionsMediumPriority =

    // Medium priority extensions
    type TaskValidationBuilderBase with

        member inline this.Source(t: Task<'T>) : TaskValidation<'T, 'Error> =
            t
            |> Task.map Ok

        member inline this.Source(computation: Async<'T>) : TaskValidation<'T, 'Error> =
            computation
            |> Async.map Ok
            |> Async.StartImmediateAsTask

[<AutoOpen>]
module TaskValidationCEExtensionsHighPriority2 =

    // Medium priority extensions
    type TaskValidationBuilderBase with


        member inline _.Source(result: Async<Validation<_, _>>) : Task<Validation<_, _>> =
            result
            |> Async.StartImmediateAsTask

        member inline _.Source(t: ValueTask<Validation<_, _>>) : Task<Validation<_, _>> =
            task { return! t }

        member inline _.Source(result: Validation<_, _>) : Task<Validation<_, _>> =
            Task.singleton result

        member inline _.Source(result: Choice<_, _>) : Task<Validation<_, _>> =
            result
            |> Validation.ofChoice
            |> Task.singleton
