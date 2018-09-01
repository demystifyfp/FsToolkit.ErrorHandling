#load "Result.fs"
open System.Runtime.Remoting.Messaging
#load "Async.fs"
#load "Validation.fs"
#load "AsyncResult.fs"
#load "ResultOption.fs"
#load "List.fs"
open System.Threading.Tasks

open FsToolkit.ErrorHandling

let t = new Task<int>(fun _ -> 42)
t.Result
let a = async {
  let! r = AsyncResult.ofTask t
  return r
}
Async.RunSynchronously a

let tryParseInt x =
  match System.Int32.TryParse x with
  | true, x -> 
    printfn "good int: %A" x
    Ok (Some x)
  | false, _ -> 
    printfn "bad int: %A" x
    x |> sprintf "invalid value %A" |> Error


List.traverseResultA tryParseInt ["1"; "2"; "3"; "4"; "5"]
List.traverseResultA tryParseInt ["a"; "2"; "b"; "4"; "c"]