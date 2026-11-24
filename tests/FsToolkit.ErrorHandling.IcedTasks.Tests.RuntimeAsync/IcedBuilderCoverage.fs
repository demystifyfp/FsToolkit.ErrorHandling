module IcedBuilderCoverage

open System
open System.Threading
open System.Threading.Channels
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask
open Expecto
open RuntimeAsyncTestHelpers
open BuilderCoverage

let private expectCanceled (running: Task) (token: CancellationToken) =
    task {
        let! observed =
            observe (running.ContinueWith(Func<Task, unit>(fun t -> t.GetAwaiter().GetResult())))

        Expect.equal running.Status TaskStatus.Canceled "The task status"

        match observed with
        | Choice2Of2(:? OperationCanceledException as e) ->
            Expect.equal e.CancellationToken token "The cancellation must carry the caller token"
            return e
        | other -> return failtestf "Expected an OperationCanceledException, was %A" other
    }

let private cancellationTests =
    testList "Cancellation" [
        for probe in IcedBuilderProbes.cancellation do
            testCaseTask $"%s{probe.Builder} with a canceled token does not start the body"
            <| fun () ->
                task {
                    use cts = new CancellationTokenSource()
                    cts.Cancel()
                    let reached = ref false
                    let calls = ref 0
                    let gate = newGate ()
                    gate.SetResult(())
                    let running = probe.Start cts.Token reached gate.Task calls
                    let! canceled = expectCanceled running cts.Token

                    Expect.equal
                        (canceled.GetType())
                        typeof<TaskCanceledException>
                        "A canceled token at the invocation gives a canceled task, as in the legacy builder"

                    Expect.isFalse reached.Value "The body must not run"
                    Expect.equal calls.Value 0 "No cold source may start"
                }

            testCaseTask
                $"%s{probe.Builder} canceled during a suspension does not start the next cold source"
            <| fun () ->
                task {
                    use cts = new CancellationTokenSource()
                    let reached = ref false
                    let calls = ref 0
                    let gate = newGate ()
                    let running = probe.Start cts.Token reached gate.Task calls
                    Expect.isTrue reached.Value "The body must start on the call"
                    Expect.isFalse running.IsCompleted "The body must wait for the gate"
                    cts.Cancel()
                    gate.SetResult(())
                    let! _ = expectCanceled running cts.Token

                    Expect.equal
                        calls.Value
                        0
                        "The cold source after the cancellation must not start"
                }

            testCaseTask $"%s{probe.Builder} starts each cold source one time"
            <| fun () ->
                task {
                    use cts = new CancellationTokenSource()
                    let reached = ref false
                    let calls = ref 0
                    let gate = newGate ()
                    let running = probe.Start cts.Token reached gate.Task calls
                    gate.SetResult(())
                    do! running.WaitAsync(deadline)
                    Expect.equal calls.Value 1 "The cold source must start one time"
                }
    ]

let private coldSourceCancellation name (probes: IcedBuilderProbes.CancelProbe list) =
    testList $"Cancellation before %s{name}" [
        for probe in probes do
            testCaseTask
                $"%s{probe.Builder} canceled during a suspension does not start the source of a %s{name}"
            <| fun () ->
                task {
                    use cts = new CancellationTokenSource()
                    let reached = ref false
                    let calls = ref 0
                    let gate = newGate ()
                    let running = probe.Start cts.Token reached gate.Task calls
                    Expect.isFalse running.IsCompleted "The body must wait for the gate"
                    cts.Cancel()
                    gate.SetResult(())
                    let! _ = expectCanceled running cts.Token

                    Expect.equal
                        calls.Value
                        0
                        $"The source of the %s{name} after the cancellation must not start"
                }
    ]

let private bindCancellationTests =
    coldSourceCancellation "Bind" IcedBuilderProbes.bindCancellation

let private returnFromCancellationTests =
    coldSourceCancellation "ReturnFrom" IcedBuilderProbes.returnFromCancellation

let private synchronousLoopTests =
    testList "Cancellation inside a loop without a bind" [
        for name, expected, start in IcedBuilderProbes.synchronousLoopCancellation do
            testCaseTask
                $"%s{name} completes a while and a for loop after the body cancels the token"
            <| fun () ->
                task {
                    use cts = new CancellationTokenSource()
                    let! actual = (start cts).WaitAsync(deadline)

                    Expect.equal
                        actual
                        expected
                        "A loop without a bind does not check the token, as in the legacy builder"
                }
    ]

let private typedValidationTests =
    testList "Validation with a string error" [
        testCaseTask "ConfigureAwait(false), Task.Yield and a configured Validation source"
        <| fun () ->
            task {
                let! actual = runGated IcedBuilderProbes.typedValidation
                Expect.equal actual (Ok 2) "The value after the binds"
            }
    ]

let private pooledTests =
    testList "Pooled ValueTask sources" [
        for name, expected, bind in IcedBuilderProbes.pooled do
            testCaseTask $"%s{name} reads a pending pooled ValueTask one time"
            <| fun () ->
                task {
                    let channel =
                        Channel.CreateUnbounded<int>(UnboundedChannelOptions(SingleReader = true))

                    let running = bind (channel.Reader.ReadAsync())
                    Expect.isFalse running.IsCompleted "The builder must wait for the pooled source"

                    channel.Writer.TryWrite 1
                    |> ignore

                    let! actual = running.WaitAsync(deadline)
                    Expect.equal actual expected "The value of the pooled source"
                }
    ]

let private reuseTests =
    testList "Cold computations" [
        for name, build in IcedBuilderProbes.reusable do
            testCaseTask $"%s{name} runs the body again on each invocation"
            <| fun () ->
                task {
                    let runs = ref 0
                    let computation = build runs

                    Expect.equal
                        runs.Value
                        0
                        "A cold computation must not start until it gets a token"

                    let! first = (computation CancellationToken.None).WaitAsync(deadline)
                    let! second = (computation CancellationToken.None).WaitAsync(deadline)
                    Expect.equal runs.Value 2 "Each invocation runs the body"
                    Expect.notEqual first second "Each invocation gives its own result"
                }
    ]

let private regressionTests =
    testList "Loop and disposal shapes" [
        for r in IcedBuilderProbes.regressions do
            let forExpected, whileExpected, useExpected = r.Expected

            testCaseTask $"%s{r.Builder} let! in a for loop with a mutable"
            <| fun () ->
                task {
                    let! actual = runGated r.ForLoop
                    Expect.equal actual forExpected "The loop must add each awaited value"
                }

            testCaseTask $"%s{r.Builder} let! in a while loop with a mutable"
            <| fun () ->
                task {
                    let! actual = runGated r.WhileLoop
                    Expect.equal actual whileExpected "The loop must stop after two awaited values"
                }

            testCaseTask $"%s{r.Builder} use waits for a pending DisposeAsync after a suspension"
            <| fun () ->
                task {
                    let gate = newGate ()
                    let disposed = newGate ()
                    let started = newGate ()
                    let disposals = ref 0
                    let running = r.SuspendedUse gate.Task started disposed.Task disposals
                    Expect.isFalse running.IsCompleted "The body must wait for its first source"

                    Expect.equal
                        disposals.Value
                        0
                        "DisposeAsync must not start while the body waits"

                    gate.SetResult(())
                    do! started.Task.WaitAsync(deadline)
                    let completedBeforeDispose = running.IsCompleted
                    disposed.SetResult(())
                    let! actual = running.WaitAsync(deadline)
                    Expect.isFalse completedBeforeDispose "The task must wait for DisposeAsync"
                    Expect.equal actual useExpected "The value of the body"
                    Expect.equal disposals.Value 1 "DisposeAsync must run one time"
                }
    ]

let private interleavingTests =
    testList "Interleaved invocations" [
        for name, build in IcedBuilderProbes.interleaved do
            testCaseTask $"%s{name} gives each invocation its own token"
            <| fun () ->
                task {
                    let gate = newGate ()
                    let seen = ref CancellationToken.None
                    let computation = build gate.Task seen
                    use canceled = new CancellationTokenSource()
                    use live = new CancellationTokenSource()
                    let first = computation canceled.Token
                    let second = computation live.Token
                    canceled.Cancel()
                    gate.SetResult(())
                    let! observed = observe first
                    let! _ = second.WaitAsync(deadline)

                    match observed with
                    | Choice2Of2(:? OperationCanceledException as e) ->
                        Expect.equal
                            e.CancellationToken
                            canceled.Token
                            "The canceled invocation keeps its token"
                    | other ->
                        failtestf "Expected the first invocation to be canceled, was %A" other

                    Expect.equal
                        seen.Value
                        live.Token
                        "The live factory must get the token of its invocation"
                }
    ]

let private validationMergeTests =
    let ok (value: int) (_: CancellationToken) =
        Task.FromResult(Ok value: Validation<int, string>)

    testList "Validation and!" [
        testCaseTask "Cancellation while the left operand is pending cancels the merge"
        <| fun () ->
            task {
                use cts = new CancellationTokenSource()

                let left =
                    TaskCompletionSource<Validation<int, string>>(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    )

                let running =
                    IcedBuilderProbes.mergeValidation (fun _ -> left.Task) (ok 2) cts.Token

                Expect.isFalse running.IsCompleted "The merge must wait for the left operand"
                cts.Cancel()
                left.SetResult(Ok 1)
                let! _ = expectCanceled running cts.Token
                return ()
            }

        testCaseTask "Cancellation in the right factory does not wait for a pending left operand"
        <| fun () ->
            task {
                use cts = new CancellationTokenSource()

                let left =
                    TaskCompletionSource<Validation<int, string>>(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    )

                let right token =
                    cts.Cancel()
                    ok 2 token

                let running = IcedBuilderProbes.mergeValidation (fun _ -> left.Task) right cts.Token
                let canceledBeforeLeftFinished = running.IsCanceled
                left.SetResult(Ok 1)
                let! _ = expectCanceled running cts.Token

                Expect.isTrue
                    canceledBeforeLeftFinished
                    "Cancellation must complete the invocation before the left operand returns"
            }

        testCaseTask "Both factories start before a completed pooled operand is consumed"
        <| fun () ->
            task {
                let log = Log()

                let source =
                    { new Sources.IValueTaskSource<int> with
                        member _.GetStatus _ = Sources.ValueTaskSourceStatus.Succeeded

                        member _.OnCompleted(_, _, _, _) =
                            failtest "The left operand is already complete"

                        member _.GetResult _ =
                            log.Add "read left"
                            1
                    }

                let left (_: CancellationToken) =
                    log.Add "start left"
                    ValueTask<int>(source, 0s)

                let right (_: CancellationToken) =
                    log.Add "start right"
                    Task.FromResult 20

                let run: CancellationToken -> Task<Validation<int, string>> =
                    cancellableTaskValidation {
                        let! a = left
                        and! b = right
                        return a + b
                    }

                let! result = (run CancellationToken.None).WaitAsync(deadline)
                Expect.equal result (Ok 21) "The validation result"

                Expect.equal
                    log.Entries
                    [
                        "start left"
                        "start right"
                        "read left"
                    ]
                    "The pooled operand must be consumed once, after both factories start"
            }

        testCaseTask "A left factory that throws does not start the right factory"
        <| fun () ->
            task {
                let log = Log()

                let left (_: CancellationToken) : Task<Validation<int, string>> =
                    log.Add "left"
                    raise (InvalidOperationException "left")

                let right (ct: CancellationToken) =
                    log.Add "right"
                    ok 2 ct

                let running = IcedBuilderProbes.mergeValidation left right CancellationToken.None
                let! observed = observe running

                match observed with
                | Choice2Of2(:? InvalidOperationException) -> ()
                | other -> failtestf "Expected the left exception, was %A" other

                Expect.isTrue running.IsFaulted "The invocation must return a faulted task"

                Expect.equal log.Entries [ "left" ] "Only the left factory may start"
            }

        testCaseTask "A left factory that returns a faulted task still starts the right factory"
        <| fun () ->
            task {
                let log = Log()

                let left (_: CancellationToken) =
                    log.Add "left"
                    Task.FromException<Validation<int, string>>(InvalidOperationException "left")

                let right (ct: CancellationToken) =
                    log.Add "right"
                    ok 2 ct

                let! observed =
                    observe (IcedBuilderProbes.mergeValidation left right CancellationToken.None)

                match observed with
                | Choice2Of2(:? InvalidOperationException) -> ()
                | other -> failtestf "Expected the left exception, was %A" other

                Expect.equal
                    log.Entries
                    [
                        "left"
                        "right"
                    ]
                    "Both factories start before the faulted task is awaited"
            }
    ]

let private coldAwaiterTests =
    let expected = sprintf "%A" (Ok 42: Result<int, string>)

    testList "Cold awaiter sources" [
        for name, returnFrom, bind in IcedBuilderProbes.coldAwaiters do
            testCaseTask $"%s{name} return! of a factory that returns a TaskAwaiter"
            <| fun () ->
                task {
                    let! actual = (returnFrom ()).WaitAsync(deadline)
                    Expect.equal actual expected "The awaiter result"
                }

            testCaseTask $"%s{name} let! of a factory that returns a TaskAwaiter"
            <| fun () ->
                task {
                    let! actual = (bind ()).WaitAsync(deadline)
                    Expect.equal actual expected "The awaiter result"
                }
    ]

let private asyncSequenceCancellationTests =
    testList "Cancellation during an IAsyncEnumerable loop" [
        for name, start in IcedBuilderProbes.asyncSequenceCancellation do
            testCaseTask $"%s{name} does not run the body after MoveNextAsync cancels the token"
            <| fun () ->
                task {
                    use cts = new CancellationTokenSource()
                    let moves = ref 0
                    let calls = ref 0
                    let! _ = expectCanceled (start cts moves calls) cts.Token
                    Expect.equal calls.Value 0 "The body must not run after the cancellation"
                    Expect.equal moves.Value 1 "The loop must stop after the cancellation"
                }
    ]

let private queuedCancellationTests =
    testList "Cancellation while a background body is queued" [
        for name, _ in IcedBuilderProbes.queued do
            testCaseTask $"%s{name} does not start a body that was canceled in the queue"
            <| fun () ->
                task {
                    let host =
                        match Environment.GetEnvironmentVariable "DOTNET_HOST_PATH" with
                        | null
                        | "" -> "dotnet"
                        | path -> path

                    let info =
                        Diagnostics.ProcessStartInfo(
                            host,
                            [
                                "exec"
                                typeof<IcedBuilderProbes.Marker>.Assembly.Location
                                "--queued-cancellation"
                                name
                            ]
                        )

                    info.UseShellExecute <- false
                    info.RedirectStandardError <- true
                    use probe = nonNull (Diagnostics.Process.Start info)

                    try
                        use timeout = new CancellationTokenSource(deadline)
                        let! errors = probe.StandardError.ReadToEndAsync(timeout.Token)
                        do! probe.WaitForExitAsync(timeout.Token)

                        let status =
                            enum<TaskStatus> (
                                probe.ExitCode
                                / 10
                            )

                        let starts = probe.ExitCode % 10

                        Expect.equal
                            (status, starts)
                            (TaskStatus.Canceled, 0)
                            $"The queued body must not start after cancellation. Probe errors: %s{errors}"
                    finally
                        if not probe.HasExited then
                            probe.Kill(true)

                            probe.WaitForExit(int deadline.TotalMilliseconds)
                            |> ignore
                }
    ]

let private onPostingContext start =
    (ConfigureAwaitTestHelpers.runWithPendingGate start).WaitAsync(deadline)

let private contextTests =
    testList "Context flow matches FSharp.Core task" [
        for builder, capture, preview in IcedContextProbes.contextCases do
            testCaseTask $"%s{builder} continues on the calling context when capture is %b{capture}"
            <| fun () ->
                task {
                    let! expected =
                        onPostingContext (fun context gate ->
                            task {
                                do! gate.ConfigureAwait(capture)
                                return ConfigureAwaitTestHelpers.onContext context
                            }
                        )

                    let! actual = onPostingContext preview
                    Expect.equal expected capture "The reference task must suspend on the test gate"
                    Expect.equal actual expected "The bind must follow the source awaiter"
                }

        for builder, suspends, preview in IcedContextProbes.slotCases do
            testCaseTask
                $"%s{builder} keeps AsyncLocal writes after a block (suspends = %b{suspends})"
            <| fun () ->
                task {
                    let slot = AsyncLocal<int>()
                    slot.Value <- 21

                    let! expected =
                        onPostingContext (fun _ gate ->
                            task {
                                try
                                    if suspends then
                                        for _ in [ 1 ] do
                                            do! gate
                                            slot.Value <- 42
                                    else
                                        slot.Value <- 42
                                finally
                                    ()

                                return slot.Value
                            }
                        )

                    let! actual = onPostingContext (fun _ gate -> preview slot gate)

                    Expect.equal
                        expected
                        42
                        "The reference task sees its assignment after the block"

                    Expect.equal actual expected "Code after the block must see the assignment"
                    Expect.equal slot.Value 21 "The caller keeps its own AsyncLocal value"
                }
    ]

[<Tests>]
let allTests =
    let probeModule = moduleOf typeof<IcedBuilderProbes.Marker>

    let controlModules = [ moduleOf typeof<IcedStateMachineProbes.Marker> ]

    testList "IcedTasks builders" [
        resolutionTests IcedBuilderProbes.all
        suspensionTests probeModule IcedBuilderProbes.all
        backgroundTests IcedBuilderProbes.all
        testCaseTask "A completed background value task result holds the synchronous result"
        <| fun () ->
            task {
                let! direct =
                    Task.Run(fun () ->
                        let run = backgroundCancellableValueTaskResult { return 42 }

                        let actual: ValueTask<Result<int, string>> = run CancellationToken.None
                        actual.Equals(ValueTask<Result<int, string>>(Ok 42))
                    )

                Expect.isTrue direct "The default scheduler path must return the inline result"
            }
        cancellationTests
        bindCancellationTests
        returnFromCancellationTests
        synchronousLoopTests
        typedValidationTests
        pooledTests
        reuseTests
        regressionTests
        interleavingTests
        validationMergeTests
        coldAwaiterTests
        asyncSequenceCancellationTests
        queuedCancellationTests
        contextTests
        scopeTests probeModule.Assembly controlModules IcedStateMachineProbes.controls
        libraryTests [
            typeof<FsToolkit.ErrorHandling.TaskResultBuilder>.Assembly
            IcedBuilderProbes.all.Head.Value.GetType().Assembly
        ]
    ]
