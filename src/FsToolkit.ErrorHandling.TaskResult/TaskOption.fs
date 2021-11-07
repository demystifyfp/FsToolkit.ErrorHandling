namespace FsToolkit.ErrorHandling

open System.Threading.Tasks
open FSharp.Control.Tasks.Affine

[<RequireQualifiedAccess>]
module TaskOption =

    let inline map f ar = Task.map (Option.map f) ar

    let bind f (ar: Task<_>) =
        task {
            let! opt = ar

            let t =
                match opt with
                | Some x -> f x
                | None -> task { return None }

            return! t
        }

    let retn x = task { return Some x }

    let apply f x =
        bind (fun f' -> bind (fun x' -> retn (f' x')) x) f

    let zip x1 x2 =
        Task.zip x1 x2
        |> Task.map (fun (r1, r2) -> Option.zip r1 r2)
