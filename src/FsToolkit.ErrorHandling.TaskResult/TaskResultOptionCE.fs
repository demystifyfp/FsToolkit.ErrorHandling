namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FSharp.Control.Tasks.Affine.Unsafe
open FSharp.Control.Tasks.Affine
open Ply

[<AutoOpen>]
module TaskResultOptionCE =

    type TaskResultOptionBuilder() =
        member inline _.Return(value: 'T) : Ply<Result<'T option, 'TError>> =
            uply.Return <| result.Return(Some value)

        member inline _.ReturnFrom(taskResult: Task<Result<'T option, 'TError>>) : Ply<Result<'T option, 'TError>> =
            uply.ReturnFrom taskResult

        member inline _.Bind
            (
                taskResult: Task<Result<'T option, 'TError>>,
                binder: 'T -> Ply<Result<'U option, 'TError>>
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

        member inline _.Delay f = uply.Delay f

        member inline _.Run(f: unit -> Ply<'m>) = task.Run f

    let taskResultOption = TaskResultOptionBuilder()
