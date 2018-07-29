#load "Result.fs"
#load "Validation.fs"
#load "List.fs"
open FsToolkit.ErrorHandling


let tryParseInt x =
  let v = 
    match System.Int32.TryParse x with
    | true, x -> Ok x
    | false, _ -> 
      sprintf "%s is not a valid integer" x
      |> Error
  Validation.ofResult v


let add a b = 
  a + b

open ValidationOperators

let test =
  Validation.map2 add (tryParseInt "1") (tryParseInt "2")
  |> Validation.toResult