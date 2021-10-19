namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.Affine.Unsafe
open FSharp.Control.Tasks.Affine
open Ply

[<AutoOpen>]
module TaskOptionCE =
    type TaskOptionBuilder() =
        member val SomeUnit = Some ()

        member inline _.Return (value: 'T)
          : Ply<_ option> =
          uply.Return <| option.Return value

        member inline _.ReturnFrom
            (taskResult: Task<_ option>)
            : Ply<_ option> =
          uply.ReturnFrom taskResult

        member inline _.Zero () : Ply<_ option> =
          uply.Return <| option.Zero()

        member inline _.Bind
            (taskResult: Task<_ option>,
             binder: 'T -> Ply<_ option>)
            : Ply<_ option> =
            let binder' r = 
              match r with
              | Some x -> binder x
              | None -> uply.Return None
            uply.Bind(taskResult, binder')

        member inline _.Delay
            (generator: unit -> Ply<_ option>) =
          uply.Delay(generator)

        member inline _.Combine
          (computation1: Ply<'T option>,
           computation2: unit -> Ply<'U option>)
          : Ply<'U option> = uply {
            match! computation1 with
            | None -> return None
            | Some _ -> return! computation2()
          }

        member inline _.TryWith
            (computation: unit -> Ply<_ option>,
             handler: exn -> Ply<_ option>) :
             Ply<_ option> =
             uply.TryWith(computation, handler)

        member inline _.TryFinally
            (computation: unit -> Ply<_ option>,
             compensation: unit -> unit)
            : Ply<_ option> =
             uply.TryFinally(computation, compensation)

        member inline _.Using
            (resource: 'T when 'T :> IDisposable,
             binder: 'T -> Ply<_ option>)
            : Ply<_ option> =
            uply.Using(resource, binder)

        member _.While
            (guard: unit -> bool, computation: unit -> Ply<'U option>)
            : Ply<'U option> = uply {
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
            (sequence: #seq<'T>, binder: 'T -> Ply<'U option>)
            : Ply<'U option> = uply {
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

        member inline this.BindReturn(x: Task<'T option>, f) = this.Bind(x, fun x -> this.Return(f x))
        member inline _.MergeSources(t1: Task<'T option>, t2: Task<'T1 option>) = TaskOption.zip t1 t2
        member inline _.Run(f : unit -> Ply<'m>) = task.Run f

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(task : Task<_ option>) : Task<_ option> = task

        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(t : ValueTask<_ option>) : Task<_ option> = task { return! t }

        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(async : Async<_ option>) : Task<_ option> = async |> Async.StartAsTask

        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(p : Ply<_ option>) : Task<_ option> = task { return! p }

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
    member inline __.Source(r: 't option) = Task.singleton r

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
