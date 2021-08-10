namespace FsToolkit.ErrorHandling.Operator.TaskOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskOption = 

  let inline (<!>) (f : _ -> ^b) (x : ^a) = TaskOption.map f x
  let inline (<*>) (f : ^b) (x : ^a) = TaskOption.apply f x
  let inline (>>=) (x : ^a) (f : _ -> ^b) = TaskOption.bind f x