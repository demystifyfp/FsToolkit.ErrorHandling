#I @"./../src/FsToolkit.ErrorHandling"
#load "Result.fs"
#load "Async.fs"
#load "AsyncResult.fs"
#load "ResultCE.fs"
#load "AsyncResultCE.fs"
#load "ResultOp.fs"
#load "ResultOption.fs"
#load "ResultOptionCE.fs"
#load "ResultOptionOp.fs"
#load "Validation.fs"
#load "ValidationOp.fs"
#load "Option.fs"
#load "List.fs"

open System
open FsToolkit.ErrorHandling

let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> 
    Error (sprintf "unable to parse '%s' to integer" str)

["1";"foo";"3";"bar"]
|> List.traverseResultA tryParseInt 
