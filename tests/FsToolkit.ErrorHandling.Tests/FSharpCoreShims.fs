module FSharpCoreShimTests

#nowarn "44"

#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#else
open Expecto
#endif

module ResultShim = FsToolkit.ErrorHandling.FSharpCore11ResultShims.Result
module OptionShim = FsToolkit.ErrorHandling.FSharpCore11OptionShims.Option

let allTests =
    testList "FSharp.Core shims" [
        testCase "Result shims preserve success and error behavior"
        <| fun _ ->
            Expect.equal (ResultShim.map ((+) 1) (Ok 1)) (Ok 2) "map success"
            Expect.equal (ResultShim.mapError ((+) 1) (Error 1)) (Error 2) "map error"
            Expect.equal (ResultShim.bind (fun x -> Ok(x + 1)) (Ok 1)) (Ok 2) "bind success"
            Expect.isTrue (ResultShim.isOk (Ok 1)) "recognize success"
            Expect.isTrue (ResultShim.isError (Error 1)) "recognize error"
            Expect.equal (ResultShim.defaultValue 2 (Error "error")) 2 "default value"

            let mutable called = false

            let fallback error =
                called <- true
                error + 1

            Expect.equal (ResultShim.defaultWith fallback (Ok 1)) 1 "keep success"
            Expect.isFalse called "fallback is lazy"
            Expect.equal (ResultShim.defaultWith fallback (Error 2)) 3 "map error to default"
            Expect.isTrue called "fallback handles error"

        testCase "Option shims preserve mapping and short-circuiting"
        <| fun _ ->
            Expect.equal (OptionShim.bind (fun x -> Some(x + 1)) (Some 1)) (Some 2) "bind"
            Expect.equal (OptionShim.map ((+) 1) (Some 1)) (Some 2) "map"
            Expect.equal (OptionShim.map2 (+) (Some 1) (Some 2)) (Some 3) "map2"

            Expect.equal
                (OptionShim.map3 (fun x y z -> x + y + z) (Some 1) None (Some 3))
                None
                "map3"

#if !FABLE_COMPILER
        testCase "Only needed shim modules are auto-opened"
        <| fun _ ->
            let assembly = typeof<FsToolkit.ErrorHandling.ResultCE.ResultBuilder>.Assembly

            let isAutoOpen name =
                let moduleType =
                    assembly.GetTypes()
                    |> Array.find (fun t ->
                        t.FullName = "FsToolkit.ErrorHandling."
                                     + name
                    )

                moduleType.IsDefined(typeof<Microsoft.FSharp.Core.AutoOpenAttribute>, false)

#if NET9_0_OR_GREATER
            Expect.isFalse
                (isAutoOpen "FSharpCore11ResultShims")
                "Result is provided by FSharp.Core"
#else
            Expect.isTrue (isAutoOpen "FSharpCore11ResultShims") "Result needs compatibility shims"
#endif
            Expect.isFalse
                (isAutoOpen "FSharpCore11OptionShims")
                "Option is provided by FSharp.Core"
#endif
    ]
