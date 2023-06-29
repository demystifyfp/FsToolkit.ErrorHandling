namespace FsToolkit.ErrorHandling

open System
open IcedTasks

[<AutoOpen>]
module CancellableTaskValidationCE =

    type CancellableTaskValidationBuilder() =
        member inline _.Return(value: 'ok) : CancellableTaskValidation<'ok, 'error> =
            CancellableTaskValidation.ok value

        member inline _.ReturnFrom
            (result: CancellableTaskValidation<'ok, 'error>)
            : CancellableTaskValidation<'ok, 'error> =
            result

        member inline _.Bind
            (
                result: CancellableTaskValidation<'okInput, 'error>,
                [<InlineIfLambda>] binder: 'okInput -> CancellableTaskValidation<'okOutput, 'error>
            ) : CancellableTaskValidation<'okOutput, 'error> =
            CancellableTaskValidation.bind binder result

        member inline this.Zero() : CancellableTaskValidation<unit, 'error> = this.Return()

        member inline _.Delay
            ([<InlineIfLambda>] generator: unit -> CancellableTaskValidation<'ok, 'error>)
            : unit -> CancellableTaskValidation<'ok, 'error> =
            generator

        member inline _.Run
            ([<InlineIfLambda>] generator: unit -> CancellableTaskValidation<'ok, 'error>)
            : CancellableTaskValidation<'ok, 'error> =
            generator ()

        member inline this.Combine
            (
                result: CancellableTaskValidation<unit, 'error>,
                [<InlineIfLambda>] binder: unit -> CancellableTaskValidation<'ok, 'error>
            ) : CancellableTaskValidation<'ok, 'error> =
            this.Bind(result, binder)

        member inline this.TryWith
            (
                [<InlineIfLambda>] generator: unit -> CancellableTaskValidation<'ok, 'error>,
                [<InlineIfLambda>] handler: exn -> CancellableTaskValidation<'ok, 'error>
            ) : CancellableTaskValidation<'ok, 'error> =
            cancellableTask {
                return!
                    try
                        this.Run generator
                    with e ->
                        handler e
            }

        member inline this.TryFinally
            (
                [<InlineIfLambda>] generator: unit -> CancellableTaskValidation<'ok, 'error>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : CancellableTaskValidation<'ok, 'error> =
            cancellableTask {
                return!
                    try
                        this.Run generator
                    finally
                        compensation ()
            }

        member inline this.Using
            (
                resource: 'disposable :> IDisposable,
                [<InlineIfLambda>] binder:
                    'disposable -> CancellableTaskValidation<'okOutput, 'error>
            ) : CancellableTaskValidation<'okOutput, 'error> =
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
                [<InlineIfLambda>] generator: unit -> CancellableTaskValidation<unit, 'error>
            ) : CancellableTaskValidation<unit, 'error> =
            let mutable doContinue = true
            let mutable result = Ok()

            cancellableTask {
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
                [<InlineIfLambda>] binder: 'ok -> CancellableTaskValidation<unit, 'error>
            ) : CancellableTaskValidation<unit, 'error> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        member inline _.BindReturn
            (
                input: CancellableTaskValidation<'okInput, 'error>,
                [<InlineIfLambda>] mapper: 'okInput -> 'okOutput
            ) : CancellableTaskValidation<'okOutput, 'error> =
            CancellableTaskValidation.map mapper input

        member inline _.MergeSources
            (
                left: CancellableTaskValidation<'left, 'error>,
                right: CancellableTaskValidation<'right, 'error>
            ) : CancellableTaskValidation<'left * 'right, 'error> =
            CancellableTaskValidation.zip left right

        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        member inline _.Source
            (result: CancellableTaskValidation<'ok, 'error>)
            : CancellableTaskValidation<'ok, 'error> =
            result

    let cancellableTaskValidation = CancellableTaskValidationBuilder()

[<AutoOpen>]
module CancellableTaskValidationCEExtensions =

    // Having members as extensions gives them lower priority in
    // overload resolution and allows skipping more type annotations.
    type CancellableTaskValidationBuilder with

        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) : #seq<_> = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(s: Result<'ok, 'error>) : CancellableTaskValidation<'ok, 'error> =
            CancellableTaskValidation.ofResult s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        /// <returns></returns>
        member inline _.Source
            (choice: Choice<'ok, 'error>)
            : CancellableTaskValidation<'ok, 'error> =
            CancellableTaskValidation.ofChoice choice
