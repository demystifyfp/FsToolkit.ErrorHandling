module TaskResultRuntimeAsyncTests

open System
open System.IO
open System.Reflection
open System.Reflection.Emit
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open Expecto
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask
open RuntimeAsyncTestHelpers

type private R = Result<int, string>

module private ForwardedSource =
    [<NoComparison>]
    type Input = Input of Task<R>

    type RuntimeTaskResultBuilder with
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        member builder.Source(Input input) = builder.Source input

    let run input : Task<R> =
        taskResult {
            let! value = Input input
            return value + 1
        }

    [<NoComparison>]
    type BaseInput = BaseInput of Task<R>

    type RuntimeTaskResultBuilderBase with
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        member builder.Source(BaseInput input) = builder.Source input

    let throughBase input onBound : Task<R> =
        taskResult {
            let! _ = BaseInput input
            onBound ()
            return 42
        }

let private runtimeNamespace = "FsToolkit.ErrorHandling.Polyfill.RuntimeTask"

let private inputs: R list = [
    Ok 1
    Error "source"
]

let private expectBind (input: R) (actual: R) (reached: bool) =
    match input with
    | Ok n ->
        Expect.equal actual (Ok(n + 1)) "An Ok source must give its value to the continuation"
        Expect.isTrue reached "The code after an Ok bind must run"
    | Error e ->
        Expect.equal actual (Error e) "An Error source must stop the computation with that Error"
        Expect.isFalse reached "The code after an Error bind must not run"

let private builderTests =
    testList "Builder" [
        testCaseTask "A non-inline Source extension suspends and preserves the error"
        <| fun () ->
            task {
                let input =
                    TaskCompletionSource<R>(TaskCreationOptions.RunContinuationsAsynchronously)

                let running = ForwardedSource.run input.Task
                Expect.isFalse running.IsCompleted "The builder must wait for the forwarded source"
                input.SetResult(Error "source")
                let! actual = running.WaitAsync(TimeSpan.FromSeconds 10.0)

                Expect.equal
                    actual
                    (Error "source")
                    "The forwarded Error must skip the continuation"
            }

        testCaseTask "A Source extension on the base builder short-circuits a forwarded Error"
        <| fun () ->
            task {
                let input =
                    TaskCompletionSource<R>(TaskCreationOptions.RunContinuationsAsynchronously)

                let mutable reached = false
                let running = ForwardedSource.throughBase input.Task (fun () -> reached <- true)
                Expect.isFalse running.IsCompleted "The base adapter must wait for the input"
                input.SetResult(Error "source")
                let! actual = running.WaitAsync(deadline)

                Expect.equal
                    actual
                    (Error "source")
                    "The base adapter must preserve the original Error"

                Expect.isFalse reached "The forwarded Error must skip the continuation"
            }

        testCase "taskResult resolves to the runtime-async builder"
        <| fun () ->
            let builderType = taskResult.GetType()

            Expect.equal
                builderType.Namespace
                runtimeNamespace
                $"The open must give the runtime-async builder, not %s{builderType.FullName}"
    ]

let private misplacedSuspension () =
    let builder =
        PersistedAssemblyBuilder(AssemblyName "MisplacedSuspension", typeof<obj>.Assembly)

    let typeBuilder =
        builder
            .DefineDynamicModule("MisplacedSuspension")
            .DefineType("Misplaced", TypeAttributes.Public)

    let method =
        typeBuilder.DefineMethod(
            "Run",
            MethodAttributes.Public
            ||| MethodAttributes.Static,
            typeof<Void>,
            [| typeof<Task> |]
        )

    let il = method.GetILGenerator()
    il.Emit(OpCodes.Ldarg_0)
    il.Emit(OpCodes.Call, nonNull (typeof<AsyncHelpers>.GetMethod("Await", [| typeof<Task> |])))
    il.Emit(OpCodes.Ret)

    typeBuilder.CreateType()
    |> ignore

    use stream = new MemoryStream()
    builder.Save stream
    Assembly.Load(stream.ToArray()).GetTypes()

let private ilTests =
    let probeModule = nonNull typeof<RuntimeAsyncProbes.Marker>.DeclaringType

    testList "IL" [
        testCase "The IL scan finds an AsyncHelpers call in a method without the Async flag"
        <| fun () ->
            let misplaced =
                suspensionCallers (misplacedSuspension ())
                |> List.filter (
                    isRuntimeAsync
                    >> not
                )
                |> List.map describe

            Expect.equal misplaced [ "Misplaced::Run" ] "The scan must report the emitted call"

        testCase "Each direct AsyncHelpers caller in the test assembly has the Async flag"
        <| fun () ->
            let callers = suspensionCallers (probeModule.Assembly.GetTypes())
            Expect.isNonEmpty callers "The scan must find the runtime-async suspensions"

            let misplaced =
                callers
                |> List.filter (
                    isRuntimeAsync
                    >> not
                )
                |> List.map describe

            Expect.isEmpty misplaced "A suspension outside a runtime-async method"

        testCaseTask "The state-machine taskResult has no runtime-async method"
        <| fun () ->
            task {
                let! result = runGated StateMachineProbes.letBang
                Expect.equal result (Ok 1) "The probe value"

                let group =
                    methodGroup
                        (nonNull typeof<StateMachineProbes.Marker>.DeclaringType)
                        (nameof StateMachineProbes.letBang)

                Expect.isNonEmpty group "The compiled methods of the control probe"

                let flagged =
                    group
                    |> List.filter isRuntimeAsync
                    |> List.map describe

                Expect.isEmpty flagged "The control probe must compile without runtime async"
            }

        testCase "The state-machine probe module contains an IAsyncStateMachine"
        <| fun () ->
            let machines =
                nestedTypes (nonNull typeof<StateMachineProbes.Marker>.DeclaringType)
                |> List.filter isStateMachine

            Expect.isNonEmpty machines "The scan must find the state machine of the control probe"

        testList "Each probe suspends in its own runtime-async method" [
            for probe in RuntimeAsyncProbes.all do
                testCaseTask probe.Name
                <| fun () ->
                    task {
                        let! result = runGated probe.Start
                        Expect.equal result (Ok probe.Expected) "The probe value"

                        let group = methodGroup probeModule probe.Method
                        Expect.isNonEmpty group $"The compiled methods of %s{probe.Method}"

                        Expect.exists
                            group
                            (fun method ->
                                isRuntimeAsync method
                                && callsSuspension method
                            )
                            "The probe must suspend in a method that has the Async flag"
                    }
        ]

        testCase "The probe module contains no IAsyncStateMachine"
        <| fun () ->
            let machines =
                nestedTypes (nonNull typeof<RuntimeAsyncProbes.Marker>.DeclaringType)
                |> List.filter isStateMachine
                |> List.map (fun t -> t.FullName)

            Expect.isEmpty
                machines
                "No source, merge, or cleanup form may compile to a state machine"

        testCase "The runtime-async namespace has types and no IAsyncStateMachine"
        <| fun () ->
            let types =
                typeof<TaskResultBuilder>.Assembly.GetTypes()
                |> Array.filter (fun t ->
                    match t.Namespace with
                    | null -> false
                    | ns -> ns = runtimeNamespace
                )

            Expect.isNonEmpty types $"The net11.0 build must contain %s{runtimeNamespace}"

            let machines =
                types
                |> Array.filter isStateMachine
                |> Array.map (fun t -> t.FullName)

            Expect.isEmpty machines "The runtime-async builder must not contain a state machine"
    ]

let private sourceTests =
    let bindCases: (string * (Task<unit> -> R -> bool ref -> Task<R>)) list = [
        "let! of Task<Result>",
        fun gate input reached ->
            taskResult {
                let! x = after gate input
                reached.Value <- true
                return x + 1
            }
        "let! of ValueTask<Result>",
        fun gate input reached ->
            taskResult {
                let! x = ValueTask<R>(after gate input)
                reached.Value <- true
                return x + 1
            }
        "let! of Async<Result>",
        fun gate input reached ->
            taskResult {
                let! x = Async.AwaitTask(after gate input)
                reached.Value <- true
                return x + 1
            }
        "let! of Result after a suspension",
        fun gate input reached ->
            taskResult {
                do! gate
                let! x = input
                reached.Value <- true
                return x + 1
            }
    ]

    testList "Sources" [
        for name, start in bindCases do
            for input in inputs do
                testCaseTask $"%s{name}, pending, %A{input}"
                <| fun () ->
                    task {
                        let reached = ref false
                        let! actual = runGated (fun gate -> start gate input reached)
                        expectBind input actual reached.Value
                    }

        testCase "A computation with completed sources completes synchronously"
        <| fun () ->
            let running: Task<R> =
                taskResult {
                    let! a = Task.FromResult 1
                    let! b = ValueTask<R>(Ok 2)
                    let! c = (Ok 3: R)
                    do! Task.CompletedTask
                    return a + b + c
                }

            Expect.isTrue running.IsCompleted "No source suspended, so the task must be complete"
            Expect.equal running.Result (Ok 6) "The result"
    ]

let private mergeTests =
    let zipOracle (a: R) (b: R) =
        match a, b with
        | Error e, _ -> Error e
        | _, Error e -> Error e
        | Ok x, Ok y -> Ok(x + y)

    let mergeCases: (string * (Task<R> -> Task<R> -> Task<R>)) list = [
        "and! of two Task<Result> with BindReturn",
        fun a b ->
            taskResult {
                let! x = a
                and! y = b
                return x + y
            }
        "and! of ValueTask<Result> and Async<Result>",
        fun a b ->
            taskResult {
                let! x = ValueTask<R> a
                and! y = Async.AwaitTask b
                return x + y
            }
    ]

    testList "and!" [
        for name, start in mergeCases do
            for left in inputs do
                for right in
                    [
                        Ok 2
                        Error "right"
                    ] do
                    for leftFirst in
                        [
                            true
                            false
                        ] do
                        let firstName = if leftFirst then "left" else "right"

                        testCaseTask $"%s{name}, %A{left}, %A{right}, release %s{firstName} first"
                        <| fun () ->
                            task {
                                let leftGate = newGate ()
                                let rightGate = newGate ()

                                let running =
                                    start (after leftGate.Task left) (after rightGate.Task right)

                                let first, second =
                                    if leftFirst then
                                        leftGate, rightGate
                                    else
                                        rightGate, leftGate

                                first.SetResult(())

                                Expect.isFalse running.IsCompleted "and! must wait for each operand"

                                second.SetResult(())
                                let! actual = running.WaitAsync(deadline)

                                Expect.equal
                                    actual
                                    (zipOracle left right)
                                    "and! must give the left Error first, then the right Error"
                            }

        testCaseTask "and! with an Error Result operand and a pending operand"
        <| fun () ->
            task {
                let! actual =
                    runGated (fun gate ->
                        taskResult {
                            let! a = after gate (Ok 1: R)
                            and! b = (Error "sync": R)
                            return a + b
                        }
                    )

                Expect.equal actual (Error "sync") "The Error operand"
            }

        testCaseTask "and! with a completed left Error waits for the pending right operand"
        <| fun () ->
            task {
                let right = newGate ()
                let mutable reached = false

                let running: Task<R> =
                    taskResult {
                        let! a = Task.FromResult(Error "left": R)
                        and! b = after right.Task (Ok 2: R)
                        reached <- true
                        return a + b
                    }

                let publishedEarly = running.IsCompleted
                right.SetResult(())
                let! actual = running.WaitAsync(deadline)

                Expect.isFalse
                    publishedEarly
                    "and! must not publish while the right operand is pending"

                Expect.isFalse reached "The code after and! must not run for an Error"
                Expect.equal actual (Error "left") "The left Error"
            }
    ]

let private cleanupTests =
    let bodyCases: (string * (unit -> R)) list = [
        "Error", (fun () -> Error "body")
        "exception", (fun () -> raise (InvalidOperationException "body"))
    ]

    let expectOutcome name (observed: Choice<R, exn>) =
        match name, observed with
        | "Error", Choice1Of2 r -> Expect.equal r (Error "body") "The Error must be the result"
        | "exception", Choice2Of2 e ->
            Expect.isTrue
                (e :? InvalidOperationException)
                $"The body exception must be the result, was %A{e}"
        | _, other -> failtestf "Unexpected outcome %A for %s" other name

    testList "Cleanup" [
        for name, fail in bodyCases do
            testCaseTask $"use runs Dispose once in LIFO order after a suspension, %s{name}"
            <| fun () ->
                task {
                    let log = Log()
                    let gate = newGate ()

                    let running: Task<R> =
                        taskResult {
                            use _a = disposable log "a"
                            use _b = disposable log "b"
                            do! gate.Task
                            let! x = fail ()
                            log.Add "after"
                            return x
                        }

                    Expect.isEmpty log.Entries "Nothing is disposed while the body is suspended"
                    gate.SetResult(())
                    let! observed = observe running
                    expectOutcome name observed

                    Expect.equal
                        log.Entries
                        [
                            "dispose b"
                            "dispose a"
                        ]
                        "Dispose runs once for each resource, in LIFO order"
                }

            testCaseTask $"try/finally runs the finally block once after a suspension, %s{name}"
            <| fun () ->
                task {
                    let log = Log()
                    let gate = newGate ()

                    let running: Task<R> =
                        taskResult {
                            try
                                try
                                    do! gate.Task
                                    let! x = fail ()
                                    log.Add "after"
                                    return x
                                finally
                                    log.Add "inner"
                            finally
                                log.Add "outer"
                        }

                    gate.SetResult(())
                    let! observed = observe running
                    expectOutcome name observed

                    Expect.equal
                        log.Entries
                        [
                            "inner"
                            "outer"
                        ]
                        "Each finally block runs once, inner first"
                }

            testCaseTask $"for over seq disposes the enumerator, %s{name}"
            <| fun () ->
                task {
                    let log = Log()
                    let gate = newGate ()

                    let running: Task<R> =
                        taskResult {
                            for i in
                                trackedSeq log [
                                    1
                                    2
                                    3
                                ] do
                                do! gate.Task
                                log.Add $"item %d{i}"
                                let! _ = fail ()
                                log.Add "after"

                            return 0
                        }

                    gate.SetResult(())
                    let! observed = observe running
                    expectOutcome name observed

                    Expect.equal
                        log.Entries
                        [
                            "item 1"
                            "dispose enumerator"
                        ]
                        "The loop stops at the first item and disposes the enumerator"
                }

        testCaseTask "try/with does not catch an Error"
        <| fun () ->
            task {
                let mutable handled = false

                let! actual =
                    runGated (fun gate ->
                        taskResult {
                            try
                                do! gate
                                return! (Error "body": R)
                            with _ ->
                                handled <- true
                                return 0
                        }
                    )

                Expect.equal actual (Error "body") "The Error passes through try/with"
                Expect.isFalse handled "An Error is not an exception"
            }

        testCaseTask "An exception from a finally block replaces an Error"
        <| fun () ->
            task {
                let! observed =
                    runGated (fun gate ->
                        taskResult {
                            try
                                do! gate
                                return! (Error "body": R)
                            finally
                                raise (ArgumentException "finally")
                        }
                    )
                    |> observe

                match observed with
                | Choice2Of2(:? ArgumentException as e) ->
                    Expect.equal e.Message "finally" "The finally exception"
                | other -> failtestf "Expected the finally exception, was %A" other
            }
    ]

let private failureTests =
    testList "Failures" [
        testCase "An exception before the first await gives a faulted task and does not throw"
        <| fun () ->
            let error = InvalidOperationException "early"

            let running: Task<R> =
                taskResult {
                    raise error
                    return 0
                }

            Expect.isTrue running.IsFaulted "The task must be faulted"

            Expect.isTrue
                (obj.ReferenceEquals((nonNull running.Exception).InnerException, error))
                "The exception object"

        testCase "The body runs synchronously until the first suspension"
        <| fun () ->
            let gate = newGate ()
            let mutable started = false

            let running: Task<R> =
                taskResult {
                    started <- true
                    do! gate.Task
                    return 1
                }

            Expect.isTrue started "The body must start on the call"
            Expect.isFalse running.IsCompleted "The body is suspended"
            gate.SetResult(())
            Expect.equal (running.WaitAsync(deadline).GetAwaiter().GetResult()) (Ok 1) "The result"

        testCaseTask "A faulted source keeps the exception object"
        <| fun () ->
            task {
                let gate = newGate ()
                let error = InvalidOperationException "source"

                let running: Task<R> =
                    taskResult {
                        let! x = (faultAfter gate.Task error: Task<R>)
                        return x
                    }

                gate.SetResult(())
                let! observed = observe running

                Expect.isTrue
                    (match observed with
                     | Choice2Of2 e -> obj.ReferenceEquals(e, error)
                     | _ -> false)
                    $"The source exception, was %A{observed}"

                Expect.isTrue running.IsFaulted "The task must be faulted"
            }

        testCaseTask "OperationCanceledException in the body gives a canceled task"
        <| fun () ->
            task {
                let gate = newGate ()

                let running: Task<R> =
                    taskResult {
                        do! gate.Task
                        return raise (OperationCanceledException "body")
                    }

                gate.SetResult(())
                let! _ = observe running
                Expect.equal running.Status TaskStatus.Canceled "The task status"
            }

        testCaseTask "A canceled source gives a canceled task"
        <| fun () ->
            task {
                let source =
                    TaskCompletionSource<R>(TaskCreationOptions.RunContinuationsAsynchronously)

                let running: Task<R> =
                    taskResult {
                        let! x = source.Task
                        return x
                    }

                source.SetCanceled()
                let! _ = observe running
                Expect.equal running.Status TaskStatus.Canceled "The task status"
            }

        testCaseTask "An AsyncLocal value set in the body does not flow to the caller"
        <| fun () ->
            task {
                let local = AsyncLocal<int>()
                local.Value <- 1
                let gate = newGate ()

                let running: Task<R> =
                    taskResult {
                        local.Value <- 2
                        do! gate.Task
                        return local.Value
                    }

                Expect.equal local.Value 1 "The caller keeps its value after the body suspends"
                gate.SetResult(())
                let! actual = running.WaitAsync(deadline)
                Expect.equal actual (Ok 2) "The body keeps its value after it resumes"
                Expect.equal local.Value 1 "The caller keeps its value"
            }
    ]

let allTests =
    testList "TaskResult runtime async" [
        builderTests
        ilTests
        sourceTests
        mergeTests
        cleanupTests
        failureTests
    ]
