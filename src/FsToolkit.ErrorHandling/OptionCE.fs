namespace FsToolkit.ErrorHandling

[<AutoOpen>]
module OptionCE =  
    open System
    type OptionBuilder() =
        member this.Return(x) = Some x

        member this.ReturnFrom(m: 'T option) = m

        member this.Bind(m, f) = Option.bind f m

        member this.Zero() = this.Return ()

        member this.Combine(m, f) = Option.bind f m

        member this.Delay(f: unit -> _) = f

        member this.Run(f) = f()

        member this.TryWith(m, h) =
            try this.Run m
            with e -> h e

        member this.TryFinally(m, compensation) =
            try this.Run m
            finally compensation()

        member this.Using
            (resource: 'T when 'T :> IDisposable, binder) : Option<_>=
            this.TryFinally (
                (fun () -> binder resource),
                (fun () -> if not <| obj.ReferenceEquals(resource, null) then resource.Dispose ())
            )

        member this.While
            (guard: unit -> bool, generator: unit -> Option<_>)
            : Option<_> =
            if not <| guard () then this.Zero ()
            else this.Bind(this.Run generator, fun () -> this.While (guard, generator))

        member this.For
            (sequence: #seq<'T>, binder: 'T -> Option<_>)
            : Option<_> =
            this.Using(sequence.GetEnumerator (), fun enum ->
                this.While(enum.MoveNext,
                    this.Delay(fun () -> binder enum.Current)))
                    
    let option = OptionBuilder()