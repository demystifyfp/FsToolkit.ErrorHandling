module BuilderCoverage

open System
open System.Reflection
open System.Threading
open System.Threading.Tasks
open Expecto
open ConfigureAwaitTestHelpers
open RuntimeAsyncTestHelpers

let runtimeNamespace = "FsToolkit.ErrorHandling.Polyfill.RuntimeTask"

type Caller =
    | Plain
    | Context
    | Scheduler

let private startFrom caller (start: unit -> Task<string>) : Task<string> =
    match caller with
    | Plain -> start ()
    | Context ->
        let previous = SynchronizationContext.Current
        SynchronizationContext.SetSynchronizationContext(ThreadPoolPostContext())

        try
            start ()
        finally
            SynchronizationContext.SetSynchronizationContext previous
    | Scheduler ->
        let pair = ConcurrentExclusiveSchedulerPair()

        Task.Factory
            .StartNew(
                Func<Task<string>>(start),
                CancellationToken.None,
                TaskCreationOptions.None,
                pair.ExclusiveScheduler
            )
            .Unwrap()

let private runProbe caller (probe: BuilderProbe) =
    task {
        let gate = newGate ()
        let onDefault = ref false
        let running = startFrom caller (fun () -> probe.Start gate.Task onDefault)
        let completedEarly = running.IsCompleted
        gate.SetResult(())
        let! actual = running.WaitAsync(deadline)

        Expect.isFalse completedEarly $"%s{probe.Builder} must suspend while its source is pending"

        Expect.equal actual probe.Expected $"The value of %s{probe.Builder}"
        return onDefault.Value
    }

let resolutionTests (probes: BuilderProbe list) =
    testList "Each builder resolves to the runtime-async builder" [
        for probe in probes do
            testCase probe.Builder
            <| fun () ->
                let builderType = probe.Value.GetType()

                Expect.equal
                    builderType.Namespace
                    runtimeNamespace
                    $"The opt-in open must give the runtime-async %s{probe.Builder}, not %s{builderType.FullName}"
    ]

let suspensionTests (probeModule: Type) (probes: BuilderProbe list) =
    testList "Each builder suspends in a runtime-async method" [
        for probe in probes do
            testCaseTask probe.Builder
            <| fun () ->
                task {
                    let! _ = runProbe Plain probe
                    let group = methodGroup probeModule probe.Method
                    Expect.isNonEmpty group $"The compiled methods of %s{probe.Method}"

                    Expect.exists
                        group
                        (fun method ->
                            isRuntimeAsync method
                            && callsSuspension method
                        )
                        $"%s{probe.Builder} must suspend in a method that has the Async flag"
                }

        testCase "The probe module contains no IAsyncStateMachine"
        <| fun () ->
            let machines =
                nestedTypes probeModule
                |> List.filter isStateMachine
                |> List.map (fun t -> t.FullName)

            Expect.equal machines [] "No builder probe may compile to a state machine"
    ]

let backgroundTests (probes: BuilderProbe list) =
    testList "Scheduling" [
        for probe in probes do
            for caller in
                [
                    Context
                    Scheduler
                ] do
                if probe.Background then
                    testCaseTask
                        $"%s{probe.Builder} runs on the default scheduler from a %A{caller} caller"
                    <| fun () ->
                        task {
                            let! onDefault = runProbe caller probe

                            Expect.isTrue
                                onDefault
                                "The body must run without the caller context and scheduler"
                        }
                else
                    testCaseTask $"%s{probe.Builder} starts on the caller from a %A{caller} caller"
                    <| fun () ->
                        task {
                            let! onDefault = runProbe caller probe

                            Expect.isFalse
                                onDefault
                                "The body must start synchronously on the caller context or scheduler"
                        }
    ]

let private companionResources =
    lazy
        let assembly = Assembly.GetExecutingAssembly()

        [
            for name in assembly.GetManifestResourceNames() do
                if name.StartsWith("companion-tests/", StringComparison.Ordinal) then
                    use stream = nonNull (assembly.GetManifestResourceStream(name))
                    yield Convert.ToHexString(Security.Cryptography.SHA256.HashData(stream)), name
                    stream.Position <- 0L
                    yield Convert.ToHexString(Security.Cryptography.SHA1.HashData(stream)), name
        ]
        |> Map.ofList

/// The source line ranges of each `let functionTests =` block in a file.
let private companionBlocks =
    let cache = Collections.Concurrent.ConcurrentDictionary<string, (int * int) list>()

    fun (document: string) ->
        cache.GetOrAdd(
            document,
            fun document ->
                let lines =
                    match Map.tryFind document companionResources.Value with
                    | None -> [||]
                    | Some name ->
                        use stream =
                            nonNull (
                                Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
                            )

                        use reader = new IO.StreamReader(stream)
                        reader.ReadToEnd().Split('\n')

                let indent (line: string) =
                    line.Length
                    - line.TrimStart().Length

                [
                    for start in
                        0 .. lines.Length
                             - 1 do
                        if lines[start].Trim() = "let functionTests =" then
                            let depth = indent lines[start]

                            let stop =
                                seq {
                                    start + 1 .. lines.Length
                                                 - 1
                                }
                                |> Seq.tryFind (fun i ->
                                    lines[i].Trim()
                                    <> ""
                                    && not (lines[i].StartsWith "#")
                                    && indent lines[i]
                                       <= depth
                                )
                                |> Option.defaultValue lines.Length

                            start + 1, stop
                ]
        )

/// The inline companion functions, such as CancellableTaskOption.bind, expand the legacy builders in the caller.
let isCompanionFunctionTest (site: LegacyReference) =
    match siteLines site with
    | [] -> false
    | lines ->
        lines
        |> List.forall (fun (document, line) ->
            companionBlocks document
            |> List.exists (fun (first, last) ->
                line
                >= first
                && line
                   <= last
            )
        )

/// The control modules use the legacy builders on purpose, so the scan must flag each of their methods.
let scopeTests
    (testAssembly: Assembly)
    (controlModules: Type list)
    (controls: (string * string) list)
    =
    let isControl (t: Type) =
        controlModules
        |> List.exists (fun control ->
            t = control
            || t.Name = "$"
                        + control.Name
            || (nonNull t.FullName)
                .StartsWith(
                    (nonNull control.FullName)
                    + "+"
                )
        )

    let sites () =
        testAssembly.GetTypes()
        |> Array.filter (
            isControl
            >> not
        )
        |> legacyReferenceSites

    testList "Scope" [
        testCase "The scan flags each legacy builder in the control modules"
        <| fun () ->
            let flagged =
                legacyReferences (
                    controlModules
                    |> List.collect moduleTypes
                )

            for builder, method in controls do
                Expect.exists
                    flagged
                    (fun entry ->
                        entry.Contains($"::{method}")
                        || entry.Contains($"+{method}@")
                    )
                    $"The scan must flag the legacy %s{builder} in %s{method}"

        testCase "The control modules are not companion function tests"
        <| fun () ->
            let exempt =
                controlModules
                |> List.collect moduleTypes
                |> legacyReferenceSites
                |> List.filter isCompanionFunctionTest
                |> List.map (fun site -> site.Description)

            Expect.equal exempt [] "The exemption must not hide a legacy control"

        testCase
            "No code outside the control modules and companion function tests references a legacy builder"
        <| fun () ->
            Expect.equal
                (sites ()
                 |> List.filter (
                     isCompanionFunctionTest
                     >> not
                 )
                 |> List.map (fun site -> site.Description))
                []
                "Each linked CE test must resolve its builder to the runtime-async implementation"
    ]

let libraryTests (assemblies: Assembly list) =
    testList "Libraries" [
        for assembly in assemblies do
            testCase
                $"%s{assembly.GetName().Name} has runtime-async types and no IAsyncStateMachine"
            <| fun () ->
                let types =
                    assembly.GetTypes()
                    |> Array.filter (fun t -> t.Namespace = runtimeNamespace)

                Expect.isNonEmpty types $"The net11.0 build must contain %s{runtimeNamespace}"

                let machines =
                    types
                    |> Array.filter isStateMachine
                    |> Array.map (fun t -> t.FullName)

                Expect.equal
                    (List.ofArray machines)
                    []
                    "A runtime-async builder must not contain a state machine"

            testCase $"Each AsyncHelpers caller in %s{assembly.GetName().Name} has the Async flag"
            <| fun () ->
                let misplaced =
                    suspensionCallers (assembly.GetTypes())
                    |> List.filter (
                        isRuntimeAsync
                        >> not
                    )
                    |> List.map describe

                Expect.equal misplaced [] "A suspension outside a runtime-async method"
    ]
