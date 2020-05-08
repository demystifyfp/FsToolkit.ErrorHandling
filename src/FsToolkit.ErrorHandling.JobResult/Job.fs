namespace FsToolkit.ErrorHandling

open Hopac
open Hopac.Infixes

[<RequireQualifiedAccess>]
module Job =
    let singleton = Job.result
    let apply' x  f = Job.apply f x
    let map2 f x y =
        (apply' (apply' (singleton f) x) y)

    let map3 f x y z =
        apply' (map2 f x y) z

    let zip j1 j2 = j1 <&> j2