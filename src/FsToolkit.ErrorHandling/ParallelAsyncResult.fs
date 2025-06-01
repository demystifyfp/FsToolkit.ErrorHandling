namespace FsToolkit.ErrorHandling

open System

[<RequireQualifiedAccess>]
module ParallelAsyncResult =

    [<AutoOpen>]
    module InternalHelpers =

        type AsyncResultErrorException<'a>(value: 'a) =
            inherit Exception()
            member this.Value = value

        let toBoxedAsync (input: Async<Result<'ok, 'error>>) : Async<ObjNull> =
            async {
                match! input with
                | Ok x -> return box x
                | Error e -> return raise (AsyncResultErrorException<'error>(e))
            }

    /// <summary>
    /// Transforms two AsyncResults in one that executes them concurrently and combines the results using the specified function.
    /// If either AsyncResult resolves to an error, then the other is cancelled and only the first error is returned.
    /// </summary>
    /// <param name="mapper">The function to apply to the values of the AsyncResult values.</param>
    /// <param name="input1">The first AsyncResult value to transform.</param>
    /// <param name="input2">The second AsyncResult value to transform.</param>
    /// <returns>The transformed AsyncResult value.</returns>
    let inline map2
        ([<InlineIfLambda>] mapper: 'a -> 'b -> 'c)
        (input1: Async<Result<'a, 'error>>)
        (input2: Async<Result<'b, 'error>>)
        : Async<Result<'c, 'error>> =
        async {
            try
                return!
                    Async.parallelMap2
                        (fun a b ->
                            let a = unbox<'a> a
                            let b = unbox<'b> b
                            Ok(mapper a b)
                        )
                        (toBoxedAsync input1)
                        (toBoxedAsync input2)

            with :? AsyncResultErrorException<'error> as exn ->
                return Error exn.Value
        }

    /// <summary>
    /// Transforms three AsyncResults in one that executes them concurrently and combines the results using the specified function.
    /// If any AsyncResult resolves to an error, then the others are cancelled and only the first error is returned.
    /// </summary>
    /// <param name="mapper">The function to apply to the values of the AsyncResult values.</param>
    /// <param name="input1">The first AsyncResult value to transform.</param>
    /// <param name="input2">The second AsyncResult value to transform.</param>
    /// <param name="input3">The third AsyncResult value to transform.</param>
    /// <returns>The transformed AsyncResult value.</returns>
    let inline map3
        ([<InlineIfLambda>] mapper: 'a -> 'b -> 'c -> 'd)
        (input1: Async<Result<'a, 'error>>)
        (input2: Async<Result<'b, 'error>>)
        (input3: Async<Result<'c, 'error>>)
        : Async<Result<'d, 'error>> =
        async {
            try
                return!
                    Async.parallelMap3
                        (fun a b c ->
                            let a = unbox<'a> a
                            let b = unbox<'b> b
                            let c = unbox<'c> c
                            Ok(mapper a b c)
                        )
                        (toBoxedAsync input1)
                        (toBoxedAsync input2)
                        (toBoxedAsync input3)

            with :? AsyncResultErrorException<'error> as exn ->
                return Error exn.Value
        }

    let inline zip
        (a: Async<Result<'a, 'error>>)
        (b: Async<Result<'b, 'error>>)
        : Async<Result<'a * 'b, 'error>> =
        map2 (fun a b -> a, b) a b
