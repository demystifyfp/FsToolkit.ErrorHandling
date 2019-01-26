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

  let foldResult onSuccess onError ar =
    Async.map (Result.fold onSuccess onError) ar

  let ofTask aTask = 
    aTask
    |> Async.AwaitTask 
    |> Async.Catch 
    |> Async.map Result.ofChoice
  
  let retn x =
    Ok x
    |> Async.singleton
  
  let returnError x =
    Error x
    |> Async.singleton

  let map2 f xR yR =
    Async.map2 (Result.map2 f) xR yR

  let map3 f xR yR zR =
    Async.map3 (Result.map3 f) xR yR zR

  let apply fAR xAR =
    map2 (fun f x -> f x) fAR xAR

  /// Returns the specified error if the async-wrapped value is false.
  let requireTrue error value =
    value |> Async.map (Result.requireTrue error)

  /// Returns the specified error if the async-wrapped value is true.
  let requireFalse error value =
    value |> Async.map (Result.requireFalse error) 


module AsyncResultOperators =

  let inline (<!>) f x = AsyncResult.map f x
  let inline (<*>) f x = AsyncResult.apply f x
  let inline (>>=) x f = AsyncResult.bind f x

