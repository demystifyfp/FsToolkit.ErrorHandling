namespace FsToolkit.ErrorHandling

open ResultComputationExpression
open AsyncResultComputationExpression

[<RequireQualifiedAccess>]
module List =

  let rec private traverseResultM' state f xs =
    match xs with
    | [] -> state
    | x :: xs -> 
      let r = result {
        let! y = f x
        let! ys = state
        return ys @ [y]
      }  
      match r with
      | Ok _ -> traverseResultM' r f xs
      | Error _ -> r

  let rec private traverseAsyncResultM' state f xs =
    match xs with
    | [] -> state
    | x :: xs -> 
      async {
        let! r = asyncResult {
          let! ys = state
          let! y = f x
          return ys @ [y]
        }  
        match r with
        | Ok _ -> 
          return! traverseAsyncResultM' (Async.singleton r) f xs
        | Error _ -> return r
      }

  let traverseResultM f xs =
    traverseResultM' (Ok []) f xs
  
  let sequenceResultM xs =
    traverseResultM id xs

  let traverseAsyncResultM f xs =
    traverseAsyncResultM' (AsyncResult.retn []) f xs

  let sequenceAsyncResultM xs =
    traverseAsyncResultM id xs
  

  let rec private traverseResultA' state f xs =
    match xs with
    | [] -> state
    | x :: xs ->
      let fR = 
        f x |> Result.mapError List.singleton
      match state, fR with
      | Ok ys, Ok y -> 
        traverseResultA' (Ok (ys @ [y])) f xs
      | Error errs, Error e -> 
        traverseResultA' (Error (e @ errs)) f xs
      | Ok _, Error e | Error e , Ok _  -> 
        traverseResultA' (Error e) f xs

  let rec private traverseAsyncResultA' state f xs =
    match xs with
    | [] -> state
    | x :: xs ->
      async {
        let! s = state
        let! fR = f x |> AsyncResult.mapError List.singleton
        match s, fR with
        | Ok ys, Ok y -> 
          return! traverseAsyncResultA' (AsyncResult.retn (ys @ [y])) f xs
        | Error errs, Error e -> 
          return! traverseAsyncResultA' (AsyncResult.returnError (errs @ e)) f xs
        | Ok _, Error e | Error e , Ok _  -> 
          return! traverseAsyncResultA' (AsyncResult.returnError  e) f xs
      }

  let traverseResultA f xs =
    traverseResultA' (Ok []) f xs

  let sequenceResultA xs =
    traverseResultA id xs

  let traverseValidationA f xs =
    let cons head tail = head :: tail
    let initState = Validation.retn []
    let folder head tail =
      Validation.map2 cons (f head) tail
    List.foldBack folder xs initState

  let sequenceValidationA xs =
    traverseValidationA id xs


  let traverseAsyncResultA f xs =
    traverseAsyncResultA' (AsyncResult.retn []) f xs

  let sequenceAsyncResultA xs =
    traverseAsyncResultA id xs