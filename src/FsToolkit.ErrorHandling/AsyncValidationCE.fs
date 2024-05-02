namespace FsToolkit.ErrorHandling

open System

[<AutoOpen>]
module AsyncValidationCE =

    type AsyncValidationBuilder() =
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
            : unit -> AsyncValidation<'ok, 'error> =
            generator

        member inline _.Run
            ([<InlineIfLambda>] generator: unit -> AsyncValidation<'ok, 'error>)
            : AsyncValidation<'ok, 'error> =
            generator ()

        member inline this.Combine
            (
                result: AsyncValidation<unit, 'error>,
                [<InlineIfLambda>] binder: unit -> AsyncValidation<'ok, 'error>
            ) : AsyncValidation<'ok, 'error> =
            this.Bind(result, binder)

        member inline this.TryWith
            (
                [<InlineIfLambda>] generator: unit -> AsyncValidation<'ok, 'error>,
                [<InlineIfLambda>] handler: exn -> AsyncValidation<'ok, 'error>
            ) : AsyncValidation<'ok, 'error> =
            async {
                return!
                    try
                        this.Run generator
                    with e ->
                        handler e
            }

        member inline this.TryFinally
            (
                [<InlineIfLambda>] generator: unit -> AsyncValidation<'ok, 'error>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : AsyncValidation<'ok, 'error> =
            async {
                return!
                    try
                        this.Run generator
                    finally
                        compensation ()
            }

        member inline this.Using
            (
                resource: 'disposable :> IDisposable,
                [<InlineIfLambda>] binder: 'disposable -> AsyncValidation<'okOutput, 'error>
            ) : AsyncValidation<'okOutput, 'error> =
            this.TryFinally(
                (fun () -> binder resource),
                (fun () ->
                    if not (obj.ReferenceEquals(resource, null)) then
                        resource.Dispose()
                )
            )

        member inline this.While
            (
                [<InlineIfLambda>] guard: unit -> bool,
                [<InlineIfLambda>] generator: unit -> AsyncValidation<unit, 'error>
            ) : AsyncValidation<unit, 'error> =
            let mutable doContinue = true
            let mutable result = Ok()

            async {
                while doContinue
                      && guard () do
                    let! x = generator ()

                    match x with
                    | Ok() -> ()
                    | Error e ->
                        doContinue <- false
                        result <- Error e

                return result
            }

        member inline this.For
            (
                sequence: #seq<'ok>,
                [<InlineIfLambda>] binder: 'ok -> AsyncValidation<unit, 'error>
            ) : AsyncValidation<unit, 'error> =
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
            (
                left: AsyncValidation<'left, 'error>,
                right: AsyncValidation<'right, 'error>
            ) : AsyncValidation<'left * 'right, 'error> =
            AsyncValidation.zip left right

    let asyncValidation = AsyncValidationBuilder()

    [<AutoOpen>]
    module LowPriority =

        type AsyncValidationBuilder with

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

    [<AutoOpen>]
    module MediumPriority =

        open System.Threading.Tasks

        type AsyncValidationBuilder with

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

        open System.Threading.Tasks

        // Having members as extensions gives them lower priority in
        // overload resolution and allows skipping more type annotations.
        type AsyncValidationBuilder with

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source(s: Validation<'ok, 'error>) : AsyncValidation<'ok, 'error> =
                Async.retn s

#if !FABLE_COMPILER

            /// <summary>
            /// Method lets us transform data types into our internal representation.
            /// </summary>
            member inline _.Source
                (result: Task<Validation<'ok, 'error>>)
                : AsyncValidation<'ok, 'error> =
                Async.AwaitTask result

#endif

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
