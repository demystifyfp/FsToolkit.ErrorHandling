module FsToolkit.ErrorHandling.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let allTests = testList "All Tests" [
  ResultTests.allTests
  ResultCETests.allTests
  ResultOptionTests.allTests
  OptionTests.allTests
  OptionCETests.allTests
  AsyncOptionCETests.allTests
  ListTests.allTests
  SeqTests.allTests
#if !FABLE_COMPILER
  SeqPerformanceTests.allTests
#endif
  AsyncResultTests.allTests
  AsyncResultCETests.allTests
  AsyncResultOptionTests.allTests
  ValidationTests.allTests
]

[<EntryPoint>]
let main argv =
  #if FABLE_COMPILER
  Mocha.runTests allTests
  #else
  Tests.runTestsWithArgs defaultConfig argv allTests  
  #endif