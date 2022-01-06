module FsToolkit.ErrorHandling.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let allTests =
    testList
        "All Tests"
        [ ResultTests.allTests
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
          ValueOptionCETests.allTests ]
