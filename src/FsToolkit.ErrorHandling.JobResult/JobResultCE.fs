namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System
open Hopac

[<AutoOpen>]
module JobResultCE = 

  type JobResultBuilder() =

    member __.Return (value: 'T) : Job<Result<'T, 'TError>> =
      job.Return <| result.Return value

    member __.ReturnFrom
        (asyncResult: Async<Result<'T, 'TError>>)
        : Job<Result<'T, 'TError>> =
      asyncResult |> Job.fromAsync

    member __.ReturnFrom
        (jobResult: Job<Result<'T, 'TError>>)
        : Job<Result<'T, 'TError>> =
      jobResult

    member __.ReturnFrom
        (taskResult: Task<Result<'T, 'TError>>)
        : Job<Result<'T, 'TError>> =
      Job.awaitTask taskResult

    member __.ReturnFrom
        (taskResult: unit -> Task<Result<'T, 'TError>>)
        : Job<Result<'T, 'TError>> =
      Job.fromTask taskResult

    member __.ReturnFrom
        (result: Result<'T, 'TError>)
        : Job<Result<'T, 'TError>> =
      job.Return result

    member __.ReturnFrom
        (result: Choice<'T, 'TError>)
        : Job<Result<'T, 'TError>> =
      result
      |> Result.ofChoice
      |> __.ReturnFrom

    member __.Zero () : Job<Result<unit, 'TError>> =
      job.Return <| result.Zero ()

    member __.Bind
        (jobResult: Job<Result<'T, 'TError>>,
         binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      job {
        let! result = jobResult
        match result with
        | Ok x -> return! binder x
        | Error x -> return Error x
      }
    member this.Bind
        (asyncResult: Async<Result<'T, 'TError>>,
         binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(Job.fromAsync asyncResult, binder)

    member this.Bind
        (taskResult: Task<Result<'T, 'TError>>,
         binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(Job.awaitTask taskResult, binder)

    member this.Bind
        (taskResult: unit -> Task<Result<'T, 'TError>>,
         binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(Job.fromTask taskResult, binder)

    member this.Bind
        (result: Result<'T, 'TError>, binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(this.ReturnFrom result, binder)

    member this.Bind
        (result: Choice<'T, 'TError>, binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(this.ReturnFrom result, binder)

    member __.Delay
        (generator: unit -> Job<Result<'T, 'TError>>)
        : Job<Result<'T, 'TError>> =
      Job.delay generator

    member this.Combine
        (computation1: Job<Result<unit, 'TError>>,
         computation2: Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(computation1, fun () -> computation2)

    member __.TryWith
        (computation: Job<Result<'T, 'TError>>,
         handler: System.Exception -> Job<Result<'T, 'TError>>)
        : Job<Result<'T, 'TError>> =
      Job.tryWith computation handler

    member __.TryWith
        (computation: unit -> Job<Result<'T, 'TError>>,
         handler: System.Exception -> Job<Result<'T, 'TError>>)
        : Job<Result<'T, 'TError>> =
      Job.tryWithDelay computation handler

    member __.TryFinally
        (computation: Job<Result<'T, 'TError>>,
         compensation: unit -> unit)
        : Job<Result<'T, 'TError>> =
      Job.tryFinallyFun computation  compensation

    member __.TryFinally
        (computation: unit -> Job<Result<'T, 'TError>>,
         compensation: unit -> unit)
        : Job<Result<'T, 'TError>> =
      Job.tryFinallyFunDelay computation  compensation
      
    member __.Using
        (resource: 'T when 'T :> IDisposable,
         binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      job.Using(resource, binder)

    member this.While
        (guard: unit -> bool, computation: Job<Result<unit, 'TError>>)
        : Job<Result<unit, 'TError>> =
      if not <| guard () then this.Zero ()
      else this.Bind(computation, fun () -> this.While (guard, computation))

    member this.For
        (sequence: #seq<'T>, binder: 'T -> Job<Result<unit, 'TError>>)
        : Job<Result<unit, 'TError>> =
      this.Using(sequence.GetEnumerator (), fun enum ->
        this.While(enum.MoveNext,
          this.Delay(fun () -> binder enum.Current)))



    member __.BindReturn(x: Job<Result<'T,'U>>, f) = JobResult.map f x
    member __.BindReturn(x: Async<Result<'T,'U>>, f) = __.BindReturn(x |> Job.fromAsync, f)
    member __.BindReturn(x: Async<Choice<'T,'U>>, f) = __.BindReturn(x |> Async.map Result.ofChoice, f)
    member __.BindReturn(x: Result<'T,'U>, f) = __.BindReturn(x |> Job.singleton, f) 
    member __.BindReturn(x: Choice<'T,'U>, f) = __.BindReturn(x |> Result.ofChoice |> Job.singleton, f) 
    member __.BindReturn(x: Task<Result<'T,'U>>, f) = __.BindReturn(x |> Job.awaitTask, f) 


    member __.MergeSources(t1: Job<Result<'T,'U>>, t2: Job<Result<'T1,'U>>) = JobResult.zip t1 t2
    member __.MergeSources(t1: Task<Result<'T,'U>>, t2: Task<Result<'T1,'U>>) = JobResult.zip (Job.awaitTask t1) (Job.awaitTask t2)
    member __.MergeSources(t1: Async<Result<'T,'U>>, t2: Async<Result<'T1,'U>>) = JobResult.zip (Job.fromAsync t1) (Job.fromAsync t2)



[<AutoOpen>]
module JobResultCEExtensions =
  open Hopac
  // Having Job<_> members as extensions gives them lower priority in
  // overload resolution between Job<_> and Job<Result<_,_>>.
  type JobResultBuilder with

    member __.ReturnFrom (job': Job<'T>) : Job<Result<'T, 'TError>> =
      job {
        let! x = job'
        return Ok x
      }

    member __.ReturnFrom (async': Async<'T>) : Job<Result<'T, 'TError>> =
      job {
        let! x = async' |> Job.fromAsync
        return Ok x
      }

    member __.ReturnFrom (task: Task<'T>) : Job<Result<'T, 'TError>> =
      job {
        let! x = task
        return Ok x
      }   
      
    member __.ReturnFrom (task: unit -> Task<'T>) : Job<Result<'T, 'TError>> =
      job {
        let! x = task |> Job.fromTask
        return Ok x
      }

    member __.ReturnFrom (task: Task) : Job<Result<unit, 'TError>> =
      job {
        do! Job.awaitUnitTask task
        return result.Zero ()
      }

    member this.Bind
        (job': Job<'T>, binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      let jResult = job {
        let! x = job'
        return Ok x
      }
      this.Bind(jResult, binder)

    member this.Bind
        (task: Async<'T>, binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(Job.fromAsync task, binder)

    member this.Bind
        (task: Task<'T>, binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(Job.awaitTask task, binder)

    member this.Bind
        (task: unit -> Task<'T>, binder: 'T -> Job<Result<'U, 'TError>>)
        : Job<Result<'U, 'TError>> =
      this.Bind(Job.fromTask task, binder)

    member this.Bind
        (task: Task, binder: unit -> Job<Result<'T, 'TError>>)
        : Job<Result<'T, 'TError>> =
      this.Bind(Job.awaitUnitTask task, binder)

    member __.BindReturn(x: Job<'T>, f) = 
      __.BindReturn(x |> Job.map Result.Ok, f)
    member __.BindReturn(x: Async<'T>, f) = 
      __.BindReturn(x |> Async.map Result.Ok, f)
    member __.BindReturn(x: Task<'T>, f) = 
      __.BindReturn(x |> Task.map Result.Ok, f)
    member __.BindReturn(x: Task, f) = 
      __.BindReturn(x |> Task.ofUnit |> Task.map Result.Ok, f)



    member __.MergeSources(t1: Job<Result<'T,'U>>, t2: Async<Result<'T1,'U>>) =  JobResult.zip (t1) (t2 |> Job.fromAsync) 
    member __.MergeSources(t1: Async<Result<'T,'U>>, t2: Job<Result<'T1,'U>>) = JobResult.zip (t1 |> Job.fromAsync) (t2) 


    member __.MergeSources(t1: Job<Result<'T,'U>>, t2: Task<Result<'T1,'U>>) =  JobResult.zip (t1) (t2 |> Job.awaitTask) 
    member __.MergeSources(t1: Task<Result<'T,'U>>, t2: Job<Result<'T1,'U>>) = JobResult.zip (t1 |> Job.awaitTask) (t2) 

    member __.MergeSources(t1: Task<Result<'T,'U>>, t2: Async<Result<'T1,'U>>) = AsyncResult.zip (t1 |> Async.AwaitTask) (t2) |> Job.fromAsync
    member __.MergeSources(t1: Async<Result<'T,'U>>, t2: Task<Result<'T1,'U>>) = AsyncResult.zip (t1) (t2 |> Async.AwaitTask) |> Job.fromAsync

    member __.MergeSources(t1: Task<Result<'T,'U>>, t2: Result<'T1,'U>) = AsyncResult.zip (t1 |> Async.AwaitTask) (t2  |> Async.singleton) |> Job.fromAsync
    member __.MergeSources(t1: Result<'T,'U>, t2: Task<Result<'T1,'U>>) = AsyncResult.zip (t1 |> Async.singleton) (t2 |> Async.AwaitTask) |> Job.fromAsync
    

    member __.MergeSources(t1: Async<Result<'T,'U>>, t2: Async<'T1>) = AsyncResult.zip t1 (t2  |> Async.map Result.Ok) |> Job.fromAsync
    member __.MergeSources(t1: Async<'T>, t2: Async<Result<'T1,'U>>) = AsyncResult.zip (t1 |> Async.map Result.Ok) t2 |> Job.fromAsync

    member __.MergeSources(t1: Job<Result<'T,'U>>, t2: Result<'T1,'U>) = JobResult.zip t1 (t2  |> Job.singleton) 
    member __.MergeSources(t1: Result<'T,'U>, t2: Job<Result<'T1,'U>>) = JobResult.zip (t1 |> Job.singleton) t2 
    member __.MergeSources(t1: Result<'T,'U>, t2: Result<'T1,'U>) = AsyncResult.zip (t1 |> Async.singleton) (t2 |> Async.singleton) |> Job.fromAsync 

    member __.MergeSources(t1: Async<Result<'T,'U>>, t2: Result<'T1,'U>) = AsyncResult.zip t1 (t2  |> Async.singleton) |> Job.fromAsync
    member __.MergeSources(t1: Result<'T,'U>, t2: Async<Result<'T1,'U>>) = AsyncResult.zip (t1 |> Async.singleton) t2 |> Job.fromAsync


    member __.MergeSources(t1: Async<Result<'T,'U>>, t2: Choice<'T1,'U>) = AsyncResult.zip t1 (t2 |> Result.ofChoice |> Async.singleton) |> Job.fromAsync
    member __.MergeSources(t1: Choice<'T,'U>, t2: Async<Result<'T1,'U>>) = AsyncResult.zip (t1 |> Result.ofChoice |> Async.singleton) t2 |> Job.fromAsync

    member __.MergeSources(t1: Job<Result<'T,'U>>, t2: Choice<'T1,'U>) = JobResult.zip t1 (t2 |> Result.ofChoice |> Job.singleton) 
    member __.MergeSources(t1: Choice<'T,'U>, t2: Job<Result<'T1,'U>>) = JobResult.zip (t1 |> Result.ofChoice |> Job.singleton) t2 
    member __.MergeSources(t1: Choice<'T,'U>, t2: Choice<'T1,'U>) = AsyncResult.zip (t1 |> Result.ofChoice |> Async.singleton) (t2 |> Result.ofChoice |> Async.singleton) |> Job.fromAsync

    member __.MergeSources(t1: Choice<'T,'U>, t2: Result<'T1,'U>) = AsyncResult.zip (t1 |> Result.ofChoice |> Async.singleton) (t2 |> Async.singleton) |> Job.fromAsync
    member __.MergeSources(t1: Result<'T,'U>, t2: Choice<'T1,'U>) = AsyncResult.zip (t1 |> Async.singleton) (t2 |> Result.ofChoice |> Async.singleton) |> Job.fromAsync
    

[<AutoOpen>]
module JobResultCEExtensions2 =
  // Having Async<_> members as extensions gives them lower priority in
  // overload resolution between Async<_> and Async<Result<_,_>>.
  type JobResultBuilder with
    member __.MergeSources(t1: Job<'T>, t2: Job<'T1>) = JobResult.zip (t1 |> Job.map Result.Ok) (t2 |> Job.map Result.Ok)
    member __.MergeSources(t1: Async<'T>, t2: Async<'T1>) = AsyncResult.zip (t1 |> Async.map Result.Ok) (t2 |> Async.map Result.Ok) |> Job.fromAsync
    member __.MergeSources(t1: Task<'T>, t2: Task<'T1>) = TaskResult.zip (t1 |> Task.map Result.Ok) (t2 |> Task.map Result.Ok) |> Job.awaitTask


  let jobResult = JobResultBuilder() 