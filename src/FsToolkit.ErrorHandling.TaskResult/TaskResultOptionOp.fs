namespace FsToolkit.ErrorHandling.Operator.TaskResultOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskResultOption = 

  let inline (<!>) f x = TaskResultOption.map f x
  let inline (<*>) f x = TaskResultOption.apply f x
  let inline (>>=) x f = TaskResultOption.bind f x