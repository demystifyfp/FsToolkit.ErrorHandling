namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FSharp.Control.Tasks.Affine

[<RequireQualifiedAccess>]
module TaskOption = 
    
    let inline map f (ar : ^a) =
        Task.map (Option.map f) ar

    let inline bind (f : _ -> ^b) (ar: ^a) =
      task {
        match! ar with
        | Some x -> return! f x
        | None -> return None         
      }

    let retn x =
      task { return Some x }

    let inline apply (f : ^b) (x :^a) =
        bind (fun f' ->
          bind (fun x' -> retn (f' x')) x) f

    let inline zip (x1 : ^a) (x2 : ^b) = 
      Task.zip x1 x2
      |> Task.map(fun (r1, r2) -> Option.zip r1 r2)
