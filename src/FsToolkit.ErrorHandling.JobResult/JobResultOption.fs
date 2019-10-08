namespace FsToolkit.ErrorHandling
[<RequireQualifiedAccess>]
module JobResultOption =  
  open Hopac
  let map f jro =
    JobResult.map (Option.map f) jro

  let bind f jro =
    let binder opt = 
      match opt with
      | Some x -> f x
      | None -> JobResult.retn None
    JobResult.bind binder jro
  
  let map2 f xJRO yJRO =
    JobResult.map2 (Option.map2 f) xJRO yJRO

  let map3 f xJRO yJRO zJRO =
    JobResult.map3 (Option.map3 f) xJRO yJRO zJRO

  let retn value =
    Some value
    |> Ok
    |> Job.result

  let apply fJRO xJRO =
    map2 (fun f x -> f x) fJRO xJRO

  /// Replaces the wrapped value with unit
  let ignore jro =
      jro |> map ignore