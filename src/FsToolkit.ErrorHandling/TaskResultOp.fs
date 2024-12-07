namespace FsToolkit.ErrorHandling.Operator.TaskResult

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskResult =

    let inline (<!>) ([<InlineIfLambda>] f) x = TaskResult.map f x
    let inline (<*>) f x = TaskResult.apply f x
    let inline (>>=) x ([<InlineIfLambda>] f) = TaskResult.bind f x
