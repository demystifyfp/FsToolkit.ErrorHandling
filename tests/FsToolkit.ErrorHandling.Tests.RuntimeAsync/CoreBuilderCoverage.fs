module CoreBuilderCoverage

open Expecto
open FsToolkit.ErrorHandling
open BuilderCoverage

let allTests =
    let probeModule = RuntimeAsyncTestHelpers.moduleOf typeof<CoreBuilderProbes.Marker>

    let controlModule =
        RuntimeAsyncTestHelpers.moduleOf typeof<StateMachineProbes.Marker>

    testList "Core builders" [
        resolutionTests CoreBuilderProbes.all
        suspensionTests probeModule CoreBuilderProbes.all
        backgroundTests CoreBuilderProbes.all
        scopeTests probeModule.Assembly [ controlModule ] StateMachineProbes.controls
        libraryTests [ typeof<TaskResultBuilder>.Assembly ]
    ]
