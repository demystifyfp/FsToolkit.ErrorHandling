namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System

[<AutoOpen>]
module AsyncOptionCE =
    type AsyncOptionBuilder() =

        member __.Return (value: 'T) : Async<Option<_>> =
          async.Return <| option.Return value

        member __.ReturnFrom
            (asyncResult: Async<Option<_>>)
            : Async<Option<_>> =
          asyncResult

        member __.Zero () : Async<Option<_>> =
          async.Return <| option.Zero ()

        member inline __.Bind
            (asyncResult: Async<Option<_>>,
             binder: 'T -> Async<Option<_>>)
            : Async<Option<_>> =
          async {
            let! result = asyncResult
            match result with
            | Some x -> return! binder x
            | None -> return None
          }

        member __.Delay
            (generator: unit -> Async<Option<_>>)
            : Async<Option<_>> =
          async.Delay generator

        member this.Combine
            (computation1: Async<Option<_>>,
             computation2: Async<Option<_>>)
            : Async<Option<_>> =
          this.Bind(computation1, fun () -> computation2)

        member __.TryWith
            (computation: Async<Option<_>>,
             handler: System.Exception -> Async<Option<_>>)
            : Async<Option<_>> =
          async.TryWith(computation, handler)

        member __.TryFinally
            (computation: Async<Option<_>>,
             compensation: unit -> unit)
            : Async<Option<_>> =
          async.TryFinally(computation, compensation)

        member __.Using
            (resource: 'T when 'T :> IDisposable,
             binder: 'T -> Async<Option<_>>)
            : Async<Option<_>> =
            __.TryFinally (
                (binder resource),
                (fun () -> if not <| obj.ReferenceEquals(resource, null) then resource.Dispose ())
            )

        member this.While
            (guard: unit -> bool, computation: Async<Option<_>>)
            : Async<Option<_>> =
          if not <| guard () then this.Zero ()
          else this.Bind(computation, fun () -> this.While (guard, computation))

        member this.For
            (sequence: #seq<'T>, binder: 'T -> Async<Option<_>>)
            : Async<Option<_>> =
          this.Using(sequence.GetEnumerator (), fun enum ->
            this.While(enum.MoveNext,
              this.Delay(fun () -> binder enum.Current)))

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(async : Async<Option<_>>) : Async<Option<_>> = async

#if !FABLE_COMPILER
        /// <summary>
        /// Method lets us transform data types into our internal representation.  
        /// </summary>
        member inline _.Source(task : Task<Option<_>>) : Async<Option<_>> = task |> Async.AwaitTask
#endif

    let asyncOption = AsyncOptionBuilder() 


[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module AsyncOptionCEExtensions =

   type AsyncOptionBuilder with
    /// <summary>
    /// Needed to allow `for..in` and `for..do` functionality
    /// </summary>
    member inline __.Source(s: #seq<_>) = s

    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(r: Option<'t>) = Async.singleton r
    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(a: Async<'t>) = a |> Async.map Some

#if !FABLE_COMPILER
    /// <summary>
    /// Method lets us transform data types into our internal representation.
    /// </summary>
    member inline __.Source(a: Task<'t>) = a |> Async.AwaitTask |> Async.map Some
#endif