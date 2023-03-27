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


    /// <summary>
    /// Returns result of running <paramref name="onSome"/> if it is <c>Some</c>, otherwise returns result of running <paramref name="onNone"/>
    /// </summary>
    /// <param name="onSome">The function to run if <paramref name="input"/> is <c>Some</c></param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>None</c></param>
    /// <param name="input">The input option.</param>
    /// <returns>
    /// The result of running <paramref name="onSome"/> if the input is <c>Some</c>, else returns result of running <paramref name="onNone"/>.
    /// </returns>
    let inline either
        ([<InlineIfLambda>] onSome: 'input -> Job<'output>)
        (onNone: Job<'output>)
        (input: Job<'input option>)
        : Job<'output> =
        input
        |> Job.bind (
            function
            | Some v -> onSome v
            | None -> onNone
        )
