namespace FsToolkit.ErrorHandling.Operator.Option

open FsToolkit.ErrorHandling

[<AutoOpen>]
module Option =
    let inline (>>=)
        (input: Option<'input>)
        ([<InlineIfLambda>] binder: 'input -> Option<'output>)
        : Option<'output> =
        Option.bind binder input
