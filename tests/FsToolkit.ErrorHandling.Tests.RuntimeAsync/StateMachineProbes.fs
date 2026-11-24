module StateMachineProbes

open System.Runtime.CompilerServices
open System.Threading.Tasks
open FsToolkit.ErrorHandling

type Marker = class end

[<MethodImpl(MethodImplOptions.NoInlining)>]
let letBang (gate: Task<unit>) : Task<Result<int, string>> =
    taskResult {
        let! x = RuntimeAsyncTestHelpers.after gate (Ok 1: Result<int, string>)
        return x
    }

let builder = taskResult

let private gated (gate: Task<unit>) value =
    RuntimeAsyncTestHelpers.after gate value

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyBackgroundTaskResult (gate: Task<unit>) : Task<Result<int, string>> =
    backgroundTaskResult {
        let! x = gated gate (Ok 1: Result<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyTaskOption (gate: Task<unit>) : Task<int option> =
    taskOption {
        let! x = gated gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyBackgroundTaskOption (gate: Task<unit>) : Task<int option> =
    backgroundTaskOption {
        let! x = gated gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyTaskValueOption (gate: Task<unit>) : Task<int voption> =
    taskValueOption {
        let! x = gated gate (ValueSome 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyBackgroundTaskValueOption (gate: Task<unit>) : Task<int voption> =
    backgroundTaskValueOption {
        let! x = gated gate (ValueSome 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyTaskValidation (gate: Task<unit>) : Task<Validation<int, string>> =
    taskValidation {
        let! x = gated gate (Ok 1: Validation<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyBackgroundTaskValidation (gate: Task<unit>) : Task<Validation<int, string>> =
    backgroundTaskValidation {
        let! x = gated gate (Ok 1: Validation<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyTaskResultOption (gate: Task<unit>) : Task<Result<int option, string>> =
    taskResultOption {
        let! x = gated gate (Ok(Some 1): Result<int option, string>)
        return x
    }

let controls = [
    "taskResult", nameof letBang
    "backgroundTaskResult", nameof legacyBackgroundTaskResult
    "taskOption", nameof legacyTaskOption
    "backgroundTaskOption", nameof legacyBackgroundTaskOption
    "taskValueOption", nameof legacyTaskValueOption
    "backgroundTaskValueOption", nameof legacyBackgroundTaskValueOption
    "taskValidation", nameof legacyTaskValidation
    "backgroundTaskValidation", nameof legacyBackgroundTaskValidation
    "taskResultOption", nameof legacyTaskResultOption
]
