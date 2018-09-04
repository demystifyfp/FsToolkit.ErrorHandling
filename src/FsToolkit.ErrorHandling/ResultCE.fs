namespace FsToolkit.ErrorHandling.CE.Result

[<AutoOpen>]
module Result =

  type ResultBuilder() =
    member __.Return value = Ok value
    member __.ReturnFrom value = value

    member __.Bind (result, binder) =
      Result.bind binder result

    member __.Combine(r1, r2) =
      r1
      |> Result.bind (fun _ -> r2)

    member __.Delay f = f ()


  let result = ResultBuilder()