namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Async = 

  let singleton value = value |> async.Return

  let bind f x = async.Bind(x, f)

  let apply f x =
    bind (fun f' ->
      bind (fun x' -> singleton(f' x')) x) f

  let map f x = x |> bind (f >> singleton)

  let map2 f x y =
    (apply (apply (singleton f) x) y)

  let map3 f x y z =
    apply (map2 f x y) z

  let zip a1 a2 = async {
    let! r1 = a1
    let! r2 = a2
    return r1,r2
  }

module AsyncOperators =

  let inline (<!>) f x = Async.map f x
  let inline (<*>) f x = Async.apply f x
  let inline (>>=) x f = Async.bind f x
