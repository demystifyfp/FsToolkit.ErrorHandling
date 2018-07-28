namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module ResultOption =
  let map f ro =
    Result.map (Option.map f) ro
