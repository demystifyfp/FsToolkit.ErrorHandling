module FSharpCore11ShimBehaviorTests

open System
open System.Threading
open System.Threading.Tasks
open Expecto
open FsToolkit.ErrorHandling

let private signal () =
    TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

let private faulted (exns: exn list) =
    let source = TaskCompletionSource<int>()
    source.SetException exns
    source.Task

let private canceled () =
    Task.FromCanceled<int>(CancellationToken(true))

let private settle (task: Task) = task.ContinueWith(fun (_: Task) -> ())

// FSharp.Core 11.0.101 lacks dotnet/fsharp#20695: https://github.com/demystifyfp/FsToolkit.ErrorHandling/issues/384
let private canceledTokenTest name test =
#if NET11_0_OR_GREATER
    ptestCaseTask $"{name} (pending #384)" test
#else
    testCaseTask name test
#endif

type private GatedTasks(size: int) =
    let started = Array.init size (fun _ -> signal ())
    let released = Array.init size (fun _ -> signal ())

    member _.Started(index: int) = started[index].Task

    member _.Release(index: int) = released[index].SetResult()

    member _.Factories: (CancellationToken -> Task<int>) list = [
        for index in 0 .. size - 1 ->
            fun (_: CancellationToken) ->
                task {
                    started[index].SetResult()
                    do! released[index].Task
                    return index + 1
                }
    ]

let awaitTests =
    testList "Async.Await" [
        testCaseAsync "a task with one fault raises the inner exception"
        <| async {
            let! result =
                faulted [ InvalidOperationException "boom" ]
                |> Async.Await
                |> Async.Catch

            match result with
            | Choice2Of2(:? InvalidOperationException) -> ()
            | other -> failtestf "Expected InvalidOperationException, got %A" other
        }

        testCaseAsync "a task with two faults keeps the AggregateException"
        <| async {
            let! result =
                faulted [
                    InvalidOperationException "first"
                    ArgumentException "second"
                ]
                |> Async.Await
                |> Async.Catch

            match result with
            | Choice2Of2(:? AggregateException as e) ->
                Expect.equal e.InnerExceptions.Count 2 "Both faults are kept"
            | other -> failtestf "Expected AggregateException, got %A" other
        }
    ]

let taskCatchTests =
    testList "Task.catch and Task.catchWith" [
        testCaseTask "catch returns Ok for a successful task"
        <| fun () ->
            task {
                let! result = Task.catch (Task.FromResult 42)
                Expect.equal result (Ok 42) "Successful task"
            }

        testCaseTask "catch returns the inner exception for a faulted task"
        <| fun () ->
            task {
                let! result = Task.catch (faulted [ InvalidOperationException "boom" ])

                match result with
                | Error(:? InvalidOperationException) -> ()
                | other -> failtestf "Expected Error InvalidOperationException, got %A" other
            }

        testCaseTask "catch leaves a canceled task canceled"
        <| fun () ->
            task {
                let caught = Task.catch (canceled ())
                do! settle caught
                Expect.isTrue caught.IsCanceled "Canceled input stays canceled"
            }

        testCaseTask "catchWith returns the handler value for a faulted task"
        <| fun () ->
            task {
                let! result =
                    faulted [ InvalidOperationException "boom" ]
                    |> Task.catchWith (fun e -> e.Message.Length)

                Expect.equal result 4 "Handler value"
            }

        testCaseTask "catchWith leaves a canceled task canceled"
        <| fun () ->
            task {
                let caught =
                    canceled ()
                    |> Task.catchWith (fun _ -> -1)

                do! settle caught
                Expect.isTrue caught.IsCanceled "Canceled input stays canceled"
            }
    ]

let asyncCatchTests =
    testList "Async.catch and Async.catchWith" [
        testCaseAsync "catch returns Ok for a successful computation"
        <| async {
            let! result = Async.catch (async { return 42 })
            Expect.equal result (Ok 42) "Successful computation"
        }

        testCaseAsync "catch returns Error for a raised exception"
        <| async {
            let! result = Async.catch (async { return raise (InvalidOperationException "boom") })

            match result with
            | Error(:? InvalidOperationException) -> ()
            | other -> failtestf "Expected Error InvalidOperationException, got %A" other
        }

        testCaseAsync "catchWith returns the handler value for a raised exception"
        <| async {
            let! result =
                async { return raise (InvalidOperationException "boom") }
                |> Async.catchWith (fun e -> e.Message.Length)

            Expect.equal result 4 "Handler value"
        }
    ]

let taskSequentialTests =
    testList "Task.sequential and Task.sequentialDo" [
        testCaseTask "sequential runs each factory after the previous one completes"
        <| fun () ->
            task {
                let gated = GatedTasks 3
                let running = Task.sequential CancellationToken.None gated.Factories

                for index in 0..2 do
                    do! gated.Started index

                    if index < 2 then
                        Expect.isFalse
                            (gated.Started(index + 1)).IsCompleted
                            "Next factory waits for the current one"

                    gated.Release index

                let! results = running

                Expect.equal
                    results
                    [|
                        1
                        2
                        3
                    |]
                    "Results keep the input order"
            }

        testCaseTask "sequentialDo runs each factory after the previous one completes"
        <| fun () ->
            task {
                let gated = GatedTasks 2

                let running =
                    gated.Factories
                    |> List.map (fun factory ct -> task { let! _ = factory ct in return () })
                    |> Task.sequentialDo CancellationToken.None

                do! gated.Started 0
                Expect.isFalse (gated.Started 1).IsCompleted "Second factory waits for the first"
                gated.Release 0
                do! gated.Started 1
                gated.Release 1
                do! running
            }

        canceledTokenTest "sequential with a canceled token calls no factory"
        <| fun () ->
            task {
                let calls = ref 0

                let running =
                    Task.sequential (CancellationToken(true)) [
                        fun _ ->
                            calls.Value <-
                                calls.Value
                                + 1

                            Task.FromResult 1
                    ]

                do! settle running
                Expect.isTrue running.IsCanceled "Canceled token cancels the task"
                Expect.equal calls.Value 0 "No factory runs"
            }

        canceledTokenTest "sequentialDo with a canceled token calls no factory"
        <| fun () ->
            task {
                let calls = ref 0

                let running =
                    Task.sequentialDo (CancellationToken(true)) [
                        fun _ ->
                            calls.Value <-
                                calls.Value
                                + 1

                            Task.FromResult()
                    ]

                do! settle running
                Expect.isTrue running.IsCanceled "Canceled token cancels the task"
                Expect.equal calls.Value 0 "No factory runs"
            }
    ]

let taskParallelLimitTests =
    testList "Task.parallelLimit and Task.parallelDoLimit" [
        testCaseTask "parallelLimit 2 keeps two factories running"
        <| fun () ->
            task {
                let gated = GatedTasks 4
                let running = Task.parallelLimit 2 CancellationToken.None gated.Factories

                do! gated.Started 0
                do! gated.Started 1
                Expect.isFalse (gated.Started 2).IsCompleted "Third factory waits for a free slot"
                gated.Release 0
                do! gated.Started 2
                Expect.isFalse (gated.Started 3).IsCompleted "Fourth factory waits for a free slot"
                gated.Release 1
                do! gated.Started 3
                gated.Release 2
                gated.Release 3

                let! results = running

                Expect.equal
                    results
                    [|
                        1
                        2
                        3
                        4
                    |]
                    "Results keep the input order"
            }

        testCaseTask "parallelDoLimit 1 runs one factory at a time"
        <| fun () ->
            task {
                let gated = GatedTasks 2

                let running =
                    gated.Factories
                    |> List.map (fun factory ct -> task { let! _ = factory ct in return () })
                    |> Task.parallelDoLimit 1 CancellationToken.None

                do! gated.Started 0
                Expect.isFalse (gated.Started 1).IsCompleted "Second factory waits for the first"
                gated.Release 0
                do! gated.Started 1
                gated.Release 1
                do! running
            }
    ]

let taskMapTests =
    testList "Task.map with a mapper that throws OperationCanceledException" [
        testCaseTask "a completed input gives a faulted task"
        <| fun () ->
            task {
                let mapped =
                    Task.FromResult 1
                    |> Task.map (fun (_: int) -> raise (OperationCanceledException()))

                do! settle mapped
                Expect.isTrue mapped.IsFaulted "Completed input: the mapper runs at once"
            }

        testCaseTask "a pending input gives a canceled task"
        <| fun () ->
            task {
                let input =
                    TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously)

                let mapped =
                    input.Task
                    |> Task.map (fun (_: int) -> raise (OperationCanceledException()))

                input.SetResult 1
                do! settle mapped
                Expect.isTrue mapped.IsCanceled "Pending input: the mapper runs in a continuation"
            }
    ]

let allTests =
    testList "FSharp.Core 11 shim behavior" [
        awaitTests
        taskCatchTests
        asyncCatchTests
        taskSequentialTests
        taskParallelLimitTests
        taskMapTests
    ]
