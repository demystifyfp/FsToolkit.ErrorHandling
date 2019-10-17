namespace FsToolkit.ErrorHandling

open System 

[<AutoOpen>]
module ResultCE =

  type ResultBuilder() =
    member __.Return (value: 'T) : Result<'T, 'TError> =
      Ok value

    member __.ReturnFrom (result: Result<'T, 'TError>) : Result<'T, 'TError> =
      result

    member this.Zero () : Result<unit, 'TError> =
      this.Return ()

    member __.Bind
        (result: Result<'T, 'TError>, binder: 'T -> Result<'U, 'TError>)
        : Result<'U, 'TError> =
      Result.bind binder result

    member __.Delay
        (generator: unit -> Result<'T, 'TError>)
        : unit -> Result<'T, 'TError> =
      generator

    member __.Run
        (generator: unit -> Result<'T, 'TError>)
        : Result<'T, 'TError> =
      generator ()

    member this.Combine
        (result: Result<unit, 'TError>, binder: unit -> Result<'T, 'TError>)
        : Result<'T, 'TError> =
      this.Bind(result, binder)

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

[<AutoOpen>]
module ResultCEExtensions =

  // Having Choice<_> members as extensions gives them lower priority in
  // overload resolution and allows skipping more type annotations.
  type ResultBuilder with

    member __.ReturnFrom (result: Choice<'T, 'TError>) : Result<'T, 'TError> =
      Result.ofChoice result

    member __.Bind
        (result: Choice<'T, 'TError>, binder: 'T -> Result<'U, 'TError>)
        : Result<'U, 'TError> =
        result
        |> Result.ofChoice
        |> Result.bind binder 



  let result = ResultBuilder()
