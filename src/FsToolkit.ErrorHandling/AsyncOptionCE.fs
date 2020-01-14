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

        #if !FABLE_COMPILER
        member __.ReturnFrom
            (taskResult: Task<Option<_>>)
            : Async<Option<_>> =
          Async.AwaitTask taskResult
        #endif

        member __.ReturnFrom
            (result: Option<_>)
            : Async<Option<_>> =
          async.Return result

        member __.Zero () : Async<Option<_>> =
          async.Return <| option.Zero ()

        member __.Bind
            (asyncResult: Async<Option<_>>,
             binder: 'T -> Async<Option<_>>)
            : Async<Option<_>> =
          async {
            let! result = asyncResult
            match result with
            | Some x -> return! binder x
            | None -> return None
          }

        #if !FABLE_COMPILER
        member this.Bind
            (taskResult: Task<Option<_>>,
             binder: 'T -> Async<Option<_>>)
            : Async<Option<_>> =
          this.Bind(Async.AwaitTask taskResult, binder)
        #endif

        member this.Bind
            (result: Option<_>, binder: 'T -> Async<Option<_>>)
            : Async<Option<_>> =
          this.Bind(this.ReturnFrom result, binder)


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


    let asyncOption = AsyncOptionBuilder() 