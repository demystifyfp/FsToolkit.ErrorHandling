namespace FsToolkit.ErrorHandling.Operator.AsyncOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncOption = 

  let inline (<!>) f x = AsyncOption.map f x
  let inline (<*>) f x = AsyncOption.apply f x
  let inline (>>=) x f = AsyncOption.bind f x