namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks

[<AutoOpen>]
module AsyncResultOptionCE =

    type AsyncResultOptionBuilder() =
        member inline _.Return(value: 'ok) : AsyncResultOption<'ok, 'error> =
            AsyncResultOption.retn value

        member inline _.ReturnFrom
            (value: Async<Result<'ok option, 'error>>)
            : Async<Result<'ok option, 'error>> =
            value

        member inline _.Bind
            (
                input: AsyncResultOption<'okInput, 'error>,
                [<InlineIfLambda>] binder: 'okInput -> AsyncResultOption<'okOutput, 'error>
            ) : AsyncResultOption<'okOutput, 'error> =
            AsyncResultOption.bind binder input

        member inline _.Combine(aro1, aro2) =
            aro1
            |> AsyncResultOption.bind (fun _ -> aro2)

        member inline _.Delay([<InlineIfLambda>] f: unit -> Async<'a>) : Async<'a> = async.Delay f

        member inline _.Zero() = Async.singleton (Ok(option.Zero()))

        member inline _.TryWith
            (
                computation: AsyncResultOption<'ok, 'error>,
                [<InlineIfLambda>] handler: Exception -> AsyncResultOption<'ok, 'error>
            ) : AsyncResultOption<'ok, 'error> =
            async.TryWith(computation, handler)

        member inline _.TryFinally
            (
                computation: AsyncResultOption<'ok, 'error>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : AsyncResultOption<'ok, 'error> =
            async.TryFinally(computation, compensation)


        member inline _.Using
            (
                resource: 'ok :> IDisposable,
                [<InlineIfLambda>] binder: 'ok -> AsyncResultOption<'okOutput, 'error>
            ) : AsyncResultOption<'okOutput, 'error> =
            async.Using(resource, binder)


        member inline this.While
            (
                [<InlineIfLambda>] guard: unit -> bool,
                computation: AsyncResultOption<unit, 'error>
            ) : AsyncResultOption<unit, 'error> =
            if guard () then
                let mutable whileAsync = Unchecked.defaultof<_>

                whileAsync <-
                    this.Bind(computation, (fun () -> if guard () then whileAsync else this.Zero()))

                whileAsync
            else
                this.Zero()


        member inline this.For
            (
                sequence: #seq<'ok>,
                [<InlineIfLambda>] binder: 'ok -> AsyncResultOption<unit, 'error>
            ) : AsyncResultOption<unit, 'error> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum ->
                    this.While(
                        (fun () -> enum.MoveNext()),
                        this.Delay(fun () -> binder enum.Current)
                    )
            )

        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source
            (result: AsyncResultOption<'ok, 'error>)
            : AsyncResultOption<'ok, 'error> =
            result
#if !FABLE_COMPILER

        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source
            (result: Task<Result<Option<'ok>, 'error>>)
            : AsyncResultOption<'ok, 'error> =
            result
            |> Async.AwaitTask
#endif
    let asyncResultOption = new AsyncResultOptionBuilder()


[<AutoOpen>]
module AsyncResultOptionCEExtensions =

    type AsyncResultOptionBuilder with

        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<'value>) : #seq<'value> = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Result<'ok, 'error>) : AsyncResultOption<'ok, 'error> =
            AsyncResultOption.ofResult result

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline this.Source(result: Choice<'ok, 'error>) : AsyncResultOption<'ok, 'error> =
            this.Source(Result.ofChoice result)

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline this.Source(optional: Option<'ok>) : AsyncResultOption<'ok, 'error> =
            AsyncResultOption.ofOption optional

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline this.Source(async: Async<'ok>) : AsyncResultOption<'ok, 'error> =
            Async.map (Some >> Ok) async

#if !FABLE_COMPILER
        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline this.Source(async: Task<'ok>) : AsyncResultOption<'ok, 'error> =
            async
            |> Async.AwaitTask
            |> Async.map (Some >> Ok)

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline this.Source(async: Task) : AsyncResultOption<unit, 'error> =
            async
            |> Async.AwaitTask
            |> Async.map (Some >> Ok)
#endif
[<AutoOpen>]
module AsyncResultOptionCEExtensionsHighPriority =

    type AsyncResultOptionBuilder with

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source
            (result: Async<Result<'ok, 'error>>)
            : AsyncResultOption<'ok, 'error> =
            AsyncResultOption.ofAsyncResult result

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Async<Option<'ok>>) : AsyncResultOption<'ok, 'error> =
            AsyncResultOption.ofAsyncOption result


#if !FABLE_COMPILER
        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Task<Result<'ok, 'error>>) : AsyncResultOption<'ok, 'error> =
            result
            |> Async.AwaitTask
            |> AsyncResultOption.ofAsyncResult

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Task<Option<'ok>>) : AsyncResultOption<'ok, 'error> =
            result
            |> Async.AwaitTask
            |> AsyncResultOption.ofAsyncOption
#endif
