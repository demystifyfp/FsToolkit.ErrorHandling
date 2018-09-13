namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Validation =

  let ofResult x =
    Result.mapError List.singleton x
  
  let apply f x =
    match f, x with
    | Ok f, Ok x -> Ok (f x)
    | Error errs, Ok _ | Ok _, Error errs -> Error errs
    | Error errs1, Error errs2 -> Error  (errs1 @ errs2)

  let retn x = ofResult (Ok x)
  
  let map2 f x y =
    apply (apply (retn f) x) y
  
  let map3 f x y z =
    apply (map2 f x y) z