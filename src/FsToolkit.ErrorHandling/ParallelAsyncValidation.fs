namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module ParallelAsyncValidation =

    /// <summary>
    /// Execute two AsyncValidations concurrently and combines the results using the specified function.
    /// Both are run to completion so that all errors are accumulated.
    /// </summary>
    /// <param name="mapper">The function to apply to the values of the AsyncValidation values.</param>
    /// <param name="input1">The first AsyncValidation value to transform.</param>
    /// <param name="input2">The second AsyncValidation value to transform.</param>
    /// <returns>The transformed AsyncValidation value.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'a -> 'b -> 'c)
        (input1: AsyncValidation<'a, 'error>)
        (input2: AsyncValidation<'b, 'error>)
        : AsyncValidation<'c, 'error> =
        Async.parallelMap2
            (fun a b ->
                match a, b with
                | Ok a, Ok b -> Ok(mapper a b)
                | Ok _, Error v -> Error v
                | Error u, Ok _ -> Error u
                | Error u, Error v -> Error(u @ v)
            )
            input1
            input2

    let inline zip
        (a: AsyncValidation<'a, 'error>)
        (b: AsyncValidation<'b, 'error>)
        : AsyncValidation<'a * 'b, 'error> =
        map2 (fun a b -> a, b) a b
