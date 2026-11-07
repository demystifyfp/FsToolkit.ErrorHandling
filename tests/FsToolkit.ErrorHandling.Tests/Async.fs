module AsyncTests

open Expecto
open FsToolkit.ErrorHandling
open System
open System.Collections.Concurrent
open System.Threading
open System.Threading.Tasks

let private canceledTask<'T> () =
    Task.FromCanceled<'T>(CancellationToken(true))

let private runOnSingleThreadContext (computation: Async<unit>) =
    let queue = new BlockingCollection<unit -> unit>()

    let context =
        { new SynchronizationContext() with
            member this.Post(callback, state) =
                queue.Add(fun () -> callback.Invoke state)

            member this.CreateCopy() = this
        }

    let finished = new ManualResetEventSlim(false)

    let pump =
        Thread(fun () ->
            SynchronizationContext.SetSynchronizationContext context

            Async.StartImmediate(
                async {
                    try
                        do! computation
                    finally
                        finished.Set()
                }
            )

            let mutable work = ignore

            while not finished.IsSet do
                if queue.TryTake(&work, 50) then
                    work ()
        )

    pump.IsBackground <- true
    pump.Start()

    if not (pump.Join(TimeSpan.FromSeconds 10.)) then
        failwith "The single-thread context did not finish within 10 seconds"

    pump.ManagedThreadId

let asyncAwaitTests =
    testList "Async.Await" [
        testCase "resumes on the caller's SynchronizationContext"
        <| fun _ ->
            let resumedOn = ref -1

            let pumpThreadId =
                runOnSingleThreadContext (
                    async {
                        let! _ = Async.Await(Task.Delay(20).ContinueWith(fun (_: Task) -> 1))
                        resumedOn.Value <- Thread.CurrentThread.ManagedThreadId
                    }
                )

            Expect.equal
                resumedOn.Value
                pumpThreadId
                "Async.Await must resume on the thread of the captured SynchronizationContext"
    ]
