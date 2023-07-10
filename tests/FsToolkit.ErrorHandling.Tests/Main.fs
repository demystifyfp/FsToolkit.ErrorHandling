module FsToolkit.ErrorHandling.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

//needed for `dotnet test` to work
[<Tests>]
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
    ]


[<EntryPoint>]
let main argv =
#if FABLE_COMPILER
    Mocha.runTests allTests
#else
    Tests.runTestsWithCLIArgs [] Array.empty allTests
#endif
