namespace FsToolkit.ErrorHandling

open System 

[<AutoOpen>]
module ResultCE =

  type ResultBuilder() =
    member __.Return (value: 'T) : Result<'T, 'TError> =
      Ok value

    member inline __.ReturnFrom (result: Result<'T, 'TError>) : Result<'T, 'TError> =
      result

    member this.Zero () : Result<unit, 'TError> =
      this.Return ()

    member inline __.Bind
        (result: Result<'T, 'TError>, binder: 'T -> Result<'U, 'TError>)
        : Result<'U, 'TError> =
      Result.bind binder result

    member __.Delay
        (generator: unit -> Result<'T, 'TError>)
        : unit -> Result<'T, 'TError> =
      generator

    member inline __.Run
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

    member _.BindReturn(x: Result<'T,'U>, f) = Result.map f x

    member _.MergeSources(t1: Result<'T,'U>, t2: Result<'T1,'U>) = Result.zip t1 t2

    /// <summary>
    /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
    /// 
    /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    member inline _.Source(result : Result<_,_>) : Result<_,_> = result

  let result = ResultBuilder()

[<AutoOpen>]
module ResultCEExtensions =

  type ResultBuilder with
    /// <summary>
    /// Needed to allow `for..in` and `for..do` functionality
    /// </summary>
    member inline __.Source(s: #seq<_>) = s


// Having Choice<_> members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
[<AutoOpen>]
module ResultCEChoiceExtensions =
  type ResultBuilder with
    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    /// <returns></returns>
    member inline _.Source(choice : Choice<_,_>) = Result.ofChoice choice

