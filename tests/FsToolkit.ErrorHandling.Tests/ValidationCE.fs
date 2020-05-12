module ValidationCETests 


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open SampleDomain
open TestData
open TestHelpers
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

let ``ValidationCE return Tests`` =
    testList "ValidationCE return Tests" [
        testCase "Return string" <| fun _ ->
            let data = "Foo"
            let actual = validation { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
    ]

let ``ValidationCE return! Tests`` =
    testList "ValidationCE return! Tests" [
        testCase "Return Ok result" <| fun _ ->
            let data = Result.Ok "Foo"
            let actual = validation { return! data }
            Expect.equal actual (data) "Should be ok"
        testCase "Return Error result" <| fun _ ->
            let innerData = "Foo"
            let expected = Validation.error innerData
            let data = Result.Error innerData
            let actual = validation { return! data }
            Expect.equal actual expected "Should be error"
        testCase "Return Ok Choice" <| fun _ ->
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let actual = validation { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        testCase "Return Error Choice" <| fun _ ->
            let innerData = "Foo"
            let expected = Validation.error innerData
            let data = Choice2Of2 innerData
            let actual = validation { return! data }
            Expect.equal actual expected "Should be error"
        testCase "Return Ok Validation" <| fun _ ->
            let innerData = "Foo"
            let data = Validation.ok innerData
            let actual = validation { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        testCase "Return Error Validation" <| fun _ ->
            let innerData = "Foo"
            let expected = Validation.error innerData
            let data = Validation.error innerData
            let actual = validation { return! data }
            Expect.equal actual expected "Should be ok"
    ]


let ``ValidationCE bind Tests`` =
    testList "ValidationCE bind Tests" [
        testCase "let! Ok result" <| fun _ ->
            let data = Result.Ok "Foo"
            let actual = 
                validation { 
                    let! f = data  
                    return f 
                }
            Expect.equal actual (data) "Should be ok"
        testCase "let! Error result" <| fun _ ->
            let innerData = "Foo"
            let data = Result.Error innerData
            let expected = Validation.error innerData
            let actual = 
                validation { 
                    let! f = data  
                    return f 
                }
            Expect.equal actual expected "Should be ok"
        testCase "let! Ok Choice" <| fun _ ->
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let actual = 
                validation { 
                    let! f = data  
                    return f 
                }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        testCase "let! Error Choice" <| fun _ ->
            let innerData = "Foo"
            let data = Choice2Of2 innerData
            let expected = Validation.error innerData
            let actual = 
                validation { 
                    let! f = data  
                    return f 
                }
            Expect.equal actual expected "Should be ok"
        testCase "let! Ok Validation" <| fun _ ->
            let innerData = "Foo"
            let data = Validation.ok innerData
            let actual = 
                validation { 
                    let! f = data  
                    return f 
                }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        testCase "let! Error Validation" <| fun _ ->
            let innerData = "Foo"
            let data = Validation.error innerData
            let expected = Validation.error innerData
            let actual = 
                validation { 
                    let! f = data  
                    return f 
                }
            Expect.equal actual expected "Should be ok"
        testCase "do! Ok result" <| fun _ ->
            let data = Result.Ok ()
            let actual = validation { do! data }
            Expect.equal actual (data) "Should be ok"
        testCase "do! Error result" <| fun _ ->
            let innerData = ()
            let expected = Validation.error innerData
            let data = Result.Error innerData
            let actual = validation {  do! data }
            Expect.equal actual expected "Should be error"
        testCase "do! Ok Choice" <| fun _ ->
            let innerData = ()
            let data = Choice1Of2 innerData
            let actual = validation {  do! data }
            Expect.equal actual (Validation.ok innerData) "Should be ok"
        testCase "do! Error Choice" <| fun _ ->
            let innerData = ()
            let expected = Validation.error innerData
            let data = Choice2Of2 innerData
            let actual = validation { do! data }
            Expect.equal actual expected "Should be error"
        testCase "do! Ok Validation" <| fun _ ->
            let innerData = ()
            let data = Validation.ok innerData
            let actual = validation {  do! data }
            Expect.equal actual (Validation.ok innerData) "Should be ok"
        testCase "do! Error Validation" <| fun _ ->
            let innerData = ()
            let expected = Validation.error innerData
            let data = Validation.error innerData
            let actual = validation { do! data }
            Expect.equal actual expected "Should be error"
    ]


let ``ValidationCE combine/zero/delay/run Tests`` =
    testList "ValidationCE combine/zero/delay/run Tests" [
        testCase "Zero/Combine/Delay/Run" <| fun () ->
            let data = 42
            let actual = validation {
                let result = data
                if true then ()
                return result
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
    ]


let ``ValidationCE try Tests`` =
    testList "ValidationCE try Tests" [
        testCase "Try With" <| fun () ->
            let data = 42
            let actual = validation {
                let data = data
                try ()
                with _ -> ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "Try Finally" <| fun () ->
            let data = 42
            let actual = validation {
                let data = data
                try ()
                finally ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
    ]

let makeDisposable () =
    { new System.IDisposable
        with member this.Dispose() = () }


let ``ValidationCE using Tests`` =
    testList "ValidationCE using Tests" [
        testCase "use normal disposable" <| fun () ->
            let data = 42
            let actual = validation {
                use d = makeDisposable ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "use! normal wrapped disposable" <| fun () ->
            let data = 42
            let actual = validation {
                use! d = makeDisposable () |> Result.Ok
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "use null disposable" <| fun () ->
            let data = 42
            let actual = validation {
                use d = null
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
    ]



let ``ValidationCE loop Tests`` =
    testList "ValidationCE loop Tests" [
        testCase "while" <| fun () ->
            let data = 42
            let mutable index = 0
            let actual = validation {
                while index < 10 do
                    index <- index + 1
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "for in" <| fun () ->
            let data = 42
            let actual = validation {
                for i in [1..10] do
                    ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        testCase "for to" <| fun () ->
            let data = 42
            let actual = validation {
                for i = 1 to 10 do
                    ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
    ]

let ``ValidationCE applicative tests`` =
    testList "ValidationCE applicative tests" [
        testCase "Happy Path Result" <| fun () ->
            let actual : Validation<int, string> = validation {
                let! a = Ok 3
                and! b = Ok 2
                and! c = Ok 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        testCase "Happy Path Valiation" <| fun () ->
            let actual : Validation<int, string> = validation {
                let! a = Validation.ok 3
                and! b = Validation.ok 2
                and! c = Validation.ok 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"

        testCase "Happy Path Result/Valiation" <| fun () ->
            let actual : Validation<int, string> = validation {
                let! a = Validation.ok 3
                and! b = Ok 2
                and! c = Validation.ok 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"

        testCase "Happy Path Choice" <| fun () ->
            let actual = validation {
                let! a = Choice1Of2 3
                and! b = Choice1Of2 2
                and! c = Choice1Of2 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"

        testCase "Happy Path Result/Choice/Validation" <| fun () ->
            let actual = validation {
                let! a = Ok 3
                and! b = Choice1Of2 2
                and! c = Validation.ok 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"


        testCase "Fail Path Result" <| fun () ->
            let expected = Error ["Error 1"; "Error 2"]
            let actual = validation {
                let! a = Ok 3
                and! b = Ok 2
                and! c = Error "Error 1"
                and! d = Error "Error 2"
                return a + b - c - d
            }
            Expect.equal actual expected "Should be Error"

        testCase "Fail Path Validation" <| fun () ->
            let expected = Validation.error "TryParse failure"
            let actual = validation {
                let! a = Validation.ok 3
                and! b = Validation.ok 2
                and! c = expected
                return a + b - c
            }
            Expect.equal actual expected "Should be Error"
    ]

let allTests = testList "Validation CE Tests" [
    ``ValidationCE return Tests``
    ``ValidationCE return! Tests``
    ``ValidationCE bind Tests``
    ``ValidationCE combine/zero/delay/run Tests``
    ``ValidationCE try Tests``
    ``ValidationCE using Tests``
    ``ValidationCE loop Tests``
    ``ValidationCE applicative tests``
]