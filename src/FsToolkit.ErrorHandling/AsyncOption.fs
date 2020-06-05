namespace FsToolkit.ErrorHandling

open System.Threading.Tasks

[<RequireQualifiedAccess>]
module AsyncOption = 
    
    let inline map f ar =
        Async.map (Option.map f) ar