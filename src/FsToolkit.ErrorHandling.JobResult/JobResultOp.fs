namespace FsToolkit.ErrorHandling.Operator.JobResult

open FsToolkit.ErrorHandling

[<AutoOpen>]
module JobResult = 

  let inline (<!>) f x = JobResult.map f x
  let inline (<*>) f x = JobResult.apply f x
  let inline (>>=) x f = JobResult.bind f x

