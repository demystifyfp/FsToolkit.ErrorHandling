namespace FsToolkit.ErrorHandling


open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module TaskResultCE = 

  type TaskResultBuilder() =

    member __.Return (value: 'T) : Task<Result<'T, 'TError>> =
      Task.singleton <| result.Return value

    member __.ReturnFrom
        (asyncResult: Async<Result<'T, 'TError>>)
        : Task<Result<'T, 'TError>> =
      Async.StartAsTask asyncResult

    member __.ReturnFrom
        (taskResult: Task<Result<'T, 'TError>>)
        : Task<Result<'T, 'TError>> =
      taskResult

    member __.ReturnFrom
        (result: Result<'T, 'TError>)
        : Task<Result<'T, 'TError>> =
      Task.singleton result

    member __.ReturnFrom
        (result: Choice<'T, 'TError>)
        : Task<Result<'T, 'TError>> =
      result
      |> Result.ofChoice
      |> Task.singleton 

    member __.Zero () : Task<Result<unit, 'TError>> =
      Task.singleton <| result.Zero ()

    member __.Bind
        (asyncResult: Task<Result<'T, 'TError>>,
         binder: 'T -> Task<Result<'U, 'TError>>)
        : Task<Result<'U, 'TError>> =
      task {
        let! result = asyncResult
        match result with
        | Ok x -> return! binder x
        | Error x -> return Error x
      }

    member this.Bind
        (taskResult: Async<Result<'T, 'TError>>,
         binder: 'T -> Task<Result<'U, 'TError>>)
        : Task<Result<'U, 'TError>> =
      this.Bind(Async.StartAsTask taskResult, binder)

    member this.Bind
        (result: Result<'T, 'TError>, binder: 'T -> Task<Result<'U, 'TError>>)
        : Task<Result<'U, 'TError>> =
      this.Bind(this.ReturnFrom result, binder)

    member this.Bind
        (result: Choice<'T, 'TError>, binder: 'T -> Task<Result<'U, 'TError>>)
        : Task<Result<'U, 'TError>> =
      this.Bind(this.ReturnFrom result, binder)

    member __.Delay
        (generator: unit -> Task<Result<'T, 'TError>>)
        : Task<Result<'T, 'TError>> =
      generator ()

    member this.Combine
        (computation1: Task<Result<unit, 'TError>>,
         computation2: Task<Result<'U, 'TError>>)
        : Task<Result<'U, 'TError>> =
      this.Bind(computation1, fun () -> computation2)

    member __.TryWith
        (computation: Task<Result<'T, 'TError>>,
         handler: System.Exception -> Task<Result<'T, 'TError>>) :
         Task<Result<'T, 'TError>> =
         task {
           try 
            return!  computation
           with e ->
            return! handler e
         }

    member __.TryFinally
        (computation: Task<Result<'T, 'TError>>,
         compensation: unit -> unit)
        : Task<Result<'T, 'TError>> =
         task {
           try 
            return! computation
           finally 
            compensation ()
         }

    // member __.TryFinally
    //     (computation: unit ->  Task<Result<'T, 'TError>>,
    //      compensation: unit -> unit)
    //     : Task<Result<'T, 'TError>> =
    //      task {
    //        try 
    //         return!  computation ()
    //        finally 
    //         compensation ()
    //      }

    member __.Using
        (resource: 'T when 'T :> IDisposable,
         binder: 'T -> Task<Result<'U, 'TError>>)
        : Task<Result<'U, 'TError>> =

        let body' = binder resource
        __.TryFinally(body', fun () -> 
          match resource with 
              | null -> () 
              | disp -> disp.Dispose())

    member this.While
        (guard: unit -> bool, computation: Task<Result<unit, 'TError>>)
        : Task<Result<unit, 'TError>> =
      if not <| guard () then this.Zero ()
      else this.Bind(computation, fun () -> this.While (guard, computation))

    member this.For
        (sequence: #seq<'T>, binder: 'T -> Task<Result<unit, 'TError>>)
        : Task<Result<unit, 'TError>> =
      this.Using(sequence.GetEnumerator (), fun enum ->
        this.While(enum.MoveNext,
          this.Delay(fun () -> binder enum.Current)))



[<AutoOpen>]
module TaskResultCEExtensions =

  // Having Async<_> members as extensions gives them lower priority in
  // overload resolution between Async<_> and Async<Result<_,_>>.
  type TaskResultBuilder with

    member __.ReturnFrom (async': Async<'T>) : Task<Result<'T, 'TError>> =
      task {
        let! x = async'
        return Ok x
      }

    member __.ReturnFrom (t: Task<'T>) : Task<Result<'T, 'TError>> =
      task {
        let! x = t
        return Ok x
      }

    member __.ReturnFrom (t: Task) : Task<Result<unit, 'TError>> =
      task {
        do! t
        return result.Zero ()
      }

    member this.Bind
        (async': Async<'T>, binder: 'T -> Task<Result<'U, 'TError>>)
        : Task<Result<'U, 'TError>> =
      let asyncResult = task {
        let! x = async'
        return Ok x
      }
      this.Bind(asyncResult, binder)

    member this.Bind
        (t: Task<'T>, binder: 'T -> Task<Result<'U, 'TError>>)
        : Task<Result<'U, 'TError>> =
      let asyncResult = task {
        let! x = t
        return Ok x
      }
      this.Bind(asyncResult, binder)

    member this.Bind
        (t: Task, binder: unit -> Task<Result<'T, 'TError>>)
        : Task<Result<'T, 'TError>> =
      let asyncResult = task {
        do! t
        return result.Zero()
      }
      this.Bind(asyncResult, binder)

  let taskResult = TaskResultBuilder() 
