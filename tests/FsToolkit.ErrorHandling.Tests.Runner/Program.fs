module FsToolkit.ErrorHandling.Tests.Runner

open FsToolkit.ErrorHandling.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

[<EntryPoint>]
let main argv =
  #if FABLE_COMPILER
  Mocha.runTests allTests
  #else
  Tests.runTestsWithArgs defaultConfig argv allTests
  #endif