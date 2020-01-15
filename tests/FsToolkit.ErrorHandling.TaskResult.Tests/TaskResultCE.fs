module TaskResultCETests 



open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive

[<Tests>]
let ``TaskResultCE return Tests`` =
    testList "TaskResultCE  Tests" [
        testCaseTask "Return string" <| task {
            let data = "Foo"
            let! actual = taskResult { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]


[<Tests>]
let ``TaskResultCE return! Tests`` =
    testList "TaskResultCE return! Tests" [
        testCaseTask "Return Ok Result" <| task {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = taskResult { return! data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseTask "Return Ok Choice" <| task {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = taskResult { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

        testCaseTask "Return Ok AsyncResult" <| task {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = taskResult { return! Async.singleton data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseTask "Return Ok TaskResult" <| task {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = taskResult { return! Task.FromResult data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseTask "Return Async" <| task {
            let innerData = "Foo"
            let! actual = taskResult { return! Async.singleton innerData }
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseTask "Return Task Generic" <| task {
            let innerData = "Foo"
            let! actual = taskResult { return! Task.singleton innerData }
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseTask "Return Task" <| task {
            let innerData = "Foo"
            let! actual = taskResult { return! Task.FromResult innerData :> Task }
        
            Expect.equal actual (Result.Ok ()) "Should be ok"
        }
    ]


[<Tests>]
let ``TaskResultCE bind Tests`` =
    testList "TaskResultCE bind Tests" [
        testCaseTask "Bind Ok Result" <| task {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = taskResult { 
                let! data = data
                return data 
            }        
            Expect.equal actual (data) "Should be ok"

        }
        testCaseTask "Bind Ok Choice" <| task {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = taskResult { 
                let! data = data
                return data 
            }        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

 
        testCaseTask "Bind Ok AsyncResult" <| task {
            let innerData = "Foo"
            let data = Result.Ok innerData |> Async.singleton
            let! actual = taskResult { 
                let! data = data
                return data 
            }        
        
            Expect.equal actual (data |> Async.RunSynchronously) "Should be ok"
        }
        testCaseTask "Bind Ok TaskResult" <| task {
            let innerData = "Foo"
            let data = Result.Ok innerData |> Task.singleton
            let! actual = taskResult { 
                let! data = data
                return data 
            }        
        
            Expect.equal actual (data.Result) "Should be ok"
        }
        testCaseTask "Bind Async" <| task {
            let innerData = "Foo"
            let! actual = taskResult { 
                let! data = Async.singleton innerData
                return data 
            }        
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseTask "Bind Task Generic" <| task {
            let innerData = "Foo"
            let! actual = taskResult { 
                let! data = Task.FromResult innerData
                return data 
            }        
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseTask "Bind Task" <| task {
            let innerData = "Foo"
            let! actual = taskResult { 
                do! Task.FromResult innerData :> Task
            }        
        
            Expect.equal actual (Result.Ok ()) "Should be ok"
        }
    ]


[<Tests>]
let ``TaskResultCE combine/zero/delay/run Tests`` =
    testList "TaskResultCE combine/zero/delay/run Tests" [
        testCaseTask "Zero/Combine/Delay/Run" <| task {
            let data = 42
            let! actual = taskResult {
                let result = data
                if true then ()
                return result
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]



[<Tests>]
let ``TaskResultCE try Tests`` =
    testList "TaskResultCE try Tests" [
        testCaseTask "Try With" <| task {
            let data = 42
            let! actual = taskResult {
                let data = data
                try ()
                with _ -> ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseTask "Try Finally" <| task {
            let data = 42
            let! actual = taskResult {
                let data = data
                try ()
                finally ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]

let makeDisposable () =
    { new System.IDisposable
        with member this.Dispose() = () }

[<Tests>]
let ``TaskResultCE using Tests`` =
    testList "TaskResultCE using Tests" [
        testCaseTask "use normal disposable" <| task {
            let data = 42
            let! actual = taskResult {
                use d = makeDisposable ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseTask "use! normal wrapped disposable" <| task {
            let data = 42
            let! actual = taskResult {
                use! d = makeDisposable () |> Result.Ok
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseTask "use null disposable" <| task {
            let data = 42
            let! actual = taskResult {
                use d = null
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]


[<Tests>]
let ``TaskResultCE loop Tests`` =
    testList "TaskResultCE loop Tests" [
        testCaseTask "while" <| task {
            let data = 42
            let mutable index = 0
            let! actual = taskResult {
                while index < 10 do
                    index <- index + 1
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseTask "for in" <| task {
            let data = 42
            let! actual = taskResult {
                for i in [1..10] do
                    ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseTask "for to" <| task {
            let data = 42
            let! actual = taskResult {
                for i = 1 to 10 do
                    ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }        
        testCaseTask "for in fail" <| task {
            
            let mutable loopCount = 0
            let expected = Error "error"
            let data = [
                Ok "42"
                Ok "1024"
                expected
                Ok "1M"
            ]
            let! actual = taskResult {
                for i in data do
                    let! x = i
                    loopCount <- loopCount + 1
                    ()
                return "ok"
            }
            Expect.equal 2 loopCount "Should only loop twice"
            Expect.equal actual expected "Should be and error"
        }
    ]