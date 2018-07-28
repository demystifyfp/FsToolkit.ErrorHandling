namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Result =

  let apply f x =
    Result.bind (fun f' ->
      Result.bind (fun x' -> Ok (f' x')) x) f

  let map2 f x y =
    (apply (apply (Ok f) x) y)
  
  let map3 f x y z =
    apply (map2 f x y) z

  let traverseSeq f seq =
    let cons head tail = head :: tail
    let initState = Ok []
    let folder head tail =
      map2 cons (f head) tail
    Seq.foldBack folder seq initState
    |> Result.map List.toSeq

  let traverseOption f opt =
    match opt with
    | None -> Ok None
    | Some v -> f v |> Result.map Some



module ResultOperators =

  let inline (<!>) f x = Result.map f x
  let inline (<*>) f x = Result.apply f x
  let inline (>>=) f x = Result.bind f x
