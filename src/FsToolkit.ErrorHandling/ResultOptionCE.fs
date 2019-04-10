namespace FsToolkit.ErrorHandling

[<AutoOpen>]
module ResultOptionCE =

  type ResultOptionBuilder() =
    member __.Return value = ResultOption.retn value
    member __.ReturnFrom value = value

    member __.Bind (resultOpt, binder) =
      ResultOption.bind binder resultOpt

    member __.Combine(r1, r2) =
      r1
      |> ResultOption.bind (fun _ -> r2)

    member __.Delay f = f ()


  let resultOption = ResultOptionBuilder()