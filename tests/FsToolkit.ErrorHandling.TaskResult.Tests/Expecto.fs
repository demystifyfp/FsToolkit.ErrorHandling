module Expecto

open Expecto
open FsToolkit.ErrorHandling

let testCaseTask name test =
    testCaseAsync name (async { return! test () |> Async.AwaitTask })

let ptestCaseTask name test =
    ptestCaseAsync name (async { return! test () |> Async.AwaitTask })

let ftestCaseTask name test =
    ftestCaseAsync name (async { return! test () |> Async.AwaitTask })
