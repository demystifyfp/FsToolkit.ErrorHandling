namespace FsToolkit.ErrorHandling.Operator.TaskValidation

open FsToolkit.ErrorHandling

[<AutoOpen>]
module TaskValidation =
    let inline (<!>)
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: TaskValidation<'okInput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        TaskValidation.map mapper input

    let inline (<!^>)
        ([<InlineIfLambda>] mapper: 'okInput -> 'okOutput)
        (input: Result<'okInput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        TaskValidation.map mapper (TaskValidation.ofResult input)

    let inline (<*>)
        (applier: TaskValidation<('okInput -> 'okOutput), 'error>)
        (input: TaskValidation<'okInput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        TaskValidation.apply applier input

    let inline (<*^>)
        (applier: TaskValidation<('okInput -> 'okOutput), 'error>)
        (input: Result<'okInput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        TaskValidation.apply applier (TaskValidation.ofResult input)

    let inline (>>=)
        (input: TaskValidation<'okInput, 'error>)
        ([<InlineIfLambda>] binder: 'okInput -> TaskValidation<'okOutput, 'error>)
        : TaskValidation<'okOutput, 'error> =
        TaskValidation.bind binder input
