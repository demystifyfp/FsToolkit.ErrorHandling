module FsToolkit.ErrorHandling.WebSharper.Runner

open WebSharper

[<SPAEntryPoint>]
let run () =
    FsToolkit.ErrorHandling.Tests.main [||]
    |> ignore
