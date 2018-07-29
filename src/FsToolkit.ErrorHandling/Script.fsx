#load "Result.fs"
#load "ResultOption.fs"

open FsToolkit.ErrorHandling


let tryParseInt x =
  match System.Int32.TryParse x with
  | true, x -> Ok (Some x)
  | false, _ -> Ok None

let add a b c = 
  a + b + c

open ResultOptionOperators

let test () =
  ResultOption.retn add 
    <*> tryParseInt "1"
    <*> tryParseInt "2"
    <*> tryParseInt "3"

test ()