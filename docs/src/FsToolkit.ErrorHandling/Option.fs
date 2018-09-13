namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Option =

  let traverseResult f opt =
    match opt with
    | None -> Ok None
    | Some v -> f v |> Result.map Some

  let sequenceResult opt = 
    traverseResult id opt