namespace FsToolkit.ErrorHandling

[<AutoOpen>]
module OptionCE =
    open System


    type OptionBuilderBase() =

        member inline _.Bind(x: 'b voption, [<InlineIfLambda>] f: 'b -> voption<'c>) : voption<'c> =
            ValueOption.bind f x

        member inline _.BindReturn(x: 'b voption, [<InlineIfLambda>] f: 'b -> 'c) : voption<'c> =
            ValueOption.map f x

        member inline _.Return(x: 'a) = ValueSome x

        member inline _.ReturnFrom(x: 'a voption) = x

        member inline _.Zero() = ValueSome()

        member inline _.Delay([<InlineIfLambda>] f: unit -> voption<_>) = f

        member inline _.Combine
            (m: 'input voption, [<InlineIfLambda>] binder: 'input -> 'output voption)
            : 'output voption =
            ValueOption.bind binder m

        member inline this.Combine(m1: unit voption, m2: 'output voption) : 'output voption =
            this.Bind(m1, (fun () -> m2))


        member inline this.TryWith([<InlineIfLambda>] computation, handler) : 'value =
            try
                computation ()
            with e ->
                handler e

        member inline this.TryFinally([<InlineIfLambda>] computation, compensation) =
            try
                computation ()
            finally
                compensation ()


        member inline this.Using
            (
                resource: 'disposable :> IDisposableNull,
                [<InlineIfLambda>] binder: 'disposable -> 'value voption
            ) : 'value voption =
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
                [<InlineIfLambda>] generator: unit -> unit voption
            ) : unit voption =

            let mutable doContinue = true
            let mutable result = ValueSome()

            while doContinue
                  && guard () do
                match generator () with
                | ValueSome() -> ()
                | ValueNone ->
                    doContinue <- false
                    result <- ValueNone

            result

        member inline this.For
            (sequence: #seq<'value> voption, [<InlineIfLambda>] binder: 'value -> unit voption)
            : unit voption =
            sequence
            |> ValueOption.bind (fun sequence ->
                this.Using(
                    sequence.GetEnumerator(),
                    fun enum ->
                        this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
                )
            )


    [<AutoOpen>]
    module BuilderExtensions =
        type OptionBuilderBase with
            member inline _.Source(o) = ValueOption.ofObj o
            member inline _.Source(o: Nullable<'a>) = ValueOption.ofNullable o

    [<AutoOpen>]
    module BuilderExtensions3 =
        type OptionBuilderBase with
            /// <summary>
            /// Needed to allow `for..in` and `for..do` functionality
            /// </summary>
            // member inline _.Source(s: string) = ValueOption.ofObj s
#if NET9_0_OR_GREATER && !FABLE_COMPILER
            member inline _.Source(s: #seq<'value> | null) = ValueOption.ofObj s
#else
            member inline _.Source(s: #seq<'value>) = ValueOption.ofNull s
#endif

    type OptionBuilder() =

        inherit OptionBuilderBase()

        member inline _.Source(o: 'a option) = Option.toValueOption o

        member inline _.Run(f) =
            match f () with
            | ValueSome x -> Some x
            | ValueNone -> None


    type ValueOptionBuilder() =

        inherit OptionBuilderBase()

        member inline _.Source(o: 'a voption) = o

        member inline _.Run(f) = f ()

    [<AutoOpen>]
    module OptionCEExtensions =
        let option = OptionBuilder()
        let voption = ValueOptionBuilder()

        type ValueOptionBuilder with
            member inline _.Source(o: 'a option) = Option.toValueOption o

        type OptionBuilder with
            member inline _.Source(o: 'a voption) = o
