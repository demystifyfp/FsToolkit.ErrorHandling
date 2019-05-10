namespace FsToolkit.ErrorHandling

[<AutoOpen>]
module JobResultOptionCE =  
  open Hopac
  type JobResultOptionBuilder() =
    member __.Return value = JobResultOption.retn value
    member __.ReturnFrom value = value
    member __.Bind (result, binder) =
      JobResultOption.bind binder result

    member __.Combine(aro1, aro2) =
      aro1
      |> JobResultOption.bind (fun _ -> aro2)

    member __.Delay f =
      Job.delay f

  let jobResultOption = new JobResultOptionBuilder()