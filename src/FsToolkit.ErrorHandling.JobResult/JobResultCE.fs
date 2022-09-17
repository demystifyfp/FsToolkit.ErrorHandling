namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open System
open Hopac

[<AutoOpen>]
module JobResultCE =

    type JobResultBuilder() =

        member inline_.Return(value: 'T) : Job<Result<'T, 'TError>> =
            job.Return
            <| result.Return value

        member inline _.ReturnFrom(jobResult: Job<Result<'T, 'TError>>) : Job<Result<'T, 'TError>> =
            jobResult

        member inline_.Zero() : Job<Result<unit, 'TError>> =
            job.Return
            <| result.Zero()

        member inline _.Bind
            (
                jobResult: Job<Result<'T, 'TError>>,
                [<InlineIfLambda>] binder: 'T -> Job<Result<'U, 'TError>>
            ) : Job<Result<'U, 'TError>> =
            job {
                let! result = jobResult

                match result with
                | Ok x -> return! binder x
                | Error x -> return Error x
            }

        member inline _.Delay
            ([<InlineIfLambda>] generator: unit -> Job<Result<'T, 'TError>>)
            : Job<Result<'T, 'TError>> =
            Job.delay generator

        member inline this.Combine
            (
                computation1: Job<Result<unit, 'TError>>,
                computation2: Job<Result<'U, 'TError>>
            ) : Job<Result<'U, 'TError>> =
            this.Bind(computation1, (fun () -> computation2))

        member inline _.TryWith
            (
                computation: Job<Result<'T, 'TError>>,
                [<InlineIfLambda>] handler: System.Exception -> Job<Result<'T, 'TError>>
            ) : Job<Result<'T, 'TError>> =
            Job.tryWith computation handler

        member inline _.TryWith
            (
                [<InlineIfLambda>] computation: unit -> Job<Result<'T, 'TError>>,
                [<InlineIfLambda>] handler: System.Exception -> Job<Result<'T, 'TError>>
            ) : Job<Result<'T, 'TError>> =
            Job.tryWithDelay computation handler

        member inline _.TryFinally
            (
                computation: Job<Result<'T, 'TError>>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : Job<Result<'T, 'TError>> =
            Job.tryFinallyFun computation compensation

        member inline _.TryFinally
            (
                [<InlineIfLambda>] computation: unit -> Job<Result<'T, 'TError>>,
                [<InlineIfLambda>] compensation: unit -> unit
            ) : Job<Result<'T, 'TError>> =
            Job.tryFinallyFunDelay computation compensation

        member inline _.Using
            (
                resource: 'T :> IDisposable,
                [<InlineIfLambda>] binder: 'T -> Job<Result<'U, 'TError>>
            ) : Job<Result<'U, 'TError>> =
            job.Using(resource, binder)

        member this.While
            (
                guard: unit -> bool,
                computation: Job<Result<unit, 'TError>>
            ) : Job<Result<unit, 'TError>> =
            if
                not
                <| guard ()
            then
                this.Zero()
            else
                this.Bind(computation, (fun () -> this.While(guard, computation)))

        member inline this.For
            (
                sequence: #seq<'T>,
                [<InlineIfLambda>] binder: 'T -> Job<Result<unit, 'TError>>
            ) : Job<Result<unit, 'TError>> =
            this.Using(
                sequence.GetEnumerator(),
                fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> binder enum.Current))
            )

        member inline _.BindReturn(x: Job<Result<'T, 'U>>, [<InlineIfLambda>] f) = JobResult.map f x

        member inline _.MergeSources(t1: Job<Result<'T, 'U>>, t2: Job<Result<'T1, 'U>>) =
            JobResult.zip t1 t2

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        ///
        /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
        /// </summary>
        member inline _.Source(job': Job<Result<_, _>>) : Job<Result<_, _>> = job'

        /// <summary>
        /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
        /// </summary>
        member inline _.Source(task: Task<Result<_, _>>) : Job<Result<_, _>> =
            task
            |> Job.awaitTask

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Async<Result<_, _>>) : Job<Result<_, _>> =
            result
            |> Job.fromAsync

    let jobResult = JobResultBuilder()

[<AutoOpen>]
module JobResultCEExtensions =
    open Hopac
    // Having members as extensions gives them lower priority in
    // overload resolution between Job<_> and Job<Result<_,_>>.
    type JobResultBuilder with

        /// <summary>
        /// Needed to allow `for..in` and `for..do` functionality
        /// </summary>
        member inline _.Source(s: #seq<_>) = s

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(result: Result<_, _>) : Job<Result<_, _>> = Job.result result

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(choice: Choice<_, _>) : Job<Result<_, _>> =
            choice
            |> Result.ofChoice
            |> Job.result

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(job': Job<_>) : Job<Result<_, _>> =
            job'
            |> Job.map Ok

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(asyncComputation: Async<_>) : Job<Result<_, _>> =
            asyncComputation
            |> Job.fromAsync
            |> Job.map Ok

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(task: Task<_>) : Job<Result<_, _>> =
            task
            |> Job.awaitTask
            |> Job.map Ok

        /// <summary>
        /// Method lets us transform data types into our internal representation.
        /// </summary>
        member inline _.Source(t: Task) : Job<Result<_, _>> =
            t
            |> Job.awaitUnitTask
            |> Job.map Ok
