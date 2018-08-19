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

  let traverseResultM f xs =
    traverseResultM' (Ok []) f xs
  
  let sequenceResultM xs =
    traverseResultM id xs

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
    let cons head tail = head :: tail
    let initState = AsyncResult.retn []
    let folder head tail = 
      AsyncResult.map2 cons (f head) tail
    List.foldBack folder xs initState

  let sequenceAsyncResultA xs =
    traverseAsyncResultA id xs
  

  let traverseAsyncResultM f xs =
    let cons head tail = head :: tail
    let initState = AsyncResult.retn []
    let folder head tail = asyncResult {
      let! h = f head
      let! t = tail
      return (cons h t)
    }
    List.foldBack folder xs initState

 
  let sequenceAsyncResultM xs =
    traverseAsyncResultM id xs

  
 