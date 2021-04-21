namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FSharp.Control.Tasks.Affine

[<RequireQualifiedAccess>]
module Task =
  let singleton value = value |> Task.FromResult

  let bind (f : 'a -> Task<'b>) (x : Task<'a>) = task {
      let! x = x
      return! f x
  }

  let apply f x =
    bind (fun f' ->
      bind (fun x' -> singleton(f' x')) x) f

  let map f x = x |> bind (f >> singleton)

  let map2 f x y =
    (apply (apply (singleton f) x) y)

  let map3 f x y z =
    apply (map2 f x y) z

  
  /// Takes two tasks and returns a tuple of the pair
  let zip (a1 : Task<_>) (a2 : Task<_>) = task {
    let! r1 = a1
    let! r2 = a2
    return r1,r2
  }

  let ofUnit (t : Task) = task { return! t }

  /// Creates a `Task` that attempts to execute the provided task,
  /// returning `Choice1Of2` with the result if the task completes
  /// without exceptions, or `Choice2Of2` with the exception if an
  /// exception is thrown.
  let catch (x : Task<_>) =
    task {
      try
        let! r = x
        return Choice1Of2 r
      with
      | e -> return Choice2Of2 e
    }