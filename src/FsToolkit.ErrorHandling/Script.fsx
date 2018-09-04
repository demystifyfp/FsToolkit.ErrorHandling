#load "Result.fs"
#load "ResultCE.fs"
#load "Async.fs"
#load "Validation.fs"
#load "ValidationOp.fs"
#load "AsyncResult.fs"
#load "ResultOption.fs"

open System
open FsToolkit.ErrorHandling.Operator.Validation

let add a b = a + b

let tryParseInt x =
  match System.Int32.TryParse x with
  | true, x -> 
    printfn "good int: %A" x
    Ok x
  | false, _ -> 
    printfn "bad int: %A" x
    x |> sprintf "invalid value %A" |> List.singleton |> Error 

let tryParseInt2 x =
  match System.Int32.TryParse x with
  | true, x -> 
    printfn "good int: %A" x
    Ok x
  | false, _ -> 
    printfn "bad int: %A" x
    x |> sprintf "invalid value %A" |> Error 


type PersonName = private PersonName of string with
  member this.Value =
    let (PersonName name) = this
    name

  static member TryCreate (name : string) =
    match name with
    | x when String.IsNullOrEmpty x -> 
      Error "Name shouldn't be empty"
    | x when x.Length > 80 ->
      Error "Name shouldn't contain more than 80 characters"
    | x -> Ok (PersonName x)
    
let res = add <!> tryParseInt "10" <*^> tryParseInt2 "b"