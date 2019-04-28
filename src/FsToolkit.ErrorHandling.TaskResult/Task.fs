namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive


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
