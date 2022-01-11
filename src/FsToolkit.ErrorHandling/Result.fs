namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Result =

  /// <summary>
  /// <c>either okMapping errorMapping result</c> evaluates to <c>match result with Ok x -> okMapping x | Error e -> errorMapping e</c>.
  /// </summary>
  /// <param name="okMapping">A function to map <typeparamref name="'ok"/> to <typeparamref name="'output"/>.</param>
  /// <param name="errorMapping">A function to map <typeparamref name="'error"/> to <typeparamref name="'output"/>.</param>
  /// <param name="result">The input result.</param>
  /// <returns>An <typeparamref name="'output"/> after applying either mapping function to the Ok or Error case.</returns>
  let inline either (okMapping : 'ok -> 'output) (errorMapping : 'error -> 'output) (result : Result<'ok, 'error>) : 'output =
    match result with
    | Ok x -> okMapping x
    | Error err -> errorMapping err

  /// <summary>
  /// Returns true if the Result is Ok.
  /// </summary>
  /// <param name="result">The input result.</param>
  /// <returns>True if the result is Ok.</returns>
  let inline isOk (result : Result<'ok, 'error>) : bool =
    either (fun _ -> true) (fun _ -> false) result

  /// <summary>
  /// Returns true if the Result is Error.
  /// </summary>
  /// <param name="result">The input result.</param>
  /// <returns>True if the result is Error.</returns>
  let inline isError (result : Result<'ok, 'error>) : bool =
    isOk result |> not

  /// <summary>
  /// <c>eitherMap okMapping errorMapping result</c> evaluates to <c>match result with Ok x -> Ok (okMapping x) | Error e -> Error (errorMapping e)</c>.
  /// </summary>
  /// <param name="okMapping">A function to map <typeparamref name="'ok"/> to <typeparamref name="'ok2"/>.</param>
  /// <param name="errorMapping">A function to map <typeparamref name="'error"/> to <typeparamref name="'error2"/>.</param>
  /// <param name="result">The input result.</param>
  /// <returns>A result that maps to either 'ok2 or 'error2.</returns>
  let inline eitherMap okMapping errorMapping (result : Result<'ok, 'error>) : Result<'ok2, 'error2>=
    either (okMapping >> Result.Ok) (errorMapping >> Result.Error) result

  /// <summary>
  /// Unpacks a function wrapped inside a Result and then executes that function on the input Result.
  /// 
  /// <c>apply applier result</c> evaluates to <c>match (applier, result) with (Ok f), (Ok x) -> Ok (f x) | (Error e, _) -> Error e | (_, Error e) -> Error e</c>.
  /// </summary>
  /// <param name="applier">The function wrapped in a Result.</param>
  /// <param name="result">The input result.</param>
  /// <returns>A result after executing the function within <paramref name="applier"/>. </returns>
  let inline apply (applier : Result<'ok -> 'ok2, 'error>) (result : Result<'ok, 'error>) : Result<'ok2, 'error>=
    applier
    |> Result.bind (fun f' -> Result.map f' result) 

  /// <summary>
  /// <c>map2 mapping result1 result2</c> evaluates to <c>match result1, result2 with Ok x, Ok y -> Ok (mapping x y) | Error e, _ | _, Error e -> Error e</c>.
  /// </summary>
  /// <param name="mapping">A function to apply to the result values.</param>
  /// <param name="result1">The first result.</param>
  /// <param name="result2">The second result.</param>
  /// <returns>A result of the input values after applying the mapping function, or Error if either of the input is Error.</returns>
  let map2 (mapping : 'ok -> 'ok2 -> 'ok3) (result1 : Result<'ok, 'error>) (result2 : Result<'ok2, 'error>) : Result<'ok3,'error> =
    apply (Result.map mapping result1) result2

  /// <summary>
  /// <c>map3 mapping result1 result2 result3</c> evaluates to <c>match result1, result2, result3 with Ok x, Ok y, Ok z -> Ok (mapping x y z) | Error e, _, _ | _, Error e, _ | _,_, Error e -> Error e</c>.
  /// </summary>
  /// <param name="mapping"></param>
  /// <param name="result1"></param>
  /// <param name="result2"></param>
  /// <param name="result3"></param>
  /// <returns></returns>
  let map3 (mapping : 'ok -> 'ok2 -> 'ok3 -> 'ok4) (result1 : Result<'ok, 'error>) (result2 : Result<'ok2, 'error>) (result3 : Result<'ok3, 'error>) : Result<'ok4,'error>  =
    apply (map2 mapping result1 result2) result3

  [<System.ObsoleteAttribute("This isn't a proper implementation of fold, use Result.either instead.")>]
  [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
  let fold onOk onError r =
    either onOk onError r

  /// <summary>
  /// Convert a Choice value to a Result value.
  /// </summary>
  /// <param name="choice">The input choice value.</param>
  /// <returns>A Result value mapping Choice1 to Ok and Choice2 to Error. </returns>
  let ofChoice (choice : Choice<'ok,'error>) : Result<'ok,'error> =
    match choice with
    | Choice1Of2 x -> Ok x
    | Choice2Of2 x -> Error x


  let inline tryCreate fieldName (x : 'a) : Result< ^b, (string * 'c)> =
    let tryCreate' x =
      (^b : (static member TryCreate : 'a -> Result< ^b, 'c>) x)
    tryCreate' x
    |> Result.mapError (fun z -> (fieldName, z))


  /// <summary>
  /// Returns <paramref name="result"/> if it is <c>Ok</c>, otherwise returns <paramref name="ifError"/> 
  /// </summary>
  /// <param name="ifError">The value to use if <paramref name="result"/> is <c>Error</c></param>
  /// <param name="result">The input result.</param>
  /// <remarks>
  /// </remarks>
  /// <example>
  /// <code>
  ///     Error ("First") |> Result.orElse (Error ("Second")) // evaluates to Error ("Second")
  ///     Error ("First") |> Result.orElseWith (Ok ("Second")) // evaluates to Ok ("Second")
  ///     Ok ("First") |> Result.orElseWith (Error ("Second")) // evaluates to Ok ("First")
  ///     Ok ("First") |> Result.orElseWith (Ok ("Second")) // evaluates to Ok ("First")
  /// </code>
  /// </example>
  /// <returns>
  /// The result if the result is Ok, else returns <paramref name="ifError"/>.
  /// </returns>  
  let inline orElse (ifError : Result<'ok,'error2>) (result : Result<'ok,'error>)  = 
    result |> either Ok (fun _ -> ifError)

    
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
  ///     Error ("First") |> Result.orElseWith (fun _ -> Error ("Second")) // evaluates to Error ("Second")
  ///     Error ("First") |> Result.orElseWith (fun _ -> Ok ("Second")) // evaluates to Ok ("Second")
  ///     Ok ("First") |> Result.orElseWith (fun _ -> Error ("Second")) // evaluates to Ok ("First")
  ///     Ok ("First") |> Result.orElseWith (fun _ -> Ok ("Second")) // evaluates to Ok ("First")
  /// </code>
  /// </example>
  /// <returns>
  /// The result if the result is Ok, else the result of executing <paramref name="ifErrorFunc"/>.
  /// </returns>
  let inline orElseWith (ifErrorFunc : 'error -> Result<'ok,'error2>) (result : Result<'ok,'error>) =
    result |> either Ok ifErrorFunc

  /// 
  /// <summary>
  /// Evaluates to <c>result |> Result.map ignore</c>
  /// </summary>
  /// <param name="result">The input result</param>
  /// <returns>A result where the Ok value is replaced with unit</returns>
  let inline ignore (result : Result<'ok,'error>) : Result<unit,'error> =
    result |> Result.map ignore

  /// <summary>
  /// Returns the specified error if the boolean value is false.
  /// </summary>
  /// <param name="error">The error to return if boolean is false</param>
  /// <param name="boolean">The boolean to check</param>
  /// <returns>The specified error if the boolean value is false otherwise returns an Ok unit.</returns>
  let requireTrue (error : 'error) (boolean : bool) : Result<unit,'error> =
    if boolean then Ok () else Error error

  /// <summary>
  /// Returns the specified error if the boolean value is true.
  /// </summary>
  /// <param name="error">The error to return if boolean is true</param>
  /// <param name="boolean">The boolean to check</param>
  /// <returns>The specified error if the boolean value is true otherwise returns an Ok unit.</returns>
  let requireFalse (error : 'error) (boolean : bool) : Result<unit,'error> =
    if not boolean then Ok () else Error error

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

  /// Converts a nullable value into a Result, using the given error if null
  let requireNotNull error value =
    match value with
    | null -> Error error
    | nonnull -> Ok nonnull

  /// Returns Ok if the two values are equal, or the specified error if not.
  /// Same as requireEqual, but with a signature that fits piping better than
  /// normal function application.
  let requireEqualTo other err this =
    if this = other then Ok () else Error err

  /// Returns Ok if the two values are equal, or the specified error if not.
  /// Same as requireEqualTo, but with a signature that fits normal function
  /// application better than piping.
  let requireEqual x1 x2 error =
    if x1 = x2 then Ok () else Error error

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

  // Returns the contained value if Error, otherwise returns ifOk.
  let defaultError ifOk result =
      match result with
      | Error error -> error
      | Ok _ -> ifOk

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

  /// Converts a Result<Async<_>,_> to an Async<Result<_,_>>
  let sequenceAsync (resAsync: Result<Async<'a>, 'b>) : Async<Result<'a, 'b>> =
    async {
      match resAsync with
      | Ok asnc ->
          let! x = asnc
          return Ok x
      | Error err -> return Error err
    }
    
  /// Returns the Ok value or runs the specified function over the error value.
  let valueOr f res =
    match res with
    | Ok x -> x
    | Error x -> f x

  /// Takes two results and returns a tuple of the pair
  let zip x1 x2 = 
    match x1,x2 with
    | Ok x1res, Ok x2res -> Ok (x1res, x2res)
    | Error e, _ -> Error e
    | _, Error e -> Error e

  /// Takes two results and returns a tuple of the error pair
  let zipError x1 x2 =
      match x1, x2 with
      | Error x1res, Error x2res -> Error(x1res, x2res)
      | Ok e, _ -> Ok e
      | _, Ok e -> Ok e

  let traverseAsync (f: 't -> Async<'u>) (res: Result<'t, 'e>) : Async<Result<'u, 'e>> =
      ((Result.map f) >> sequenceAsync) res