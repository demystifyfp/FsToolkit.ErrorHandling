namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FSharp.Control.Tasks.Affine

[<RequireQualifiedAccess>]
module Task =

  let inline ofUnit (t : Task) = task { return! t }

  let inline singleton value = value |> Task.FromResult

  let inline bind (f : _ -> ^b ) (x : ^a) = task {
    let! y = x
    return! f y
  }



  let inline apply (f : ^a) (x : ^b) =
    bind (fun f' ->
      bind (fun x' -> singleton(f' x')) x) f

  let inline map f x = x |> bind (f >> singleton)

  let inline map2 f (x : ^a) (y : ^b) =
    (apply (apply (singleton f) x) y)

  let inline map3 f (x : ^a) (y : ^b) (z : ^c) =
    apply (map2 f x y) z

  
  /// Takes two tasks and returns a tuple of the pair
  let inline zip (a1 : ^a) (a2 : ^b) = task {
    let! r1 = a1
    let! r2 = a2
    return r1,r2
  }


  /// Creates a `Task` that attempts to execute the provided task,
  /// returning `Choice1Of2` with the result if the task completes
  /// without exceptions, or `Choice2Of2` with the exception if an
  /// exception is thrown.
  let inline catch (x : ^a) =
    task {
      try
        let! r = x
        return Choice1Of2 r
      with
      | e -> return Choice2Of2 e
    }