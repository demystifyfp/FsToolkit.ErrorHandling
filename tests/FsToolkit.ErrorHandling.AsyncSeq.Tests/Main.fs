module FsToolkit.ErrorHandling.AsyncSeq.Tests

#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto

[<Tests>] //needed for `dotnet test` to work
#endif
let allTests = testList "All Tests" [ AsyncSeq.allTests ]

[<EntryPoint>]
let main argv =
#if FABLE_COMPILER_PYTHON
    Pyxpecto.runTests allTests
#endif
#if FABLE_COMPILER_JAVASCRIPT
    Mocha.runTests allTests
#endif
#if !FABLE_COMPILER
    Tests.runTestsWithArgs defaultConfig argv  allTests
#endif
