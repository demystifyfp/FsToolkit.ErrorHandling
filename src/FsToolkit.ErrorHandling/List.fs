namespace FsToolkit.ErrorHandling

open ResultComputationExpression

[<RequireQualifiedAccess>]
module List =
  let traverseResultA f xs =
    let cons head tail = head :: tail
    let initState = Ok []
    let folder head tail =
      Result.map2 cons (f head) tail
    List.foldBack folder xs initState

  let traverseResultM f xs =
    let cons head tail = head :: tail
    let initState = Ok []
    let folder head tail = result {
      let! h = f head
      let! t = tail
      return (cons h t)
    }
    List.foldBack folder xs initState

  let traverseValidationA f xs =
    let cons head tail = head :: tail
    let initState = Validation.retn []
    let folder head tail =
      Validation.map2 cons (f head) tail
    List.foldBack folder xs initState
 