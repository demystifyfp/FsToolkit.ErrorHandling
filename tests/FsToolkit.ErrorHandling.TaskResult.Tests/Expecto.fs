module Expecto

open Expecto
open FsToolkit.ErrorHandling

let testCaseTask name test = testCaseAsync name (Async.AwaitTask test)
let ptestCaseTask name test = ptestCaseAsync name (Async.AwaitTask test)
let ftestCaseTask name test = ftestCaseAsync name (Async.AwaitTask test)
