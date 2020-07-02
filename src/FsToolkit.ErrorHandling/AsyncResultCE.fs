namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System

[<AutoOpen>]
module AsyncResultCE = 

  type AsyncResultBuilder() =

    member __.Return (value: 'T) : Async<Result<'T, 'TError>> =
      async.Return <| result.Return value

    member inline __.ReturnFrom
        (asyncResult: Async<Result<'T, 'TError>>)
        : Async<Result<'T, 'TError>> =
      asyncResult

    member __.Zero () : Async<Result<unit, 'TError>> =
      async.Return <| result.Zero ()

    member inline __.Bind
        (asyncResult: Async<Result<'T, 'TError>>,
         binder: 'T -> Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      async {
        let! result = asyncResult
        match result with
        | Ok x -> return! binder x
        | Error x -> return Error x
      }

    member __.Delay
        (generator: unit -> Async<Result<'T, 'TError>>)
        : Async<Result<'T, 'TError>> =
      async.Delay generator

    member this.Combine
        (computation1: Async<Result<unit, 'TError>>,
         computation2: Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      this.Bind(computation1, fun () -> computation2)

    member __.TryWith
        (computation: Async<Result<'T, 'TError>>,
         handler: System.Exception -> Async<Result<'T, 'TError>>)
        : Async<Result<'T, 'TError>> =
      async.TryWith(computation, handler)

    member __.TryFinally
        (computation: Async<Result<'T, 'TError>>,
         compensation: unit -> unit)
        : Async<Result<'T, 'TError>> =
      async.TryFinally(computation, compensation)

    member __.Using
        (resource: 'T when 'T :> IDisposable,
         binder: 'T -> Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      async.Using(resource, binder)

    member this.While
        (guard: unit -> bool, computation: Async<Result<unit, 'TError>>)
        : Async<Result<unit, 'TError>> =
      if not <| guard () then this.Zero ()
      else this.Bind(computation, fun () -> this.While (guard, computation))

    member this.For
        (sequence: #seq<'T>, binder: 'T -> Async<Result<unit, 'TError>>)
        : Async<Result<unit, 'TError>> =
      this.Using(sequence.GetEnumerator (), fun enum ->
        this.While(enum.MoveNext,
          this.Delay(fun () -> binder enum.Current)))

    member inline __.BindReturn(x: Async<Result<'T,'U>>, f) = AsyncResult.map f x
    member inline __.MergeSources(t1: Async<Result<'T,'U>>, t2: Async<Result<'T1,'U>>) = AsyncResult.zip t1 t2


    /// <summary>
    /// Method lets us transform data types into our internal representation.  This is the identity method to recognize the self type.
    /// 
    /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
    /// </summary>
    member inline _.Source(result : Async<Result<_,_>>) : Async<Result<_,_>> = result

#if !FABLE_COMPILER
    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(task : Task<Result<_,_>>) : Async<Result<_,_>> = task |> Async.AwaitTask
#endif

  let asyncResult = AsyncResultBuilder() 

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module AsyncResultCEExtensions =

  type AsyncResultBuilder with
    /// <summary>
    /// Needed to allow `for..in` and `for..do` functionality
    /// </summary>
    member inline __.Source(s: #seq<_>) = s

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(result : Result<_,_>) : Async<Result<_,_>> = Async.singleton result

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(choice : Choice<_,_>) : Async<Result<_,_>> = 
      choice
      |> Result.ofChoice
      |> Async.singleton 

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(asyncComputation : Async<_>) : Async<Result<_,_>> = asyncComputation |> Async.map Ok

#if !FABLE_COMPILER
    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(task : Task<_>) : Async<Result<_,_>> = task |> Async.AwaitTask |> Async.map Ok

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(task : Task) : Async<Result<_,_>> =task |> Async.AwaitTask |> Async.map Ok
#endif