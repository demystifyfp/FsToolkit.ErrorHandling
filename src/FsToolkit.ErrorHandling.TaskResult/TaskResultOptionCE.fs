namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
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
            tro1
            |> TaskResultOption.bind (fun _ -> tro2)
            |> uply.ReturnFrom

        member inline _.Delay([<InlineIfLambda>] f) = uply.Delay f

        member inline _.Run([<InlineIfLambda>] f: unit -> Ply<'m>) = task.Run f

    let taskResultOption = TaskResultOptionBuilder()

#else

    type TaskResultOptionBuilder() =
        member inline _.Return(value: 'T) : TaskCode<_, _> =
            option.Return value
            |> result.Return
            |> task.Return

        member inline _.ReturnFrom(taskResult: Task<Result<'T option, 'TError>>) : TaskCode<_, _> =

            task.ReturnFrom taskResult

        member inline _.Bind
            (
                taskResult: Task<Result<'T option, 'TError>>,
                [<InlineIfLambda>] binder: 'T -> TaskCode<_, _>
            ) : TaskCode<_, _> =

            task.Bind(
                taskResult,
                function
                | Ok (Some x) -> binder x
                | Ok None -> task.Return <| Ok None
                | Error x -> task.Return <| Error x
            )

        member inline _.Combine(tro1, tro2) =

            task.Combine(tro1, tro2)

        member inline _.Delay(f) = task.Delay f

        member inline _.Run([<InlineIfLambda>] f: TaskCode<_, _>) = task.Run f

    let taskResultOption = TaskResultOptionBuilder()

#endif
