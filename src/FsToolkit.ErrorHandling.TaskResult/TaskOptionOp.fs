namespace FsToolkit.ErrorHandling.Operator.TaskOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskOption = 

  let inline (<!>) f x = TaskOption.map f x
  let inline (<*>) f x = TaskOption.apply f x
  let inline (>>=) x f = TaskOption.bind f x