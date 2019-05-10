namespace FsToolkit.ErrorHandling

open Hopac

[<RequireQualifiedAccess>]
module JobResult = 

  let map f tr =
    Job.map (Result.map f) tr

  let mapError f tr =
    Job.map (Result.mapError f) tr    

  let bind (f : 'a -> Job<Result<'c,'b>>) (tr : Job<Result<'a,'b>>) = 
    Job.bind (Result.either f (Error >> Job.result)) tr

  let foldResult onSuccess onError tr =
    Job.map (Result.fold onSuccess onError) tr

  let ofAsync aAsync = 
    aAsync
    |> Job.fromAsync 
    |> Job.catch
    |> Job.map Result.ofChoice

  let fromTask aTask = 
    aTask
    |> Job.fromTask 
    |> Job.catch
    |> Job.map Result.ofChoice

  let fromUnitTask aTask = 
    aTask
    |> Job.fromUnitTask
    |> Job.catch
    |> Job.map Result.ofChoice
  
  let retn x =
    Ok x
    |> Job.result

  let returnError x =
    Error x
    |> Job.result

  let map2 f xTR yTR =
    Job.map2 (Result.map2 f) xTR yTR

  let map3 f xTR yTR zTR =
    Job.map3 (Result.map3 f) xTR yTR zTR

  let apply fTR xTR =
    map2 (fun f x -> f x) fTR xTR

  /// Returns the specified error if the task-wrapped value is false.
  let requireTrue error value = 
    value |> Job.map (Result.requireTrue error)

  /// Returns the specified error if the task-wrapped value is true.
  let requireFalse error value =
    value |> Job.map (Result.requireFalse error) 

  // Converts an task-wrapped Option to a Result, using the given error if None.
  let requireSome error option =
    option |> Job.map (Result.requireSome error)

  // Converts an task-wrapped Option to a Result, using the given error if Some.
  let requireNone error option =
    option |> Job.map (Result.requireNone error)

  /// Returns Ok if the task-wrapped value and the provided value are equal, or the specified error if not.
  let requireEqual x1 x2 error =
    x2 |> Job.map (fun x2' -> Result.requireEqual x1 x2' error)

  /// Returns Ok if the two values are equal, or the specified error if not.
  let requireEqualTo other error this =
    this |> Job.map (Result.requireEqualTo other error)

  /// Returns Ok if the task-wrapped sequence is empty, or the specified error if not.
  let requireEmpty error xs =
    xs |> Job.map (Result.requireEmpty error)

  /// Returns Ok if the task-wrapped sequence is not-empty, or the specified error if not.
  let requireNotEmpty error xs =
    xs |> Job.map (Result.requireNotEmpty error)

  /// Returns the first item of the task-wrapped sequence if it exists, or the specified
  /// error if the sequence is empty
  let requireHead error xs =
    xs |> Job.map (Result.requireHead error)

  /// Replaces an error value of an task-wrapped result with a custom error
  /// value.
  let setError error asyncResult =
    asyncResult |> Job.map (Result.setError error)

  /// Replaces a unit error value of an task-wrapped result with a custom
  /// error value. Safer than setError since you're not losing any information.
  let withError error asyncResult =
    asyncResult |> Job.map (Result.withError error)

  /// Extracts the contained value of an task-wrapped result if Ok, otherwise
  /// uses ifError.
  let defaultValue ifError asyncResult =
    asyncResult |> Job.map (Result.defaultValue ifError)

  /// Extracts the contained value of an task-wrapped result if Ok, otherwise
  /// evaluates ifErrorThunk and uses the result.
  let defaultWith ifErrorThunk asyncResult =
    asyncResult |> Job.map (Result.defaultWith ifErrorThunk)

  /// Same as defaultValue for a result where the Ok value is unit. The name
  /// describes better what is actually happening in this case.
  let ignoreError result =
    defaultValue () result

  /// If the task-wrapped result is Ok, executes the function on the Ok value.
  /// Passes through the input value.
  let tee f asyncResult =
    asyncResult |> Job.map (Result.tee f)


  /// If the task-wrapped result is Ok and the predicate returns true, executes
  /// the function on the Ok value. Passes through the input value.
  let teeIf predicate f asyncResult =
    asyncResult |> Job.map (Result.teeIf predicate f)


  /// If the task-wrapped result is Error, executes the function on the Error
  /// value. Passes through the input value.
  let teeError f asyncResult =
    asyncResult |> Job.map (Result.teeError f)

  /// If the task-wrapped result is Error and the predicate returns true,
  /// executes the function on the Error value. Passes through the input value.
  let teeErrorIf predicate f asyncResult =
    asyncResult |> Job.map (Result.teeErrorIf predicate f)