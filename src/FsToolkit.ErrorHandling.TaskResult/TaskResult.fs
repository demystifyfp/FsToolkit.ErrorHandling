namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive

[<RequireQualifiedAccess>]
module TaskResult = 

  let map f tr =
    Task.map (Result.map f) tr

  let mapError f tr =
    Task.map (Result.mapError f) tr    

  let bind f (tr : Task<_>) = task {
    let! result = tr
    let t = 
      match result with 
      | Ok x -> f x
      | Error e -> task { return Error e }
    return! t      
  }

  let foldResult onSuccess onError tr =
    Task.map (Result.fold onSuccess onError) tr

  let ofAsync aAsync = 
    aAsync
    |> Async.Catch 
    |> Async.StartAsTask 
    |> Task.map Result.ofChoice
  
  let retn x =
    Ok x
    |> Task.singleton
  
  let returnError x =
    Error x
    |> Task.singleton

  let map2 f xTR yTR =
    Task.map2 (Result.map2 f) xTR yTR

  let map3 f xTR yTR zTR =
    Task.map3 (Result.map3 f) xTR yTR zTR

  let apply fTR xTR =
    map2 (fun f x -> f x) fTR xTR

  /// Replaces the wrapped value with unit
  let ignore tr =
      tr |> map ignore

  /// Returns the specified error if the task-wrapped value is false.
  let requireTrue error value = 
    value |> Task.map (Result.requireTrue error)

  /// Returns the specified error if the task-wrapped value is true.
  let requireFalse error value =
    value |> Task.map (Result.requireFalse error) 

  // Converts an task-wrapped Option to a Result, using the given error if None.
  let requireSome error option =
    option |> Task.map (Result.requireSome error)

  // Converts an task-wrapped Option to a Result, using the given error if Some.
  let requireNone error option =
    option |> Task.map (Result.requireNone error)

  /// Returns Ok if the task-wrapped value and the provided value are equal, or the specified error if not.
  let requireEqual x1 x2 error =
    x2 |> Task.map (fun x2' -> Result.requireEqual x1 x2' error)

  /// Returns Ok if the two values are equal, or the specified error if not.
  let requireEqualTo other error this =
    this |> Task.map (Result.requireEqualTo other error)

  /// Returns Ok if the task-wrapped sequence is empty, or the specified error if not.
  let requireEmpty error xs =
    xs |> Task.map (Result.requireEmpty error)

  /// Returns Ok if the task-wrapped sequence is not-empty, or the specified error if not.
  let requireNotEmpty error xs =
    xs |> Task.map (Result.requireNotEmpty error)

  /// Returns the first item of the task-wrapped sequence if it exists, or the specified
  /// error if the sequence is empty
  let requireHead error xs =
    xs |> Task.map (Result.requireHead error)

  /// Replaces an error value of an task-wrapped result with a custom error
  /// value.
  let setError error taskResult =
    taskResult |> Task.map (Result.setError error)

  /// Replaces a unit error value of an task-wrapped result with a custom
  /// error value. Safer than setError since you're not losing any information.
  let withError error taskResult =
    taskResult |> Task.map (Result.withError error)

  /// Extracts the contained value of an task-wrapped result if Ok, otherwise
  /// uses ifError.
  let defaultValue ifError taskResult =
    taskResult |> Task.map (Result.defaultValue ifError)

  /// Extracts the contained value of an task-wrapped result if Ok, otherwise
  /// evaluates ifErrorThunk and uses the result.
  let defaultWith ifErrorThunk taskResult =
    taskResult |> Task.map (Result.defaultWith ifErrorThunk)

  /// Same as defaultValue for a result where the Ok value is unit. The name
  /// describes better what is actually happening in this case.
  let ignoreError taskResult =
    defaultValue () taskResult

  /// If the task-wrapped result is Ok, executes the function on the Ok value.
  /// Passes through the input value.
  let tee f taskResult =
    taskResult |> Task.map (Result.tee f)

  /// If the task-wrapped result is Ok and the predicate returns true, executes
  /// the function on the Ok value. Passes through the input value.
  let teeIf predicate f taskResult =
    taskResult |> Task.map (Result.teeIf predicate f)

  /// If the task-wrapped result is Error, executes the function on the Error
  /// value. Passes through the input value.
  let teeError f taskResult =
    taskResult |> Task.map (Result.teeError f)

  /// If the task-wrapped result is Error and the predicate returns true,
  /// executes the function on the Error value. Passes through the input value.
  let teeErrorIf predicate f taskResult =
    taskResult |> Task.map (Result.teeErrorIf predicate f)