namespace FsToolkit.ErrorHandling.Operator.TaskOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskOption =

    let inline (<!>) ([<InlineIfLambda>] f) x = TaskOption.map f x
    let inline (<*>) f x = TaskOption.apply f x
    let inline (>>=) x ([<InlineIfLambda>] f) = TaskOption.bind f x
