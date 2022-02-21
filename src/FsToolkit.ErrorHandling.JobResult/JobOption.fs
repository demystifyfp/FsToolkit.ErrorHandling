namespace FsToolkit.ErrorHandling

open Hopac
open Hopac.Infixes

[<RequireQualifiedAccess>]
module JobOption =

    let inline map ([<InlineIfLambda>] f) ar = Job.map (Option.map f) ar

    let inline bind ([<InlineIfLambda>] f) (ar: Job<_>) =
        job {
            let! opt = ar

            let t =
                match opt with
                | Some x -> f x
                | None -> job { return None }

            return! t
        }

    let inline retn x = job { return Some x }

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> retn (f' x')) x) f
