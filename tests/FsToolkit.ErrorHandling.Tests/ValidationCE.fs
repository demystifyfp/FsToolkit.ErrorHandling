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

let ``ResultCE return Tests`` =
    testList "ValidationCE return Tests" [
        testCase "Return string" <| fun _ ->
            let data = "Foo"
            let actual = validation { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
    ]


let ``ResultCE applicative tests`` =
    testList "ResultCE applicative tests" [
        testCase "Happy Path Result" <| fun () ->
            let actual : Validation<int, string> = validation {
                let! a = Ok 3
                and! b = Ok 2
                and! c = Ok 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        // testCase "Happy Path Valiation" <| fun () ->
        //     let actual : Validation<int, string> = validation {
        //         let! a = Validation.ok 3
        //         and! b = Validation.ok 2
        //         and! c = Validation.ok 1
        //         return a + b - c
        //     }
        //     Expect.equal actual (Ok 4) "Should be ok"

        // testCase "Happy Path Result/Valiation" <| fun () ->
        //     let actual : Validation<int, string> = validation {
        //         let! a = Validation.ok 3
        //         and! b = Ok 2
        //         and! c = Validation.ok 1
        //         return a + b - c
        //     }
        //     Expect.equal actual (Ok 4) "Should be ok"

        // testCase "Happy Path Choice" <| fun () ->
        //     let actual = result {
        //         let! a = Choice1Of2 3
        //         and! b = Choice1Of2 2
        //         and! c = Choice1Of2 1
        //         return a + b - c
        //     }
        //     Expect.equal actual (Ok 4) "Should be ok"

        // testCase "Happy Path Result/Choice" <| fun () ->
        //     let actual = result {
        //         let! a = Ok 3
        //         and! b = Choice1Of2 2
        //         and! c = Choice1Of2 1
        //         return a + b - c
        //     }
        //     Expect.equal actual (Ok 4) "Should be ok"

        // testCase "Fail Path Result" <| fun () ->
        //     let expected = Error "TryParse failure"
        //     let actual = validation {
        //         let! a = Ok 3
        //         and! b = Ok 2
        //         and! c = expected
        //         return a + b - c
        //     }
        //     Expect.equal actual expected "Should be Error"

        testCase "Fail Path Result" <| fun () ->
            let expected = Error ["Error 1"; "Error 2"]
            let actual = validation {
                let! a = Ok 3
                // and! b = Ok 2
                and! b = (Ok 2 : Result<_,string>)
                and! c = Error "Error 1"
                and! d = Error "Error 2"
                return a + b - c - d
            }
            Expect.equal actual expected "Should be Error"

        // testCase "Fail Path Validation" <| fun () ->
        //     let expected = Validation.error "TryParse failure"
        //     let actual = validation {
        //         let! a = Ok 3
        //         and! b = Ok 2
        //         and! c = expected
        //         return a + b - c
        //     }
        //     Expect.equal actual expected "Should be Error"
            
        // testCase "Fail Path Choice" <| fun () ->
        //     let errorMsg = "TryParse failure"
        //     let actual = result {
        //         let! a = Choice1Of2 3
        //         and! b = Choice1Of2 2
        //         and! c = Choice2Of2 errorMsg
        //         return a + b - c
        //     }
        //     Expect.equal actual (Error errorMsg) "Should be Error"

        // testCase "Fail Path Result/Choice" <| fun () ->
        //     let errorMsg = "TryParse failure"
        //     let actual = result {
        //         let! a = Choice1Of2 3
        //         and! b = Ok 2
        //         and! c = Error errorMsg
        //         return a + b - c
        //     }
        //     Expect.equal actual (Error errorMsg) "Should be Error"
    ]

let allTests = testList "Validation CE Tests" [
    ``ResultCE return Tests`` 
    ``ResultCE applicative tests``
]