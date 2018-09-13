namespace FsToolkit.ErrorHandling.Operator.Result

open FsToolkit.ErrorHandling

[<AutoOpen>]
module Result =
  let inline (<!>) f x = Result.map f x
  let inline (<*>) f x = Result.apply f x
  let inline (>>=) x f = Result.bind f x