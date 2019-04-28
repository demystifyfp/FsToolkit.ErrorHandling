namespace FsToolkit.ErrorHandling.Operator.Task
open FsToolkit.ErrorHandling

[<AutoOpen>]
module Task =
  let inline (<!>) f x = Task.map f x
  let inline (<*>) f x = Task.apply f x
  let inline (>>=) x f = Task.bind f x