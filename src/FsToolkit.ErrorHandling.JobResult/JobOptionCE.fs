namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System
open Hopac

[<AutoOpen>]
module JobOptionCE =

    type JobOptionBuilder() =

        member inline _.Return(value: 'T) : Job<_ option> =
            Some value
            |> job.Return

        member inline _.ReturnFrom(jobResult: Job<_ option>) : Job<_ option> = jobResult

        member inline _.Zero() : Job<_ option> =
            Some()
            |> job.Return

        member inline _.Bind
            (jobResult: Job<_ option>, [<InlineIfLambda>] binder: 'T -> Job<_ option>)
            : Job<_ option> =
            job {
                let! result = jobResult

                match result with
                | Some x -> return! binder x
                | None -> return None
            }

        member inline this.Bind
            (
                [<InlineIfLambda>] taskResult: unit -> Task<_ option>,
                [<InlineIfLambda>] binder: 'T -> Job<_ option>
            ) : Job<_ option> =
            this.Bind(Job.fromTask taskResult, binder)

        member inline _.Delay([<InlineIfLambda>] generator: unit -> Job<_ option>) : Job<_ option> =
            Job.delay generator

        member inline this.Combine
            (computation1: Job<_ option>, computation2: Job<_ option>)
            : Job<_ option> =
            this.Bind(computation1, (fun () -> computation2))

        member inline _.TryWith
            (
                computation: Job<_ option>,
                [<InlineIfLambda>] handler: System.Exception -> Job<_ option>
            ) : Job<_ option> =
            Job.tryWith computation handler

        member inline _.TryWith
            (
                [<InlineIfLambda>] computation: unit -> Job<_ option>,
                [<InlineIfLambda>] handler: System.Exception -> Job<_ option>
            ) : Job<_ option> =
            Job.tryWithDelay computation handler

        member inline _.TryFinally
            (computation: Job<_ option>, [<InlineIfLambda>] compensation: unit -> unit)
            : Job<_ option> =
            Job.tryFinallyFun computation compensation

        member inline _.TryFinally
            (computation: unit -> Job<_ option>, [<InlineIfLambda>] compensation: unit -> unit)
            : Job<_ option> =
            Job.tryFinallyFunDelay computation compensation

        member inline _.Using
            (resource: 'T :> IDisposableNull, [<InlineIfLambda>] binder: 'T -> Job<_ option>)
            : Job<_ option> =
            job.Using(resource, binder)

        member this.While(guard: unit -> bool, computation: Job<_ option>) : Job<_ option> =
            job {
                let mutable doContinue = true
                let mutable result = Some()

                while doContinue
                      && guard () do
                    match! computation with
                    | Some() -> ()
                    | None ->
                        doContinue <- false
                        result <- None

                return result

            }

        member inline this.For
            (sequence: #seq<'T>, [<InlineIfLambda>] binder: 'T -> Job<_ option>)
            : Job<_ option> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(job: Job<_ option>) : Job<_ option> = job


    let jobOption = JobOptionBuilder()

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module JobOptionCEExtensions =

    type JobOptionBuilder with

        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(r: 't option) = Job.singleton r

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: Job<'t>) =
            a
            |> Job.map Some

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: Async<'t>) =
            a
            |> Job.fromAsync
            |> Job.map Some

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(a: Task<'t>) =
            a
            |> Job.awaitTask
            |> Job.map Some

[<AutoOpen>]
// Having members as extensions gives them lower priority in
// overload resolution and allows skipping more type annotations.
module JobOptionCEExtensions2 =

    type JobOptionBuilder with

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(async: Async<_ option>) : Job<_ option> =
            async
            |> Job.fromAsync

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(task: Task<_ option>) : Job<_ option> =
            task
            |> Job.awaitTask
