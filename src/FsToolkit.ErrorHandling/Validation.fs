namespace FsToolkit.ErrorHandling


type Validation<'a, 'b> =
  Result<'a, 'b list>


[<RequireQualifiedAccess>]
module Validation =

  let apply f x =
    match f, x with
    | Ok f, Ok x -> Ok (f x)
    | Error errs, Ok _ | Ok _, Error errs -> Error [errs]
    | Error errs1, Error errs2 -> Error [errs1;errs2]
  
  let apply2 f x =
    match f, x with
    | Ok f, Ok x -> Ok (f x)
    | Error errs, Ok _ | Ok _, Error errs -> Error errs
    | Error errs1, Error errs2 -> Error  (errs1 @ errs2)

  let ofResult x =
    Result.mapError List.singleton x

  