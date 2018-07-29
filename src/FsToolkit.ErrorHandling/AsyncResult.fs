namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module AsyncResult = 

  let map f ar =
    Async.map (Result.map f) ar

  let mapError f ar =
    Async.map (Result.mapError f) ar    

  let bind f ar = async {
    let! result = ar
    let t = 
      match result with 
      | Ok x -> f x
      | Error e -> async { return Error e }
    return! t      
  }

  let bindResult f r =
    f r 
    |> Async.singleton

  let fold onSuccess onError ar =
    Async.map (Result.fold onSuccess onError) ar

  let ofTask aTask = 
    aTask
    |> Async.AwaitTask 
    |> Async.Catch 
    |> Async.map Result.ofChoice
  
  let retn x =
    Ok x
    |> Async.singleton

  let map2 f xR yR =
    Async.map2 (Result.map2 f) xR yR

  let map3 f xR yR zR =
    Async.map3 (Result.map3 f) xR yR zR

  let apply fAR xAR =
    map2 (fun f x -> f x) fAR xAR


module AsyncResultComputationExpression = 

  type AsyncResultBuilder() =
    member __.Return value = AsyncResult.retn value
    member __.ReturnFrom value = value
    member __.Bind (result, binder) =
      AsyncResult.bind binder result

    member __.Combine(ar1, ar2) =
      ar1
      |> AsyncResult.bind (fun _ -> ar2)

    member __.Delay f =
      async.Delay f

  let asyncResult = AsyncResultBuilder() 


