namespace FsToolkit.ErrorHandling.Operator.JobOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module JobOption = 

  let inline (<!>) f x = JobOption.map f x
  let inline (<*>) f x = JobOption.apply f x
  let inline (>>=) x f = JobOption.bind f x