namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.Affine.Unsafe
open FSharp.Control.Tasks.Affine
open Ply

[<AutoOpen>]
module TaskOptionCE =
    type TaskOptionBuilder() =
        member val internal SomeUnit = Some ()

        member inline _.Return (value: 'T)
          : Ply<Option<_>> =
          uply.Return <| option.Return value

        member inline _.ReturnFrom
            (taskResult: Task<Option<_>>)
            : Ply<Option<_>> =
          uply.ReturnFrom taskResult

        member inline _.Zero () : Ply<Option<_>> =
          uply.Return <| option.Zero()

        member inline _.Bind
            (taskResult: Task<Option<_>>,
             binder: 'T -> Ply<Option<_>>)
            : Ply<Option<_>> =
            let binder' r = 
              match r with
              | Some x -> binder x
              | None -> uply.Return None
            uply.Bind(taskResult, binder')

        member inline _.Delay
            (generator: unit -> Ply<Option<_>>) =
          uply.Delay(generator)

        member inline _.Combine
          (computation1: Ply<Option<'T>>,
           computation2: unit -> Ply<Option<'U>>)
          : Ply<Option<'U>> = uply {
            match! computation1 with
            | None -> return None
            | Some _ -> return! computation2()
          }

        member inline _.TryWith
            (computation: unit -> Ply<Option<_>>,
             handler: exn -> Ply<Option<_>>) :
             Ply<Option<_>> =
             uply.TryWith(computation, handler)

        member inline _.TryFinally
            (computation: unit -> Ply<Option<_>>,
             compensation: unit -> unit)
            : Ply<Option<_>> =
             uply.TryFinally(computation, compensation)

        member inline _.Using
            (resource: 'T when 'T :> IDisposable,
             binder: 'T -> Ply<Option<_>>)
            : Ply<Option<_>> =
            uply.Using(resource, binder)

        member _.While
            (guard: unit -> bool, computation: unit -> Ply<Option<'U>>)
            : Ply<Option<'U>> = uply {
              let mutable fin, result = false, None
              while not fin && guard() do
                match! computation() with
                | Some _ as o ->
                  result <- o
                | None ->
                  result <- None
                  fin <- true
              return result
            }

        member _.For
            (sequence: #seq<'T>, binder: 'T -> Ply<Option<'U>>)
            : Ply<Option<'U>> = uply {
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
            }

        member inline this.BindReturn(x: Task<Option<'T>>, f) = this.Bind(x, fun x -> this.Return(f x))
        member inline _.MergeSources(t1: Task<Option<'T>>, t2: Task<Option<'T1>>) = TaskOption.zip t1 t2
        member inline _.Run(f : unit -> Ply<'m>) = task.Run f

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(task : Task<Option<_>>) : Task<Option<_>> = task

        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(t : ValueTask<Option<_>>) : Task<Option<_>> = task { return! t }

        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(async : Async<Option<_>>) : Task<Option<_>> = async |> Async.StartAsTask

        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(p : Ply<Option<_>>) : Task<Option<_>> = task { return! p }

    let taskOption = TaskOptionBuilder() 

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module TaskOptionCEExtensions =

   type TaskOptionBuilder with
    /// <summary>
    /// Needed to allow `for..in` and `for..do` functionality
    /// </summary>
    member inline __.Source(s: #seq<_>) = s

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(r: Option<'t>) = Task.singleton r

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(a: Task<'t>) = a |> Task.map Some

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline x.Source(a: Task) = task {
        do! a
        return x.SomeUnit }

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(a: ValueTask<'t>) = a |> Task.mapV Some

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline x.Source(a: ValueTask) = task {
        do! a
        return x.SomeUnit }

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(a: Async<'t>) = a |> Async.StartAsTask |> Task.map Some
