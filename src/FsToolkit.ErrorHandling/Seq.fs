namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Seq =
  
  let traverseResult f xs =
    List.ofSeq xs
    |> List.traverseResult f
    |> Result.map Seq.ofList

  let sequenceResultM xs =
    traverseResult id xs
  
  let traverseAsyncResultA f xs =
    List.ofSeq xs
    |> List.traverseAsyncResultA f
    |> AsyncResult.map Seq.ofList

  let sequenceAsyncResultA xs =
    traverseAsyncResultA id xs
  
  let traverseAsyncResultM f xs =
    List.ofSeq xs
    |> List.traverseAsyncResultM f
    |> AsyncResult.map Seq.ofList

  let sequenceAsyncResultM xs =
    traverseAsyncResultM id xs
  
  let traverseValidationA f xs =
    List.ofSeq xs
    |> List.traverseValidationA f
    |> Result.map Seq.ofList

  let sequenceValidationA xs =
    traverseValidationA id xs