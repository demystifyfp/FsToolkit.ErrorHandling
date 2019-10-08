namespace FsToolkit.ErrorHandling

#if FABLE_COMPILER
//https://github.com/fable-compiler/Fable/issues/1842
module Option = 
  let map2 f a b =
    a 
    |> Option.bind (fun a' -> 
      b |> Option.map (fun b' -> f a' b'))
  let map3 f a b c =
    a 
    |> Option.bind (fun a' -> 
      b |> Option.bind (fun b' -> 
        c |> Option.map(fun c' -> f a' b' c')))
#endif
[<RequireQualifiedAccess>]
module AsyncResultOption =  
  let map f aro =
    AsyncResult.map (Option.map f) aro

  let bind f aro =
    let binder opt = 
      match opt with
      | Some x -> f x
      | None -> AsyncResult.retn None
    AsyncResult.bind binder aro
  
  let map2 f arox aroy =
    AsyncResult.map2 (Option.map2 f) arox aroy

  let map3 f arox aroy aroz =
    AsyncResult.map3 (Option.map3 f) arox aroy aroz

  let retn value =
    async { return Ok (Some value) }

  let apply fARO xARO =
    map2 (fun f x -> f x) fARO xARO

  /// Replaces the wrapped value with unit
  let ignore aro =
      aro |> map ignore