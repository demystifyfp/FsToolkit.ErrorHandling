namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.Affine
open Ply

[<AutoOpen>]
module TaskOptionCE =
    type TaskOptionBuilder() =
        member inline _.Return (value: 'T)
          : Ply<Option<_>> =
          task.Return <| option.Return value

        member inline _.ReturnFrom
            (taskResult: Task<Option<_>>)
            : Ply<Option<_>> =
          task.ReturnFrom taskResult

        member inline this.ReturnFrom
            (asyncResult: Async<Option<_>>)
            : Ply<Option<_>> =
          this.ReturnFrom (Async.StartAsTask asyncResult)

        member inline _.ReturnFrom
            (result: Option<_>)
            : Ply<Option<_>> =
          task.Return result

        member inline _.Zero () : Ply<Option<_>> =
          task.Return <| option.Zero()

        member inline _.Bind
            (taskResult: Task<Option<_>>,
             binder: 'T -> Ply<Option<_>>)
            : Ply<Option<_>> =
            let binder' r = 
              match r with
              | Some x -> binder x
              | None -> task.Return None
            task.Bind(taskResult, binder')
     
        member inline this.Bind
            (asyncResult: Async<Option<_>>,
             binder: 'T -> Ply<Option<_>>)
            : Ply<Option<_>> =
          this.Bind(Async.StartAsTask asyncResult, binder)

        member inline this.Bind
            (result: Option<_>, binder: 'T -> Ply<Option<_>>)
            : Ply<Option<_>> =
            let result = 
              result
              |> Task.singleton
            this.Bind(result, binder)

        member inline _.Delay
            (generator: unit -> Ply<Option<_>>) =
          task.Delay(generator)

        member inline _.Combine
          (computation1: Ply<Option<'T>>,
           computation2: unit -> Ply<Option<'U>>)
          : Ply<Option<'U>> =
            task {
              match! computation1 with
              | None -> return None
              | Some _ -> return! computation2()
            } |> task.ReturnFrom

        member inline _.TryWith
            (computation: unit -> Ply<Option<_>>,
             handler: exn -> Ply<Option<_>>) :
             Ply<Option<_>> =
             task.TryWith(computation, handler)

        member inline _.TryFinally
            (computation: unit -> Ply<Option<_>>,
             compensation: unit -> unit)
            : Ply<Option<_>> =
             task.TryFinally(computation, compensation)

        member inline _.Using
            (resource: 'T when 'T :> IDisposable,
             binder: 'T -> Ply<Option<_>>)
            : Ply<Option<_>> =
            task.Using(resource, binder)

        member _.While
            (guard: unit -> bool, computation: unit -> Ply<Option<'U>>)
            : Ply<Option<'U>> =
              task {
                let mutable fin, result = false, None
                while not fin && guard() do
                  match! computation() with
                  | Some _ as o ->
                    result <- o
                  | None ->
                    result <- None
                    fin <- true
                return result
              } |> task.ReturnFrom

        member _.For
            (sequence: #seq<'T>, binder: 'T -> Ply<Option<'U>>)
            : Ply<Option<'U>> =
              task {
                use enumerator = sequence.GetEnumerator()
                let mutable fin, result = false, None
                while not fin && enumerator.MoveNext() do
                  match! binder enumerator.Current with
                  | Some _ as o ->
                    result <- o
                  | None ->
                    result <- None
                    fin <- true
                return result
              } |> task.ReturnFrom

        member inline _.Run(f : unit -> Ply<'m>) = task.Run f

    let taskOption = TaskOptionBuilder() 
