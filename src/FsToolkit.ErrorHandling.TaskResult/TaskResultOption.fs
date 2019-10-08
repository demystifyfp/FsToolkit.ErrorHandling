namespace FsToolkit.ErrorHandling
open FSharp.Control.Tasks.V2.ContextInsensitive
[<RequireQualifiedAccess>]
module TaskResultOption =  
  let map f tro =
    TaskResult.map (Option.map f) tro

  let bind f tro =
    let binder opt = 
      match opt with
      | Some x -> f x
      | None -> TaskResult.retn None
    TaskResult.bind binder tro
  
  let map2 f xTRO yTRO =
    TaskResult.map2 (Option.map2 f) xTRO yTRO

  let map3 f xTRO yTRO zTRO =
    TaskResult.map3 (Option.map3 f) xTRO yTRO zTRO

  let retn value =
    task { return Ok (Some value) }

  let apply fTRO xTRO =
    map2 (fun f x -> f x) fTRO xTRO

  /// Replaces the wrapped value with unit
  let ignore tro =
      tro |> map ignore