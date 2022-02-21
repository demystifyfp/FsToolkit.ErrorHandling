namespace FsToolkit.ErrorHandling.Operator.JobOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module JobOption =

    let inline (<!>) ([<InlineIfLambda>] f) x = JobOption.map f x
    let inline (<*>) f x = JobOption.apply f x
    let inline (>>=) x ([<InlineIfLambda>] f) = JobOption.bind f x
