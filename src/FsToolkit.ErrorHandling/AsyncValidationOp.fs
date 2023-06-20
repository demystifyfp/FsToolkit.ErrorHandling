namespace FsToolkit.ErrorHandling.Operator.AsyncValidation

open FsToolkit.ErrorHandling

[<AutoOpen>]
module AsyncValidation =
    let inline (<!>)
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: AsyncValidation<'okInput, 'error>)
        : AsyncValidation<'okOutput, 'error> =
        AsyncValidation.map mapper input

    let inline (<!^>)
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: Result<'okInput, 'error>)
        : AsyncValidation<'okOutput, 'error> =
        AsyncValidation.map mapper (AsyncValidation.ofResult input)

    let inline (<*>)
        (applier: AsyncValidation<('okInput -> 'okOutput), 'error>)
        (input: AsyncValidation<'okInput, 'error>)
        : AsyncValidation<'okOutput, 'error> =
        AsyncValidation.apply applier input

    let inline (<*^>)
        (applier: AsyncValidation<('okInput -> 'okOutput), 'error>)
        (input: Result<'okInput, 'error>)
        : AsyncValidation<'okOutput, 'error> =
        AsyncValidation.apply applier (AsyncValidation.ofResult input)

    let inline (>>=)
        (input: AsyncValidation<'okInput, 'error>)
        ([<InlineIfLambda>] binder: 'okInput -> AsyncValidation<'okOutput, 'error>)
        : AsyncValidation<'okOutput, 'error> =
        AsyncValidation.bind binder input
