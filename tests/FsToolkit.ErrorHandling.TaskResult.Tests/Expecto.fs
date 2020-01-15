module Expecto

open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive

let testCaseTask name test = testCaseAsync name (Async.AwaitTask test)
let ptestCaseTask name test = ptestCaseAsync name (Async.AwaitTask test)
let ftestCaseTask name test = ftestCaseAsync name (Async.AwaitTask test)
