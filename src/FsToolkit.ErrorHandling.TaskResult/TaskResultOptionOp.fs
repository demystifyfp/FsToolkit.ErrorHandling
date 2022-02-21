namespace FsToolkit.ErrorHandling.Operator.TaskResultOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskResultOption =

    let inline (<!>) ([<InlineIfLambda>] f) x = TaskResultOption.map f x
    let inline (<*>) f x = TaskResultOption.apply f x
    let inline (>>=) x ([<InlineIfLambda>] f) = TaskResultOption.bind f x
