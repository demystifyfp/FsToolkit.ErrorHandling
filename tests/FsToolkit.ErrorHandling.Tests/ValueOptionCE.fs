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
            let (data: Uri) = null

            let actual =
                voption {
                    let! v = data
                    return v
                }

            Expect.equal actual ValueNone ""

        testCase "MemoryStream null"
        <| fun () ->
            let (data: IO.MemoryStream) = null

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


let ``ValueOptionCE applicative tests`` =
    testList "ValueOptionCE applicative tests" [
        testCase "Happy Path ValueOption.ValueSome"
        <| fun () ->
            let actual =
                voption {
                    let! a = ValueSome 3
                    and! b = ValueSome 2
                    and! c = ValueSome 1
                    return a + b - c
                }

            Expect.equal actual (ValueSome 4) "Should be ValueSome 4"

        testCase "Happy Path Nullable"
        <| fun () ->
            let actual =
                voption {
                    let! a = Nullable<_> 3
                    and! b = Nullable<_> 2
                    and! c = Nullable<_> 1
                    return a + b - c
                }

            Expect.equal actual (ValueSome 4) "Should be ValueSome 4"

        testCase "Happy Path null Objects"
        <| fun () ->
            // let hello = CustomClass
            let actual =
                voption {
                    let! a = CustomClass 3
                    and! b = CustomClass 2
                    and! c = CustomClass 1

                    return
                        a.getX
                        + b.getX
                        - c.getX
                }

            Expect.equal actual (ValueSome 4) "Should be ValueSome 4"


        testCase "Happy Path strings"
        <| fun () ->
            let hello = "Hello "
            let world = "world "
            let fromfsharp = "from F#"

            let actual =
                voption {
                    let! a = hello
                    and! b = world
                    and! c = fromfsharp
                    return a + b + c
                }

            Expect.equal actual (ValueSome "Hello world from F#") "Should be Some"

        testCase "Happy Path ResizeArray"
        <| fun () ->
            let r1 = ResizeArray [ 3 ]
            let r2 = ResizeArray [ 2 ]
            let r3 = ResizeArray [ 1 ]

            let actual =
                voption {
                    let! a = r1
                    and! b = r2
                    and! c = r3
                    a.AddRange b
                    a.AddRange c

                    return Seq.sum a
                }

            Expect.equal actual (ValueSome 6) "Should be Some"

        testCase "Happy Path Option.Some/Nullable"
        <| fun () ->
            let actual =
                voption {
                    let! a = ValueSome 3
                    and! b = Nullable 2
                    and! c = Nullable 1
                    return a + b - c
                }

            Expect.equal actual (ValueSome 4) "Should be ValueSome 4"

        testCase "Happy Path Option.Some/Nullable/Objects"
        <| fun () ->
            let actual =
                voption {
                    let! a = ValueSome 3
                    and! b = Nullable 2
                    and! c = CustomClass 1

                    return
                        a + b
                        - c.getX
                }

            Expect.equal actual (ValueSome 4) "Should be ValueSome 4"


        testCase "Happy Combo all"
        <| fun () ->
            let actual =
                voption {
                    let! a = Nullable<_> 3
                    and! b = ValueSome 2
                    and! c = "hello"
                    and! d = ResizeArray [ 1 ]
                    and! e = CustomClass 5
                    and! f = Uri "https://github.com/"
                    return sprintf "%d %d %s %d %d %s" a b c (Seq.head d) e.getX (string f)
                }

            Expect.equal actual (ValueSome "3 2 hello 1 5 https://github.com/") "Should be Some"
        testCase "Fail Path ValueOption.ValueNone"
        <| fun () ->
            let actual =
                voption {
                    let! a = ValueSome 3
                    and! b = ValueSome 2
                    and! c = None
                    return a + b - c
                }

            Expect.equal actual ValueNone "Should be None"

        testCase "Fail Path Nullable"
        <| fun () ->
            let actual =
                voption {
                    let! a = Nullable 3
                    and! b = Nullable 2
                    and! c = Nullable<_>()
                    return a + b - c
                }

            Expect.equal actual (ValueNone) "Should be None"

        testCase "Fail Path Objects"
        <| fun () ->
            let c1 = CustomClass 3
            let c2 = CustomClass 2
            let c3: CustomClass = null

            let actual =
                voption {
                    let! a = c1
                    and! b = c2
                    and! c = c3

                    return
                        a.getX
                        + b.getX
                        - c.getX
                }

            Expect.equal actual (ValueNone) "Should be None"


        testCase "Fail Path strings"
        <| fun () ->
            let c1 = CustomClass 3
            let c2 = CustomClass 2
            let c3: CustomClass = null

            let actual =
                voption {
                    let! a = c1
                    and! b = c2
                    and! c = c3

                    return
                        a.getX
                        + b.getX
                        - c.getX
                }

            Expect.equal actual (ValueNone) "Should be None"

        testCase "Fail Path ValueOption.ValueSome/Nullable"
        <| fun () ->
            let actual =
                voption {
                    let! a = Nullable<_> 3
                    and! b = ValueSome 2
                    and! c = Nullable<_>()
                    return a + b - c
                }

            Expect.equal actual ValueNone "Should be None"

        testCase "ValueOption.ValueSome"
        <| fun () ->
            let actual =
                voption {
                    let! a = ValueSome 3
                    return a
                }

            Expect.equal actual (ValueSome 3) "Should be None"

        testCase "ValueOption.ValueNone"
        <| fun () ->
            let actual =
                voption {
                    let! a = ValueNone
                    return a
                }

            Expect.equal actual (ValueNone) "Should be None"
    ]


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
        ``ValueOptionCE applicative tests``
        ``ValueOptionCE inference checks``
    ]
#else
let allTests = testList "ValueOption CE tests" []
#endif
