namespace FsToolkit.ErrorHandling


open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
#nowarn "0044"
// FSharp.Control.Tasks.TaskBuilder is marked obselete, see [why](https://github.com/rspeele/TaskBuilder.fs/blob/master/TaskBuilder.fs#L17-L19)
// We however need access to the underlying `Step` type to be able to pull off the same tricks making this a usable CE
open FSharp.Control.Tasks.TaskBuilder


[<AutoOpen>]
module TaskOptionCE =  
    let rec combineO (step :  Step<Option<_>>) (continuation : unit -> Step<Option<_>>) =
      match step with
      | Return (Some v) -> continuation()
      | Return (None) -> Return (None)
      | ReturnFrom t ->
          Await (t.GetAwaiter(), continuation)
      | Await (awaitable, next) ->
          Await (awaitable, fun () -> combineO (next()) continuation)

    let whileLoopO (cond : unit -> bool) (body : unit -> Step<Option<_>>) :  Step<Option<_>> =
      if cond() then
          // Create a self-referencing closure to test whether to repeat the loop on future iterations.
          let rec repeat () =
              if cond() then
                  let body = body()
                  match body with
                  | Return (Some v) -> repeat()
                  | Return (None) -> body
                  | ReturnFrom t -> Await(t.GetAwaiter(), repeat)
                  | Await (awaitable, next) ->
                      Await (awaitable, fun () -> combineO (next()) repeat)
              else Return (option.Zero ())
          // Run the body the first time and chain it to the repeat logic.
          combineO (body()) repeat
      else Return (option.Zero ())
    /// Implements a loop that runs `body` for each element in `sequence`.
    let forLoopR (sequence : #seq<'a> ) (body : 'a -> Step<Option<_>>) =
      // A for loop is just a using statement on the sequence's enumerator...
      using (sequence.GetEnumerator())
          // ... and its body is a while loop that advances the enumerator and runs the body on each element.
          (fun e -> whileLoopO e.MoveNext (fun () -> body e.Current))

    type TaskOptionBuilder() =
        member __.Return (value: 'T) 
          : Step<Option<_>> =
          Return <| option.Return value

        member __.ReturnFrom
            (taskResult: Task<Option<_>>)
            : Step<Option<_>> =
          ReturnFrom taskResult

        member __.ReturnFrom
            (asyncResult: Async<Option<_>>)
            : Step<Option<_>> =
          __.ReturnFrom (Async.StartAsTask asyncResult)

        member __.ReturnFrom
            (result: Option<_>)
            : Step<Option<_>> =
          Return result

        member __.Zero () : Step<Option<_>> =
          ret <| option.Zero()

        member __.Bind
            (taskResult: Task<Option<_>>,
             binder: 'T -> Step<Option<_>>)
            : Step<Option<_>> =
            let binder' r = 
              match r with
              | Some x -> binder x
              | None -> ret None
            bindTaskConfigureFalse taskResult binder'
     
        member this.Bind
            (asyncResult: Async<Option<_>>,
             binder: 'T -> Step<Option<_>>)
            : Step<Option<_>> =
          this.Bind(Async.StartAsTask asyncResult, binder)

        member this.Bind
            (result: Option<_>, binder: 'T -> Step<Option<_>>)
            : Step<Option<_>> =
            let result = 
              result
              |> Task.singleton
            this.Bind(result, binder)


        member __.Delay
            (generator: unit -> Step<Option<_>>) =
          task.Delay(generator)

        member this.Combine
            (computation1: Step<Option<_>>,
             computation2: unit -> Step<Option<_>>)
            : Step<Option<_>> =
          combineO computation1 computation2

        member __.TryWith
            (computation: unit -> Step<Option<_>>,
             handler: System.Exception -> Step<Option<_>>) :
             Step<Option<_>> =
             task.TryWith(computation, handler)

        member __.TryFinally
            (computation: unit -> Step<Option<_>>,
             compensation: unit -> unit)
            : Step<Option<_>> =
             task.TryFinally(computation, compensation)

        member __.Using
            (resource: 'T when 'T :> IDisposable,
             binder: 'T -> Step<Option<_>>)
            : Step<Option<_>> =
            task.Using(resource, binder)

        member this.While
            (guard: unit -> bool, computation: unit -> Step<Option<_>>)
            : Step<Option<_>> =
           whileLoopO guard computation

        member this.For
            (sequence: #seq<'T>, binder: 'T -> Step<Option<_>>)
            : Step<Option<_>> =
          forLoopR sequence binder

        member inline __.Run(f : unit -> Step<'m>) = run f

    let taskOption = TaskOptionBuilder() 