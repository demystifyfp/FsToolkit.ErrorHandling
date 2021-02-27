namespace FsToolkit.ErrorHandling

open System 

[<AutoOpen>]
module ValidationCE =
    type ValidationBuilder() =
        member __.Return (value: 'T) =
            Validation.ok value

        member inline __.ReturnFrom (result : Validation<'T, 'err>) =
            result

        member inline __.Bind
            (result: Validation<'T, 'TError>, binder: 'T -> Validation<'U, 'TError>)
            : Validation<'U, 'TError> =
            Validation.bind binder result

        member this.Zero () : Validation<unit, 'TError> =
            this.Return ()

        member __.Delay (generator: unit -> Validation<'T, 'TError>) : unit -> Validation<'T, 'TError> =
            generator

        member __.Run
            (generator: unit -> Validation<'T, 'TError>)
            : Validation<'T, 'TError> =
            generator ()

        member this.Combine
            (result: Validation<unit, 'TError>, binder: unit -> Validation<'T, 'TError>)
            : Validation<'T, 'TError> =
            this.Bind(result, binder)

        member this.TryWith
            (generator: unit -> Validation<'T, 'TError>, handler: exn -> Validation<'T, 'TError>)
            : Validation<'T, 'TError> =
            try 
                this.Run generator 
            with e -> handler e

        member this.TryFinally
            (generator: unit -> Validation<'T, 'TError>, compensation: unit -> unit)
            : Validation<'T, 'TError> =
            try this.Run generator finally compensation ()

        member this.Using
            (resource: 'T when 'T :> IDisposable, binder: 'T -> Validation<'U, 'TError>)
            : Validation<'U, 'TError> =
            this.TryFinally (
                (fun () -> binder resource),
                (fun () -> if not <| obj.ReferenceEquals(resource, null) then resource.Dispose ())
            )

        member this.While
            (guard: unit -> bool, generator: unit -> Validation<unit, 'TError>)
            : Validation<unit, 'TError> =
            if not <| guard () then this.Zero ()
            else this.Bind(this.Run generator, fun () -> this.While (guard, generator))

        member this.For
            (sequence: #seq<'T>, binder: 'T -> Validation<unit, 'TError>)
            : Validation<unit, 'TError> =
            this.Using(sequence.GetEnumerator (), fun enum ->
                this.While(enum.MoveNext,
                    this.Delay(fun () -> binder enum.Current)))

        member _.BindReturn(x: Validation<'T,'U>, f) = Validation.map f x

        member _.MergeSources(t1: Validation<'T,'U>, t2: Validation<'T1,'U>) = Validation.zip t1 t2

        /// <summary>
        /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
        /// 
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        member inline _.Source(result : Validation<_,_>) : Validation<_,_> = result

    let validation = ValidationBuilder()

[<AutoOpen>]
module ValidationCEExtensions =

  // Having members as extensions gives them lower priority in
  // overload resolution and allows skipping more type annotations.
    type ValidationBuilder with
        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline __.Source(s: #seq<_>) = s
        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline __.Source(s: Result<'T, 'Error>) = Validation.ofResult s
        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        /// <returns></returns>
        member inline _.Source(choice : Choice<_,_>) = Validation.ofChoice choice
