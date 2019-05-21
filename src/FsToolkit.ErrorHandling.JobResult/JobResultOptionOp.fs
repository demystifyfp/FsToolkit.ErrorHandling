namespace FsToolkit.ErrorHandling.Operator.JobResultOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module JobResultOption = 

  let inline (<!>) f x = JobResultOption.map f x
  let inline (<*>) f x = JobResultOption.apply f x
  let inline (>>=) x f = JobResultOption.bind f x