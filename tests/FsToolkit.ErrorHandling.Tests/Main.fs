module FsToolkit.ErrorHandling.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto

[<Tests>] //needed for `dotnet test` to work
#endif
let allTests =
    testList "All Tests" [
        ResultTests.allTests
        ResultCETests.allTests
        ResultOptionTests.allTests
        OptionTests.allTests
        OptionCETests.allTests
        AsyncOptionTests.allTests
        AsyncOptionCETests.allTests
        ListTests.allTests
        SeqTests.allTests
        AsyncResultTests.allTests
        AsyncResultCETests.allTests
        AsyncResultOptionTests.allTests
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
