namespace FsToolkit.ErrorHandling


open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
#nowarn "0044"
// FSharp.Control.Tasks.TaskBuilder is marked obselete, see [why](https://github.com/rspeele/TaskBuilder.fs/blob/master/TaskBuilder.fs#L17-L19)
// We however need access to the underlying `Step` type to be able to pull off the same tricks making this a usable CE
open FSharp.Control.Tasks.TaskBuilder

[<AutoOpen>]
module TaskResultCE = 

  let rec combineR (step :  Step<Result<'T, 'TError>>) (continuation : unit -> Step<Result<'U,'TError>>) =
      match step with
      | Return (Ok v) -> continuation()
      | Return (Error v) -> Return (Error v)
      | ReturnFrom t ->
          Await (t.GetAwaiter(), continuation)
      | Await (awaitable, next) ->
          Await (awaitable, fun () -> combineR (next()) continuation)

  let whileLoopR (cond : unit -> bool) (body : unit -> Step<Result<unit, 'TError>>) :  Step<Result<unit, 'TError>> =
      if cond() then
          // Create a self-referencing closure to test whether to repeat the loop on future iterations.
          let rec repeat () =
              if cond() then
                  let body = body()
                  match body with
                  | Return (Ok v) -> repeat()
                  | Return (Error v) -> body
                  | ReturnFrom t -> Await(t.GetAwaiter(), repeat)
                  | Await (awaitable, next) ->
                      Await (awaitable, fun () -> combineR (next()) repeat)
              else Return (result.Zero ())
          // Run the body the first time and chain it to the repeat logic.
          combineR (body()) repeat
      else Return (result.Zero ())
  /// Implements a loop that runs `body` for each element in `sequence`.
  let forLoopR (sequence : #seq<'a> ) (body : 'a -> Step<Result<_,_>>) =
      // A for loop is just a using statement on the sequence's enumerator...
      using (sequence.GetEnumerator())
          // ... and its body is a while loop that advances the enumerator and runs the body on each element.
          (fun e -> whileLoopR e.MoveNext (fun () -> body e.Current))

  type TaskResultBuilder() =

    member __.Return (value: 'T) 
      : Step<Result<'T,'TError>> =
      Return <| result.Return value

    member inline __.ReturnFrom
        (taskResult: Task<Result<'T, 'TError>>)
        : Step<Result<'T, 'TError>> =
      ReturnFrom taskResult

    member __.Zero () : Step<Result<unit, 'TError>> =
      ret <| result.Zero()

    member inline __.Bind
        (taskResult: Task<Result<'T, 'TError>>,
         binder: 'T -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
        let binder' r = 
          match r with
          | Ok x -> binder x
          | Error x -> ret <| Error x
        bindTaskConfigureFalse taskResult binder'

    member __.Delay
        (generator: unit -> Step<Result<'T, 'TError>>) =
      task.Delay(generator)

    member this.Combine
        (computation1: Step<Result<unit, 'TError>>,
         computation2: unit -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
      combineR computation1 computation2

    member __.TryWith
        (computation: unit -> Step<Result<'T, 'TError>>,
         handler: System.Exception -> Step<Result<'T, 'TError>>) :
         Step<Result<'T, 'TError>> =
      task.TryWith(computation, handler)

    member __.TryFinally
        (computation: unit -> Step<Result<'T, 'TError>>,
         compensation: unit -> unit)
        : Step<Result<'T, 'TError>> =
      task.TryFinally(computation, compensation)

    member __.Using
        (resource: 'T when 'T :> IDisposable,
         binder: 'T -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
        task.Using(resource, binder)

    member this.While
        (guard: unit -> bool, computation: unit -> Step<Result<unit, 'TError>>)
        : Step<Result<unit, 'TError>> =
      whileLoopR guard computation

    member this.For
        (sequence: #seq<'T>, binder: 'T -> Step<Result<unit, 'TError>>)
        : Step<Result<unit, 'TError>> =
      forLoopR sequence binder

    member inline __.BindReturn(x: Task<Result<'T,'U>>, f) = TaskResult.map f x |> ReturnFrom 
    member inline __.MergeSources(t1: Task<Result<'T,'U>>, t2: Task<Result<'T1,'U>>) = TaskResult.zip t1 t2
    member inline __.Run(f : unit -> Step<'m>) = run f

    /// <summary>
    /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
    /// 
    /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
    /// </summary>
    member inline _.Source(task : Task<Result<_,_>>) : Task<Result<_,_>> = task 

    /// <summary>
    /// Method lets us transform data types into our internal representation.  
    /// </summary>
    member inline _.Source(result : Async<Result<_,_>>) : Task<Result<_,_>> = result |> Async.StartAsTask

  let taskResult = TaskResultBuilder() 

// Having members as extensions gives them lower priority in
// overload resolution between Task<_> and Task<Result<_,_>>.
[<AutoOpen>]
module TaskResultCEExtensions =
  type TaskResultBuilder with
    /// <summary>
    /// Needed to allow `for..in` and `for..do` functionality
    /// </summary>
    member inline __.Source(s: #seq<_>) = s

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(result : Result<_,_>) : Task<Result<_,_>> = Task.singleton result

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(choice : Choice<_,_>) : Task<Result<_,_>> = 
      choice
      |> Result.ofChoice
      |> Task.singleton 

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(asyncComputation : Async<_>) : Task<Result<_,_>> = asyncComputation |> Async.StartAsTask |> Task.map Ok

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(task : Task<_>) : Task<Result<_,_>> = task |> Task.map Ok

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline _.Source(t : Task) : Task<Result<_,_>> = task { return! t } |> Task.map Ok
