namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks


#if NETSTANDARD2_0
open Ply
open System.Runtime.CompilerServices

[<AutoOpen>]
module TaskOptionCE =
    type TaskOptionBuilder() =
        member val SomeUnit = Some()

        member inline _.Return(value: 'T) : Ply<_ option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply.Return
            <| option.Return value

        member inline _.ReturnFrom(taskResult: Task<_ option>) : Ply<_ option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply.ReturnFrom taskResult

        member inline _.Zero() : Ply<_ option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply.Return
            <| option.Zero()

        member inline _.Bind
            (
                taskResult: Task<_ option>,
                [<InlineIfLambda>] binder: 'T -> Ply<_ option>
            ) : Ply<_ option> =
            let binder' r =
                match r with
                | Some x -> binder x
                | None -> FSharp.Control.Tasks.Affine.Unsafe.uply.Return None

            FSharp.Control.Tasks.Affine.Unsafe.uply.Bind(taskResult, binder')

        member inline _.Delay([<InlineIfLambda>] generator: unit -> Ply<_ option>) =
            FSharp.Control.Tasks.Affine.Unsafe.uply.Delay(generator)

        member inline _.Combine
            (
                computation1: Ply<'T option>,
                [<InlineIfLambda>] computation2: unit -> Ply<'U option>
            ) : Ply<'U option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply {
                match! computation1 with
                | None -> return None
                | Some _ -> return! computation2 ()
            }

        member inline _.TryWith
            (
                [<InlineIfLambda>] computation: unit -> Ply<_ option>,
                [<InlineIfLambda>] handler: exn -> Ply<_ option>
            ) : Ply<_ option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply.TryWith(computation, handler)

        member inline _.TryFinally
            (
                [<InlineIfLambda>] computation: unit -> Ply<_ option>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : Ply<_ option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply.TryFinally(computation, compensation)

        member inline _.Using
            (
                resource: 'T :> IDisposable,
                [<InlineIfLambda>] binder: 'T -> Ply<_ option>
            ) : Ply<_ option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply.Using(resource, binder)

        member inline _.While
            (
                [<InlineIfLambda>] guard: unit -> bool,
                [<InlineIfLambda>] computation: unit -> Ply<'U option>
            ) : Ply<'U option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply {
                let mutable fin, result = false, None

                while not fin && guard () do
                    match! computation () with
                    | Some _ as o -> result <- o
                    | None ->
                        result <- None
                        fin <- true

                return result
            }

        member inline _.For(sequence: #seq<'T>, binder: 'T -> Ply<'U option>) : Ply<'U option> =
            FSharp.Control.Tasks.Affine.Unsafe.uply {
                use enumerator = sequence.GetEnumerator()
                let mutable fin, result = false, None

                while not fin && enumerator.MoveNext() do
                    match! binder enumerator.Current with
                    | Some _ as o -> result <- o
                    | None ->
                        result <- None
                        fin <- true

                return result
            }

        member inline this.BindReturn(x: Task<'T option>, [<InlineIfLambda>] f) =
            this.Bind(x, (fun x -> this.Return(f x)))

        member inline _.MergeSources(t1: Task<'T option>, t2: Task<'T1 option>) = TaskOption.zip t1 t2
        member inline _.Run([<InlineIfLambda>] f: unit -> Ply<'m>) = FSharp.Control.Tasks.Affine.task.Run f

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(task: Task<_ option>) : Task<_ option> = task

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(t: ValueTask<_ option>) : Task<_ option> =
            FSharp.Control.Tasks.Affine.task { return! t }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(async: Async<_ option>) : Task<_ option> = async |> Async.StartAsTask

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(p: Ply<_ option>) : Task<_ option> =
            FSharp.Control.Tasks.Affine.task { return! p }

    let taskOption = TaskOptionBuilder()

    type BackgroundTaskOptionBuilder() =
        member val SomeUnit = Some()

        member inline _.Return(value: 'T) : Ply<_ option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply.Return
            <| option.Return value

        member inline _.ReturnFrom(taskResult: Task<_ option>) : Ply<_ option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply.ReturnFrom taskResult

        member inline _.Zero() : Ply<_ option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply.Return
            <| option.Zero()

        member inline _.Bind
            (
                taskResult: Task<_ option>,
                [<InlineIfLambda>] binder: 'T -> Ply<_ option>
            ) : Ply<_ option> =
            let binder' r =
                match r with
                | Some x -> binder x
                | None -> FSharp.Control.Tasks.NonAffine.Unsafe.uply.Return None

            FSharp.Control.Tasks.NonAffine.Unsafe.uply.Bind(taskResult, binder')

        member inline _.Delay([<InlineIfLambda>] generator: unit -> Ply<_ option>) =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply.Delay(generator)

        member inline _.Combine
            (
                computation1: Ply<'T option>,
                [<InlineIfLambda>] computation2: unit -> Ply<'U option>
            ) : Ply<'U option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply {
                match! computation1 with
                | None -> return None
                | Some _ -> return! computation2 ()
            }

        member inline _.TryWith
            (
                [<InlineIfLambda>] computation: unit -> Ply<_ option>,
                [<InlineIfLambda>] handler: exn -> Ply<_ option>
            ) : Ply<_ option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply.TryWith(computation, handler)

        member inline _.TryFinally
            (
                [<InlineIfLambda>] computation: unit -> Ply<_ option>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : Ply<_ option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply.TryFinally(computation, compensation)

        member inline _.Using
            (
                resource: 'T :> IDisposable,
                [<InlineIfLambda>] binder: 'T -> Ply<_ option>
            ) : Ply<_ option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply.Using(resource, binder)

        member inline _.While
            (
                [<InlineIfLambda>] guard: unit -> bool,
                [<InlineIfLambda>] computation: unit -> Ply<'U option>
            ) : Ply<'U option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply {
                let mutable fin, result = false, None

                while not fin && guard () do
                    match! computation () with
                    | Some _ as o -> result <- o
                    | None ->
                        result <- None
                        fin <- true

                return result
            }

        member inline _.For(sequence: #seq<'T>, binder: 'T -> Ply<'U option>) : Ply<'U option> =
            FSharp.Control.Tasks.NonAffine.Unsafe.uply {
                use enumerator = sequence.GetEnumerator()
                let mutable fin, result = false, None

                while not fin && enumerator.MoveNext() do
                    match! binder enumerator.Current with
                    | Some _ as o -> result <- o
                    | None ->
                        result <- None
                        fin <- true

                return result
            }

        member inline this.BindReturn(x: Task<'T option>, [<InlineIfLambda>] f) =
            this.Bind(x, (fun x -> this.Return(f x)))

        member inline _.MergeSources(t1: Task<'T option>, t2: Task<'T1 option>) =
            FSharp.Control.Tasks.NonAffine.task {
                let! o1 = t1
                let! o2 = t2
                return Option.zip o1 o2
            }

        member inline _.Run([<InlineIfLambda>] f: unit -> Ply<'m>) =
            FSharp.Control.Tasks.NonAffine.task.Run f

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(task: Task<_ option>) : Task<_ option> = task

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(t: ValueTask<_ option>) : Task<_ option> =
            FSharp.Control.Tasks.NonAffine.task { return! t }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(async: Async<_ option>) : Task<_ option> = async |> Async.StartAsTask

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(p: Ply<_ option>) : Task<_ option> =
            FSharp.Control.Tasks.NonAffine.task { return! p }

    let backgroundTaskOption = BackgroundTaskOptionBuilder()



[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module TaskOptionCEExtensionsLower =

    type TaskOptionBuilder with
        member inline this.Source(t: ^TaskLike) : Task<'T option> =
            FSharp.Control.Tasks.Affine.task {
                let! r = t
                return Some r
            }

    type BackgroundTaskOptionBuilder with
        member inline this.Source(t: ^TaskLike) : Task<'T option> =
            FSharp.Control.Tasks.NonAffine.task {
                let! r = t
                return Some r
            }

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module TaskOptionCEExtensions =

    type TaskOptionBuilder with
        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(r: 't option) = Task.singleton r

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: Task<'T>) =
            FSharp.Control.Tasks.Affine.task {
                let! o = a
                return Some o
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline x.Source(a: Task) =
            FSharp.Control.Tasks.Affine.task {
                do! a
                return x.SomeUnit
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: ValueTask<'T>) =
            FSharp.Control.Tasks.Affine.task {
                let! o = a
                return Some o
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline x.Source(a: ValueTask) =
            FSharp.Control.Tasks.Affine.task {
                do! a
                return x.SomeUnit
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: Async<'t>) =
            FSharp.Control.Tasks.Affine.task {
                let! o = a |> Async.StartAsTask
                return Some o
            }


    type BackgroundTaskOptionBuilder with
        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(r: 't option) = Task.singleton r

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: Task<'T>) =
            FSharp.Control.Tasks.NonAffine.task {
                let! o = a
                return Some o
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline x.Source(a: Task) =
            FSharp.Control.Tasks.NonAffine.task {
                do! a
                return x.SomeUnit
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: ValueTask<'T>) =
            FSharp.Control.Tasks.NonAffine.task {
                let! o = a
                return Some o
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline x.Source(a: ValueTask) =
            FSharp.Control.Tasks.NonAffine.task {
                do! a
                return x.SomeUnit
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: Async<'t>) =
            FSharp.Control.Tasks.NonAffine.task {
                let! o = a |> Async.StartAsTask
                return Some o
            }



#else

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


/// Task<'T option>
type TaskOption<'T> = Task<'T option>

[<Struct; NoComparison; NoEquality>]
type TaskOptionStateMachineData<'T> =

    [<DefaultValue(false)>]
    val mutable Result: 'T option voption

    [<DefaultValue(false)>]
    val mutable MethodBuilder: AsyncTaskOptionMethodBuilder<'T>

    member this.IsResultNone =
        match this.Result with
        | ValueNone -> false
        | ValueSome (None) -> true
        | ValueSome _ -> false

    member this.SetResult() =
        match this.Result with
        | ValueNone -> this.MethodBuilder.SetResult None
        | ValueSome x -> this.MethodBuilder.SetResult x


    member this.IsTaskCompleted = this.MethodBuilder.Task.IsCompleted

and AsyncTaskOptionMethodBuilder<'TOverall> = AsyncTaskMethodBuilder<'TOverall option>
and TaskOptionStateMachine<'TOverall> = ResumableStateMachine<TaskOptionStateMachineData<'TOverall>>
and TaskOptionResumptionFunc<'TOverall> = ResumptionFunc<TaskOptionStateMachineData<'TOverall>>
and TaskOptionResumptionDynamicInfo<'TOverall> = ResumptionDynamicInfo<TaskOptionStateMachineData<'TOverall>>
and TaskOptionCode<'TOverall, 'T> = ResumableCode<TaskOptionStateMachineData<'TOverall>, 'T>


module TaskOptionBuilderBase =

    let rec WhileDynamic
        (
            sm: byref<TaskOptionStateMachine<_>>,
            condition: unit -> bool,
            body: TaskOptionCode<_, _>
        ) : bool =
        if condition () then
            if body.Invoke(&sm) then
                if sm.Data.IsResultNone then
                    // Set the result now to allow short-circuiting of the rest of the CE.
                    // Run/RunDynamic will skip setting the result if it's already been set.
                    // Combine/CombineDynamic will not continue if the result has been set.
                    sm.Data.SetResult()
                    true
                else
                    WhileDynamic(&sm, condition, body)
            else
                let rf = sm.ResumptionDynamicInfo.ResumptionFunc

                sm.ResumptionDynamicInfo.ResumptionFunc <-
                    (TaskOptionResumptionFunc<_>(fun sm -> WhileBodyDynamicAux(&sm, condition, body, rf)))

                false
        else
            true

    and WhileBodyDynamicAux
        (
            sm: byref<TaskOptionStateMachine<_>>,
            condition: unit -> bool,
            body: TaskOptionCode<_, _>,
            rf: TaskOptionResumptionFunc<_>
        ) : bool =
        if rf.Invoke(&sm) then
            if sm.Data.IsResultNone then
                // Set the result now to allow short-circuiting of the rest of the CE.
                // Run/RunDynamic will skip setting the result if it's already been set.
                // Combine/CombineDynamic will not continue if the result has been set.
                sm.Data.SetResult()
                true
            else
                WhileDynamic(&sm, condition, body)
        else
            let rf = sm.ResumptionDynamicInfo.ResumptionFunc

            sm.ResumptionDynamicInfo.ResumptionFunc <-
                (TaskOptionResumptionFunc<_>(fun sm -> WhileBodyDynamicAux(&sm, condition, body, rf)))

            false


type TaskOptionBuilderBase() =

    member inline _.Delay(generator: unit -> TaskOptionCode<'TOverall, 'T>) : TaskOptionCode<'TOverall, 'T> =
        TaskOptionCode<'TOverall, 'T>(fun sm -> (generator ()).Invoke(&sm))

    /// Used to represent no-ops like the implicit empty "else" branch of an "if" expression.
    // [<DefaultValue>] // TODO: Figureout if this attribute is needed, without it it allows tests to pass with implicit else branches resulting in `Some ()`
    member inline _.Zero<'TOverall>() : TaskOptionCode<'TOverall, unit> = ResumableCode.Zero()

    member inline _.Return(value: 'T) : TaskOptionCode<'T, 'T> =
        TaskOptionCode<'T, _>
            (fun sm ->
                sm.Data.Result <- ValueSome(Some value)
                true)



    static member inline CombineDynamic
        (
            sm: byref<TaskOptionStateMachine<_>>,
            task1: TaskOptionCode<'TOverall, unit>,
            task2: TaskOptionCode<'TOverall, 'T>
        ) : bool =
        let shouldContinue = task1.Invoke(&sm)

        if sm.Data.IsTaskCompleted then
            true
        elif shouldContinue then
            task2.Invoke(&sm)
        else
            let rec resume (mf: TaskOptionResumptionFunc<_>) =
                TaskOptionResumptionFunc<_>
                    (fun sm ->
                        let shouldContinue = mf.Invoke(&sm)

                        if sm.Data.IsTaskCompleted then
                            true
                        elif shouldContinue then
                            task2.Invoke(&sm)
                        else
                            sm.ResumptionDynamicInfo.ResumptionFunc <-
                                (resume (sm.ResumptionDynamicInfo.ResumptionFunc))

                            false)

            sm.ResumptionDynamicInfo.ResumptionFunc <- (resume (sm.ResumptionDynamicInfo.ResumptionFunc))
            false

    /// Chains together a step with its following step.
    /// Note that this requires that the first step has no result.
    /// This prevents constructs like `task { return 1; return 2; }`.
    member inline _.Combine
        (
            task1: TaskOptionCode<'TOverall, unit>,
            task2: TaskOptionCode<'TOverall, 'T>
        ) : TaskOptionCode<'TOverall, 'T> =

        TaskOptionCode<'TOverall, 'T>
            (fun sm ->
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
                    TaskOptionBuilderBase.CombineDynamic(&sm, task1, task2))

    /// Builds a step that executes the body while the condition predicate is true.
    member inline _.While
        (
            [<InlineIfLambda>] condition: unit -> bool,
            body: TaskOptionCode<'TOverall, unit>
        ) : TaskOptionCode<'TOverall, unit> =
        TaskOptionCode<'TOverall, unit>
            (fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    let mutable __stack_go = true

                    while __stack_go
                          && not sm.Data.IsResultNone
                          && condition () do
                        // printfn "While -> %A" sm.Data.Result
                        // NOTE: The body of the state machine code for 'while' may contain await points, so resuming
                        // the code will branch directly into the expanded 'body', branching directly into the while loop
                        let __stack_body_fin = body.Invoke(&sm)
                        // printfn "While After Invoke --> %A" sm.Data.Result
                        // If the body completed, we go back around the loop (__stack_go = true)
                        // If the body yielded, we yield (__stack_go = false)
                        __stack_go <- __stack_body_fin

                    if sm.Data.IsResultNone then
                        // Set the result now to allow short-circuiting of the rest of the CE.
                        // Run/RunDynamic will skip setting the result if it's already been set.
                        // Combine/CombineDynamic will not continue if the result has been set.
                        sm.Data.SetResult()

                    __stack_go
                //-- RESUMABLE CODE END
                else
                    TaskOptionBuilderBase.WhileDynamic(&sm, condition, body))


    /// Wraps a step in a try/with. This catches exceptions both in the evaluation of the function
    /// to retrieve the step, and in the continuation of the step (if any).
    member inline _.TryWith
        (
            body: TaskOptionCode<'TOverall, 'T>,
            catch: exn -> TaskOptionCode<'TOverall, 'T>
        ) : TaskOptionCode<'TOverall, 'T> =
        ResumableCode.TryWith(body, catch)

    /// Wraps a step in a try/finally. This catches exceptions both in the evaluation of the function
    /// to retrieve the step, and in the continuation of the step (if any).
    member inline _.TryFinally
        (
            body: TaskOptionCode<'TOverall, 'T>,
            [<InlineIfLambda>] compensation: unit -> unit
        ) : TaskOptionCode<'TOverall, 'T> =
        ResumableCode.TryFinally(
            body,
            ResumableCode<_, _>
                (fun _sm ->
                    compensation ()
                    true)
        )

    member inline this.For
        (
            sequence: seq<'T>,
            body: 'T -> TaskOptionCode<'TOverall, unit>
        ) : TaskOptionCode<'TOverall, unit> =
        ResumableCode.Using(
            sequence.GetEnumerator(),
            // ... and its body is a while loop that advances the enumerator and runs the body on each element.
            (fun e ->
                this.While(
                    (fun () -> e.MoveNext()),
                    TaskOptionCode<'TOverall, unit>(fun sm -> (body e.Current).Invoke(&sm))
                ))
        )

    member inline internal this.TryFinallyAsync
        (
            body: TaskOptionCode<'TOverall, 'T>,
            compensation: unit -> ValueTask
        ) : TaskOptionCode<'TOverall, 'T> =
        ResumableCode.TryFinallyAsync(
            body,
            ResumableCode<_, _>
                (fun sm ->
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
                            TaskOptionResumptionFunc<'TOverall>
                                (fun sm ->
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

    member inline this.Using<'Resource, 'TOverall, 'T when 'Resource :> IAsyncDisposable>
        (
            resource: 'Resource,
            body: 'Resource -> TaskOptionCode<'TOverall, 'T>
        ) : TaskOptionCode<'TOverall, 'T> =
        this.TryFinallyAsync(
            (fun sm -> (body resource).Invoke(&sm)),
            (fun () ->
                if not (isNull (box resource)) then
                    resource.DisposeAsync()
                else
                    ValueTask())
        )

    member inline this.Source(computation: Async<'T option>) : TaskOption<'T> = computation |> Async.StartAsTask
    member inline this.Source(taskOption: TaskOption<'T>) : TaskOption<'T> = taskOption
    member inline this.Source(taskOption: ValueTask<'T option>) : TaskOption<'T> = task { return! taskOption }


type TaskOptionBuilder() =

    inherit TaskOptionBuilderBase()

    // This is the dynamic implementation - this is not used
    // for statically compiled tasks.  An executor (resumptionFuncExecutor) is
    // registered with the state machine, plus the initial resumption.
    // The executor stays constant throughout the execution, it wraps each step
    // of the execution in a try/with.  The resumption is changed at each step
    // to represent the continuation of the computation.
    static member RunDynamic(code: TaskOptionCode<'T, 'T>) : TaskOption<'T> =
        let mutable sm = TaskOptionStateMachine<'T>()

        let initialResumptionFunc =
            TaskOptionResumptionFunc<'T>(fun sm -> code.Invoke(&sm))

        let resumptionInfo =
            { new TaskOptionResumptionDynamicInfo<_>(initialResumptionFunc) with
                member info.MoveNext(sm) =
                    let mutable savedExn = null

                    try
                        sm.ResumptionDynamicInfo.ResumptionData <- null
                        let step = info.ResumptionFunc.Invoke(&sm)

                        // If the `sm.Data.MethodBuilder` has already been set somewhere else (like While/WhileDynamic), we shouldn't continue
                        if sm.Data.IsTaskCompleted then ()

                        if step then
                            sm.Data.SetResult()
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

        sm.ResumptionDynamicInfo <- resumptionInfo
        sm.Data.MethodBuilder <- AsyncTaskOptionMethodBuilder<'T>.Create ()
        sm.Data.MethodBuilder.Start(&sm)
        sm.Data.MethodBuilder.Task

    member inline _.Run(code: TaskOptionCode<'T, 'T>) : TaskOption<'T> =
        if __useResumableCode then
            __stateMachine<TaskOptionStateMachineData<'T>, TaskOption<'T>>
                (MoveNextMethodImpl<_>
                    (fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint
                        let mutable __stack_exn: Exception = null

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if __stack_code_fin && not sm.Data.IsTaskCompleted then
                                sm.Data.SetResult()
                        with
                        | exn -> __stack_exn <- exn
                        // Run SetException outside the stack unwind, see https://github.com/dotnet/roslyn/issues/26567
                        match __stack_exn with
                        | null -> ()
                        | exn -> sm.Data.MethodBuilder.SetException exn
                        //-- RESUMABLE CODE END
                        ))
                (SetStateMachineMethodImpl<_>(fun sm state -> sm.Data.MethodBuilder.SetStateMachine(state)))
                (AfterCode<_, _>
                    (fun sm ->
                        sm.Data.MethodBuilder <- AsyncTaskOptionMethodBuilder<'T>.Create ()
                        sm.Data.MethodBuilder.Start(&sm)
                        sm.Data.MethodBuilder.Task))
        else
            TaskOptionBuilder.RunDynamic(code)

type BackgroundTaskOptionBuilder() =

    inherit TaskOptionBuilderBase()

    static member RunDynamic(code: TaskOptionCode<'T, 'T>) : TaskOption<'T> =
        // backgroundTask { .. } escapes to a background thread where necessary
        // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
        if
            isNull SynchronizationContext.Current
            && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
        then
            TaskOptionBuilder.RunDynamic(code)
        else
            Task.Run<'T option>(fun () -> TaskOptionBuilder.RunDynamic(code))


    //// Same as TaskBuilder.Run except the start is inside Task.Run if necessary
    member inline _.Run(code: TaskOptionCode<'T, 'T>) : TaskOption<'T> =
        if __useResumableCode then
            __stateMachine<TaskOptionStateMachineData<'T>, TaskOption<'T>>
                (MoveNextMethodImpl<_>
                    (fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint

                        try
                            let __stack_code_fin = code.Invoke(&sm)

                            if __stack_code_fin && not sm.Data.IsTaskCompleted then
                                sm.Data.MethodBuilder.SetResult(sm.Data.Result.Value)
                        with
                        | exn -> sm.Data.MethodBuilder.SetException exn
                        //-- RESUMABLE CODE END
                        ))
                (SetStateMachineMethodImpl<_>(fun sm state -> sm.Data.MethodBuilder.SetStateMachine(state)))
                (AfterCode<_, TaskOption<'T>>
                    (fun sm ->
                        // backgroundTask { .. } escapes to a background thread where necessary
                        // See spec of ConfigureAwait(false) at https://devblogs.microsoft.com/dotnet/configureawait-faq/
                        if
                            isNull SynchronizationContext.Current
                            && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)
                        then
                            sm.Data.MethodBuilder <- AsyncTaskOptionMethodBuilder<'T>.Create ()
                            sm.Data.MethodBuilder.Start(&sm)
                            sm.Data.MethodBuilder.Task
                        else
                            let sm = sm // copy contents of state machine so we can capture it

                            Task.Run<'T option>
                                (fun () ->
                                    let mutable sm = sm // host local mutable copy of contents of state machine on this thread pool thread
                                    sm.Data.MethodBuilder <- AsyncTaskOptionMethodBuilder<'T>.Create ()
                                    sm.Data.MethodBuilder.Start(&sm)
                                    sm.Data.MethodBuilder.Task)))
        else
            BackgroundTaskOptionBuilder.RunDynamic(code)

[<AutoOpen>]
module TaskOptionBuilder =

    let taskOption = TaskOptionBuilder()
    let backgroundTaskOption = BackgroundTaskOptionBuilder()


open Microsoft.FSharp.Control
open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators

[<AutoOpen>]
module TaskOptionCEExtensionsLowPriority =
    // Low priority extensions
    type TaskOptionBuilderBase with

        [<NoEagerConstraintApplication>]
        static member inline BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall when ^TaskLike: (member GetAwaiter :
            unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted :
            unit -> bool) and ^Awaiter: (member GetResult : unit -> 'TResult1 option)>
            (
                sm: byref<_>,
                task: ^TaskLike,
                continuation: ('TResult1 -> TaskOptionCode<'TOverall, 'TResult2>)
            ) : bool =

            let mutable awaiter =
                (^TaskLike: (member GetAwaiter : unit -> ^Awaiter) (task))

            let cont =
                (TaskOptionResumptionFunc<'TOverall>
                    (fun sm ->
                        let result =
                            (^Awaiter: (member GetResult : unit -> 'TResult1 option) (awaiter))

                        match result with
                        | Some result -> (continuation result).Invoke(&sm)
                        | None ->
                            sm.Data.Result <- ValueSome None
                            true))

            // shortcut to continue immediately
            if (^Awaiter: (member get_IsCompleted : unit -> bool) (awaiter)) then
                cont.Invoke(&sm)
            else
                sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)
                sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                false

        [<NoEagerConstraintApplication>]
        member inline _.Bind< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall when ^TaskLike: (member GetAwaiter :
            unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted :
            unit -> bool) and ^Awaiter: (member GetResult : unit -> 'TResult1 option)>
            (
                task: ^TaskLike,
                continuation: ('TResult1 -> TaskOptionCode<'TOverall, 'TResult2>)
            ) : TaskOptionCode<'TOverall, 'TResult2> =

            TaskOptionCode<'TOverall, _>
                (fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        // Get an awaiter from the awaitable
                        let mutable awaiter =
                            (^TaskLike: (member GetAwaiter : unit -> ^Awaiter) (task))

                        let mutable __stack_fin = true

                        if not (^Awaiter: (member get_IsCompleted : unit -> bool) (awaiter)) then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result =
                                (^Awaiter: (member GetResult : unit -> 'TResult1 option) (awaiter))

                            match result with
                            | Some result -> (continuation result).Invoke(&sm)
                            | None ->
                                sm.Data.Result <- ValueSome None
                                true
                        else
                            sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                            false
                    else
                        TaskOptionBuilderBase.BindDynamic< ^TaskLike, 'TResult1, 'TResult2, ^Awaiter, 'TOverall>(
                            &sm,
                            task,
                            continuation
                        )
                    //-- RESUMABLE CODE END
                    )

        [<NoEagerConstraintApplication>]
        member inline this.ReturnFrom< ^TaskLike, ^Awaiter, 'T when ^TaskLike: (member GetAwaiter : unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted :
            unit -> bool) and ^Awaiter: (member GetResult : unit -> 'T option)>
            (task: ^TaskLike)
            : TaskOptionCode<'T, 'T> =

            this.Bind(task, (fun v -> this.Return v))

        [<NoEagerConstraintApplication>]
        member inline this.Source< ^TaskLike, ^Awaiter, 'T when ^TaskLike: (member GetAwaiter : unit -> ^Awaiter) and ^Awaiter :> ICriticalNotifyCompletion and ^Awaiter: (member get_IsCompleted :
            unit -> bool) and ^Awaiter: (member GetResult : unit -> 'T)>
            (t: ^TaskLike)
            : TaskOption<'T> =

            task {
                let! r = t
                return Some r
            }

        member inline _.Using<'Resource, 'TOverall, 'T when 'Resource :> IDisposable>
            (
                resource: 'Resource,
                body: 'Resource -> TaskOptionCode<'TOverall, 'T>
            ) =
            ResumableCode.Using(resource, body)

[<AutoOpen>]
module TaskOptionCEExtensionsHighPriority =
    // High priority extensions
    type TaskOptionBuilderBase with
        static member BindDynamic
            (
                sm: byref<_>,
                task: TaskOption<'TResult1>,
                continuation: ('TResult1 -> TaskOptionCode<'TOverall, 'TResult2>)
            ) : bool =
            let mutable awaiter = task.GetAwaiter()

            let cont =
                (TaskOptionResumptionFunc<'TOverall>
                    (fun sm ->
                        let result = awaiter.GetResult()

                        match result with
                        | Some result -> (continuation result).Invoke(&sm)
                        | None ->
                            sm.Data.Result <- ValueSome None
                            true))

            // shortcut to continue immediately
            if awaiter.IsCompleted then
                cont.Invoke(&sm)
            else
                sm.ResumptionDynamicInfo.ResumptionData <- (awaiter :> ICriticalNotifyCompletion)
                sm.ResumptionDynamicInfo.ResumptionFunc <- cont
                false

        member inline _.Bind
            (
                task: TaskOption<'TResult1>,
                continuation: ('TResult1 -> TaskOptionCode<'TOverall, 'TResult2>)
            ) : TaskOptionCode<'TOverall, 'TResult2> =

            TaskOptionCode<'TOverall, _>
                (fun sm ->
                    if __useResumableCode then
                        //-- RESUMABLE CODE START
                        // Get an awaiter from the task
                        let mutable awaiter = task.GetAwaiter()

                        let mutable __stack_fin = true

                        if not awaiter.IsCompleted then
                            // This will yield with __stack_yield_fin = false
                            // This will resume with __stack_yield_fin = true
                            let __stack_yield_fin = ResumableCode.Yield().Invoke(&sm)
                            __stack_fin <- __stack_yield_fin

                        if __stack_fin then
                            let result = awaiter.GetResult()

                            match result with
                            | Some result -> (continuation result).Invoke(&sm)
                            | None ->
                                sm.Data.Result <- ValueSome None
                                true

                        else
                            sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                            false

                    else
                        TaskOptionBuilderBase.BindDynamic(&sm, task, continuation)
                    //-- RESUMABLE CODE END
                    )

        member inline this.BindReturn(x: TaskOption<'T>, [<InlineIfLambda>] f) =
            this.Bind(x, (fun x -> this.Return(f x)))

        member inline _.MergeSources(t1: TaskOption<'T>, t2: TaskOption<'T1>) = TaskOption.zip t1 t2

        member inline this.ReturnFrom(task: TaskOption<'T>) : TaskOptionCode<'T, 'T> =
            this.Bind(task, (fun v -> this.Return v))

        member inline _.Source(s: #seq<_>) = s

[<AutoOpen>]
module TaskOptionCEExtensionsMediumPriority =

    // Medium priority extensions
    type TaskOptionBuilderBase with
        member inline this.Source(t: Task<'T>) : TaskOption<'T> = t |> Task.map Some

        member inline this.Source(t: Task) : TaskOption<unit> =
            task {
                do! t
                return Some()
            }

        member inline this.Source(t: ValueTask<'T>) : TaskOption<'T> = t |> Task.mapV Some

        member inline this.Source(t: ValueTask) : TaskOption<unit> =
            task {
                do! t
                return Some()
            }

        member inline this.Source(opt: Option<'T>) : TaskOption<'T> = Task.FromResult opt

        member inline this.Source(computation: Async<'T>) : TaskOption<'T> =
            computation |> Async.map Some |> Async.StartAsTask

#endif
