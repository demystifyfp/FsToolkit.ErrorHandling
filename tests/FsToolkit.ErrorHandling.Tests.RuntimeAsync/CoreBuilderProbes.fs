module CoreBuilderProbes

open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask
open RuntimeAsyncTestHelpers

type Marker = class end

let private onDefaultScheduler () =
    isNull SynchronizationContext.Current
    && obj.ReferenceEquals(TaskScheduler.Current, TaskScheduler.Default)

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeTaskResult (gate: Task<unit>) (onDefault: bool ref) : Task<Result<int, string>> =
    taskResult {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: Result<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeBackgroundTaskResult (gate: Task<unit>) (onDefault: bool ref) : Task<Result<int, string>> =
    backgroundTaskResult {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: Result<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeTaskOption (gate: Task<unit>) (onDefault: bool ref) : Task<int option> =
    taskOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeBackgroundTaskOption (gate: Task<unit>) (onDefault: bool ref) : Task<int option> =
    backgroundTaskOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeTaskValueOption (gate: Task<unit>) (onDefault: bool ref) : Task<int voption> =
    taskValueOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (ValueSome 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeBackgroundTaskValueOption (gate: Task<unit>) (onDefault: bool ref) : Task<int voption> =
    backgroundTaskValueOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (ValueSome 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeTaskValidation (gate: Task<unit>) (onDefault: bool ref) : Task<Validation<int, string>> =
    taskValidation {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: Validation<int, string>)
        and! y = after gate (Ok 2: Validation<int, string>)
        return x + y
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeBackgroundTaskValidation
    (gate: Task<unit>)
    (onDefault: bool ref)
    : Task<Validation<int, string>> =
    backgroundTaskValidation {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok 1: Validation<int, string>)
        and! y = after gate (Ok 2: Validation<int, string>)
        return x + y
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let probeTaskResultOption
    (gate: Task<unit>)
    (onDefault: bool ref)
    : Task<Result<int option, string>> =
    taskResultOption {
        onDefault.Value <- onDefaultScheduler ()
        let! x = after gate (Ok(Some 1): Result<int option, string>)
        return x
    }

let private render (running: Task<'T>) : Task<string> =
    running.ContinueWith(
        (fun (t: Task<'T>) -> sprintf "%A" t.Result),
        CancellationToken.None,
        TaskContinuationOptions.ExecuteSynchronously,
        TaskScheduler.Default
    )

let private probe
    name
    builder
    method
    background
    expected
    (start: Task<unit> -> bool ref -> Task<'T>)
    =
    {
        Builder = name
        Value =
            (box builder
             |> nonNull)
        Method = method
        Background = background
        Expected = sprintf "%A" (expected: 'T)
        Start = fun gate onDefault -> render (start gate onDefault)
    }

let all: BuilderProbe list = [
    probe
        "taskResult"
        taskResult
        (nameof probeTaskResult)
        false
        (Ok 1: Result<int, string>)
        probeTaskResult
    probe
        "backgroundTaskResult"
        backgroundTaskResult
        (nameof probeBackgroundTaskResult)
        true
        (Ok 1: Result<int, string>)
        probeBackgroundTaskResult
    probe "taskOption" taskOption (nameof probeTaskOption) false (Some 1) probeTaskOption
    probe
        "backgroundTaskOption"
        backgroundTaskOption
        (nameof probeBackgroundTaskOption)
        true
        (Some 1)
        probeBackgroundTaskOption
    probe
        "taskValueOption"
        taskValueOption
        (nameof probeTaskValueOption)
        false
        (ValueSome 1)
        probeTaskValueOption
    probe
        "backgroundTaskValueOption"
        backgroundTaskValueOption
        (nameof probeBackgroundTaskValueOption)
        true
        (ValueSome 1)
        probeBackgroundTaskValueOption
    probe
        "taskValidation"
        taskValidation
        (nameof probeTaskValidation)
        false
        (Ok 3: Validation<int, string>)
        probeTaskValidation
    probe
        "backgroundTaskValidation"
        backgroundTaskValidation
        (nameof probeBackgroundTaskValidation)
        true
        (Ok 3: Validation<int, string>)
        probeBackgroundTaskValidation
    probe
        "taskResultOption"
        taskResultOption
        (nameof probeTaskResultOption)
        false
        (Ok(Some 1): Result<int option, string>)
        probeTaskResultOption
]
