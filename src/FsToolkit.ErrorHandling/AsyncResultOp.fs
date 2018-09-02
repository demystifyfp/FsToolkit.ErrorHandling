namespace FsToolkit.ErrorHandling.Operator.AsyncResult

open FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncResult = 

  let inline (<!>) f x = AsyncResult.map f x
  let inline (<*>) f x = AsyncResult.apply f x
  let inline (>>=) x f = AsyncResult.bind f x

