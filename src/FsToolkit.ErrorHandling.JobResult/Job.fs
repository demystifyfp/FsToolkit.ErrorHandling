namespace FsToolkit.ErrorHandling

open Hopac
open Hopac.Infixes

[<RequireQualifiedAccess>]
module Job =
    let inline singleton x = Job.result x
    let inline apply' x f = Job.apply f x
    let inline map2 ([<InlineIfLambda>] f) x y = (apply' (apply' (singleton f) x) y)

    let inline map3 ([<InlineIfLambda>] f) x y z = apply' (map2 f x y) z

    let inline zip j1 j2 = j1 <&> j2
