namespace FsToolkit.ErrorHandling

open System 

[<AutoOpen>]
module ValidationCE =
    type ValidationBuilder() =
        member __.Return (value: 'T) =
            Validation.ok value

        member inline __.ReturnFrom (result : Validation<'T, 'err>) =
            result

        member inline _.BindReturn(x: Validation<'T,'U>, f) : Validation<_,_> = 
            Result.map f x

        member inline __.Bind
            (result: Validation<'T, 'TError>, binder: 'T -> Validation<'U, 'TError>)
            : Validation<'U, 'TError> =
            Validation.bind binder result

        member inline _.MergeSources(t1: Validation<'T,'U>, t2: Validation<'T1,'U>) : Validation<_,_> = 
            Validation.zip t1 t2

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

    let validation = ValidationBuilder()

[<AutoOpen>]
module ValidationCEExtensions =

  // Having members as extensions gives them lower priority in
  // overload resolution and allows skipping more type annotations.
    type ValidationBuilder with
        member inline __.ReturnFrom (result : Result<'T, 'err>) = 
            result
            |> Validation.ofResult

        member inline __.ReturnFrom (result : Choice<'T, 'err>) = 
            result
            |> Validation.ofChoice

        member inline __.Bind
            (result: Result<'T, 'TError>, binder: 'T -> Validation<'U, 'TError>)
            : Validation<'U, 'TError> =
            result
            |> Validation.ofResult
            |> Validation.bind binder 

        member inline __.Bind
            (result: Choice<'T, 'TError>, binder: 'T -> Validation<'U, 'TError>)
            : Validation<'U, 'TError> =
            result
            |> Validation.ofChoice
            |> Validation.bind binder 

        member inline _.BindReturn(x: Result<'T,'U>, f) : Validation<_,_> = 
            x |> Validation.ofResult |> Result.map f
        member inline _.BindReturn(x: Choice<'T,'U>, f) : Validation<_,_> = 
            x |> Validation.ofChoice |> Result.map f

        member inline _.MergeSources(t1: Validation<'T,'U>, t2: Result<'T1,'U>) : Validation<_,_> = 
            Validation.zip t1 (Validation.ofResult t2)
        member inline _.MergeSources(t1: Result<'T,'U>, t2: Validation<'T1,'U>) : Validation<_,_> = 
            Validation.zip (Validation.ofResult t1) t2
        member inline _.MergeSources(t1: Result<'T,'U>, t2: Result<'T1,'U>) : Validation<_,_> = 
            Validation.zip (Validation.ofResult t1) (Validation.ofResult t2)


        member inline _.MergeSources(t1: Validation<'T,'U>, t2: Choice<'T1,'U>) : Validation<_,_> = 
            Validation.zip t1 (Validation.ofChoice t2)
        member inline _.MergeSources(t1: Choice<'T,'U>, t2: Validation<'T1,'U>) : Validation<_,_> = 
            Validation.zip (Validation.ofChoice t1) t2
        member inline _.MergeSources(t1: Choice<'T,'U>, t2: Choice<'T1,'U>) : Validation<_,_> = 
            Validation.zip (Validation.ofChoice t1) (Validation.ofChoice t2)
        
