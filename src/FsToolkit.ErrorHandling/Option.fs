namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Option =

  let traverseResult f opt =
    match opt with
    | None -> Ok None
    | Some v -> f v |> Result.map Some

  let sequenceResult opt = 
    traverseResult id opt

  #if !FABLE_COMPILER
  let inline tryParse< ^T when ^T : (static member TryParse : string * byref< ^T > -> bool) and  ^T : (new : unit -> ^T) > valueToParse =
    let mutable output = new ^T()
    let parsed = ( ^T : (static member TryParse  : string * byref< ^T > -> bool ) (valueToParse, &output) )
    match parsed with
    | true -> Some output
    | _ -> None
  #endif
