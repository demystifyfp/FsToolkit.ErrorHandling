module IcedContextProbes

open System.Threading
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask
open ConfigureAwaitTestHelpers

let private readResult (computation: CancellationToken -> Task<Result<'T, 'Error>>) =
    let running = computation CancellationToken.None

    task {
        let! result = running
        return Result.defaultWith (fun _ -> failwith "Expected Ok") result
    }

let private readOption (computation: CancellationToken -> Task<'T option>) =
    let running = computation CancellationToken.None

    task {
        let! result = running
        return Option.get result
    }

let contextCases: (string * bool * (SynchronizationContext -> Task<unit> -> Task<bool>)) list = [
    "cancellableTaskResult",
    true,
    fun context gate ->
        cancellableTaskResult {
            let! () = gate
            return onContext context
        }
        |> readResult
    "cancellableTaskResult",
    false,
    fun context gate ->
        cancellableTaskResult {
            let! () = gate.ConfigureAwait(false)
            return onContext context
        }
        |> readResult
    "cancellableTaskOption",
    true,
    fun context gate ->
        cancellableTaskOption {
            let! () = gate
            return onContext context
        }
        |> readOption
    "cancellableTaskOption",
    false,
    fun context gate ->
        cancellableTaskOption {
            let! () = gate.ConfigureAwait(false)
            return onContext context
        }
        |> readOption
    "cancellableTaskValidation",
    true,
    fun context gate ->
        cancellableTaskValidation {
            let! () = gate
            return onContext context
        }
        |> readResult
    "cancellableTaskValidation",
    false,
    fun context gate ->
        cancellableTaskValidation {
            let! () = gate.ConfigureAwait(false)
            return onContext context
        }
        |> readResult
]

let slotCases: (string * bool * (AsyncLocal<int> -> Task<unit> -> Task<int>)) list = [
    "cancellableTaskResult",
    false,
    fun (slot: AsyncLocal<int>) (_: Task<unit>) ->
        cancellableTaskResult {
            try
                slot.Value <- 42
            finally
                ()

            return slot.Value
        }
        |> readResult
    "cancellableTaskResult",
    true,
    fun (slot: AsyncLocal<int>) (gate: Task<unit>) ->
        cancellableTaskResult {
            try
                for _ in [ 1 ] do
                    slot.Value <- 30
                    do! gate
                    slot.Value <- 42
            finally
                ()

            return slot.Value
        }
        |> readResult
    "cancellableTaskOption",
    false,
    fun (slot: AsyncLocal<int>) (_: Task<unit>) ->
        cancellableTaskOption {
            try
                slot.Value <- 42
            finally
                ()

            return slot.Value
        }
        |> readOption
    "cancellableTaskOption",
    true,
    fun (slot: AsyncLocal<int>) (gate: Task<unit>) ->
        cancellableTaskOption {
            try
                for _ in [ 1 ] do
                    slot.Value <- 30
                    do! gate
                    slot.Value <- 42
            finally
                ()

            return slot.Value
        }
        |> readOption
    "cancellableTaskValidation",
    false,
    fun (slot: AsyncLocal<int>) (_: Task<unit>) ->
        cancellableTaskValidation {
            try
                slot.Value <- 42
            finally
                ()

            return slot.Value
        }
        |> readResult
    "cancellableTaskValidation",
    true,
    fun (slot: AsyncLocal<int>) (gate: Task<unit>) ->
        cancellableTaskValidation {
            try
                for _ in [ 1 ] do
                    slot.Value <- 30
                    do! gate
                    slot.Value <- 42
            finally
                ()

            return slot.Value
        }
        |> readResult
]
