namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module JobResultOption =
    open Hopac
    let inline map ([<InlineIfLambda>] f) jro = JobResult.map (Option.map f) jro

    let inline bind ([<InlineIfLambda>] f) jro =
        let binder opt =
            match opt with
            | Some x -> f x
            | None -> JobResult.singleton None

        JobResult.bind binder jro

    let inline map2 ([<InlineIfLambda>] f) xJRO yJRO =
        JobResult.map2 (Option.map2 f) xJRO yJRO

    let inline map3 ([<InlineIfLambda>] f) xJRO yJRO zJRO =
        JobResult.map3 (Option.map3 f) xJRO yJRO zJRO

    let inline singleton value =
        Some value
        |> Ok
        |> Job.result

    let apply fJRO xJRO = map2 (fun f x -> f x) fJRO xJRO

    /// Replaces the wrapped value with unit
    let inline ignore<'a, 'b> (jro: Job<Result<'a option, 'b>>) =
        jro
        |> map ignore<'a>
