module FsToolkit.ErrorHandling.IcedTasks.Tests.RuntimeAsync.Main

open System
open System.Threading
open System.Threading.Tasks

/// Holds every pool worker, starts a background builder from a context, cancels it while it is queued, then releases the pool.
/// The exit code is the TaskStatus times ten plus the number of body starts.
let private queuedCancellation (builder: string) =
    let start =
        IcedBuilderProbes.queued
        |> List.find (fun (name, _) -> name = builder)
        |> snd

    let workers = Environment.ProcessorCount
    let mutable minWorkers = 0
    let mutable io = 0
    ThreadPool.GetMinThreads(&minWorkers, &io)

    if
        not (ThreadPool.SetMinThreads(workers, io))
        || not (ThreadPool.SetMaxThreads(workers, io))
    then
        failwith "The probe process must limit its thread pool"

    use release = new ManualResetEventSlim(false)
    use busy = new CountdownEvent(workers)

    for _ in 1..workers do
        ThreadPool.UnsafeQueueUserWorkItem(
            (fun _ ->
                busy.Signal()
                |> ignore

                release.Wait()
            ),
            null
        )
        |> ignore

    if not (busy.Wait(RuntimeAsyncTestHelpers.deadline)) then
        failwith "The pool workers did not start"

    let started = ref 0
    use cts = new CancellationTokenSource()
    let previous = SynchronizationContext.Current
    SynchronizationContext.SetSynchronizationContext(SynchronizationContext())

    let running =
        try
            start
                cts.Token
                (fun () ->
                    Interlocked.Increment(&started.contents)
                    |> ignore
                )
        finally
            SynchronizationContext.SetSynchronizationContext previous

    cts.Cancel()
    release.Set()

    try
        running.Wait(RuntimeAsyncTestHelpers.deadline)
        |> ignore
    with _ ->
        ()

    int running.Status
    * 10
    + min started.Value 9

[<EntryPoint>]
let main argv =
    match argv with
    | [| "--queued-cancellation"; builder |] -> queuedCancellation builder
    | _ -> Expecto.Tests.runTestsInAssemblyWithCLIArgs [] argv
