namespace FsToolkit.ErrorHandling

[<AutoOpen>]
module OptionCE =
    open System

    type OptionBuilder() =
        member inline _.Return(x) = Some x

        member inline _.ReturnFrom(m: 'T option) = m

        member inline _.Bind(m: 'a option, f: 'a -> 'b option) = Option.bind f m

        // Could not get it to work solely with Source. In loop cases it would potentially match the #seq overload and ask for type annotation
        member this.Bind(m: 'a when 'a: null, f: 'a -> 'b option) = this.Bind(m |> Option.ofObj, f)

        member inline this.Zero() = this.Return()

        member inline _.Combine(m, f) = Option.bind f m
        member inline this.Combine(m1: _ option, m2: _ option) = this.Bind(m1, (fun () -> m2))

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

        member inline this.Using(resource: 'T :> IDisposable, binder) : _ option =
            this.TryFinally(
                (fun () -> binder resource),
                (fun () ->
                    if not <| obj.ReferenceEquals(resource, null) then
                        resource.Dispose())
            )

        member this.While(guard: unit -> bool, generator: unit -> _ option) : _ option =
            if not <| guard () then
                this.Zero()
            else
                this.Bind(this.Run generator, (fun () -> this.While(guard, generator)))

        member inline this.For(sequence: #seq<'T>, binder: 'T -> _ option) : _ option =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        member inline _.BindReturn(x, f) = Option.map f x
        member inline _.BindReturn(x, f) = x |> Option.ofObj |> Option.map f
        member inline _.MergeSources(option1, option2) = Option.zip option1 option2

        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(result: _ option) : _ option = result


    let option = OptionBuilder()


[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module OptionExtensionsLower =
    type OptionBuilder with
        member inline _.Source(nullableObj: 'a when 'a: null) = nullableObj |> Option.ofObj
        member inline _.Source(m: string) = m |> Option.ofObj

        member inline _.MergeSources(nullableObj1, option2) =
            Option.zip (Option.ofObj nullableObj1) option2


        member inline _.MergeSources(option1, nullableObj2) =
            Option.zip (option1) (Option.ofObj nullableObj2)


        member inline _.MergeSources(nullableObj1, nullableObj2) =
            Option.zip (Option.ofObj nullableObj1) (Option.ofObj nullableObj2)

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
// The later declared, the higher than previous extension methods, this is magic
module OptionExtensions =
    open System

    type OptionBuilder with
        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) = s


        // /// <summary>
        // /// Method lets us transform data types into our internal representation.
        // /// </summary>
        member inline _.Source(nullable: Nullable<'a>) : 'a option = nullable |> Option.ofNullable
