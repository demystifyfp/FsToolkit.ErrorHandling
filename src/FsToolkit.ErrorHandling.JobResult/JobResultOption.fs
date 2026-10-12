namespace FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module JobResultOption =
    open Hopac
    let inline some value = JobResult.ok (Some value)
    let inline none<'ok, 'error> : Job<Result<'ok option, 'error>> = JobResult.ok None
    let inline map ([<InlineIfLambda>] f) jro = JobResult.map (Option.map f) jro

    [<System.Obsolete "Please use some instead of singleton">]
    let inline singleton (value: 'ok) : Job<Result<'ok option, 'error>> = some value

    let inline bind ([<InlineIfLambda>] f) jro =
        let binder opt =
            match opt with
            | Some x -> f x
            | None -> JobResult.ok None

        JobResult.bind binder jro

    let inline map2 ([<InlineIfLambda>] f) xJRO yJRO =
        JobResult.map2 (Option.map2 f) xJRO yJRO

    let inline map3 ([<InlineIfLambda>] f) xJRO yJRO zJRO =
        JobResult.map3 (Option.map3 f) xJRO yJRO zJRO

    let apply fJRO xJRO = map2 (fun f x -> f x) fJRO xJRO

    /// Replaces the wrapped value with unit
    let inline ignore<'a, 'b> (jro: Job<Result<'a option, 'b>>) =
        jro
        |> map ignore<'a>

    /// Bind the Job&lt;'input option&gt; with a synchronous Result-returning function.
    /// A None input will be mapped to Ok None.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: Job<'input option>)
        : Job<Result<'output option, 'error>> =
        input
        |> Job.bindResult (
            function
            | Some x ->
                binder x
                |> Result.map Some
            | None -> Ok None
        )

    let inline req ([<InlineIfLambda>] reqF) errOrF value = bindResult (reqF errOrF) value
    let inline reqFilter predicate error value = req (Req.filter predicate) error value
