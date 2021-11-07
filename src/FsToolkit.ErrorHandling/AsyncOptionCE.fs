namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System

[<AutoOpen>]
module AsyncOptionCE =
    type AsyncOptionBuilder() =

        member __.Return(value: 'T) : Async<_ option> = async.Return <| option.Return value

        member __.ReturnFrom(asyncResult: Async<_ option>) : Async<_ option> = asyncResult

        member __.Zero() : Async<_ option> = async.Return <| option.Zero()

        member inline __.Bind(asyncResult: Async<_ option>, binder: 'T -> Async<_ option>) : Async<_ option> =
            async {
                let! result = asyncResult

                match result with
                | Some x -> return! binder x
                | None -> return None
            }

        member __.Delay(generator: unit -> Async<_ option>) : Async<_ option> = async.Delay generator

        member this.Combine(computation1: Async<_ option>, computation2: Async<_ option>) : Async<_ option> =
            this.Bind(computation1, (fun () -> computation2))

        member __.TryWith
            (
                computation: Async<_ option>,
                handler: System.Exception -> Async<_ option>
            ) : Async<_ option> =
            async.TryWith(computation, handler)

        member __.TryFinally(computation: Async<_ option>, compensation: unit -> unit) : Async<_ option> =
            async.TryFinally(computation, compensation)

        member __.Using(resource: 'T :> IDisposable, binder: 'T -> Async<_ option>) : Async<_ option> =
            __.TryFinally(
                (binder resource),
                (fun () ->
                    if not <| obj.ReferenceEquals(resource, null) then
                        resource.Dispose())
            )

        member this.While(guard: unit -> bool, computation: Async<_ option>) : Async<_ option> =
            if not <| guard () then
                this.Zero()
            else
                this.Bind(computation, (fun () -> this.While(guard, computation)))

        member this.For(sequence: #seq<'T>, binder: 'T -> Async<_ option>) : Async<_ option> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(async: Async<_ option>) : Async<_ option> = async

#if !FABLE_COMPILER
        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(task: Task<_ option>) : Async<_ option> = task |> Async.AwaitTask
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
        member inline __.Source(r: 't option) = Async.singleton r
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
