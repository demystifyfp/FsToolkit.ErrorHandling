module FsToolkit.ErrorHandling.Tests

#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
//needed for `dotnet test` to work
[<Tests>]
#endif
let allTests =
    testList "All Tests" [
        ResultTests.allTests
        ResultCETests.allTests
        ResultOptionTests.allTests
        ResultOptionCETests.allTests
        OptionTests.allTests
        OptionCETests.allTests
        AsyncOptionTests.allTests
        AsyncOptionCETests.allTests
        ListTests.allTests
        ArrayTests.allTests
        SeqTests.allTests
        AsyncResultTests.allTests
        AsyncResultCETests.allTests
        AsyncResultOptionTests.allTests
        AsyncResultOptionCETests.allTests
        AsyncValidationTests.allTests
        AsyncValidationCETests.allTests
        ValidationTests.allTests
        ValidationCETests.allTests
        ValueOptionTests.allTests
        ValueOptionCETests.allTests
        ParallelAsyncResultTests.allTests
        ParallelAsyncResultCETests.allTests
        ParallelAsyncValidationTests.allTests
        ParallelAsyncValidationCETests.allTests
        #if !FABLE_COMPILER
        BackgroundTaskOptionCETests.allTests
        BackgroundTaskResultCETests.allTests
        TaskOptionTests.allTests
        TaskOptionCETests.allTests
        TaskResultTests.allTests
        TaskResultCETests.allTests
        TaskResultOptionTests.allTests
        TaskValidationTests.allTests
        TaskValidationCETests.allTests
        #endif
    ]

[<EntryPoint>]
let main argv =
#if FABLE_COMPILER_PYTHON
    Pyxpecto.runTests allTests
#endif
#if FABLE_COMPILER_JAVASCRIPT
    Mocha.runTests allTests
#endif
#if !FABLE_COMPILER
    Tests.runTestsWithCLIArgs [] Array.empty allTests
#endif
