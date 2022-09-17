namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FsToolkit.ErrorHandling
#if NETSTANDARD2_0
open FSharp.Control.Tasks.Affine.Unsafe
open FSharp.Control.Tasks.Affine
open Ply
#endif
[<AutoOpen>]
module TaskResultOptionCE =

#if NETSTANDARD2_0
    type TaskResultOptionBuilder() =
        member inline _.Return(value: 'T) : Ply<Result<'T option, 'TError>> =
            uply.Return <| result.Return(Some value)

        member inline _.ReturnFrom(taskResult: Task<Result<'T option, 'TError>>) : Ply<Result<'T option, 'TError>> =
            uply.ReturnFrom taskResult

        member inline _.Bind
            (
                taskResult: Task<Result<'T option, 'TError>>,
                [<InlineIfLambda>] binder: 'T -> Ply<Result<'U option, 'TError>>
            ) : Ply<Result<'U option, 'TError>> =
            let binder' r =
                match r with
                | Ok (Some x) -> binder x
                | Ok None -> uply.Return <| Ok None
                | Error x -> uply.Return <| Error x

            uply.Bind(taskResult, binder')

        member inline _.Combine(tro1, tro2) =
            tro1 |> TaskResultOption.bind (fun _ -> tro2) |> uply.ReturnFrom

        member inline _.Delay([<InlineIfLambda>] f) = uply.Delay f

        member inline _.Run([<InlineIfLambda>] f: unit -> Ply<'m>) = task.Run f

    let taskResultOption = TaskResultOptionBuilder()

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


    /// Task<Result<'T option, 'Error>>
    type TaskResultOption<'T, 'Error> = Task<Result<'T option, 'Error>>

    [<Struct; NoComparison; NoEquality>]
    type TaskResultOptionStateMachineData<'T, 'Error> =

        [<DefaultValue(false)>]
        val mutable Result: Result<'T option, 'Error>

        [<DefaultValue(false)>]
        val mutable MethodBuilder: AsyncTaskResultOptionMethodBuilder<'T, 'Error>

        member this.IsResultError =
            match this.Result with
            | Error _ -> true
            | Ok None -> true
            | _ -> false

        member this.IsTaskCompleted = this.MethodBuilder.Task.IsCompleted

    and AsyncTaskResultOptionMethodBuilder<'TOverall, 'Error> = AsyncTaskMethodBuilder<Result<'TOverall option, 'Error>>

    and TaskResultOptionStateMachine<'TOverall, 'Error> =
        ResumableStateMachine<TaskResultOptionStateMachineData<'TOverall, 'Error>>

    and TaskResultOptionResumptionFunc<'TOverall, 'Error> =
        ResumptionFunc<TaskResultOptionStateMachineData<'TOverall, 'Error>>

    and TaskResultOptionResumptionDynamicInfo<'TOverall, 'Error> =
        ResumptionDynamicInfo<TaskResultOptionStateMachineData<'TOverall, 'Error>>

    and TaskResultOptionCode<'TOverall, 'Error, 'T> =
        ResumableCode<TaskResultOptionStateMachineData<'TOverall, 'Error>, 'T>

    type TaskResultOptionBuilder() =
        member inline _.Return(value: 'T) : TaskResultOptionCode<'T, 'Error, 'T> =
            TaskResultOptionCode<'T, 'Error, _>(fun sm ->
                // printfn "Return Called --> "
                sm.Data.Result <- Ok(Some value)

                true)

        static member BindDynamic
            (
                sm: byref<_>,
                task: TaskResultOption<'TResult1, 'Error>,
                continuation: ('TResult1 -> TaskResultOptionCode<'TOverall, 'Error, 'TResult2>)
            ) : bool =
            let mutable awaiter = task.GetAwaiter()

            let cont =
                (TaskResultOptionResumptionFunc<'TOverall, 'Error>(fun sm ->
                    // printfn "ByndDynamic --> %A" sm.Data.Result
                    let result = awaiter.GetResult()

                    match result with
                    | Ok (Some result) -> (continuation result).Invoke(&sm)
                    | Ok None ->
                        sm.Data.Result <- Ok None
                        true
                    | Error e ->
                        sm.Data.Result <- Error e
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
                task: TaskResultOption<'TResult1, 'Error>,
                continuation: ('TResult1 -> TaskResultOptionCode<'TOverall, 'Error, 'TResult2>)
            ) : TaskResultOptionCode<'TOverall, 'Error, 'TResult2> =

            TaskResultOptionCode<'TOverall, 'Error, _>(fun sm ->
                if __useResumableCode then
                    //-- RESUMABLE CODE START
                    // Get an awaiter from the task
                    // printfn "Bynd--> %A" sm.Data.Result
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
                        | Ok (Some result) -> (continuation result).Invoke(&sm)
                        | Ok None ->
                            sm.Data.Result <- Ok None
                            true
                        | Error e ->
                            sm.Data.Result <- Error e
                            true

                    else if sm.Data.MethodBuilder.Task.IsCompleted then
                        true
                    else
                        sm.Data.MethodBuilder.AwaitUnsafeOnCompleted(&awaiter, &sm)
                        false

                else
                    TaskResultOptionBuilder.BindDynamic(&sm, task, continuation)
            //-- RESUMABLE CODE END
            )


        member inline this.ReturnFrom(task: TaskResultOption<'T, 'Error>) : TaskResultOptionCode<'T, 'Error, 'T> =
            this.Bind(task, (fun v -> this.Return v))


        member inline _.Combine(tro1, tro2) =

            ResumableCode.Combine(tro1, tro2)

        member inline _.Delay
            (generator: unit -> TaskResultOptionCode<'TOverall, 'Error, 'T>)
            : TaskResultOptionCode<'TOverall, 'Error, 'T> =
            TaskResultOptionCode<'TOverall, 'Error, 'T>(fun sm -> (generator ()).Invoke(&sm))

        static member RunDynamic(code: TaskResultOptionCode<'T, 'Error, 'T>) : TaskResultOption<'T, 'Error> =
            let mutable sm = TaskResultOptionStateMachine<'T, 'Error>()

            let initialResumptionFunc =
                TaskResultOptionResumptionFunc<'T, 'Error>(fun sm -> code.Invoke(&sm))

            let resumptionInfo =
                { new TaskResultOptionResumptionDynamicInfo<_, _>(initialResumptionFunc) with
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
                                let mutable awaiter =
                                    sm.ResumptionDynamicInfo.ResumptionData :?> ICriticalNotifyCompletion

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

            sm.ResumptionDynamicInfo <- resumptionInfo
            sm.Data.MethodBuilder <- AsyncTaskResultOptionMethodBuilder<'T, 'Error>.Create ()
            sm.Data.MethodBuilder.Start(&sm)
            sm.Data.MethodBuilder.Task

        member inline _.Run(code: TaskResultOptionCode<'T, 'Error, 'T>) : TaskResultOption<'T, 'Error> =
            if __useResumableCode then
                __stateMachine<TaskResultOptionStateMachineData<'T, 'Error>, TaskResultOption<'T, 'Error>>
                    (MoveNextMethodImpl<_>(fun sm ->
                        //-- RESUMABLE CODE START
                        __resumeAt sm.ResumptionPoint

                        let mutable __stack_exn: Exception = null

                        try
                            // printfn "Run BeforeInvoke Task.Status  --> %A" sm.Data.MethodBuilder.Task.Status
                            let __stack_code_fin = code.Invoke(&sm)
                            // printfn "Run Task.Status --> %A" sm.Data.MethodBuilder.Task.Status
                            // If the `sm.Data.MethodBuilder` has already been set somewhere else (like While/WhileDynamic), we shouldn't continue
                            if __stack_code_fin && not sm.Data.IsTaskCompleted then

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
                    (SetStateMachineMethodImpl<_>(fun sm state -> sm.Data.MethodBuilder.SetStateMachine(state)))
                    (AfterCode<_, _>(fun sm ->
                        sm.Data.MethodBuilder <- AsyncTaskResultOptionMethodBuilder<'T, 'Error>.Create ()
                        sm.Data.MethodBuilder.Start(&sm)
                        sm.Data.MethodBuilder.Task))
            else
                TaskResultOptionBuilder.RunDynamic(code)

    let taskResultOption = TaskResultOptionBuilder()

#endif
