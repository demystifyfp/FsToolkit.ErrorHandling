namespace FsToolkit.ErrorHandling


type Validation<'a, 'b> = 
  private Validation of 'a option * 'b list

[<RequireQualifiedAccess>]
module Validation =

  let map f v =
    match v with
    | Validation (Some x, []) -> 
      Validation (Some (f x), [])
    | Validation (Some _, xs) -> 
      Validation (None, xs)
    | Validation (None,y) -> 
      Validation (None,y)

  let ofResult x =
    match x with
    | Ok x -> Validation (Some x, []) 
    | Error e -> Validation (None, [e]) 
  
  let ofResult2 x =
    match x with
    | Ok x -> Validation (Some x, []) 
    | Error e -> Validation (None, e)
  
  let apply f x =
    match f, x with
    | Validation (Some f, []), Validation(Some x, []) -> 
      Validation (Some (f x), [])
    | Validation (_, errs1), Validation(_, errs2) -> 
      Validation (None, (errs1 @ errs2))

  let retn x = Validation (Some x, [])

  let map2 f x y =
    apply (apply (retn f) x) y
  
  let map3 f x y z =
    apply (map2 f x y) z

  let toResult v =
    match v with
    | Validation (Some x, []) -> Ok x
    | Validation (_, xs) -> Error xs



module ValidationOperators =
  
  let inline (<!>) f x = Validation.map f x

  let inline (<*>) f x = Validation.apply f x