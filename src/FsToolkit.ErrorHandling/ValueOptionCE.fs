namespace FsToolkit.ErrorHandling


#if !FABLE_COMPILER
[<AutoOpen>]
module ValueOptionCE =
    open System

    type ValueOptionBuilder() =
        member inline _.Return(x) = ValueSome x

        member inline _.ReturnFrom(m: 'T voption) = m

        member inline _.Bind(m: 'a voption, f: 'a -> 'b voption) = ValueOption.bind f m

        // Could not get it to work solely with Source. In loop cases it would potentially match the #seq overload and ask for type annotation
        member this.Bind(m: 'a when 'a: null, f: 'a -> 'b voption) = this.Bind(m |> ValueOption.ofObj, f)

        member inline this.Zero() = this.Return()

        member inline _.Combine(m, f) = ValueOption.bind f m
        member inline this.Combine(m1: _ voption, m2: _ voption) = this.Bind(m1, (fun () -> m2))

        member inline _.Delay(f: unit -> _) = f

        member inline _.Run(f) = f ()

        member inline this.TryWith(m, h) =
            try
                this.Run m
            with
            | e -> h e

        member inline this.TryFinally(m, compensation) =
            try
                this.Run m
            finally
                compensation ()

        member inline this.Using(resource: 'T :> IDisposable, binder) : _ voption =
            this.TryFinally(
                (fun () -> binder resource),
                (fun () ->
                    if not <| obj.ReferenceEquals(resource, null) then
                        resource.Dispose())
            )

        member this.While(guard: unit -> bool, generator: unit -> _ voption) : _ voption =
            if not <| guard () then
                this.Zero()
            else
                this.Bind(this.Run generator, (fun () -> this.While(guard, generator)))

        member inline this.For(sequence: #seq<'T>, binder: 'T -> _ voption) : _ voption =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        member inline _.BindReturn(x, f) = ValueOption.map f x

        member inline _.BindReturn(x, f) =
            x |> ValueOption.ofObj |> ValueOption.map f

        member inline _.MergeSources(option1, option2) = ValueOption.zip option1 option2

        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(result: _ voption) : _ voption = result


        // /// <summary>
        // /// Method lets us transform data types into our internal representation.
        // /// </summary>
        member inline _.Source(vopt: _ option) : _ voption = vopt |> ValueOption.ofOption

    let voption = ValueOptionBuilder()

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module ValueOptionExtensionsLower =
    type ValueOptionBuilder with
        member inline _.Source(nullableObj: 'a when 'a: null) = nullableObj |> ValueOption.ofObj
        member inline _.Source(m: string) = m |> ValueOption.ofObj

        member inline _.MergeSources(nullableObj1, option2) =
            ValueOption.zip (ValueOption.ofObj nullableObj1) option2


        member inline _.MergeSources(option1, nullableObj2) =
            ValueOption.zip (option1) (ValueOption.ofObj nullableObj2)


        member inline _.MergeSources(nullableObj1, nullableObj2) =
            ValueOption.zip (ValueOption.ofObj nullableObj1) (ValueOption.ofObj nullableObj2)

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
// The later declared, the higher than previous extension methods, this is magic
module ValueOptionExtensions =
    open System

    type ValueOptionBuilder with
        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) = s

        // /// <summary>
        // /// Method lets us transform data types into our internal representation.
        // /// </summary>
        member inline _.Source(nullable: Nullable<'a>) : 'a voption = nullable |> ValueOption.ofNullable
#endif
