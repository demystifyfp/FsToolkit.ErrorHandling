// NOTE Derived from FSharp.Core V11 source with the following alterations:
// (Task.FSharpCore11.fs is a more direct paste as there is no overlap with pre-existing functionality)
// - namespace is FsToolkit.ErrorHandling instead of Microsoft.FSharp.Control.
// - The TaskLike Await and StartTaskImmediate overloads are omitted.
// - Await is custom-written as the production implementation relies on internal FSharp.Core APIs.
//   - The Shim version has an extra throw-rethrow stacktrace frame, but is consistent wrt the other key behaviors:
//     1. Single-exception tasks are unwrapped and rethrown.
//     2. Cancellation surfaces as TaskCanceledException [and per normal async semantics is not catchable]
#if FABLE_COMPILER || !NET10_0_OR_GREATER
[<AutoOpen>]
#else
[<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
[<System.Obsolete "These shims are included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
module FsToolkit.ErrorHandling.FSharpCore11AsyncShims

open System.Threading
open System.Threading.Tasks

#if !FABLE_COMPILER // The type Augmentations on Async are all related to Task/ValueTask
#nowarn "3261" // error FS3261: Nullness warning: The types 'System.AggregateException' and 'System.AggregateException | null' do not have compatible nullability.

type Async =

    /// <summary>Creates an asynchronous computation that will wait for the given task to complete and return
    /// its result.</summary>
    ///
    /// <param name="task">The task to await.</param>
    ///
    /// <remarks>
    /// <p>Exceptions are surfaced directly: a task faulted with a single exception raises that
    /// exception; only <see cref="T:System.AggregateException"/>s carrying multiple inner exceptions are
    /// re-raised as-is. For the legacy behavior of uniformly presenting the raw underlying
    /// <see cref="T:System.AggregateException"/>, use <c>Async.AwaitTask</c>.</p>
    ///
    /// <p>If the task is canceled then <see cref="T:System.Threading.Tasks.TaskCanceledException"/> is raised.</p>
    ///
    /// <p>Note the task may be governed by a different cancellation token than the overall async computation;
    /// typically tasks should be wired to the ambient cancellation token obtained via
    /// <c>let! ct = Async.CancellationToken</c>, catching <see cref="T:System.Threading.Tasks.TaskCanceledException"/>
    /// where the overall async is started.</p>
    /// <p>For the common case where you are running a Task within an Asynchronous Computation,
    /// see <c>StartTaskImmediate</c>, which surfaces the ambient <c>CancellationToken</c>
    /// so that it can be passed to the Task being started.</p>
    /// </remarks>
    ///
    /// <category index="2">Awaiting Results</category>
    ///
    /// <example id="await-task-1">
    /// <code lang="fsharp">
    /// let t = Task.Run(fun () -> invalidOp "test"; 42)
    /// async {
    ///     try
    ///         let! _ = Async.Await t
    ///         ()
    ///     with
    ///     | :? System.InvalidOperationException as e ->
    ///         printn $"Caught: {e.Message}"
    ///     | :? System.AggregateException ->
    ///         printfn "unreachable" // will not match: single exception is unwrapped
    /// } |> Async.RunSynchronously
    /// </code>
    /// Prints <c>Caught: test</c>. The <c>AggregateException</c> branch is not reached because a
    /// single-inner exception is unwrapped. Contrast with <c>Async.AwaitTask</c>.
    /// </example>
#if NET10_0_OR_GREATER
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    [<System.Obsolete "Shim is included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
    [<System.Diagnostics.DebuggerStepThrough>]
    static member Await(task: Task<'T>) : Async<'T> =
#if NET10_0_OR_GREATER
        Microsoft.FSharp.Control.Async.Await task
#else
        async {
            try
                return! Async.AwaitTask task
            with :? System.AggregateException as e when e.InnerExceptions.Count = 1 ->
                return raise e.InnerExceptions[0]
        }
#endif

    /// <summary>Creates an asynchronous computation that will wait for the given task to complete.</summary>
    /// <param name="task">The task to await.</param>
    /// <remarks>
    /// <p>Exceptions are surfaced directly: a task faulted with a single exception raises that
    /// exception; only <see cref="T:System.AggregateException"/>s carrying multiple inner exceptions are
    /// re-raised as-is. For the legacy behavior of uniformly presenting the raw underlying
    /// <see cref="T:System.AggregateException"/>, use <c>Async.AwaitTask</c>.</p>
    ///
    /// <p>If the task is canceled then <see cref="T:System.Threading.Tasks.TaskCanceledException"/> is raised.</p>
    ///
    /// <p>Note the task may be governed by a different cancellation token than the overall async computation;
    /// typically tasks should be wired to the ambient cancellation token obtained via
    /// <c>let! ct = Async.CancellationToken</c>, catching <see cref="T:System.Threading.Tasks.TaskCanceledException"/>
    /// where the overall async is started.</p>
    /// <p>For the common case where you are running a Task within an Asynchronous Computation,
    /// see <c>StartTaskImmediate</c>, which surfaces the ambient <c>CancellationToken</c>
    /// so that it can be passed to the Task being started.</p>
    /// </remarks>
    /// <category index="2">Awaiting Results</category>
    /// <example id="await-task-2">
    /// <code lang="fsharp">
    /// let t = Task.Run(fun () -> invalidOp "test")
    /// async {
    ///     try
    ///         do! Async.Await t
    ///     with
    ///     | :? System.InvalidOperationException as e ->
    ///         printn $"Caught: {e.Message}"
    ///     | :? System.AggregateException ->
    ///         printfn "unreachable" // will not match: single exception is unwrapped
    /// } |> Async.RunSynchronously
    /// </code>
    /// Prints <c>Caught: test</c>. The <c>AggregateException</c> branch is not reached because a
    /// single-inner exception is unwrapped. Contrast with <c>Async.AwaitTask</c>.
    /// </example>
#if NET10_0_OR_GREATER
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    [<System.Obsolete "Shim is included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
    [<System.Diagnostics.DebuggerStepThrough>]
    static member Await(task: Task) : Async<unit> =
#if NET10_0_OR_GREATER
        Microsoft.FSharp.Control.Async.Await task
#else
        async {
            try
                return! Async.AwaitTask task
            with :? System.AggregateException as e when e.InnerExceptions.Count = 1 ->
                return raise e.InnerExceptions[0]
        }
#endif
#if NET10_0_OR_GREATER
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    [<System.Obsolete "Shim is included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
    static member Await(task: ValueTask) : Async<unit> =
#if NET10_0_OR_GREATER
        Microsoft.FSharp.Control.Async.Await task
#else
        Async.Await(task.AsTask())
#endif
#if NET10_0_OR_GREATER
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    [<System.Obsolete "Shim is included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
    static member Await(task: ValueTask<'t>) : Async<'t> =
#if NET10_0_OR_GREATER
        Microsoft.FSharp.Control.Async.Await task
#else
        Async.Await(task.AsTask())
#endif

#if NET10_0_OR_GREATER
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    [<System.Obsolete "Shim is included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
    static member StartTaskImmediate(createTask: CancellationToken -> Task<'T>) : Async<'T> =
#if NET10_0_OR_GREATER
        Microsoft.FSharp.Control.Async.StartTaskImmediate createTask
#else
        async.Bind(Async.CancellationToken, createTask >> Async.Await)
#endif

#if NET10_0_OR_GREATER
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    [<System.Obsolete "Shim is included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
    static member StartTaskImmediate(createTask: CancellationToken -> Task) : Async<unit> =
#if NET10_0_OR_GREATER
        Microsoft.FSharp.Control.Async.StartTaskImmediate createTask
#else
        async.Bind(Async.CancellationToken, createTask >> Async.Await)
#endif

#if NET10_0_OR_GREATER
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    [<System.Obsolete "Shim is included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
    static member StartTaskImmediate(createTask: CancellationToken -> ValueTask<'T>) : Async<'T> =
#if NET10_0_OR_GREATER
        Microsoft.FSharp.Control.Async.StartTaskImmediate createTask
#else
        async.Bind(Async.CancellationToken, createTask >> Async.Await)
#endif

#if NET10_0_OR_GREATER
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    [<System.Obsolete "Shim is included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
    static member StartTaskImmediate(createTask: CancellationToken -> ValueTask) : Async<unit> =
#if NET10_0_OR_GREATER
        Microsoft.FSharp.Control.Async.StartTaskImmediate createTask
#else
        async.Bind(Async.CancellationToken, createTask >> Async.Await)
#endif

#endif

/// <summary><p>Contains camelCase module-level functions for <see cref="T:Async`1"/> computations.</p>
/// <p>NOTE these functions duplicate those available in FSharp.Core >= 11. <code>net10</code> and later TFM builds forward to the intrinsics.</p>
/// </summary>
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
#if NET10_0_OR_GREATER
[<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
[<System.Obsolete "Shims are included solely to support inline methods compiled against FsToolkit.ErrorHandling builds for TFMs before net10.">]
#endif
module Async =

    /// <summary>Creates an asynchronous computation that returns the given value.</summary>
    /// <param name="value">The value to return.</param>
    /// <returns>An asynchronous computation that returns <c>value</c> when executed.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let computation = Async.result 42
    /// computation |> Async.RunSynchronouslyImmediate // evaluates to 42
    /// </code>
    /// </example>
    let inline result (value: 'T) : Async<'T> =
#if NET10_0_OR_GREATER
        Async.result value
#else
        async.Return value
#endif

    /// <summary>Creates an asynchronous computation that applies the mapping function to the result of the given computation.</summary>
    /// <param name="mapping">The function to apply to the result.</param>
    /// <param name="computation">The input computation.</param>
    /// <returns>An asynchronous computation that applies <c>mapping</c> to the result of <c>computation</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let computation = Async.result 21 |> Async.map (fun x -> x * 2)
    /// computation |> Async.RunSynchronouslyImmediate // evaluates to 42
    /// </code>
    /// </example>
    let inline map ([<InlineIfLambda>] mapping: 'T -> 'U) (computation: Async<'T>) : Async<'U> =
#if NET10_0_OR_GREATER
        Async.map mapping computation
#else
        async.Bind(computation, mapping >> async.Return)
#endif

    /// <summary>Creates an asynchronous computation that passes the result of the given computation to the binder function.</summary>
    /// <param name="binder">A function that takes the result of the computation and returns a new asynchronous computation.</param>
    /// <param name="computation">The input computation.</param>
    /// <returns>An asynchronous computation that performs a monadic bind on the result of <c>computation</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let computation = Async.result 21 |> Async.bind (fun x -> Async.result (x * 2))
    /// computation |> Async.RunSynchronouslyImmediate // evaluates to 42
    /// </code>
    /// </example>
    let inline bind
        ([<InlineIfLambda>] binder: 'T -> Async<'U>)
        (computation: Async<'T>)
        : Async<'U> =
#if NET10_0_OR_GREATER
        Async.bind binder computation
#else
        async.Bind(computation, binder)
#endif

    /// <summary>Creates an asynchronous computation that runs the given computation and ignores its result.</summary>
    /// <param name="computation">The input computation.</param>
    /// <returns>A computation that is equivalent to the input computation, but disregards the result.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let readFile filename numBytes: Async&lt;unit&gt; =
    ///     async {
    ///         use file = System.IO.File.OpenRead(filename)
    ///         do! file.AsyncRead(numBytes) |> Async.ignore&lt;byte[]&gt;
    ///     }
    /// </code>
    /// </example>
    /// <example id="async-ignore-2">
    /// <code lang="fsharp">
    /// let computation : Async&lt;unit&gt; = Async.result 42 |> Async.ignore&lt;int&gt;
    /// computation |> Async.RunSynchronously // evaluates to ()
    /// </code>
    /// </example>
    [<RequiresExplicitTypeArguments>]
    let inline ignore<'T> (computation: Async<'T>) : Async<unit> =
#if NET10_0_OR_GREATER
        Async.ignore<'T> computation
#else
        Async.Ignore computation
#endif

    /// <summary>Creates an asynchronous computation that yields the original result on success, or the result of
    /// <c>handler exn</c> for non-cancellation exceptions.</summary>
    /// <remarks><c>OperationCanceledException</c> and derived types such as <c>TaskCanceledException</c> propagate unchanged,
    /// and therefore are never passed to <c>handler</c>.
    /// </remarks>
    /// <param name="handler">A function to handle (non-cancellation) exceptions, yielding a recovery value based on the exception.
    /// Any exception thrown by <c>handler</c> will propagate.</param>
    /// <param name="computation">The input computation.</param>
    /// <returns>An asynchronous computation that yields the result of <c>computation</c> on success,
    /// or <c>handler exn</c> on failure.
    /// Propagates the underlying cancellation exception where cancellation occurs.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let safeDiv x y =
    ///     async { return x / y }
    ///     |> Async.catchWith (fun _ -> 0)
    /// safeDiv 10 0 |> Async.RunSynchronouslyImmediate // evaluates to 0
    /// </code>
    /// </example>
    let catchWith (handler: exn -> 'T) (computation: Async<'T>) : Async<'T> =
#if NET10_0_OR_GREATER
        Async.catchWith handler computation
#else
        async {
            try
                return! computation
            with e ->
                return handler e
        }
#endif

    /// <summary>Creates an asynchronous computation that reifies the outcome of the given <c>computation</c> as a <c>Result</c>:
    /// <c>Ok</c> on success, <c>Error</c> on failure, so exceptions become values. Cancellation still propagates.</summary>
    /// <remarks><c>OperationCanceledException</c> and derived types such as <c>TaskCanceledException</c> propagate unchanged.</remarks>
    /// <param name="computation">The input computation.</param>
    /// <returns>An asynchronous computation that yields a <c>Result</c>: <c>Ok</c> with the outcome on success,
    /// or <c>Error</c> with the exception on failure.
    /// Propagates the underlying cancellation exception when cancellation occurs.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let safeDiv x y =
    ///     async { return x / y } |> Async.catch
    /// safeDiv 10 2 |> Async.RunSynchronouslyImmediate // evaluates to Ok 5
    /// safeDiv 10 0 |> Async.RunSynchronouslyImmediate // evaluates to Error (DivideByZeroException ...)
    /// </code>
    /// </example>
    let catch (computation: Async<'T>) : Async<Result<'T, exn>> =
#if NET10_0_OR_GREATER
        Async.catch computation
#else
        async {
            try
                let! v = computation
                return Result.Ok v
            with e ->
                return Result.Error e
        }
#endif

    /// <summary>An asynchronous computation that returns <c>unit</c>. This is equivalent to <c>async.Zero()</c>.</summary>
    /// <example>
    /// <code lang="fsharp">
    /// Async.empty |> Async.RunSynchronouslyImmediate // evaluates to ()
    /// </code>
    /// </example>
    let empty: Async<unit> =
#if NET10_0_OR_GREATER
        Async.empty
#else
        async.Zero()
#endif

    /// <summary>Creates an asynchronous computation that executes each of the <c>computations</c> in sequence, returning <c>unit</c>.</summary>
    /// <param name="computations">A sequence of unit computations to be executed in sequence.</param>
    /// <returns>A computation that runs all inputs in sequence and returns <c>unit</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// // NOTE numbers are guaranteed to be printed in order 1..10
    /// seq { for i in 1..10 -> async { printfn "%d" i } }
    /// |> Async.sequentialDo
    /// |> Async.RunSynchronouslyImmediate
    /// </code>
    /// </example>
    let sequentialDo (computations: seq<Async<unit>>) : Async<unit> =
#if NET10_0_OR_GREATER
        Async.sequentialDo computations
#else
        Async.Sequential computations
        |> ignore<unit[]>
#endif

    /// <summary>Creates an asynchronous computation that executes all the supplied asynchronous computations
    /// with concurrency limited to at most <c>maxDegreeOfParallelism</c>,
    /// and returns their results as an array in the same order as the inputs.</summary>
    /// <remarks>While the result order matches the input order, the relative start and completion order of computations is arbitrary.</remarks>
    /// <param name="maxDegreeOfParallelism">The maximum number of computations to run concurrently. Must be &gt; 0.</param>
    /// <param name="computations">A sequence of computations to be parallelized.</param>
    /// <returns>A computation that returns an array of results from the input computations in the same order they were supplied.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// let results =
    ///     seq { for i in 1..10 -> async { return i * i } }
    ///     |> Async.parallelLimit 3
    ///     |> Async.RunSynchronouslyImmediate
    /// results // evaluates to [| 1; 4; 9; 16; 25; 36; 49; 64; 81; 100 |]
    /// </code>
    /// </example>
    let parallelLimit (maxDegreeOfParallelism: int) (computations: seq<Async<'T>>) : Async<'T[]> =
#if NET10_0_OR_GREATER
        Async.parallelLimit maxDegreeOfParallelism computations
#else
        Async.Parallel(computations, maxDegreeOfParallelism = maxDegreeOfParallelism)
#endif

    /// <summary>Creates an asynchronous computation that executes all the supplied asynchronous computations returning unit,
    /// with concurrency limited to at most <c>maxDegreeOfParallelism</c>.</summary>
    /// <remarks>The relative start and completion order of computations is arbitrary.</remarks>
    /// <param name="maxDegreeOfParallelism">The maximum number of computations to run concurrently. Must be &gt; 0.</param>
    /// <param name="computations">A sequence of unit computations to be parallelized.</param>
    /// <returns>A computation that runs all inputs with limited parallelism and returns <c>unit</c>.</returns>
    /// <example>
    /// <code lang="fsharp">
    /// seq { for i in 1..10 -> async { printfn "%d" i } } // NOTE output order can vary
    /// |> Async.parallelDoLimit 3
    /// |> Async.RunSynchronouslyImmediate
    /// </code>
    /// </example>
    let parallelDoLimit
        (maxDegreeOfParallelism: int)
        (computations: seq<Async<unit>>)
        : Async<unit> =
#if NET10_0_OR_GREATER
        Async.parallelDoLimit maxDegreeOfParallelism computations
#else
        parallelLimit maxDegreeOfParallelism computations
        |> ignore<unit[]>
#endif
