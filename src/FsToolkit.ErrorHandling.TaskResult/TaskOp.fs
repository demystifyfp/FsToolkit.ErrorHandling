namespace FsToolkit.ErrorHandling.Operator.Task

open FsToolkit.ErrorHandling

[<AutoOpen>]
module Task =
    let inline (<!>) ([<InlineIfLambda>] f) x = Task.map f x
    let inline (<*>) f x = Task.apply f x
    let inline (>>=) x ([<InlineIfLambda>] f) = Task.bind f x
