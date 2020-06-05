namespace FsToolkit.ErrorHandling

open System.Threading.Tasks

[<RequireQualifiedAccess>]
module AsyncOption = 
    
    let inline map f ar =
        Async.map (Option.map f) ar

    let bind f ar = async {
        let! opt = ar
        let t = 
          match opt with 
          | Some x -> f x
          | None -> async { return None }
        return! t      
      }

    let retn x =
      async { return Some x }

    let apply f x =
        bind (fun f' ->
          bind (fun x' -> retn (f' x')) x) f