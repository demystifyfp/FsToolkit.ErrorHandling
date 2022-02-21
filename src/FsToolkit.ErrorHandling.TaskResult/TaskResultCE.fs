namespace FsToolkit.ErrorHandling

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.Affine.Unsafe
open FSharp.Control.Tasks.Affine
open Ply

[<AutoOpen>]
module TaskResultCE =

    type TaskResultBuilder() =

        member inline _.Return(value: 'T) : Ply<Result<'T, 'TError>> = uply.Return(result.Return value)

        member inline _.ReturnFrom(taskResult: Task<Result<'T, 'TError>>) : Ply<Result<'T, 'TError>> =
            uply.ReturnFrom taskResult

        member inline _.Zero() : Ply<Result<unit, 'TError>> = uply.Return <| result.Zero()

        member inline _.Bind
            (
                taskResult: Task<Result<'T, 'TError>>,
                [<InlineIfLambda>] binder: 'T -> Ply<Result<'U, 'TError>>
            ) : Ply<Result<'U, 'TError>> =
            let binder' r =
                match r with
                | Ok x -> binder x
                | Error x -> uply.Return <| Error x

            uply.Bind(taskResult, binder')

        member inline _.Delay([<InlineIfLambda>] generator: unit -> Ply<Result<'T, 'TError>>) = uply.Delay(generator)

        member inline _.Combine
            (
                computation1: Ply<Result<unit, 'TError>>,
                [<InlineIfLambda>] computation2: unit -> Ply<Result<'U, 'TError>>
            ) : Ply<Result<'U, 'TError>> =
            uply {
                match! computation1 with
                | Error e -> return Error e
                | Ok _ -> return! computation2 ()
            }

        member inline _.TryWith
            (
                [<InlineIfLambda>] computation: unit -> Ply<Result<'T, 'TError>>,
                [<InlineIfLambda>] handler: exn -> Ply<Result<'T, 'TError>>
            ) : Ply<Result<'T, 'TError>> =
            uply.TryWith(computation, handler)

        member inline _.TryFinally
            (
                [<InlineIfLambda>] computation: unit -> Ply<Result<'T, 'TError>>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : Ply<Result<'T, 'TError>> =
            uply.TryFinally(computation, compensation)

        member inline _.Using
            (
                resource: 'T :> IDisposable,
                [<InlineIfLambda>] binder: 'T -> Ply<Result<'U, 'TError>>
            ) : Ply<Result<'U, 'TError>> =
            uply.Using(resource, binder)

        member inline _.While
            (
                [<InlineIfLambda>] guard: unit -> bool,
                [<InlineIfLambda>] computation: unit -> Ply<Result<unit, 'TError>>
            ) : Ply<Result<unit, 'TError>> =
            uply {
                let mutable fin, result = false, Ok()

                while not fin && guard () do
                    match! computation () with
                    | Ok x -> x
                    | Error _ as e ->
                        result <- e
                        fin <- true

                return result
            }

        member inline _.For
            (
                sequence: #seq<'T>,
                [<InlineIfLambda>] binder: 'T -> Ply<Result<unit, 'TError>>
            ) : Ply<Result<unit, 'TError>> =
            uply {
                use enumerator = sequence.GetEnumerator()
                let mutable fin, result = false, Ok()

                while not fin && enumerator.MoveNext() do
                    match! binder enumerator.Current with
                    | Ok x -> x
                    | Error _ as e ->
                        result <- e
                        fin <- true

                return result
            }

        member inline this.BindReturn(x: Task<Result<'T, 'U>>, [<InlineIfLambda>] f) =
            this.Bind(x, (fun x -> this.Return(f x)))

        member inline _.MergeSources(t1: Task<Result<'T, 'U>>, t2: Task<Result<'T1, 'U>>) = TaskResult.zip t1 t2
        member inline _.Run([<InlineIfLambda>] f: unit -> Ply<'m>) = task.Run f

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(task: Task<Result<_, _>>) : Task<Result<_, _>> = task

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(t: ValueTask<Result<_, _>>) : Task<Result<_, _>> = task { return! t }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Async<Result<_, _>>) : Task<Result<_, _>> = result |> Async.StartAsTask

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(p: Ply<Result<_, _>>) : Task<Result<_, _>> = task { return! p }

    let taskResult = TaskResultBuilder()

// Having members as extensions gives them lower priority in
// overload resolution between Task<_> and Task<Result<_,_>>.
[<AutoOpen>]
module TaskResultCEExtensions =
    type TaskResultBuilder with
        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Result<_, _>) : Task<Result<_, _>> = Task.singleton result

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(choice: Choice<_, _>) : Task<Result<_, _>> =
            choice |> Result.ofChoice |> Task.singleton

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(asyncComputation: Async<_>) : Task<Result<_, _>> =
            asyncComputation
            |> Async.StartAsTask
            |> Task.map Ok

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(task: Task<_>) : Task<Result<_, _>> = task |> Task.map Ok

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(t: Task) : Task<Result<_, _>> =
            task {
                do! t
                return Ok()
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(task: ValueTask<_>) : Task<Result<_, _>> = task |> Task.mapV Ok

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(t: ValueTask) : Task<Result<_, _>> =
            task {
                do! t
                return Ok()
            }

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(p: Ply<_>) : Task<Result<_, _>> =
            task {
                let! p = p
                return Ok p
            }
