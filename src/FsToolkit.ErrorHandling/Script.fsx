#load "Result.fs"
#load "Async.fs"
#load "Validation.fs"
#load "AsyncResult.fs"
#load "ResultOption.fs"
#load "List.fs"

open FsToolkit.ErrorHandling


let tryParseInt x =
  match System.Int32.TryParse x with
  | true, x -> 
    printfn "good int: %A" x
    Ok (Some x)
  | false, _ -> 
    printfn "bad int: %A" x
    x |> sprintf "invalid value %A" |> Error

let traverseResultA f xs =
  let cons head tail = head :: tail
  let initState = Ok []
  let folder head tail =
    Result.map2 cons (f head) tail
  List.foldBack folder xs initState

List.traverseResultA tryParseInt ["1"; "s"; "2"; "q"]
traverseResultA tryParseInt ["1"; "3"; "2"; "q"]
List.traverseResultM tryParseInt ["1"; "s"; "2"; "q"]

let add a b c = 
  a + b + c

open ResultOptionOperators

let test () =
  ResultOption.retn add 
    <*> tryParseInt "1"
    <*> tryParseInt "2"
    <*> tryParseInt "3"

test ()