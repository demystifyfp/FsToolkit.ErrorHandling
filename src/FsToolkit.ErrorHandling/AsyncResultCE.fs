namespace FsToolkit.ErrorHandling.ComputationExpression.AsyncResult

open FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncResult = 

  type AsyncResultBuilder() =

    member __.Return (value : Result<'a,'b>) = async {return value}

    member __.Return value = AsyncResult.retn value
    member __.ReturnFrom value = value
    member __.Bind (result, binder) =
      AsyncResult.bind binder result

    member __.Combine(ar1, ar2) =
      ar1
      |> AsyncResult.bind (fun _ -> ar2)

    member __.Delay f =
      async.Delay f

  let asyncResult = AsyncResultBuilder() 