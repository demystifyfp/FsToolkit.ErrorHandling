module FsToolkit.ErrorHandling.Tests.RuntimeAsync

open Expecto

[<Tests>]
let allTests =
    testList "All Tests" [
        TaskResultCETests.allTests
        TaskConfigureAwaitTests.taskResultTests
        TaskResultRuntimeAsyncTests.allTests
    ]

[<EntryPoint>]
let main argv =
    Tests.runTestsWithCLIArgs [] argv allTests
