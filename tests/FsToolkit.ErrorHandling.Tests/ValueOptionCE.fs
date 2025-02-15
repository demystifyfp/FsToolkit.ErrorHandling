module ValueOptionCETests


open System


#if FABLE_COMPILER_PYTHON
open Fable.Pyxpecto
#endif
#if FABLE_COMPILER_JAVASCRIPT
open Fable.Mocha
#endif
#if !FABLE_COMPILER
open Expecto
#endif
open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }


#if !FABLE_COMPILER
// type 'a option = | ValueSome of 'a | None

let ceTests =
    testList "CE Tests" [
        testCase "Return value"
        <| fun _ ->
            let expected = ValueSome 42
            let actual = voption { return 42 }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "ReturnFrom ValueSome"
        <| fun _ ->
            let expected = ValueSome 42
            let actual = voption { return! (ValueSome 42) }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "ReturnFrom ValueNone"
        <| fun _ ->
            let expected = ValueNone
            let actual = voption { return! ValueNone }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "Bind ValueSome"
        <| fun _ ->
            let expected = ValueSome 42

            let actual =
                voption {
                    let! value = ValueSome 42
                    return value
                }

            Expect.equal actual expected "Should bind value wrapped in option"
        testCase "Bind ValueNone"
        <| fun _ ->
            let expected = ValueNone

            let actual =
                voption {
                    let! value = ValueNone
                    return value
                }

            Expect.equal actual expected "Should bind value wrapped in option"
        testCase "Zero/Combine/Delay/Run"
        <| fun () ->
            let data = 42

            let actual =
                voption {
                    let result = data

                    if true then
                        ()

                    return result
                }

            Expect.equal actual (ValueSome data) "Should be ok"
        testCase "Try With"
        <| fun () ->
            let data = 42

            let actual =
                voption {
                    try
                        return data
                    with e ->
                        return raise e
                }

            Expect.equal actual (ValueSome data) "Try with failed"
        testCase "Try Finally"
        <| fun () ->
            let data = 42

            let actual =
                voption {
                    try
                        return data
                    finally
                        ()
                }

            Expect.equal actual (ValueSome data) "Try with failed"
        testCase "Using null"
        <| fun () ->
            let data = 42

            let actual =
                voption {
                    use d = null
                    return data
                }

            Expect.equal actual (ValueSome data) "Should be ok"
        testCase "Using disposeable"
        <| fun () ->
            let data = 42

            let actual =
                voption {
                    use d = makeDisposable ()
                    return data
                }

            Expect.equal actual (ValueSome data) "Should be ok"
        testCase "Using bind disposeable"
        <| fun () ->
            let data = 42

            let actual =
                voption {
                    use! d =
                        (makeDisposable ()
                         |> Some)

                    return data
                }

            Expect.equal actual (ValueSome data) "Should be ok"
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
                        voption {
                            while index < maxIndex do
                                index <- index + 1

                            return data
                        }

                    Expect.equal index maxIndex "Index should reach maxIndex"
                    Expect.equal actual (ValueSome data) "Should be ok"
        ]


        testCase "while fail"
        <| fun () ->

            let mutable loopCount = 0
            let mutable wasCalled = false

            let sideEffect () =
                wasCalled <- true
                "ok"

            let expected = ValueNone

            let data = [
                ValueSome "42"
                ValueSome "1024"
                expected
                ValueSome "1M"
                ValueSome "1M"
                ValueSome "1M"
            ]

            let actual =
                voption {
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
                voption {
                    for i in [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (ValueSome data) "Should be ok"
        testCase "For in ResizeArray"
        <| fun () ->
            let data = 42

            let actual =
                voption {
                    for i in ResizeArray [ 1..10 ] do
                        ()

                    return data
                }

            Expect.equal actual (ValueSome data) "Should be ok"
        testCase "For to"
        <| fun () ->
            let data = 42

            let actual =
                voption {
                    for i = 1 to 10 do
                        ()

                    return data
                }

            Expect.equal actual (ValueSome data) "Should be ok"
        testCase "Nullable value"
        <| fun () ->
            let data = 42

            let actual = voption { return! System.Nullable<int> data }

            Expect.equal actual (ValueSome data) ""
        testCase "Nullable null"
        <| fun () ->
            let actual = voption { return! System.Nullable<_>() }
            Expect.equal actual ValueNone ""

        testCase "string value"
        <| fun () ->
            let data = "hello"

            let actual =
                voption {
                    let! v = data
                    return v
                }

            Expect.equal actual (ValueSome data) ""

        testCase "string null"
        <| fun () ->
            let (data: string) = null

            let actual =
                voption {
                    let! v = data
                    return v
                }

            Expect.equal actual ValueNone ""

        testCase "Uri null"
        <| fun () ->
            let (data: UriNull) = null

            let actual =
                voption {
                    let! v = data
                    return v
                }

            Expect.equal actual ValueNone ""

        testCase "MemoryStream null"
        <| fun () ->
            let (data: MemoryStreamNull) = null

            let actual =
                voption {
                    let! v = data
                    return v
                }

            Expect.equal actual ValueNone ""

        testCase "ResizeArray null"
        <| fun () ->
            let (data: ResizeArray<string>) = null

            let actual =
                voption {
                    let! v = data
                    return v
                }

            Expect.equal actual ValueNone ""
    ]

[<AllowNullLiteral>]
type CustomClass(x: int) =

    member _.getX = x


let ``ValueOptionCE inference checks`` =
    testList "ValueOptionCE Inference checks" [
        testCase "Inference checks"
        <| fun () ->
            // Compilation is success
            let f res = voption { return! res }

            f (ValueSome())
            |> ignore
    ]


let allTests =
    testList "ValueOption CE tests" [
        ceTests
        ``ValueOptionCE inference checks``
    ]
#else
let allTests = testList "ValueOption CE tests" []
#endif
