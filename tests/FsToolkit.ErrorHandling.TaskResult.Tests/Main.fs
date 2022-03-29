module FsToolkit.ErrorHandling.TaskResult.Tests

open Expecto
open FSharp.Control.Tasks
open System.Threading.Tasks
open System
open FsToolkit.ErrorHandling

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
