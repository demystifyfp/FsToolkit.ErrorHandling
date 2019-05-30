namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System

[<AutoOpen>]
module AsyncResultCE = 

  type AsyncResultBuilder() =

    member __.Return (value: 'T) : Async<Result<'T, 'TError>> =
      async.Return <| result.Return value

    member __.ReturnFrom
        (asyncResult: Async<Result<'T, 'TError>>)
        : Async<Result<'T, 'TError>> =
      asyncResult

    #if !FABLE_COMPILER
    member __.ReturnFrom
        (taskResult: Task<Result<'T, 'TError>>)
        : Async<Result<'T, 'TError>> =
      Async.AwaitTask taskResult
    #endif

    member __.ReturnFrom
        (result: Result<'T, 'TError>)
        : Async<Result<'T, 'TError>> =
      async.Return result

    member __.ReturnFrom
        (result: Choice<'T, 'TError>)
        : Async<Result<'T, 'TError>> =
      result
      |> Result.ofChoice
      |> async.Return 

    member __.Zero () : Async<Result<unit, 'TError>> =
      async.Return <| result.Zero ()

    member __.Bind
        (asyncResult: Async<Result<'T, 'TError>>,
         binder: 'T -> Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      async {
        let! result = asyncResult
        match result with
        | Ok x -> return! binder x
        | Error x -> return Error x
      }

    #if !FABLE_COMPILER
    member this.Bind
        (taskResult: Task<Result<'T, 'TError>>,
         binder: 'T -> Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      this.Bind(Async.AwaitTask taskResult, binder)
    #endif

    member this.Bind
        (result: Result<'T, 'TError>, binder: 'T -> Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      this.Bind(this.ReturnFrom result, binder)

    member this.Bind
        (result: Choice<'T, 'TError>, binder: 'T -> Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      this.Bind(this.ReturnFrom result, binder)

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



[<AutoOpen>]
module AsyncResultCEExtensions =

  // Having Async<_> members as extensions gives them lower priority in
  // overload resolution between Async<_> and Async<Result<_,_>>.
  type AsyncResultBuilder with

    member __.ReturnFrom (async': Async<'T>) : Async<Result<'T, 'TError>> =
      async {
        let! x = async'
        return Ok x
      }
    #if !FABLE_COMPILER
    member __.ReturnFrom (task: Task<'T>) : Async<Result<'T, 'TError>> =
      async {
        let! x = Async.AwaitTask task
        return Ok x
      }

    member __.ReturnFrom (task: Task) : Async<Result<unit, 'TError>> =
      async {
        do! Async.AwaitTask task
        return result.Zero ()
      }
    #endif

    member this.Bind
        (async': Async<'T>, binder: 'T -> Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      let asyncResult = async {
        let! x = async'
        return Ok x
      }
      this.Bind(asyncResult, binder)


    #if !FABLE_COMPILER
    member this.Bind
        (task: Task<'T>, binder: 'T -> Async<Result<'U, 'TError>>)
        : Async<Result<'U, 'TError>> =
      this.Bind(Async.AwaitTask task, binder)

    member this.Bind
        (task: Task, binder: unit -> Async<Result<'T, 'TError>>)
        : Async<Result<'T, 'TError>> =
      this.Bind(Async.AwaitTask task, binder)
    #endif
  let asyncResult = AsyncResultBuilder() 
