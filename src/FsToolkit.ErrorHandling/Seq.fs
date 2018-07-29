namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Seq =

  let traverseResultA f xs =
    List.ofSeq xs
    |> List.traverseResultA f
    |> Result.map Seq.ofList

  let sequenceResultA xs =
    traverseResultA id xs
  
  let traverseResultM f xs =
    List.ofSeq xs
    |> List.traverseResultM f
    |> Result.map Seq.ofList

  let sequenceResultM xs =
    traverseResultM id xs
  
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