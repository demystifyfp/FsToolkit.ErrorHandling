namespace FsToolkit.ErrorHandling

open Hopac
open Hopac.Infixes

[<RequireQualifiedAccess>]
module Job =
    let inline singleton x = Job.result x
    let inline apply' x f = Job.apply f x
    let inline map2 ([<InlineIfLambda>] f) x y = (apply' (apply' (singleton f) x) y)

    let inline map3 ([<InlineIfLambda>] f) x y z = apply' (map2 f x y) z

    let inline zip left right =
        left
        <&> right

    /// Bind the Job with a synchronous Result-returning function.
    let inline bindResult
        ([<InlineIfLambda>] binder: 'input -> Result<'output, 'error>)
        (input: Job<'input>)
        : Job<Result<'output, 'error>> =
        Job.map binder input

    let inline req ([<InlineIfLambda>] reqF) errOrF value = bindResult (reqF errOrF) value
    let inline reqFilter predicate error value = req (Req.filter predicate) error value
