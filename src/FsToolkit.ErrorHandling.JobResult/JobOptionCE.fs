namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System
open Hopac

[<AutoOpen>]
module JobOptionCE = 

  type JobOptionBuilder() =

    member __.Return (value: 'T) : Job<Option<_>> =
      job.Return <| option.Return value

    member __.ReturnFrom
        (asyncResult: Async<Option<_>>)
        : Job<Option<_>> =
      asyncResult |> Job.fromAsync

    member __.ReturnFrom
        (jobResult: Job<Option<_>>)
        : Job<Option<_>> =
      jobResult

    member __.ReturnFrom
        (taskResult: Task<Option<_>>)
        : Job<Option<_>> =
      Job.awaitTask taskResult

    member __.ReturnFrom
        (taskResult: unit -> Task<Option<_>>)
        : Job<Option<_>> =
      Job.fromTask taskResult

    member __.ReturnFrom
        (result: Option<_>)
        : Job<Option<_>> =
      job.Return result

    member __.Zero () : Job<Option<_>> =
      job.Return <| option.Zero ()

    member __.Bind
        (jobResult: Job<Option<_>>,
         binder: 'T -> Job<Option<_>>)
        : Job<Option<_>> =
      job {
        let! result = jobResult
        match result with
        | Some x -> return! binder x
        | None -> return None
      }
    member this.Bind
        (asyncResult: Async<Option<_>>,
         binder: 'T -> Job<Option<_>>)
        : Job<Option<_>> =
      this.Bind(Job.fromAsync asyncResult, binder)

    member this.Bind
        (taskResult: Task<Option<_>>,
         binder: 'T -> Job<Option<_>>)
        : Job<Option<_>> =
      this.Bind(Job.awaitTask taskResult, binder)

    member this.Bind
        (taskResult: unit -> Task<Option<_>>,
         binder: 'T -> Job<Option<_>>)
        : Job<Option<_>> =
      this.Bind(Job.fromTask taskResult, binder)

    member this.Bind
        (result: Option<_>, binder: 'T -> Job<Option<_>>)
        : Job<Option<_>> =
      this.Bind(this.ReturnFrom result, binder)

    member __.Delay
        (generator: unit -> Job<Option<_>>)
        : Job<Option<_>> =
      Job.delay generator

    member this.Combine
        (computation1: Job<Option<_>>,
         computation2: Job<Option<_>>)
        : Job<Option<_>> =
      this.Bind(computation1, fun () -> computation2)

    member __.TryWith
        (computation: Job<Option<_>>,
         handler: System.Exception -> Job<Option<_>>)
        : Job<Option<_>> =
      Job.tryWith computation handler

    member __.TryWith
        (computation: unit -> Job<Option<_>>,
         handler: System.Exception -> Job<Option<_>>)
        : Job<Option<_>> =
      Job.tryWithDelay computation handler

    member __.TryFinally
        (computation: Job<Option<_>>,
         compensation: unit -> unit)
        : Job<Option<_>> =
      Job.tryFinallyFun computation  compensation

    member __.TryFinally
        (computation: unit -> Job<Option<_>>,
         compensation: unit -> unit)
        : Job<Option<_>> =
      Job.tryFinallyFunDelay computation  compensation
      
    member __.Using
        (resource: 'T when 'T :> IDisposable,
         binder: 'T -> Job<Option<_>>)
        : Job<Option<_>> =
      job.Using(resource, binder)

    member this.While
        (guard: unit -> bool, computation: Job<Option<_>>)
        : Job<Option<_>> =
      if not <| guard () then this.Zero ()
      else this.Bind(computation, fun () -> this.While (guard, computation))

    member this.For
        (sequence: #seq<'T>, binder: 'T -> Job<Option<_>>)
        : Job<Option<_>> =
      this.Using(sequence.GetEnumerator (), fun enum ->
        this.While(enum.MoveNext,
          this.Delay(fun () -> binder enum.Current)))

  let jobOption = JobOptionBuilder() 
