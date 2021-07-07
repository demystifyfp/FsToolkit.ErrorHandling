namespace FsToolkit.ErrorHandling

type Validation<'a,'err> = Result<'a, 'err list>

[<RequireQualifiedAccess>]
module Validation =

  let ok x : Validation<_,_> = Ok x
  let error e : Validation<_,_> = List.singleton e |> Error

  let ofResult x : Validation<_,_> =
    Result.mapError List.singleton x

  let ofChoice x : Validation<_,_> =
    Result.ofChoice x
    |> ofResult
  
  let apply f (x : Validation<_,_>) : Validation<_,_> =
    match f, x with
    | Ok f, Ok x -> Ok (f x)
    | Error errs, Ok _ | Ok _, Error errs -> Error errs
    | Error errs1, Error errs2 -> Error  (errs1 @ errs2)

  let retn x = ok x

  let returnError e = error 

  
  /// <summary>
  /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise returns <paramref name="ifError"/> 
  /// </summary>
  /// <param name="ifError">The value to use if <paramref name="result"/> is <c>Error</c></param>
  /// <param name="result">The input result.</param>
  /// <remarks>
  /// </remarks>
  /// <example>
  /// <code>
  ///     Error (["First"]) |> Validation.orElse (Error (["Second"])) // evaluates to Error (["Second"])
  ///     Error (["First"]) |> Validation.orElseWith (Ok ("Second")) // evaluates to Ok ("Second")
  ///     Ok ("First") |> Validation.orElseWith (Error (["Second"])) // evaluates to Ok ("First")
  ///     Ok ("First") |> Validation.orElseWith (Ok ("Second")) // evaluates to Ok ("First")
  /// </code>
  /// </example>
  /// <returns>
  /// The result if the result is Ok, else returns <paramref name="ifError"/>.
  /// </returns>  
  let inline orElse (ifError : Validation<'ok,'error2>) (result : Validation<'ok,'error>) : Validation<'ok,'error2> = 
    match result with
    | Ok r -> Ok r
    | Error _ -> ifError

    
  /// <summary>
  /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise executes <paramref name="ifErrorFunc"/> and returns the result.
  /// </summary>
  /// <param name="ifErrorFunc">A function that provides an alternate result when evaluated.</param>
  /// <param name="result">The input result.</param>
  /// <remarks>
  /// <paramref name="ifErrorFunc"/>  is not executed unless <paramref name="result"/> is an <c>Error</c>.
  /// </remarks>
  /// <example>
  /// <code>
  ///     Error (["First"]) |> Validation.orElseWith (fun _ -> Error (["Second"])) // evaluates to Error (["Second"])
  ///     Error (["First"]) |> Validation.orElseWith (fun _ -> Ok ("Second")) // evaluates to Ok ("Second")
  ///     Ok ("First") |> Validation.orElseWith (fun _ -> Error (["Second"])) // evaluates to Ok ("First")
  ///     Ok ("First") |> Validation.orElseWith (fun _ -> Ok ("Second")) // evaluates to Ok ("First")
  /// </code>
  /// </example>
  /// <returns>
  /// The result if the result is Ok, else the result of executing <paramref name="ifErrorFunc"/>.
  /// </returns>
  let inline orElseWith (ifErrorFunc : 'error list -> Validation<'ok,'error2>) (result : Validation<'ok,'error>) : Validation<'ok,'error2> =
    match result with
    | Ok r -> Ok r
    | Error e -> ifErrorFunc e

  let map f (x : Validation<_,_>) : Validation<_,_>= Result.map f x
  
  let map2 f (x : Validation<_,_>) (y : Validation<_,_>) : Validation<_,_> =
    apply (apply (retn f) x) y
  
  let map3 f (x : Validation<_,_>) (y : Validation<_,_>) (z : Validation<_,_>) : Validation<_,_> =
    apply (map2 f x y) z

  let mapError f (x : Validation<_,_>) : Validation<_,_> =
    x |> Result.mapError (List.map f)

  let mapErrors f (x : Validation<_,_>) : Validation<_,_> =
    x |> Result.mapError (f)

  let bind (f : 'a -> Validation<'b, 'err>) (x : Validation<'a,'err>) : Validation<_,_>=
    Result.bind f x

  let zip (x1: Validation<_,_>) (x2 : Validation<_,_>) : Validation<_,_> = 
    match x1,x2 with
    | Ok x1res, Ok x2res -> Ok (x1res, x2res)
    | Error e, Ok _ -> Error e
    | Ok _, Error e -> Error e
    | Error e1, Error e2 -> Error (e1 @ e2)