module FsToolkit.ErrorHandling.Sources.Generator.Tests.Main

open Expecto

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssemblyWithCLIArgs [] argv
