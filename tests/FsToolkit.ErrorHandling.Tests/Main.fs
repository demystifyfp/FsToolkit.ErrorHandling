module FsToolkit.ErrorHandling.Tests

#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
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


// This is possibly the most magic used to make this work. 
// Js and ts cannot use `Async.RunSynchronously`, instead they use `Async.StartAsPromise`.
// Here we need the transpiler not to worry about the output type.
#if !FABLE_COMPILER_JAVASCRIPT && !FABLE_COMPILER_TYPESCRIPT
let (!!) (any: 'a) = any
#endif
#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
open Fable.Core.JsInterop
#endif

[<EntryPoint>]
let main argv =
#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT || FABLE_COMPILER_PYTHON
    !!Pyxpecto.runTests  [||] allTests
#endif
#if !FABLE_COMPILER
    Tests.runTestsWithCLIArgs [] Array.empty allTests
#endif
