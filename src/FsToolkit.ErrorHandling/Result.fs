namespace FsToolkit.ErrorHandling

module ResultComputationExpression =

  type ResultBuilder() =
    member __.Return value = Ok value
    member __.ReturnFrom value = value

    member __.Bind (result, binder) =
      Result.bind binder result

    member __.Combine(r1, r2) =
      r1
      |> Result.bind (fun _ -> r2)

    member __.Delay f = f ()


  let result = ResultBuilder()



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


module ResultOperators =

  let inline (<!>) f x = Result.map f x
  let inline (<*>) f x = Result.apply f x
  let inline (>>=) x f = Result.bind f x
