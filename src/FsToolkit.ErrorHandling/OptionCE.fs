namespace FsToolkit.ErrorHandling

[<AutoOpen>]
module OptionCE =
    open System

    type OptionBuilder() =
        member inline _.Return(x: 'value) : 'value option = Some x

        member inline _.ReturnFrom(m: 'value option) : 'value option = m

        member inline _.Bind
            (
                input: 'input option,
                [<InlineIfLambda>] binder: 'input -> 'output option
            ) : 'output option =
            Option.bind binder input

        // Could not get it to work solely with Source. In loop cases it would potentially match the #seq overload and ask for type annotation
        member inline this.Bind
            (
                m: 'input when 'input: null,
                [<InlineIfLambda>] binder: 'input -> 'output option
            ) : 'output option =
            this.Bind(Option.ofObj m, binder)

        member inline this.Zero() : unit option = this.Return()

        member inline _.Combine
            (
                m: 'input option,
                [<InlineIfLambda>] binder: 'input -> 'output option
            ) : 'output option =
            Option.bind binder m

        member inline this.Combine(m1: unit option, m2: 'output option) : 'output option =
            this.Bind(m1, (fun () -> m2))

        member inline _.Delay
            ([<InlineIfLambda>] delayer: unit -> 'value option)
            : (unit -> 'value option) =
            delayer

        member inline _.Run([<InlineIfLambda>] delayed) = delayed ()

        member inline this.TryWith([<InlineIfLambda>] computation, handler) : 'value =
            try
                this.Run computation
            with e ->
                handler e

        member inline this.TryFinally([<InlineIfLambda>] computation, compensation) =
            try
                this.Run computation
            finally
                compensation ()

        member inline this.Using
            (
                resource: 'disposable :> IDisposable,
                [<InlineIfLambda>] binder: 'disposable -> 'value option
            ) : 'value option =
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
                [<InlineIfLambda>] generator: unit -> unit option
            ) : unit option =

            let mutable doContinue = true
            let mutable result = Some()

            while doContinue
                  && guard () do
                match generator () with
                | Some() -> ()
                | None ->
                    doContinue <- false
                    result <- None

            result

        member inline this.For
            (
                sequence: #seq<'value>,
                [<InlineIfLambda>] binder: 'value -> unit option
            ) : unit option =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        member inline _.BindReturn
            (
                input: 'input option,
                [<InlineIfLambda>] mapper: 'input -> 'output
            ) : 'output option =
            Option.map mapper input

        member inline _.BindReturn
            (
                x: 'input,
                [<InlineIfLambda>] f: 'input -> 'output
            ) : 'output option =
            Option.map f (Option.ofObj x)

        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(result: 'value option) : 'value option = result


        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(vopt: 'value voption) : 'value option = Option.ofValueOption vopt

    /// <summary>
    /// The default instance of the `OptionBuilder` type.
    /// </summary>
    let option = OptionBuilder()

[<AutoOpen>]
module OptionExtensionsLower =
    type OptionBuilder with

        member inline _.Source(nullableObj: 'value when 'value: null) : 'value option =
            Option.ofObj nullableObj

        member inline _.Source(m: string) : string option = Option.ofObj m

[<AutoOpen>]
module OptionExtensions =
    open System

    type OptionBuilder with

        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<'value>) : #seq<'value> = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(nullable: Nullable<'value>) : 'value option =
            Option.ofNullable nullable

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(vopt: 'value voption) : 'value option = Option.ofValueOption vopt
