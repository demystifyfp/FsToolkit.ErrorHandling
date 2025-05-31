namespace FsToolkit.ErrorHandling.Operator.TaskValueOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskValueOption =

    let inline (<!>) ([<InlineIfLambda>] f) x = TaskValueOption.map f x
    let inline (<*>) f x = TaskValueOption.apply f x
    let inline (>>=) x ([<InlineIfLambda>] f) = TaskValueOption.bind f x
