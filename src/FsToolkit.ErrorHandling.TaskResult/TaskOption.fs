namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive

[<RequireQualifiedAccess>]
module TaskOption = 
    
    let inline map f ar =
        Task.map (Option.map f) ar

    let bind f (ar: Task<_>) =
      task {
        let! opt = ar
        let t = 
          match opt with 
          | Some x -> f x
          | None -> task { return None }
        return! t      
      }

    let retn x =
      task { return Some x }

    let apply f x =
        bind (fun f' ->
          bind (fun x' -> retn (f' x')) x) f