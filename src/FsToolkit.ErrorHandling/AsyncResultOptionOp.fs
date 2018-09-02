namespace FsToolkit.ErrorHandling.Operator.AsyncResultOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncResultOption = 

  let inline (<!>) f x = AsyncResultOption.map f x
  let inline (<*>) f x = AsyncResultOption.apply f x
  let inline (>>=) x f = AsyncResultOption.bind f x