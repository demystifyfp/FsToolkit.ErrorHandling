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

  let fold onOk onError r =
    match r with
    | Ok x -> onOk x
    | Error y -> onError y

  let ofChoice c =
    match c with
    | Choice1Of2 x -> Ok x
    | Choice2Of2 x -> Error x

  let inline tryCreate fieldName (x : 'a) : Result< ^b, (string * 'c)> =
    let tryCreate' x = 
      (^b : (static member TryCreate : 'a -> Result< ^b, 'c>) x)
    tryCreate' x
    |> Result.mapError (fun z -> (fieldName, z))