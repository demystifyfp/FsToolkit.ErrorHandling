namespace FsToolkit.ErrorHandling.CE.AsyncResult

open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.CE.Result
open System.Threading.Tasks

[<AutoOpen>]
module AsyncResult = 

  type AsyncResultBuilder() =

    member __.Return (value : 'a) = AsyncResult.retn value
    member __.ReturnFrom (value : Result<'a,'b>) = Async.singleton value
    member __.ReturnFrom (value : Async<Result<'a,'b>>) = value
    member __.ReturnFrom (value : Task<Result<'a,'b>>) = Async.AwaitTask value 

    member __.Zero () : Async<Result<unit, 'TError>> =
      result.Zero () |> Async.singleton 

    member __.Bind (result :  Async<Result<'a,'b>>, binder) =
      AsyncResult.bind binder result
    member __.Bind (result :  Task<Result<'a,'b>>, binder) =
      __.Bind(Async.AwaitTask result, binder )
    member __.Bind (result :  Result<'a,'b>, binder) =
      __.Bind(__.ReturnFrom result, binder )

    member __.Combine(ar1, ar2) =
      ar1
      |> AsyncResult.bind (fun _ -> ar2)

    member __.Delay f =
      async.Delay f

    member __.TryWith(computation : Async<Result<'T, 'TError>>, handler) =
      async.TryWith(computation, handler)

    member __.TryFinally (computation : Async<Result<'T, 'TError>>, compensation) =
      async.TryFinally(computation, compensation)

    member __.Using
        (resource: 'T when 'T :> System.IDisposable,
         binder: 'T -> Async<Result<'U, 'TError>>) =
      async.Using(resource, binder)

    member this.While
        (guard: unit -> bool, computation: Async<Result<unit, 'TError>>) =
      if not <| guard () then this.Zero ()
      else this.Bind(computation, fun () -> this.While (guard, computation))

    member this.For
        (sequence: #seq<'T>, binder: 'T -> Async<Result<unit, 'TError>>) =
      this.Using(sequence.GetEnumerator (), fun enum ->
        this.While(enum.MoveNext,
          this.Delay(fun () -> binder enum.Current)))

  let asyncResult = AsyncResultBuilder() 


[<AutoOpen>]
module Extensions =


  // Having Async<_> members as extensions gives them lower priority in
  // overload resolution between Async<_> and Async<Result<_,_>>.
  type AsyncResultBuilder with

    member __.ReturnFrom (async': Async<'T>) =
      async {
        let! x = async'
        return Ok x
      }

    member __.ReturnFrom (task: Task<'T>) =
      async {
        let! x = Async.AwaitTask task
        return Ok x
      }

    member __.ReturnFrom (task: Task) =
      async {
        do! Async.AwaitTask task
        return result.Zero ()
      }

    member this.Bind
        (async': Async<'T>, binder: 'T -> Async<Result<'U, 'TError>>) =
      let asyncResult = async {
        let! x = async'
        return Ok x
      }
      this.Bind(asyncResult, binder)

    member this.Bind
        (task: Task<'T>, binder: 'T -> Async<Result<'U, 'TError>>) =
      this.Bind(Async.AwaitTask task, binder)

    member this.Bind
        (task: Task, binder: unit -> Async<Result<'T, 'TError>>) =
      this.Bind(Async.AwaitTask task, binder)
        
  
  let asyncResult = AsyncResultBuilder() 