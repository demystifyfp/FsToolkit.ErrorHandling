namespace FsToolkit.ErrorHandling.Operator.CancellableTaskValidation

open FsToolkit.ErrorHandling

[<AutoOpen>]
module CancellableTaskValidation =
    let inline (<!>)
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: CancellableTaskValidation<'okInput, 'error>)
        : CancellableTaskValidation<'okOutput, 'error> =
        CancellableTaskValidation.map mapper input

    let inline (<!^>)
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: Result<'okInput, 'error>)
        : CancellableTaskValidation<'okOutput, 'error> =
        CancellableTaskValidation.map mapper (CancellableTaskValidation.ofResult input)

    let inline (<*>)
        (applier: CancellableTaskValidation<('okInput -> 'okOutput), 'error>)
        (input: CancellableTaskValidation<'okInput, 'error>)
        : CancellableTaskValidation<'okOutput, 'error> =
        CancellableTaskValidation.apply applier input

    let inline (<*^>)
        (applier: CancellableTaskValidation<('okInput -> 'okOutput), 'error>)
        (input: Result<'okInput, 'error>)
        : CancellableTaskValidation<'okOutput, 'error> =
        CancellableTaskValidation.apply applier (CancellableTaskValidation.ofResult input)

    let inline (>>=)
        (input: CancellableTaskValidation<'okInput, 'error>)
        ([<InlineIfLambda>] binder: 'okInput -> CancellableTaskValidation<'okOutput, 'error>)
        : CancellableTaskValidation<'okOutput, 'error> =
        CancellableTaskValidation.bind binder input
