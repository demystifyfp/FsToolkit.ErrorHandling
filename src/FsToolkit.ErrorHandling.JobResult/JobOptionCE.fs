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
        (jobResult: Job<Option<_>>)
        : Job<Option<_>> =
      jobResult

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
        (taskResult: unit -> Task<Option<_>>,
         binder: 'T -> Job<Option<_>>)
        : Job<Option<_>> =
      this.Bind(Job.fromTask taskResult, binder)

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

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(job : Job<Option<_>>) : Job<Option<_>> = job

        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(async : Async<Option<_>>) : Job<Option<_>> = async |> Job.fromAsync

        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(task : Task<Option<_>>) : Job<Option<_>> = task |> Job.awaitTask

  let jobOption = JobOptionBuilder() 

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module JobOptionCEExtensions =

   type JobOptionBuilder with
    /// <summary>
    /// Needed to allow `for..in` and `for..do` functionality
    /// </summary>
    member inline __.Source(s: #seq<_>) = s

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(r: Option<'t>) = Job.singleton r
    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(a: Job<'t>) = a |> Job.map Some
    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(a: Async<'t>) = a |> Job.fromAsync |> Job.map Some
    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(a: Task<'t>) = a |> Job.awaitTask |> Job.map Some
