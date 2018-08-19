namespace FsToolkit.ErrorHandling

open ResultComputationExpression
open AsyncResultComputationExpression

[<RequireQualifiedAccess>]
module List =

  let private cons' head tail = head :: tail
  

  let rec private traverseResult' state f xs =
    match xs with
    | [] -> state
    | x :: xs -> 
      let r = result {
        let! y = f x
        let! ys = state
        return ys @ [y]
      }  
      match r with
      | Ok _ -> traverseResult' r f xs
      | Error _ -> r

  let traverseResult f xs =
    traverseResult' (Ok []) f xs

  let sequenceResult xs =
    traverseResult id xs

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

  
 