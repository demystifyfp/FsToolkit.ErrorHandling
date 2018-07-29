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
  open ResultComputationExpression

  let apply f x =
    Result.bind (fun f' ->
      Result.bind (fun x' -> Ok (f' x')) x) f

  let map2 f x y =
    (apply (apply (Ok f) x) y)
  
  let map3 f x y z =
    apply (map2 f x y) z


module ResultOperators =

  let inline (<!>) f x = Result.map f x
  let inline (<*>) f x = Result.apply f x
  let inline (>>=) f x = Result.bind f x
