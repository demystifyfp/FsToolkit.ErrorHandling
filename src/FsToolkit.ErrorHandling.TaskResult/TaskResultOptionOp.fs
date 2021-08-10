namespace FsToolkit.ErrorHandling.Operator.TaskResultOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskResultOption = 

  let inline (<!>) f (x : ^b) = TaskResultOption.map f x
  let inline (<*>) (f : ^a) (x : ^b) = TaskResultOption.apply f x
  let inline (>>=) (x : ^b) (f : _ -> ^a) = TaskResultOption.bind f x