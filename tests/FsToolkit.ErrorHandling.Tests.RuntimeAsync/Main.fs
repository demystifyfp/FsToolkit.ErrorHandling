module FsToolkit.ErrorHandling.Tests.RuntimeAsync

open Expecto

[<Tests>]
let allTests =
    testList "All Tests" [
        TaskResultCETests.allTests
        BackgroundTaskResultCETests.allTests
        TaskOptionCETests.allTests
        TaskValueOptionCETests.allTests
        BackgroundTaskOptionCETests.allTests
        BackgroundTaskValueOptionCETests.allTests
        TaskValidationCETests.allTests
        TaskResultOptionTests.allTests
        TaskResultTests.allTests
        TaskConfigureAwaitTests.allTests
        TaskResultRuntimeAsyncTests.allTests
        CoreBuilderCoverage.allTests
    ]

[<EntryPoint>]
let main argv =
    Tests.runTestsWithCLIArgs [] argv allTests
