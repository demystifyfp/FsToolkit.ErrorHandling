namespace FsToolkit.ErrorHandling

open System

[<AutoOpen>]
module ParallelAsyncValidationCE =

    open System.Threading.Tasks

    type ParallelAsyncValidationBuilder() =
        member inline _.Return(value: 'ok) : AsyncValidation<'ok, 'error> = AsyncValidation.ok value

        member inline _.ReturnFrom
            (result: AsyncValidation<'ok, 'error>)
            : AsyncValidation<'ok, 'error> =
            result

        member inline _.Bind
            (
                result: AsyncValidation<'okInput, 'error>,
                [<InlineIfLambda>] binder: 'okInput -> AsyncValidation<'okOutput, 'error>
            ) : AsyncValidation<'okOutput, 'error> =
            AsyncValidation.bind binder result

        member inline this.Zero() : AsyncValidation<unit, 'error> = this.Return()

        member inline _.Delay
            ([<InlineIfLambda>] generator: unit -> AsyncValidation<'ok, 'error>)
            : AsyncValidation<'ok, 'error> =
            async.Delay generator

        member inline this.Combine
            (validation1: AsyncValidation<unit, 'error>, validation2: AsyncValidation<'ok, 'error>)
            : AsyncValidation<'ok, 'error> =
            this.Bind(validation1, (fun () -> validation2))

        member inline _.TryWith
            (
                computation: AsyncValidation<'ok, 'error>,
                [<InlineIfLambda>] handler: exn -> AsyncValidation<'ok, 'error>
            ) : AsyncValidation<'ok, 'error> =
            async.TryWith(computation, handler)

        member inline _.TryFinally
            (
                computation: AsyncValidation<'ok, 'error>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : AsyncValidation<'ok, 'error> =
            async.TryFinally(computation, compensation)

        member inline _.Using
            (
                resource: 'disposable :> IDisposableNull,
                [<InlineIfLambda>] binder: 'disposable -> AsyncValidation<'okOutput, 'error>
            ) : AsyncValidation<'okOutput, 'error> =
            async.Using(resource, binder)

        member inline this.While
            ([<InlineIfLambda>] guard: unit -> bool, computation: AsyncValidation<unit, 'error>)
            : AsyncValidation<unit, 'error> =
            if guard () then
                let mutable whileAsync = Unchecked.defaultof<_>

                whileAsync <-
                    this.Bind(computation, (fun () -> if guard () then whileAsync else this.Zero()))

                whileAsync
            else
                this.Zero()


        member inline this.For
            (sequence: #seq<'ok>, [<InlineIfLambda>] binder: 'ok -> AsyncValidation<unit, 'error>)
            : AsyncValidation<unit, 'error> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        member inline _.BindReturn
            (
                input: AsyncValidation<'okInput, 'error>,
                [<InlineIfLambda>] mapper: 'okInput -> 'okOutput
            ) : AsyncValidation<'okOutput, 'error> =
            AsyncValidation.map mapper input

        member inline _.MergeSources
            (left: AsyncValidation<'left, 'error>, right: AsyncValidation<'right, 'error>)
            : AsyncValidation<'left * 'right, 'error> =
            ParallelAsyncValidation.zip left right

    let parallelAsyncValidation = ParallelAsyncValidationBuilder()

    [<AutoOpen>]
    module LowPriority =

        type ParallelAsyncValidationBuilder with

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            /// <returns></returns>
            member inline _.Source(a: Async<'ok>) : AsyncValidation<'ok, 'error> =
                async {
                    let! result = a
                    return! AsyncValidation.ok result
                }

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source(s: Result<'ok, 'error>) : AsyncValidation<'ok, 'error> =
                AsyncValidation.ofResult s

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            /// <returns></returns>
            member inline _.Source(choice: Choice<'ok, 'error>) : AsyncValidation<'ok, 'error> =
                AsyncValidation.ofChoice choice

            /// <summary>
            /// Needed to allow `for..in` and `for..do` functionality
            /// </summary>
            member inline _.Source(s: #seq<_>) : #seq<_> = s

#if !FABLE_COMPILER

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source(s: Task<'ok>) : AsyncValidation<'ok, 'error> =
                Async.AwaitTask s
                |> Async.map Result.Ok

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source(s: Task) : AsyncValidation<unit, 'error> =
                Async.AwaitTask s
                |> Async.map Result.Ok

            member inline _.TryFinallyAsync
                (
                    computation: Async<Result<'ok, 'error>>,
                    [<InlineIfLambda>] compensation: unit -> ValueTask
                ) : Async<Result<'ok, 'error>> =
                let compensation =
                    async {
                        let vTask = compensation ()

                        if vTask.IsCompletedSuccessfully then
                            return ()
                        else
                            return!
                                vTask.AsTask()
                                |> Async.AwaitTask
                    }

                Async.TryFinallyAsync(computation, compensation)


            member inline this.Using
                (
                    resource: 'ok :> IAsyncDisposable,
                    [<InlineIfLambda>] binder: 'ok -> AsyncValidation<'U, 'error>
                ) : AsyncValidation<'U, 'error> =
                this.TryFinallyAsync(
                    binder resource,
                    (fun () ->
                        if not (isNull (box resource)) then
                            resource.DisposeAsync()
                        else
                            ValueTask()
                    )
                )

#endif

    [<AutoOpen>]
    module MediumPriority =

        type ParallelAsyncValidationBuilder with

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source(s: Async<Result<'ok, 'error>>) : AsyncValidation<'ok, 'error> =
                AsyncResult.mapError List.singleton s

#if !FABLE_COMPILER

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source(s: Task<Result<'ok, 'error>>) : AsyncValidation<'ok, 'error> =
                Async.AwaitTask s
                |> AsyncResult.mapError List.singleton


#endif

    [<AutoOpen>]
    module HighPriority =

        // Having members as extensions gives them lower priority in
        // overload resolution and allows skipping more type annotations.
        type ParallelAsyncValidationBuilder with

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source(s: Validation<'ok, 'error>) : AsyncValidation<'ok, 'error> =
                Async.singleton s

#if !FABLE_COMPILER

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source
                (result: Task<Validation<'ok, 'error>>)
                : AsyncValidation<'ok, 'error> =
                Async.AwaitTask result

#endif


    [<AutoOpen>]
    module UltraPriority =

        type ParallelAsyncValidationBuilder with

            /// <summary>
            /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
            ///
            /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            member inline _.Source
                (result: AsyncValidation<'ok, 'error>)
                : AsyncValidation<'ok, 'error> =
                result
