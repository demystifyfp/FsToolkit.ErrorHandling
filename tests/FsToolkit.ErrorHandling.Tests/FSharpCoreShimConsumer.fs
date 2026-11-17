module FSharpCoreShimConsumerTests

#if FABLE_COMPILER_PYTHON || FABLE_COMPILER_JAVASCRIPT
open Fable.Pyxpecto
#else
open Expecto
#endif

open FsToolkit.ErrorHandling

let allTests =
    testList "FSharp.Core functions available to consumers" [
        testCase "Result.map"
        <| fun _ -> Expect.equal (Result.map ((+) 1) (Ok 1)) (Ok 2) "maps the value"

        testCase "Result.mapError"
        <| fun _ -> Expect.equal (Result.mapError ((+) 1) (Error 1)) (Error 2) "maps the error"

        testCase "Result.bind"
        <| fun _ -> Expect.equal (Result.bind (fun x -> Ok(x + 1)) (Ok 1)) (Ok 2) "binds the value"

        testCase "Result.isOk"
        <| fun _ -> Expect.isTrue (Result.isOk (Ok 1)) "recognizes Ok"

        testCase "Result.isError"
        <| fun _ -> Expect.isTrue (Result.isError (Error 1)) "recognizes Error"

        testCase "Result.defaultValue"
        <| fun _ -> Expect.equal (Result.defaultValue 2 (Error "error")) 2 "returns the default"

        testCase "Result.defaultWith"
        <| fun _ -> Expect.equal (Result.defaultWith ((+) 1) (Error 1)) 2 "computes the default"

        testCase "Option.bind"
        <| fun _ ->
            Expect.equal (Option.bind (fun x -> Some(x + 1)) (Some 1)) (Some 2) "binds the value"

        testCase "Option.map"
        <| fun _ -> Expect.equal (Option.map ((+) 1) (Some 1)) (Some 2) "maps the value"

        testCase "Option.map2"
        <| fun _ -> Expect.equal (Option.map2 (+) (Some 1) (Some 2)) (Some 3) "maps two values"

        testCase "Option.map3"
        <| fun _ ->
            Expect.equal
                (Option.map3 (fun x y z -> x + y + z) (Some 1) (Some 2) (Some 3))
                (Some 6)
                "maps three values"
    ]
