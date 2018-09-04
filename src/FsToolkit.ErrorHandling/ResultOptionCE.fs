namespace FsToolkit.ErrorHandling.CE.ResultOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module ResultOption =

  type ResultOptionBuilder() =
    member __.Return value = ResultOption.retn value
    member __.ReturnFrom value = value

    member __.Bind (resultOpt, binder) =
      ResultOption.bind binder resultOpt

    member __.Bind (result, binder) =
      result
      |> Result.map Some
      |> ResultOption.bind binder

    member __.Combine(r1, r2) =
      r1
      |> ResultOption.bind (fun _ -> r2)

    member __.Delay f = f ()


  let resultOption = ResultOptionBuilder()