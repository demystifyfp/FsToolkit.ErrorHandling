module OptionCETests


#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open FsToolkit.ErrorHandling
open System
open System.Collections.Generic

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }



// type 'a option = | Some of 'a | None

let ceTests =
    testList "CE Tests" [
        testCase "Return value"
        <| fun _ ->
            let expected = Some 42
            let actual = option { return 42 }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "ReturnFrom Some"
        <| fun _ ->
            let expected = Some 42
            let actual = option { return! (Some 42) }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "ReturnFrom None"
        <| fun _ ->
            let expected = None
            let actual = option { return! None }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "Bind Some"
        <| fun _ ->
            let expected = Some 42

            let actual =
                option {
                    let! value = Some 42
                    return value
                }

            Expect.equal actual expected "Should bind value wrapped in option"
        testCase "Bind None"
        <| fun _ ->
            let expected = None

            let actual =
                option {
                    let! value = None
                    return value
                }

            Expect.equal actual expected "Should bind value wrapped in option"
        testCase "Zero/Combine/Delay/Run"
        <| fun () ->
            let data = 42

            let actual =
                option {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (Some data) "Should be ok"
        testCase "Try With"
        <| fun () ->
            let data = 42

            let actual =
                option {
                    try
                        return data
                    with e ->
                        return raise e
                }

            Expect.equal actual (Some data) "Try with failed"
        testCase "Try Finally"
        <| fun () ->
            let data = 42

            let actual =
                option {
                    try
                        return data
                    finally
                        ()
                }

            Expect.equal actual (Some data) "Try with failed"
        testCase "Using null"
        <| fun () ->
            let data = 42

            let actual =
                option {
                    use d = null
                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        testCase "Using disposeable"
        <| fun () ->
            let data = 42

            let actual =
                option {
                    use d = makeDisposable ()
                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        testCase "Using bind disposeable"
        <| fun () ->
            let data = 42

            let actual =
                option {
                    use! d =
                        (makeDisposable ()
                         |> Some)

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        yield! [
            let maxIndices = [
                10
                1000000
            ]

            for maxIndex in maxIndices do
                testCase
                <| sprintf "While - %i" maxIndex
                <| fun () ->
                    let data = 42
                    let mutable index = 0

                    let actual =
                        option {
                            while index < maxIndex do
                                index <- index + 1

                            return data
                        }

                    Expect.equal index maxIndex "Index should reach maxIndex"
                    Expect.equal actual (Some data) "Should be ok"
        ]

        testCase "while fail"
        <| fun () ->

            let mutable loopCount = 0
            let mutable wasCalled = false

            let sideEffect () =
                wasCalled <- true
                "ok"

            let expected = None

            let data = [
                Some "42"
                Some "1024"
                expected
                Some "1M"
                Some "1M"
                Some "1M"
            ]

            let actual =
                option {
                    while loopCount < data.Length do
                        let! x = data.[loopCount]

                        loopCount <-
                            loopCount
                            + 1

                    return sideEffect ()
                }

            Expect.equal loopCount 2 "Should only loop twice"
            Expect.equal actual expected "Should be an error"
            Expect.isFalse wasCalled "No additional side effects should occur"


        testCase "For in"
        <| fun () ->
            let data = 42

            let actual =
                option { 
                    for i in [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        testCase "For in ResizeArray"
        <| fun () ->
            let data = 42

            let actual =
                option {
                    for i in ResizeArray [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        testCase "For to"
        <| fun () ->
            let data = 42

            let actual =
                option {
                    for i = 1 to 10 do
                        ()

                    return data
                }

            Expect.equal actual (Some data) "Should be ok"
        testCase "Nullable value"
        <| fun () ->
            let data = 42

            let actual = option { return! System.Nullable<int> data }

            Expect.equal actual (Some data) ""
        testCase "Nullable null"
        <| fun () ->
            let actual = option { return! System.Nullable<_>() }
            Expect.equal actual None ""

        testCase "string value"
        <| fun () ->
            let data = "hello"

            let actual =
                option {
                    let! v = data
                    return v
                }

            Expect.equal actual (Some data) ""

        testCase "string null"
        <| fun () ->
            let (data: string) = null

            let actual =
                option {
                    let! v = data
                    return v
                }

            Expect.equal actual None ""

        testCase "Uri null"
        <| fun () ->
            let (data: UriNull) = null

            let actual =
                option {
                    let! v = data
                    return v
                }

            Expect.equal actual None ""

        testCase "MemoryStream null"
        <| fun () ->
            let (data: MemoryStreamNull) = null

            let actual =
                option {
                    let! v = data
                    return v
                }

            Expect.equal actual None ""

        testCase "ResizeArray null"
        <| fun () ->
            let (data: ResizeArray<string>) = null

            let actual =
                option {
                    let! v = data
                    return v
                }

            Expect.equal actual None ""
    ]

[<AllowNullLiteral>]
type CustomClass(x: int) =

    member _.getX = x


#if NET9_0_OR_GREATER

type AB = A | B
type AbNull = AB | null

#endif

let ``OptionCE inference checks`` =
    testList "OptionCE Inference checks" [
        testCase "Argument should be inferred to Option"
        <| fun () ->
            // Compilation is success
            let f res = option { return! res }

            f (Some())
            |> ignore
#if NET9_0_OR_GREATER
        testCase "Nullable argument should be inferred" <| fun () ->
            // Compilation is real success
            let y (p : AbNull) =
                option {
                    let! p = p
                    let isa = p.IsA
                    return isa
                }
            
            Expect.equal (y A) (Some true) ""
#endif
    ] 

let allTests =
    testList "Option CE tests" [
        ceTests
        ``OptionCE inference checks``
    ]
