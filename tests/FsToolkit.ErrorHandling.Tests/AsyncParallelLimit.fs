module AsyncParallelLimitTests

#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto
#endif

open FsToolkit.ErrorHandling

type StartCounter() =
    let sync = obj ()
    let mutable count = 0
    let mutable waiters: (int * (unit -> unit)) list = []

    member _.Count = lock sync (fun () -> count)

    member _.Increment() =
        let ready =
            lock
                sync
                (fun () ->
                    count <- count + 1

                    let ready, waiting =
                        waiters
                        |> List.partition (fun (target, _) ->
                            count
                            >= target
                        )

                    waiters <- waiting
                    ready
                )

        for _, resume in ready do
            resume ()

    member _.WaitFor(target: int) : Async<unit> =
        Async.FromContinuations(fun (resume, _, _) ->
            let reached =
                lock
                    sync
                    (fun () ->
                        if
                            count
                            >= target
                        then
                            true
                        else
                            waiters <-
                                (target, resume)
                                :: waiters

                            false
                    )

            if reached then
                resume ()
        )

type GatedComputations(size: int) =
    let sync = obj ()
    let started = StartCounter()
    let releases = ResizeArray<unit -> unit>()

    member _.Started = started

    member _.Computations: Async<int> list = [
        for i in 1..size ->
            async {
                do!
                    Async.FromContinuations(fun (resume, _, _) ->
                        lock sync (fun () -> releases.Add resume)
                        started.Increment()
                    )

                return i
            }
    ]

    member _.Release(index: int) =
        let resume = lock sync (fun () -> releases[index])
        resume ()

let parallelLimitTests =
    testList "Async.parallelLimit" [
        testCaseAsync "limit 1 starts each computation after the previous one completes"
        <| async {
            let gated = GatedComputations 4
            let! running = Async.StartChild(Async.parallelLimit 1 gated.Computations)

            for started in 1..4 do
                do! gated.Started.WaitFor started

                Expect.equal
                    gated.Started.Count
                    started
                    "Started computations before the next release"

                gated.Release(
                    started
                    - 1
                )

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

        testCaseAsync "limit 2 keeps two computations running"
        <| async {
            let gated = GatedComputations 4
            let! running = Async.StartChild(Async.parallelLimit 2 gated.Computations)

            do! gated.Started.WaitFor 2
            Expect.equal gated.Started.Count 2 "Started computations before the first release"

            for released in 0..1 do
                gated.Release released

                do!
                    gated.Started.WaitFor(
                        released
                        + 3
                    )

                Expect.equal
                    gated.Started.Count
                    (released
                     + 3)
                    "One new computation for each release"

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
    ]

let parallelDoLimitTests =
    testList "Async.parallelDoLimit" [
        testCaseAsync "limit 1 starts each computation after the previous one completes"
        <| async {
            let gated = GatedComputations 3

            let! running =
                gated.Computations
                |> List.map Async.Ignore
                |> Async.parallelDoLimit 1
                |> Async.StartChild

            for started in 1..3 do
                do! gated.Started.WaitFor started

                Expect.equal
                    gated.Started.Count
                    started
                    "Started computations before the next release"

                gated.Release(
                    started
                    - 1
                )

            do! running
        }
    ]

let allTests =
    testList "Async parallel limit" [
        parallelLimitTests
        parallelDoLimitTests
    ]
