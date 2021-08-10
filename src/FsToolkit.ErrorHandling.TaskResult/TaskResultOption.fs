namespace FsToolkit.ErrorHandling

open FSharp.Control.Tasks.Affine

[<RequireQualifiedAccess>]
module TaskResultOption =  
  let inline map f (tro : ^a) =
    TaskResult.map (Option.map f) tro

  let inline bind (f : _ -> ^b) (tro : ^a) = task {
    match! tro with
    | Ok (Some x) ->
      return! f x
    | Ok (None) ->
      return Ok None
    | Error (e) ->
      return Error e
  }
  
  let inline map2 f (xTRO : ^a) (yTRO : ^b) =
    TaskResult.map2 (Option.map2 f) xTRO yTRO

  let inline map3 f (xTRO : ^a) (yTRO : ^b) (zTRO : ^c) =
    TaskResult.map3 (Option.map3 f) xTRO yTRO zTRO

  let inline retn value =
    task { return Ok (Some value) }

  let inline apply (fTRO : ^b) (xTRO : ^a) =
    map2 (fun f x -> f x) fTRO xTRO

  /// Replaces the wrapped value with unit
  let inline ignore (tro : ^a) =
      tro |> map ignore
