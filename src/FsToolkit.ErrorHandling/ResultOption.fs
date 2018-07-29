namespace FsToolkit.ErrorHandling


[<RequireQualifiedAccess>]
module ResultOption =

  let map f ro =
    Result.map (Option.map f) ro
  
  let bind f ro =
    Result.bind (function | Some x -> f x | None -> Ok None) ro

  let retn x =
    Ok (Some x)

  let apply f x =
    bind (fun f' ->
      bind (fun x' -> retn (f' x')) x) f

  let map2 f x y =
    (apply (apply (retn f) x) y)
  
  let map3 f x y z =
    apply (map2 f x y) z

module ResultOptionComputationExpression =

  type ResultOptionBuilder() =
    member __.Return value = ResultOption.retn value
    member __.ReturnFrom value = value

    member __.Bind (result, binder) =
      ResultOption.bind binder result

    member __.Combine(r1, r2) =
      r1
      |> ResultOption.bind (fun _ -> r2)

    member __.Delay f = f ()


  let resultOption = ResultOptionBuilder()

module ResultOptionOperators =

  let inline (<!>) f x = ResultOption.map f x
  let inline (<*>) f x = ResultOption.apply f x
  let inline (>>=) f x = ResultOption.bind f x