namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module AsyncResultOption =  
  let map f aro =
    AsyncResult.map (Option.map f) aro

  let bind f aro =
    let binder opt = 
      match opt with
      | Some x -> f x
      | None -> AsyncResult.retn None
    AsyncResult.bind binder aro
  
  let map2 f arox aroy =
    AsyncResult.map2 (Option.map2 f) arox aroy

  let map3 f arox aroy aroz =
    AsyncResult.map3 (Option.map3 f) arox aroy aroz

  let retn value =
    async { return Ok (Some value) }

  let apply fARO xARO =
    map2 (fun f x -> f x) fARO xARO

module AsyncResultOptionComputationExpression =

  type AsyncResultOptionBuilder() =
    member __.Return value = AsyncResultOption.retn value
    member __.ReturnFrom value = value
    member __.Bind (result, binder) =
      AsyncResultOption.bind binder result

    member __.Combine(aro1, aro2) =
      aro1
      |> AsyncResultOption.bind (fun _ -> aro2)

    member __.Delay f =
      async.Delay f

  let asyncResultOption = new AsyncResultOptionBuilder()


module AsyncResultOptionOperators =

  let inline (<!>) f x = AsyncResultOption.map f x
  let inline (<*>) f x = AsyncResultOption.apply f x
  let inline (>>=) x f = AsyncResultOption.bind f x