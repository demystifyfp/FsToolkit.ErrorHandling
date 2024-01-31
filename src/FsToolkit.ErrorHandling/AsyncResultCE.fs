namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System

[<AutoOpen>]
module AsyncResultCE =

    type AsyncResultBuilder() =

        member inline _.Return(value: 'ok) : Async<Result<'ok, 'error>> =
            result.Return value
            |> async.Return

        member inline _.ReturnFrom
            (asyncResult: Async<Result<'ok, 'error>>)
            : Async<Result<'ok, 'error>> =
            asyncResult

        member inline _.Zero() : Async<Result<unit, 'error>> =
            result.Zero()
            |> async.Return

        member inline _.Bind
            (
                asyncResult: Async<Result<'okInput, 'error>>,
                [<InlineIfLambda>] binder: 'okInput -> Async<Result<'okOutput, 'error>>
            ) : Async<Result<'okOutput, 'error>> =
            AsyncResult.bind binder asyncResult

        member inline _.Delay
            ([<InlineIfLambda>] generator: unit -> Async<Result<'ok, 'error>>)
            : Async<Result<'ok, 'error>> =
            async.Delay generator

        member inline this.Combine
            (
                computation1: Async<Result<unit, 'error>>,
                computation2: Async<Result<'ok, 'error>>
            ) : Async<Result<'ok, 'error>> =
            this.Bind(computation1, (fun () -> computation2))

        member inline _.TryWith
            (
                computation: Async<Result<'ok, 'error>>,
                [<InlineIfLambda>] handler: System.Exception -> Async<Result<'ok, 'error>>
            ) : Async<Result<'ok, 'error>> =
            async.TryWith(computation, handler)

        member inline _.TryFinally
            (
                computation: Async<Result<'ok, 'error>>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : Async<Result<'ok, 'error>> =
            async.TryFinally(computation, compensation)

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
                [<InlineIfLambda>] binder: 'ok -> Async<Result<'U, 'error>>
            ) : Async<Result<'U, 'error>> =
            this.TryFinallyAsync(
                binder resource,
                (fun () ->
                    if not (isNull (box resource)) then
                        resource.DisposeAsync()
                    else
                        ValueTask()
                )
            )

        member inline this.While
            (
                [<InlineIfLambda>] guard: unit -> bool,
                computation: Async<Result<unit, 'error>>
            ) : Async<Result<unit, 'error>> =
            if guard () then
                let mutable whileAsync = Unchecked.defaultof<_>

                whileAsync <-
                    this.Bind(computation, (fun () -> if guard () then whileAsync else this.Zero()))

                whileAsync
            else
                this.Zero()


        member inline _.BindReturn
            (
                x: Async<Result<'okInput, 'error>>,
                [<InlineIfLambda>] f: 'okInput -> 'okOutput
            ) : Async<Result<'okOutput, 'error>> =
            AsyncResult.map f x

        member inline _.MergeSources
            (
                t1: Async<Result<'leftOk, 'error>>,
                t2: Async<Result<'rightOk, 'error>>
            ) : Async<Result<'leftOk * 'rightOk, 'error>> =
            AsyncResult.zip t1 t2


        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(result: Async<Result<'ok, 'error>>) : Async<Result<'ok, 'error>> =
            result

#if !FABLE_COMPILER
        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(task: Task<Result<'ok, 'error>>) : Async<Result<'ok, 'error>> =
            task
            |> Async.AwaitTask
#endif

    let asyncResult = AsyncResultBuilder()

[<AutoOpen>]
module AsyncResultCEExtensions =

    type AsyncResultBuilder with

        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<'value>) : #seq<'value> = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Result<'ok, 'error>) : Async<Result<'ok, 'error>> =
            Async.singleton result

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(choice: Choice<'ok, 'error>) : Async<Result<'ok, 'error>> =
            choice
            |> Result.ofChoice
            |> Async.singleton

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(asyncComputation: Async<'ok>) : Async<Result<'ok, 'error>> =
            asyncComputation
            |> Async.map Ok


        member inline _.Using
            (
                resource: 'ok :> IDisposable,
                [<InlineIfLambda>] binder: 'ok -> Async<Result<'U, 'error>>
            ) : Async<Result<'U, 'error>> =
            async.Using(resource, binder)


        member inline this.For
            (
                sequence: #seq<'ok>,
                [<InlineIfLambda>] binder: 'ok -> Async<Result<unit, 'error>>
            ) : Async<Result<unit, 'error>> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum ->
                    this.While(
                        (fun () -> enum.MoveNext()),
                        this.Delay(fun () -> binder enum.Current)
                    )
            )


#if !FABLE_COMPILER
        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(task: Task<'ok>) : Async<Result<'ok, 'error>> =
            task
            |> Async.AwaitTask
            |> Async.map Ok

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(task: Task) : Async<Result<unit, 'error>> =
            task
            |> Async.AwaitTask
            |> Async.map Ok
#endif
