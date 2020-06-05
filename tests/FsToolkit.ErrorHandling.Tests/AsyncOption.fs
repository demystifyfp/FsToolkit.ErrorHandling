module AsyncOptionTests


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open TestData
open TestHelpers
open SampleDomain
open FsToolkit.ErrorHandling
// open FsToolkit.ErrorHandling.Operator.AsyncOption

let mapTests =
    testList "AsyncOption.map Tests" [
      testCaseAsync "map with Async(Some x)" <| 
        (Async.singleton (Some validTweet)
         |> AsyncOption.map remainingCharacters
         |> Expect.hasAsyncSomeValue 267)
      
      testCaseAsync "map with Async(None)" <| 
        (Async.singleton (None)
         |> AsyncOption.map remainingCharacters
         |> Expect.hasAsyncNoneValue)
    ]

let allTests = testList "Async Option Tests" [
  mapTests
]