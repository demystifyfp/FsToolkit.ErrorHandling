#load "Result.fs"
#load "Validation.fs"
#load "List.fs"
open FsToolkit.ErrorHandling


let tryParseInt x =
  match System.Int32.TryParse x with
  | true, x -> Ok x
  | false, _ -> 
    sprintf "%s is not a valid integer" x
    |> Error


let add a b c = 
  a + b + c

open ValidationOperators

let test =
  Validation.retn add 
  <*^> (tryParseInt "1") 
  <*^> (tryParseInt "2")
  <*^> (tryParseInt "3")