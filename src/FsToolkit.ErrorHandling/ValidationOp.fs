namespace FsToolkit.ErrorHandling.Operator.Validation

open FsToolkit.ErrorHandling

[<AutoOpen>]
module Validation =
  
  let inline (<!>) f (x : Result<'a, 'b list>) = Result.map f x
  let inline (<!^>) f x = 
    x
    |> Result.mapError List.singleton
    |> Result.map f
  let inline (<*>) f x = Validation.apply f x
  let inline (<*^>) f x = Validation.apply f (Validation.ofResult x)