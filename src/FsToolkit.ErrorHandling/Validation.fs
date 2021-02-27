namespace FsToolkit.ErrorHandling

type Validation<'a,'err> = Result<'a, 'err list>

[<RequireQualifiedAccess>]
module Validation =

  let ok x : Validation<_,_> = Ok x
  let error e : Validation<_,_> = List.singleton e |> Error

  let ofResult x : Validation<_,_> =
    Result.mapError List.singleton x

  let ofChoice x : Validation<_,_> =
    Result.ofChoice x
    |> ofResult
  
  let apply f (x : Validation<_,_>) : Validation<_,_> =
    match f, x with
    | Ok f, Ok x -> Ok (f x)
    | Error errs, Ok _ | Ok _, Error errs -> Error errs
    | Error errs1, Error errs2 -> Error  (errs1 @ errs2)

  let retn x = ok x

  let map f (x : Validation<_,_>) : Validation<_,_>= Result.map f x
  
  let map2 f (x : Validation<_,_>) (y : Validation<_,_>) : Validation<_,_> =
    apply (apply (retn f) x) y
  
  let map3 f (x : Validation<_,_>) (y : Validation<_,_>) (z : Validation<_,_>) : Validation<_,_> =
    apply (map2 f x y) z

  let mapError f (x : Validation<_,_>) : Validation<_,_> =
    x |> Result.mapError (List.map f)

  let mapErrors f (x : Validation<_,_>) : Validation<_,_> =
    x |> Result.mapError (f)

  let bind (f : 'a -> Validation<'b, 'err>) (x : Validation<'a,'err>) : Validation<_,_>=
    Result.bind f x

  let zip (x1: Validation<_,_>) (x2 : Validation<_,_>) : Validation<_,_> = 
    match x1,x2 with
    | Ok x1res, Ok x2res -> Ok (x1res, x2res)
    | Error e, Ok _ -> Error e
    | Ok _, Error e -> Error e
    | Error e1, Error e2 -> Error (e1 @ e2)