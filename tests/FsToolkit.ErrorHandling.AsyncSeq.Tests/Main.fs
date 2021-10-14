module FsToolkit.ErrorHandling.AsyncSeq.Tests

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let allTests = testList "All Tests" [
  AsyncSeq.allTests
]
