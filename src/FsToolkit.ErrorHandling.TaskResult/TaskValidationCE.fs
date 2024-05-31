namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks

[<AutoOpen>]
module TaskValidationCE =

    type TaskValidationBuilder() =
        member inline _.Return(value: 'ok) : TaskValidation<'ok, 'error> = TaskValidation.ok value

        member inline _.ReturnFrom
            (result: TaskValidation<'ok, 'error>)
            : TaskValidation<'ok, 'error> =
            result

        member inline _.Bind
            (
                result: TaskValidation<'okInput, 'error>,
                [<InlineIfLambda>] binder: 'okInput -> TaskValidation<'okOutput, 'error>
            ) : TaskValidation<'okOutput, 'error> =
            TaskValidation.bind binder result

        member inline this.Zero() : TaskValidation<unit, 'error> = this.Return()

        member inline _.Delay
            ([<InlineIfLambda>] generator: unit -> TaskValidation<'ok, 'error>)
            : unit -> TaskValidation<'ok, 'error> =
            generator

        member inline _.Run
            ([<InlineIfLambda>] generator: unit -> TaskValidation<'ok, 'error>)
            : TaskValidation<'ok, 'error> =
            generator ()

        member inline this.Combine
            (
                result: TaskValidation<unit, 'error>,
                [<InlineIfLambda>] binder: unit -> TaskValidation<'ok, 'error>
            ) : TaskValidation<'ok, 'error> =
            this.Bind(result, binder)

        member inline this.TryWith
            (
                [<InlineIfLambda>] generator: unit -> TaskValidation<'ok, 'error>,
                [<InlineIfLambda>] handler: exn -> TaskValidation<'ok, 'error>
            ) : TaskValidation<'ok, 'error> =
            task {
                return!
                    try
                        this.Run generator
                    with e ->
                        handler e
            }

        member inline this.TryFinally
            (
                [<InlineIfLambda>] generator: unit -> TaskValidation<'ok, 'error>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : TaskValidation<'ok, 'error> =
            task {
                return!
                    try
                        this.Run generator
                    finally
                        compensation ()
            }

        member inline this.Using
            (
                resource: 'disposable :> IDisposable,
                [<InlineIfLambda>] binder: 'disposable -> TaskValidation<'okOutput, 'error>
            ) : TaskValidation<'okOutput, 'error> =
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
                [<InlineIfLambda>] generator: unit -> TaskValidation<unit, 'error>
            ) : TaskValidation<unit, 'error> =
            let mutable doContinue = true
            let mutable result = Ok()

            task {
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
                [<InlineIfLambda>] binder: 'ok -> TaskValidation<unit, 'error>
            ) : TaskValidation<unit, 'error> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        member inline _.BindReturn
            (
                input: TaskValidation<'okInput, 'error>,
                [<InlineIfLambda>] mapper: 'okInput -> 'okOutput
            ) : TaskValidation<'okOutput, 'error> =
            TaskValidation.map mapper input

        member inline _.MergeSources
            (
                left: TaskValidation<'left, 'error>,
                right: TaskValidation<'right, 'error>
            ) : TaskValidation<'left * 'right, 'error> =
            TaskValidation.zip left right

        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        member inline _.Source(result: TaskValidation<'ok, 'error>) : TaskValidation<'ok, 'error> =
            result

    let taskValidation = TaskValidationBuilder()

[<AutoOpen>]
module HighPriority =

    // Having members as extensions gives them lower priority in
    // overload resolution and allows skipping more type annotations.
    type TaskValidationBuilder with

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(s: Task<Result<'ok, 'error>>) : TaskValidation<_, 'error> =
            s
            |> TaskResult.mapError (fun e -> [ e ])

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(s: Result<'ok, 'error>) : TaskValidation<'ok, 'error> =
            TaskValidation.ofResult s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        /// <returns></returns>
        member inline _.Source(a: Task<'ok>) : TaskValidation<'ok, 'error> =
            task {
                let! result = a
                return! TaskValidation.ok result
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        /// <returns></returns>
        member inline _.Source(choice: Choice<'ok, 'error>) : TaskValidation<'ok, 'error> =
            TaskValidation.ofChoice choice

        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) : #seq<_> = s

[<AutoOpen>]
module LowPriority =

    type TaskValidationBuilder with

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(s: Validation<'ok, 'error>) : TaskValidation<'ok, 'error> =
            Task.FromResult s
