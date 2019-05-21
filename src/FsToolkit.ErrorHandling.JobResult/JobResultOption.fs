namespace FsToolkit.ErrorHandling
[<RequireQualifiedAccess>]
module JobResultOption =  
  open Hopac
  let map f tro =
    JobResult.map (Option.map f) tro

  let bind f tro =
    let binder opt = 
      match opt with
      | Some x -> f x
      | None -> JobResult.retn None
    JobResult.bind binder tro
  
  let map2 f trox troy =
    JobResult.map2 (Option.map2 f) trox troy

  let map3 f trox troy aroz =
    JobResult.map3 (Option.map3 f) trox troy aroz

  let retn value =
    Some value
    |> Ok
    |> Job.result

  let apply fTRO xTRO =
    map2 (fun f x -> f x) fTRO xTRO