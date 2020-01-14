module OptionCETests 


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif
open FsToolkit.ErrorHandling

let makeDisposable () =
    { new System.IDisposable
        with member this.Dispose() = () }

let ceTests = 
    testList "CE Tests" [
        testCase "Return value" <| fun _ ->
            let expected = Some 42
            let actual = option  {
                return 42
            }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "ReturnFrom Some" <| fun _ ->
            let expected = Some 42
            let actual = option  {
                return! (Some 42)
            }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "ReturnFrom None" <| fun _ ->
            let expected = None
            let actual = option  {
                return! None
            }
            Expect.equal actual expected "Should return value wrapped in option"
        testCase "Bind Some" <| fun _ -> 
            let expected = Some 42
            let actual = option {
                let! value = Some 42
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        testCase "Bind None" <| fun _ -> 
            let expected = None
            let actual = option {
                let! value = None
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        testCase "Zero/Combine/Delay/Run" <| fun () ->
            let data = 42
            let actual = option {
                let result = data
                if true then ()
                return result
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "Try With" <| fun () ->
            let data = 42
            let actual = option {
                try 
                    return data
                with e -> return! raise e
            }
            Expect.equal actual (Some data) "Try with failed"
        testCase "Try Finally" <| fun () ->
            let data = 42
            let actual = option {
                try 
                    return data
                finally
                    ()
            }
            Expect.equal actual (Some data) "Try with failed"
        testCase "Using null" <| fun () ->
            let data = 42
            let actual = option {
                use d = null
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "Using disposeable" <| fun () ->
            let data = 42
            let actual = option {
                use d = makeDisposable ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "Using bind disposeable" <| fun () ->
            let data = 42
            let actual = option {
                use! d = (makeDisposable () |> Some)
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "While" <| fun () ->
            let data = 42
            let mutable index = 0
            let actual = option {
                while index < 10 do
                    index <- index + 1
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "For in" <| fun () ->
            let data = 42
            let actual = option {
                for i in [1..10] do
                    ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        testCase "For to" <| fun () ->
            let data = 42
            let actual = option {
                for i = 1 to 10 do
                    ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
    ]


let allTests = testList "Option CE tests" [
    ceTests
]