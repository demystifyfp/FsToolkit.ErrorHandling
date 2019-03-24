namespace FsToolkit.ErrorHandling.CE.Result

open System 

[<AutoOpen>]
module Result =

  type ResultBuilder() =
    member __.Return value = Ok value
    
    member __.ReturnFrom value = value

    member this.Zero ()  =
      this.Return ()

    member __.Bind (result, binder) =
      Result.bind binder result

    member __.Combine(r1, r2) =
      r1
      |> Result.bind (fun _ -> r2)

    member __.Delay f = f 


    member __.Run (generator: unit -> Result<'T, 'TError>) =
      generator ()


    member this.TryWith
        (generator: unit -> Result<'T, 'TError>,
         handler: exn -> Result<'T, 'TError>)
        : Result<'T, 'TError> =
      try this.Run generator with | e -> handler e

    member this.TryFinally
        (generator: unit -> Result<'T, 'TError>, compensation: unit -> unit)
        : Result<'T, 'TError> =
      try this.Run generator finally compensation ()

    member this.Using
        (resource: 'T when 'T :> IDisposable, binder: 'T -> Result<'U, 'TError>)
        : Result<'U, 'TError> =
      this.TryFinally (
        (fun () -> binder resource),
        (fun () -> if not <| obj.ReferenceEquals(resource, null) then resource.Dispose ())
      )

    member this.While
        (guard: unit -> bool, generator: unit -> Result<unit, 'TError>)
        : Result<unit, 'TError> =
      if not <| guard () then this.Zero ()
      else this.Bind(this.Run generator, fun () -> this.While (guard, generator))

    member this.For
        (sequence: #seq<'T>, binder: 'T -> Result<unit, 'TError>)
        : Result<unit, 'TError> =
      this.Using(sequence.GetEnumerator (), fun enum ->
        this.While(enum.MoveNext,
          this.Delay(fun () -> binder enum.Current)))

  let result = ResultBuilder()