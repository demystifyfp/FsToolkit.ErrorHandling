namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
#nowarn "0044"
// FSharp.Control.Tasks.TaskBuilder is marked obselete, see [why](https://github.com/rspeele/TaskBuilder.fs/blob/master/TaskBuilder.fs#L17-L19)
// We however need access to the underlying `Step` type to be able to pull off the same tricks making this a usable CE
open FSharp.Control.Tasks.TaskBuilder

[<AutoOpen>]
module TaskResultOptionCE =  

  type TaskResultOptionBuilder() =
    member __.Return (value: 'T) 
      : Step<Result<'T option,'TError>> =
      Return <| result.Return (Some value)

    member __.ReturnFrom
        (taskResult: Task<Result<'T option, 'TError>>)
        : Step<Result<'T option, 'TError>> =
      ReturnFrom taskResult

    member __.Bind
        (taskResult: Task<Result<'T option, 'TError>>,
         binder: 'T -> Step<Result<'U option, 'TError>>)
        : Step<Result<'U option, 'TError>> =
        let binder' r = 
          match r with
          | Ok (Some x) -> binder x
          | Ok None -> ret <| Ok None
          | Error x -> ret <| Error x
        bindTaskConfigureFalse taskResult binder'

    member __.Combine(tro1, tro2) =
      tro1
      |> TaskResultOption.bind (fun _ -> tro2)
      |> ReturnFrom

    member __.Delay f =
      task.Delay f

    member inline __.Run(f : unit -> Step<'m>) = run f

  let taskResultOption = TaskResultOptionBuilder()