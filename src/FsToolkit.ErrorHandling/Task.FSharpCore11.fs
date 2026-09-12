// NOTE Direct copy of FSharp.Core V11 with the following alterations:
// - namespace is FsToolkit.ErrorHandling instead of Microsoft.FSharp.Control
#if !NET_10_0_OR_GREATER
[<AutoOpen>]
module FsToolkit.ErrorHandling.FSharpCore11TaskShims

open FSharp.Core.CompilerServices
open System.Threading
open System.Threading.Tasks

/// <summary><p>Contains camelCase module-level functions for <see cref="T:System.Threading.Tasks.Task`1"/> computations.</p>
/// <p>NOTE these functions duplicate those available in FSharp.Core >= 11. <code>net10</code> and later TFM builds omit these shims.</p>
/// </summary>
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Task =

    /// <summary>Creates a task that returns the given value.</summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A completed task that returns <c>value</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let t = Task.result 42
    /// t.Result // evaluates to 42
    /// </code>
    /// </example>
    let inline result (value: 'T) : Task<'T> = Task.FromResult value

    /// <summary>A completed task that returns <c>unit</c>. This is a <c>Task&lt;unit&gt;</c> (not the non-generic <c>Task.CompletedTask</c>).</summary>
    /// <example>
    /// <code lang="fsharp">
    /// Task.empty.Result // evaluates to ()
    /// </code>
    /// </example>
    let empty: Task<unit> = result ()

    /// <summary>Creates a task that passes the result of the given task to the binder function.</summary>
    /// <param name="binder">A function that takes the result of the task and returns a new task.</param>
    /// <param name="task">The input task.</param>
    /// <returns>A task that performs a monadic bind on the result of <c>task</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let t = Task.result 21 |> Task.bind (fun x -> Task.result (x * 2))
    /// t.Result // evaluates to 42
    /// </code>
    /// </example>
    let inline bind ([<InlineIfLambda>] binder: 'T -> Task<'U>) (task: Task<'T>) : Task<'U> =
        if task.Status = TaskStatus.RanToCompletion then
            try
                binder task.Result
            with e ->
                Task.FromException<'U>(e)
        else
            TaskBuilder.task {
                let! v = task
                return! binder v
            }

    /// <summary>Creates a task that applies the mapping function to the result of the given task.</summary>
    /// <param name="mapping">The function to apply to the result.</param>
    /// <param name="task">The input task.</param>
    /// <returns>A task that applies <c>mapping</c> to the result of <c>task</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let t = Task.result 21 |> Task.map (fun x -> x * 2)
    /// t.Result // evaluates to 42
    /// </code>
    /// </example>
    let inline map ([<InlineIfLambda>] mapping: 'T -> 'U) (task: Task<'T>) : Task<'U> =
        if task.Status = TaskStatus.RanToCompletion then
            try
                mapping task.Result
                |> result
            with e ->
                Task.FromException<'U>(e)
        else
            TaskBuilder.task {
                let! v = task
                return mapping v
            }

    /// <summary>Creates a task that runs the given task and ignores its result.</summary>
    /// <param name="task">The input task.</param>
    /// <returns>A task that is equivalent to the input task, but disregards the result.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let t : Task&lt;unit&gt; = Task.result 42 |> Task.ignore&lt;int&gt;
    /// t.Result // evaluates to ()
    /// </code>
    /// </example>
    [<RequiresExplicitTypeArguments>]
    let inline ignore<'T> (task: Task<'T>) : Task<unit> =
        if task.Status = TaskStatus.RanToCompletion then
            empty
        else
            map ignore task

    /// <summary>Creates a <c>Task</c> that yields the original result on success, or the result of
    /// <c>handler exn</c> for non-cancellation exceptions.</summary>
    /// <remarks><c>OperationCanceledException</c> and derived types such as <c>TaskCanceledException</c> propagate unchanged
    /// (and the task remains Canceled) in order to maintain cancellation semantics, and therefore are never passed to <c>handler</c>.
    /// </remarks>
    /// <param name="handler">A function to handle (non-cancellation) exceptions, yielding a recovery value based on the exception.
    /// Any exception thrown by <c>handler</c> will propagate.</param>
    /// <param name="task">The input <c>Task</c>.</param>
    /// <returns>A <c>Task</c> that yields the result of <c>task</c> on success, or <c>handler exn</c> on failure.
    /// Propagates the underlying cancellation exception when <c>task</c> is canceled.</returns>
    /// <example id="task-catchwith-1">
    /// <code lang="fsharp">
    /// let safeDiv x y =
    ///     task { return x / y }
    ///     |> Task.catchWith (fun _ -> 0)
    /// (safeDiv 10 0).Result // evaluates to 0
    /// </code>
    /// </example>
    let inline catchWith ([<InlineIfLambda>] handler: exn -> 'T) (task: Task<'T>) : Task<'T> =
        if task.Status = TaskStatus.RanToCompletion then
            task
        else
            TaskBuilder.task {
                try
                    return! task
                with
                | :? System.OperationCanceledException as e -> return! raise e
                | e -> return handler e
            }

    /// <summary>Creates a <c>Task</c> that reifies the outcome of the given <c>Task</c> as a <c>Result</c>:
    /// <c>Ok</c> on success, <c>Error</c> on failure, so faults become values. Cancellation still propagates.</summary>
    /// <remarks><c>OperationCanceledException</c> and derived types such as <c>TaskCanceledException</c> propagate unchanged
    /// (and the task remains Canceled) in order to maintain cancellation semantics.</remarks>
    /// <param name="task">The input <c>Task</c>.</param>
    /// <returns>A <c>Task</c> that yields a <c>Result</c>: <c>Ok</c> with the outcome on success,
    /// or <c>Error</c> with the exception on failure.
    /// Propagates the underlying cancellation exception when <c>task</c> is canceled.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let safeDiv x y = task { return x / y } |> Task.catch
    /// (safeDiv 10 2).Result // evaluates to Ok 5
    /// (safeDiv 10 0).Result // evaluates to Error (DivideByZeroException ...)
    /// </code>
    /// </example>
    let catch (task: Task<'T>) : Task<Result<'T, exn>> =
        task
        |> map Ok
        |> catchWith Error

    /// <summary>Creates a task that executes each of the <c>computations</c> in sequence,
    /// returning an array of their results in order of the input sequence.</summary>
    /// <param name="ct">A cancellation token to pass to each task factory.</param>
    /// <param name="computations">A sequence of task start functions accepting a <see cref="T:System.Threading.CancellationToken"/>.</param>
    /// <returns>A task yielding an array of the results of <c>computations</c> in the order they were supplied.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// task {
    ///     return!
    ///         seq { for i in 1..10 -> fun _ct -> Task.result (i * i) }
    ///         |> Task.sequential CancellationToken.None
    /// } // returns [| 1; 4; 9; 16; 25; 36; 49; 64; 81; 100 |]
    /// </code>
    /// </example>
    let sequential
        (ct: CancellationToken)
        (computations: seq<CancellationToken -> Task<'T>>)
        : Task<'T[]> =
        task {
            let mutable results = ArrayCollector<'T>()

            for f in computations do
                let! result = f ct
                results.Add result

            return results.Close()
        }

    /// <summary>Creates a task that executes each of the <c>computations</c> in sequence, returning <c>unit</c>.</summary>
    /// <param name="ct">A cancellation token to pass to each task factory.</param>
    /// <param name="computations">A sequence of unit task start functions accepting a <see cref="T:System.Threading.CancellationToken"/>.</param>
    /// <returns>A task that runs all inputs in sequence and returns <c>unit</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// task {
    ///     return!
    ///         seq { for i in 1..10 -> fun _ct -> task { printfn "%d" i } }
    ///         // NOTE numbers are guaranteed to be printed in order 1..10
    ///         |> Task.sequentialDo CancellationToken.None
    /// }
    /// </code>
    /// </example>
    let sequentialDo
        (ct: CancellationToken)
        (computations: seq<CancellationToken -> Task<unit>>)
        : Task<unit> =
        task {
            for f in computations do
                do! f ct
        }

    /// <summary>Creates a task that executes each of the <c>computations</c> in parallel with concurrency limited
    /// to at most <c>maxDegreeOfParallelism</c>, returning an array of their results in order of the input sequence.</summary>
    /// <remarks>
    /// <p>The relative start and completion order per computation is arbitrary.</p>
    /// <p>If any of the computations Fault, the governing CancellationToken of its siblings will be Canceled.</p>
    /// <p>Where multiple computations Fault, a single exception is propagated.</p>
    /// </remarks>
    /// <param name="maxDegreeOfParallelism">The maximum number of tasks to run concurrently. Must be &gt; 0.</param>
    /// <param name="ct">An outer cancellation token used to cancel the parallel request.
    /// When multiple tasks can run concurrently, task factories receive a linked token that is also canceled if a sibling faults;
    /// otherwise they receive <c>ct</c> directly.</param>
    /// <param name="computations">A sequence of task start functions accepting a <see cref="T:System.Threading.CancellationToken"/>.</param>
    /// <returns>A task yielding an array of the results of <c>computations</c> in the order they were supplied.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// task {
    ///     return!
    ///         seq { for i in 1..10 -> fun _ct -> Task.result (i * i) }
    ///         |> Task.parallelLimit 3 CancellationToken.None
    /// } // returns [| 1; 4; 9; 16; 25; 36; 49; 64; 81; 100 |]
    /// </code>
    /// </example>
    let parallelLimit
        (maxDegreeOfParallelism: int)
        (ct: CancellationToken)
        (computations: seq<CancellationToken -> Task<'T>>)
        : Task<'T[]> =
        if maxDegreeOfParallelism < 1 then
            invalidArg
                (nameof maxDegreeOfParallelism)
                "maxDegreeOfParallelism must be greater than 0"
        // materialize first so exceptions from enumeration can't trigger ObjectDisposedException
        // from started children touching semaphore or innerCts
        match Seq.toArray computations with
        | _ when ct.IsCancellationRequested -> Task.FromCanceled<'T[]> ct
        | [||] -> result [||]
        | req when
            maxDegreeOfParallelism = 1
            || req.Length = 1
            ->
            sequential ct req
        | req ->
            task {
                let mutable pos = -1
                let res = Array.zeroCreate<'T> req.Length

                use innerCts = CancellationTokenSource.CreateLinkedTokenSource ct

                let worker () =
                    backgroundTask {
                        let mutable index = Interlocked.Increment &pos

                        while index < req.Length
                              && not innerCts.IsCancellationRequested do
                            let mutable completed = false

                            try
                                let! r = req[index] innerCts.Token
                                completed <- true
                                res[index] <- r
                            finally
                                if not completed then
                                    innerCts.Cancel()

                            index <- Interlocked.Increment &pos
                    }

                // Awaits completion of all workers (whether through success, cancellation or faulting)
                do!
                    Task.WhenAll [|
                        for _ in 1 .. min req.Length maxDegreeOfParallelism -> worker () :> Task
                    |]
                // Where cancellation was requested on the outer ct, but none of the inners saw and/or honored it by throwing TCE,
                // res may only be partially complete so we certainly can't return it
                // we instead yield a TaskCanceledException to honor standard Task Cancellation semantics
                innerCts.Token.ThrowIfCancellationRequested()
                return res
            }

    /// <summary>Creates a task that executes the <c>computations</c> in parallel,
    /// with concurrency limited to at most <c>maxDegreeOfParallelism</c>.</summary>
    /// <remarks>
    /// <p>The relative start and completion order per computation is arbitrary.</p>
    /// <p>If any of the computations Fault, the governing CancellationToken of its siblings will be Canceled.</p>
    /// <p>Where multiple computations Fault, a single exception is propagated.</p>
    /// </remarks>
    /// <param name="maxDegreeOfParallelism">The maximum number of tasks to run concurrently. Must be &gt; 0.</param>
    /// <param name="ct">An outer cancellation token used to cancel the parallel request.
    /// When multiple tasks can run concurrently, task factories receive a linked token that is also canceled if a sibling faults;
    /// otherwise they receive <c>ct</c> directly.</param>
    /// <param name="computations">A sequence of unit task start functions accepting a <see cref="T:System.Threading.CancellationToken"/>.</param>
    /// <returns>A task that runs all inputs with the specified parallelism limit and returns <c>unit</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// task {
    ///     return!
    ///         seq { for i in 1..10 -> fun _ct -> task { printfn "%d" i } } // NOTE output order can vary
    ///         |> Task.parallelDoLimit 3 CancellationToken.None
    /// }
    /// </code>
    /// </example>
    let parallelDoLimit
        (maxDegreeOfParallelism: int)
        (ct: CancellationToken)
        (computations: seq<CancellationToken -> Task<unit>>)
        : Task<unit> =
        parallelLimit maxDegreeOfParallelism ct computations
        |> ignore<unit[]>

    /// <summary>Starts the <c>computation</c> on the current thread, returning a <see cref="T:System.Threading.Tasks.Task`1"/>
    /// that represents its result.</summary>
    /// <remarks>The computation begins executing synchronously on the calling thread, offloading only at the point
    /// where it first suspends (mirroring <see cref="M:Microsoft.FSharp.Control.FSharpAsync.StartImmediateAsTask``1(Microsoft.FSharp.Control.FSharpAsync{``0},Microsoft.FSharp.Core.FSharpOption{System.Threading.CancellationToken})"/>).
    /// <c>ct</c> flows into the computation, so cancelling it cancels <c>computation</c>, and the resulting task observes
    /// that cancellation.</remarks>
    /// <param name="ct">A cancellation token to use for <c>computation</c>.</param>
    /// <param name="computation">The async computation to start.</param>
    /// <returns>A task representing the result of <c>computation</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// use cts = new CancellationTokenSource()
    /// task {
    ///     return!
    ///         async { return 42 }
    ///         |> Task.startAsyncImmediate cts.Token
    /// } // returns 42
    /// </code>
    /// </example>
    let startAsyncImmediate (ct: CancellationToken) (computation: Async<'T>) : Task<'T> =
        Async.StartImmediateAsTask(computation, cancellationToken = ct)

#if NETSTANDARD2_1
    /// <summary>Converts a <see cref="T:System.Threading.Tasks.ValueTask`1"/> to a <see cref="T:System.Threading.Tasks.Task`1"/>.</summary>
    /// <param name="valueTask">The input value task.</param>
    /// <returns>A task equivalent to the given value task.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let vt = ValueTask&lt;int&gt;(42)
    /// let t = Task.ofValueTask vt
    /// t.Result // evaluates to 42
    /// </code>
    /// </example>
    let inline ofValueTask (valueTask: ValueTask<'T>) : Task<'T> = valueTask.AsTask()
#endif
#endif
