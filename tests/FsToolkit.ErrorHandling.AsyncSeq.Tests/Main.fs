module FsToolkit.ErrorHandling.AsyncSeq.Tests

#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#endif
#if !FABLE_COMPILER
open Expecto

[<Tests>] //needed for `dotnet test` to work
#endif
let allTests = testList "All Tests" [ AsyncSeq.allTests ]

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
