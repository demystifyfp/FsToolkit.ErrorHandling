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


module ValidationOperators =
  
  let inline (<!>) f x = Result.map f x
  let inline (<*>) f x = Validation.apply f x

  let inline (<*^>) f x = Validation.apply f (Validation.ofResult x)