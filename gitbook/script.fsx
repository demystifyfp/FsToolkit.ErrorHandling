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

// string -> Result<int, string>
let tryParseInt str : Result<int, string> =
    Option.tryParse<int> str
    |> Result.requireSomeWith (fun () -> $"unable to parse '{str}' to integer")

let result =
    resultOption {
        // do! Some "a" |> Option.ignore<int> // WOULD NOT COMPILE
        do!
            Some 21
            |> Option.ignore<int>

        return 42
    }

let tryParseInt (str: string) =
    match Option.tryParse<int> str with
    | None -> Error $"unable to parse '{str}' to integer"
    | Some x -> Ok x

[
    "1"
    "foo"
    "3"
    "bar"
]
|> List.traverseResultA tryParseInt
