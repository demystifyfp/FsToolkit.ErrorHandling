module FsToolkit.ErrorHandling.Fable.Tests.Runner

open FsToolkit.ErrorHandling

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let allTests =
    testList
        "Fable Tests"
        [ Tests.allTests
          AsyncSeq.Tests.allTests ]

[<EntryPoint>]
let main argv =
#if FABLE_COMPILER
    Mocha.runTests allTests
#else
    Tests.runTestsWithArgs defaultConfig argv allTests
#endif
