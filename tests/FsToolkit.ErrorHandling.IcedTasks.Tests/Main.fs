﻿namespace FsToolkit.ErrorHandling.IcedTasks.Tests

open Expecto

module Main =
    [<EntryPoint>]
    let main argv =
        Tests.runTestsInAssemblyWithCLIArgs [] argv
