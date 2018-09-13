#I @"./../src/FsToolkit.ErrorHandling"
#load "Create.fs"
#load "Result.fs"
#load "ResultCE.fs"
#load "Validation.fs"
#load "ValidationOp.fs"

open System
open FsToolkit.ErrorHandling
let add a b = a + b
// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> Error (sprintf "unable to parse '%s' to integer" str)


Result.map2 add (tryParseInt "40") (tryParseInt "2")