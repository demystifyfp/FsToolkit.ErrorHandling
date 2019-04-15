namespace FsToolkit.ErrorHandling


open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharp.Control.Tasks.TaskBuilder

[<AutoOpen>]
module TaskResultCE = 

  let rec combineR (step :  Step<Result<unit, _>>) (continuation : unit -> Step<'b>) =
      match step with
      | Return _ -> continuation()
      | ReturnFrom t ->
          Await (t.GetAwaiter(), continuation)
      | Await (awaitable, next) ->
          Await (awaitable, fun () -> combineR (next()) continuation)

  let whileLoopR (cond : unit -> bool) (body : unit -> Step<Result<unit, _>>) :  Step<Result<unit, _>> =
      if cond() then
          // Create a self-referencing closure to test whether to repeat the loop on future iterations.
          let rec repeat () =
              if cond() then
                  let body = body()
                  match body with
                  | Return _ -> repeat()
                  | ReturnFrom t -> Await(t.GetAwaiter(), repeat)
                  | Await (awaitable, next) ->
                      Await (awaitable, fun () -> combineR (next()) repeat)
              else Return (result.Zero ())
          // Run the body the first time and chain it to the repeat logic.
          combineR (body()) repeat
      else Return (result.Zero ())
  /// Implements a loop that runs `body` for each element in `sequence`.
  let forLoopR (sequence : #seq<'a> ) (body : 'a -> Step<Result<unit,_>>) =
      // A for loop is just a using statement on the sequence's enumerator...
      using (sequence.GetEnumerator())
          // ... and its body is a while loop that advances the enumerator and runs the body on each element.
          (fun e -> whileLoopR e.MoveNext (fun () -> body e.Current))

  type TaskResultBuilder() =

    member __.Return (value: 'T) 
      : Step<Result<'T,'TError>> =
      Return <| result.Return value

    member __.ReturnFrom
        (taskResult: Task<Result<'T, 'TError>>)
        : Step<Result<'T, 'TError>> =
      ReturnFrom taskResult

    member __.ReturnFrom
        (asyncResult: Async<Result<'T, 'TError>>)
        : Step<Result<'T, 'TError>> =
      __.ReturnFrom (Async.StartAsTask asyncResult)

    member __.ReturnFrom
        (result: Result<'T, 'TError>)
        : Step<Result<'T, 'TError>> =
      Return result

    member __.ReturnFrom
        (result: Choice<'T, 'TError>)
        : Step<Result<'T, 'TError>> =
      result
      |> Result.ofChoice
      |> __.ReturnFrom

    member __.Zero () : Step<Result<unit, 'TError>> =
      ret <| result.Zero()

    member __.Bind
        (taskResult: Task<Result<'T, 'TError>>,
         binder: 'T -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
        let binder' r = 
          match r with
          | Ok x -> binder x
          | Error x -> ret <| Error x
        bindTaskConfigureFalse taskResult binder'
 
    member this.Bind
        (asyncResult: Async<Result<'T, 'TError>>,
         binder: 'T -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
      this.Bind(Async.StartAsTask asyncResult, binder)

    member this.Bind
        (result: Result<'T, 'TError>, binder: 'T -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
        let result = 
          result
          |> Task.singleton
        this.Bind(result, binder)

    member this.Bind
        (result: Choice<'T, 'TError>, binder: 'T -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
        let result = 
          result
          |> Result.ofChoice
          |> Task.singleton
        this.Bind(result, binder)

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

    member inline __.Run(f : unit -> Step<'m>) = run f


[<AutoOpen>]
module TaskResultCEExtensions =

  // Having Async<_> members as extensions gives them lower priority in
  // overload resolution between Async<_> and Async<Result<_,_>>.
  type TaskResultBuilder with

    member inline __.ReturnFrom (async': Async<'T>) : Step<Result<'T, 'TError>> =
      async'
      |> Async.map Ok
      |> Async.StartAsTask
      |> ReturnFrom


    member inline __.ReturnFrom (t : Task<_>) : Step<Result<'T, 'TError>> =
      t
      |> Task.map Ok
      |> ReturnFrom

    member __.ReturnFrom (t: Task) : Step<Result<unit, 'TError>> =
      task {
        do! t
        return result.Zero ()
      } |> __.ReturnFrom

    member this.Bind
        (async': Async<'T>, binder: 'T -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
      let body = async' |> Async.map Ok 
      this.Bind(body, binder)

    member this.Bind
        (t: Task<'T>, binder: 'T -> Step<Result<'U, 'TError>>)
        : Step<Result<'U, 'TError>> =
      let body = t |> Task.map Ok
      this.Bind(body, binder)

    member this.Bind
        (t: Task, binder: unit -> Step<Result<'T, 'TError>>)
        : Step<Result<'T, 'TError>> =
      let body = task {
        do! t
        return result.Zero()
      }
      this.Bind(body, binder)

  let taskResult = TaskResultBuilder() 
