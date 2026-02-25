namespace FsToolkit.ErrorHandling.Operator.ValueTaskValueOption

open FsToolkit.ErrorHandling

[<AutoOpen>]
module ValueTaskValueOption =

    let inline (<!>) ([<InlineIfLambda>] f) x = ValueTaskValueOption.map f x
    let inline (<*>) f x = ValueTaskValueOption.apply f x
    let inline (>>=) x ([<InlineIfLambda>] f) = ValueTaskValueOption.bind f x
