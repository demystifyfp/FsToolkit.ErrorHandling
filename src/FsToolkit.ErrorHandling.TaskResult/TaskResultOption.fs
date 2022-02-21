namespace FsToolkit.ErrorHandling

open FSharp.Control.Tasks.Affine

[<RequireQualifiedAccess>]
module TaskResultOption =
    let inline map ([<InlineIfLambda>] f) tro = TaskResult.map (Option.map f) tro

    let inline bind ([<InlineIfLambda>] f) tro =
        let binder opt =
            match opt with
            | Some x -> f x
            | None -> TaskResult.retn None

        TaskResult.bind binder tro

    let inline map2 ([<InlineIfLambda>] f) xTRO yTRO =
        TaskResult.map2 (Option.map2 f) xTRO yTRO

    let inline map3 ([<InlineIfLambda>] f) xTRO yTRO zTRO =
        TaskResult.map3 (Option.map3 f) xTRO yTRO zTRO

    let inline retn value = task { return Ok(Some value) }

    let inline apply fTRO xTRO = map2 (fun f x -> f x) fTRO xTRO

    /// Replaces the wrapped value with unit
    let inline ignore tro = tro |> map ignore
