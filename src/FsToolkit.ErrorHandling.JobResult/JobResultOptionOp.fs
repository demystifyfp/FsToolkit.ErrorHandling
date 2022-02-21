namespace FsToolkit.ErrorHandling.Operator.JobResultOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module JobResultOption =

    let inline (<!>) ([<InlineIfLambda>] f) x = JobResultOption.map f x
    let inline (<*>) f x = JobResultOption.apply f x
    let inline (>>=) x ([<InlineIfLambda>] f) = JobResultOption.bind f x
