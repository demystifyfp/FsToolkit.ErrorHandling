module BackgroundTaskOptionCETests

open Expecto
open FsToolkit.ErrorHandling
open System.Threading.Tasks

#if NETSTANDARD2_0 || NET6_0
let backgroundTask = FSharp.Control.Tasks.NonAffine.task
#endif

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = ()
    }

[<Tests>]
let ceTests =
    testList "Background TaskOption CE" [
        testCaseTask "Return value"
        <| fun () -> backgroundTask {
            let expected = Some 42
            let! actual = backgroundTaskOption { return 42 }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "ReturnFrom Some"
        <| fun () -> backgroundTask {
            let expected = Some 42
            let! actual = backgroundTaskOption { return! (Some 42) }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "ReturnFrom None"
        <| fun () -> backgroundTask {
            let expected = None
            let! actual = backgroundTaskOption { return! None }
            Expect.equal actual expected "Should return value wrapped in option"
           }

        testCaseTask "ReturnFrom Async None"
        <| fun () -> backgroundTask {
            let expected = None
            let! actual = backgroundTaskOption { return! (async.Return None) }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "ReturnFrom Async"
        <| fun () -> backgroundTask {
            let expected = Some 42
            let! actual = backgroundTaskOption { return! (async.Return 42) }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "ReturnFrom Task None"
        <| fun () -> backgroundTask {
            let expected = None
            let! actual = backgroundTaskOption { return! (Task.FromResult None) }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "ReturnFrom Task Generic"
        <| fun () -> backgroundTask {
            let expected = Some 42
            let! actual = backgroundTaskOption { return! (Task.FromResult 42) }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "ReturnFrom Task"
        <| fun () -> backgroundTask {
            let expected = Some()
            let! actual = backgroundTaskOption { return! Task.CompletedTask }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "ReturnFrom ValueTask Generic"
        <| fun () -> backgroundTask {
            let expected = Some 42
            let! actual = backgroundTaskOption { return! (ValueTask.FromResult 42) }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "ReturnFrom ValueTask"
        <| fun () -> backgroundTask {
            let expected = Some()
            let! actual = backgroundTaskOption { return! ValueTask.CompletedTask }
            Expect.equal actual expected "Should return value wrapped in option"
           }
        testCaseTask "Bind Some"
        <| fun () -> backgroundTask {
            let expected = Some 42

            let! actual = backgroundTaskOption {
                let! value = Some 42
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }
        testCaseTask "Bind None"
        <| fun () -> backgroundTask {
            let expected = None

            let! actual = backgroundTaskOption {
                let! value = None
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }
        testCaseTask "Bind Async None"
        <| fun () -> backgroundTask {
            let expected = None

            let! actual = backgroundTaskOption {
                let! value = async.Return(None)
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }
        testCaseTask "Bind Async"
        <| fun () -> backgroundTask {
            let expected = Some 42

            let! actual = backgroundTaskOption {
                let! value = async.Return 42
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }
        testCaseTask "Bind Task None"
        <| fun () -> backgroundTask {
            let expected = None

            let! actual = backgroundTaskOption {
                let! value = Task.FromResult None
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }
        testCaseTask "Bind Task Generic"
        <| fun () -> backgroundTask {
            let expected = Some 42

            let! actual = backgroundTaskOption {
                let! value = Task.FromResult 42
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }
        testCaseTask "Bind Task"
        <| fun () -> backgroundTask {
            let expected = Some()

            let! actual = backgroundTaskOption {
                let! value = Task.CompletedTask
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }
        testCaseTask "Bind ValueTask Generic"
        <| fun () -> backgroundTask {
            let expected = Some 42

            let! actual = backgroundTaskOption {
                let! value = ValueTask.FromResult 42
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }
        testCaseTask "Bind ValueTask"
        <| fun () -> backgroundTask {
            let expected = Some()

            let! actual = backgroundTaskOption {
                let! value = ValueTask.CompletedTask
                return value
            }

            Expect.equal actual expected "Should bind value wrapped in option"
           }

        testCaseTask "Task.Yield"
        <| fun () -> backgroundTask {

            let! actual = backgroundTaskOption { do! Task.Yield() }

            Expect.equal actual (Some()) "Should be ok"
           }
        testCaseTask "Zero/Combine/Delay/Run"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskOption {
                let result = data

                if true then
                    ()

                return result
            }

            Expect.equal actual (Some data) "Should be ok"
           }
        testCaseTask "Try With"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskOption {
                try
                    return data
                with e ->
                    return raise e
            }

            Expect.equal actual (Some data) "Try with failed"
           }
        testCaseTask "Try Finally"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskOption {
                try
                    return data
                finally
                    ()
            }

            Expect.equal actual (Some data) "Try with failed"
           }
        testCaseTask "Using null"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskOption {
                use d = null
                return data
            }

            Expect.equal actual (Some data) "Should be ok"
           }
        testCaseTask "Using disposeable"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskOption {
                use d = makeDisposable ()
                return data
            }

            Expect.equal actual (Some data) "Should be ok"
           }
        testCaseTask "Using bind disposeable"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskOption {
                use! d =
                    (makeDisposable ()
                     |> Some)

                return data
            }

            Expect.equal actual (Some data) "Should be ok"
           }
        testCaseTask "While"
        <| fun () -> backgroundTask {
            let data = 42
            let mutable index = 0

            let! actual = backgroundTaskOption {
                while index < 10 do
                    index <- index + 1

                return data
            }

            Expect.equal actual (Some data) "Should be ok"
           }
        testCaseTask "while fail"
        <| fun () -> backgroundTask {

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

            let! actual = backgroundTaskOption {
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
           }
        testCaseTask "For in"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskOption {
                for i in [ 1..10 ] do
                    ()

                return data
            }

            Expect.equal actual (Some data) "Should be ok"
           }
        testCaseTask "For to"
        <| fun () -> backgroundTask {
            let data = 42

            let! actual = backgroundTaskOption {
                for i = 1 to 10 do
                    ()

                return data
            }

            Expect.equal actual (Some data) "Should be ok"
           }
        testCaseTask "for in fail"
        <| fun () -> backgroundTask {

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

            let! actual = backgroundTaskOption {
                for i in data do
                    let! x = i

                    loopCount <-
                        loopCount
                        + 1

                    ()

                return sideEffect ()
            }

            Expect.equal loopCount 2 "Should only loop twice"
            Expect.equal actual expected "Should be an error"
            Expect.isFalse wasCalled "No additional side effects should occur"
           }
    ]

let specialCaseTask returnValue =
#if NETSTANDARD2_0
    Unsafe.uply { return returnValue }
#else
    Task.FromResult returnValue
#endif

[<Tests>]
let ceTestsApplicative =
    testList "BackgroundTaskOptionCE applicative tests" [
        testCaseTask "Happy Path Option/AsyncOption/Ply/ValueTask"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskOption {
                let! a = Some 3

                let! b =
                    Some 1
                    |> Async.singleton

                let! c = specialCaseTask (Some 3)
                let! d = ValueTask.FromResult(Some 5)

                return
                    a + b
                    - c
                    - d
            }

            Expect.equal actual (Some -4) "Should be ok"
           }
        testCaseTask "Fail Path Option/AsyncOption/Ply/ValueTask"
        <| fun () -> backgroundTask {
            let! actual = backgroundTaskOption {
                let! a = Some 3

                and! b =
                    Some 1
                    |> Async.singleton

                and! c = specialCaseTask (None)
                and! d = ValueTask.FromResult(Some 5)

                return
                    a + b
                    - c
                    - d
            }

            Expect.equal actual None "Should be ok"
           }
    ]
