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

  // Returns the specified error if the value is false.
  let requireTrue error value =
    if value then Ok () else Error error

  /// Returns the specified error if the value is true.
  let requireFalse error value =
    if not value then Ok () else Error error

  /// Converts an Option to a Result, using the given error if None.
  let requireSome error option =
    match option with
    | Some x -> Ok x
    | None -> Error error

  /// Converts an Option to a Result, using the given error if Some.
  let requireNone error option =
    match option with
    | Some _ -> Error error
    | None -> Ok ()

  /// Returns Ok if the two values are equal, or the specified error if not.
  let requireEqual other err this =
    if this = other then Ok () else Error err

  /// Returns Ok if the sequence is empty, or the specified error if not.
  let requireEmpty error xs =
    if Seq.isEmpty xs then Ok () else Error error

  /// Returns the specified error if the sequence is empty, or Ok if not.
  let requireNotEmpty error xs =
    if Seq.isEmpty xs then Error error else Ok ()

  /// Returns the first item of the sequence if it exists, or the specified
  /// error if the sequence is empty
  let requireHead error xs =
    match Seq.tryHead xs with
    | Some x -> Ok x
    | None -> Error error

  /// Replaces an error value with a custom error value.
  let setError error result =
    result |> Result.mapError (fun _ -> error)

  /// Replaces a unit error value with a custom error value. Safer than setError
  /// since you're not losing any information.
  let withError error result =
    result |> Result.mapError (fun () -> error)

  /// Returns the contained value if Ok, otherwise returns ifError.
  let defaultValue ifError result =
    match result with
    | Ok x -> x
    | Error _ -> ifError

  /// Returns the contained value if Ok, otherwise evaluates ifErrorThunk and
  /// returns the result.
  let defaultWith ifErrorThunk result =
    match result with
    | Ok x -> x
    | Error _ -> ifErrorThunk ()

  /// Same as defaultValue for a result where the Ok value is unit. The name
  /// describes better what is actually happening in this case.
  let ignoreError result =
    defaultValue () result

  /// If the result is Ok and the predicate returns true, executes the function
  /// on the Ok value. Passes through the input value.
  let teeIf predicate f result =
    match result with
    | Ok x ->
        if predicate x then f x
    | Error _ -> ()
    result

  /// If the result is Error and the predicate returns true, executes the
  /// function on the Error value. Passes through the input value.
  let teeErrorIf predicate f result =
    match result with
    | Ok _ -> ()
    | Error x ->
        if predicate x then f x
    result

  /// If the result is Ok, executes the function on the Ok value. Passes through
  /// the input value.
  let tee f result =
    teeIf (fun _ -> true) f result


  /// If the result is Error, executes the function on the Error value. Passes
  /// through the input value.
  let teeError f result =
    teeErrorIf (fun _ -> true) f result

  