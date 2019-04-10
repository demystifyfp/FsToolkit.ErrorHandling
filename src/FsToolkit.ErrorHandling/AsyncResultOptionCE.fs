namespace FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncResultOptionCE =  

  type AsyncResultOptionBuilder() =
    member __.Return value = AsyncResultOption.retn value
    member __.ReturnFrom value = value
    member __.Bind (result, binder) =
      AsyncResultOption.bind binder result

    member __.Combine(aro1, aro2) =
      aro1
      |> AsyncResultOption.bind (fun _ -> aro2)

    member __.Delay f =
      async.Delay f

  let asyncResultOption = new AsyncResultOptionBuilder()