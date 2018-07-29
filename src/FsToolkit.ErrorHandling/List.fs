namespace FsToolkit.ErrorHandling

open ResultComputationExpression
open AsyncResultComputationExpression

[<RequireQualifiedAccess>]
module List =
  let traverseResultA f xs =
    let cons head tail = head :: tail
    let initState = Ok []
    let folder head tail =
      Result.map2 cons (f head) tail
    List.foldBack folder xs initState

  let sequenceResultA xs =
    traverseResultA id xs

  let traverseResultM f xs =
    let cons head tail = head :: tail
    let initState = Ok []
    let folder head tail = result {
      let! h = f head
      let! t = tail
      return (cons h t)
    }
    List.foldBack folder xs initState

  let sequenceResultM xs =
    traverseResultM id xs

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

  
 