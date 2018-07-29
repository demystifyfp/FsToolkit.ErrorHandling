namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Seq =
  let traverseResultA f xs =
    List.ofSeq xs
    |> List.traverseResultA f
    |> Result.map Seq.ofList
  
  let traverseResultM f xs =
    List.ofSeq xs
    |> List.traverseResultM f
    |> Result.map Seq.ofList
  
  let traverseValidationA f xs =
    List.ofSeq xs
    |> List.traverseValidationA f
    |> Validation.map Seq.ofList