namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Async =
    let singleton (value: 'T) : Async<'T> = async { return value }
