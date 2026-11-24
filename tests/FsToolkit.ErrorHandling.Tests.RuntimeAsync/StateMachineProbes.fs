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
