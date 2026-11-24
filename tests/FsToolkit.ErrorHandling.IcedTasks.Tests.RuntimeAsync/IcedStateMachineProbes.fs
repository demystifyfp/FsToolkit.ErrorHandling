module IcedStateMachineProbes

open System.Runtime.CompilerServices
open System.Threading.Tasks
open FsToolkit.ErrorHandling

type Marker = class end

let private gated (gate: Task<unit>) value =
    RuntimeAsyncTestHelpers.after gate value

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyCancellableTaskResult (gate: Task<unit>) =
    cancellableTaskResult {
        let! x = gated gate (Ok 1: Result<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyBackgroundCancellableTaskResult (gate: Task<unit>) =
    backgroundCancellableTaskResult {
        let! x = gated gate (Ok 1: Result<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyCancellableValueTaskResult (gate: Task<unit>) =
    cancellableValueTaskResult {
        let! x = gated gate (Ok 1: Result<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyBackgroundCancellableValueTaskResult (gate: Task<unit>) =
    backgroundCancellableValueTaskResult {
        let! x = gated gate (Ok 1: Result<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyCancellableTaskValidation (gate: Task<unit>) =
    cancellableTaskValidation {
        let! x = gated gate (Ok 1: Validation<int, string>)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyCancellableTaskOption (gate: Task<unit>) =
    cancellableTaskOption {
        let! x = gated gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyBackgroundCancellableTaskOption (gate: Task<unit>) =
    backgroundCancellableTaskOption {
        let! x = gated gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyCancellableValueTaskOption (gate: Task<unit>) =
    cancellableValueTaskOption {
        let! x = gated gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyBackgroundCancellableValueTaskOption (gate: Task<unit>) =
    backgroundCancellableValueTaskOption {
        let! x = gated gate (Some 1)
        return x
    }

[<MethodImpl(MethodImplOptions.NoInlining)>]
let legacyValueTaskValueOption (gate: Task<unit>) : ValueTask<int voption> =
    valueTaskValueOption {
        let! x = gated gate (ValueSome 1)
        return x
    }

let controls = [
    "cancellableTaskResult", nameof legacyCancellableTaskResult
    "backgroundCancellableTaskResult", nameof legacyBackgroundCancellableTaskResult
    "cancellableValueTaskResult", nameof legacyCancellableValueTaskResult
    "backgroundCancellableValueTaskResult", nameof legacyBackgroundCancellableValueTaskResult
    "cancellableTaskValidation", nameof legacyCancellableTaskValidation
    "cancellableTaskOption", nameof legacyCancellableTaskOption
    "backgroundCancellableTaskOption", nameof legacyBackgroundCancellableTaskOption
    "cancellableValueTaskOption", nameof legacyCancellableValueTaskOption
    "backgroundCancellableValueTaskOption", nameof legacyBackgroundCancellableValueTaskOption
    "valueTaskValueOption", nameof legacyValueTaskValueOption
]
