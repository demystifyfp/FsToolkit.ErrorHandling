namespace FsToolkit.ErrorHandling

open Hopac

[<RequireQualifiedAccess>]
module JobOption =

    let inline map ([<InlineIfLambda>] f) ar = Job.map (Option.map f) ar

    let inline bind ([<InlineIfLambda>] f) (ar: Job<_>) =
        job {
            let! opt = ar

            let t =
                match opt with
                | Some x -> f x
                | None -> Job.result None

            return! t
        }

    let inline some x = Job.result (Some x)
    let inline none<'t> : Job<'t option> = Job.result None

    [<System.Obsolete "Use JobOption.some instead of singleton">]
    let inline singleton x = some x

    let inline apply f x =
        bind (fun f' -> bind (fun x' -> some (f' x')) x) f

    /// <summary>Applies <paramref name="onSome"/> to the input if it is <c>Some</c>, otherwise returns result of running <paramref name="onNone"/>.</summary>
    /// <param name="onSome">The function to apply if <paramref name="input"/> is <c>Some</c>.</param>
    /// <param name="onNone">The function to run if <paramref name="input"/> is <c>None</c>.</param>
    /// <param name="input">The input <c>Job&lt;'input option&gt;</c>.</param>
    /// <returns>The result of applying <paramref name="onSome"/> if the input is <c>Some</c>, else returns result of running <paramref name="onNone"/>.</returns>
    let inline eitherBind
        ([<InlineIfLambda>] onSome: 'input -> 'output)
        ([<InlineIfLambda>] onNone: unit -> 'output)
        (input: Job<'input option>)
        : Job<'output> =
        Job.map (Option.eitherBind onSome onNone) input

    [<System.Obsolete "Please use 1. `Job.bind (function Some v -> onSome v | None -> onNone)` or 2. JobOption.eitherBind (if possible; different signature) instead">]
    let inline either
        ([<InlineIfLambda>] onSome: 'input -> Job<'output>)
        (onNone: Job<'output>)
        (input: Job<'input option>)
        : Job<'output> =
        Job.bind
            (function
            | Some v -> onSome v
            | None -> onNone)
            input
