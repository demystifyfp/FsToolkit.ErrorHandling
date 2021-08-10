namespace FsToolkit.ErrorHandling.Operator.TaskResult

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskResult = 

  let inline (<!>) (f : _ -> ^b) (x : ^a) = TaskResult.map f x
  let inline (<*>) (f : ^b) (x : ^a) = TaskResult.apply f x
  let inline (>>=) (x : ^a) (f : _ -> ^b) = TaskResult.bind f x

