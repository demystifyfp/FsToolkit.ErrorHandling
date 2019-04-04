module AsyncResultCETests 



open Expecto
open SampleDomain
open TestData
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.CE.AsyncResult

[<Tests>]
let ``AsyncResultCE return Tests`` =
    testList "AsyncResultCE  Tests" [
        testCaseAsync "Return string" <| async {
            let data = "Foo"
            let! actual = asyncResult { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]


[<Tests>]
let ``AsyncResultCE return! Tests`` =
    testList "AsyncResultCE return! Tests" [
        testCaseAsync "Return Ok Result" <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { return! data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Ok Choice" <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = asyncResult { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

        testCaseAsync "Return Ok AsyncResult" <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { return! Async.singleton data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Ok TaskResult" <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { return! Task.FromResult data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Return Async" <| async {
            let innerData = "Foo"
            let! actual = asyncResult { return! Async.singleton innerData }
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Return Task Generic" <| async {
            let innerData = "Foo"
            let! actual = asyncResult { return! Task.FromResult innerData }
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Return Task" <| async {
            let innerData = "Foo"
            let! actual = asyncResult { return! Task.FromResult innerData :> Task }
        
            Expect.equal actual (Result.Ok ()) "Should be ok"
        }
    ]


[<Tests>]
let ``AsyncResultCE bind Tests`` =
    testList "AsyncResultCE bind Tests" [
        testCaseAsync "Bind Ok Result" <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { 
                let! data = data
                return data 
            }        
            Expect.equal actual (data) "Should be ok"

        }
        testCaseAsync "Bind Ok Choice" <| async {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = asyncResult { 
                let! data = data
                return data 
            }        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

        testCaseAsync "Bind Ok AsyncResult" <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { 
                let! data = data
                return data 
            }        
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Bind Ok TaskResult" <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { 
                let! data = data
                return data 
            }        
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseAsync "Bind Async" <| async {
            let innerData = "Foo"
            let! actual = asyncResult { 
                let! data = Async.singleton innerData
                return data 
            }        
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Bind Task Generic" <| async {
            let innerData = "Foo"
            let! actual = asyncResult { 
                let! data = Task.FromResult innerData
                return data 
            }        
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseAsync "Bind Task" <| async {
            let innerData = "Foo"
            let! actual = asyncResult { 
                do! Task.FromResult innerData :> Task
            }        
        
            Expect.equal actual (Result.Ok ()) "Should be ok"
        }
    ]


[<Tests>]
let ``AsyncResultCE combine/zero/delay/run Tests`` =
    testList "AsyncResultCE combine/zero/delay/run Tests" [
        testCaseAsync "Zero/Combine/Delay/Run" <| async {
            let data = 42
            let! actual = asyncResult {
                let result = data
                if true then ()
                return result
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]



[<Tests>]
let ``AsyncResultCE try Tests`` =
    testList "AsyncResultCE try Tests" [
        testCaseAsync "Try With" <| async {
            let data = 42
            let! actual = asyncResult {
                let data = data
                try ()
                with _ -> ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "Try Finally" <| async {
            let data = 42
            let! actual = asyncResult {
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
let ``AsyncResultCE using Tests`` =
    testList "AsyncResultCE using Tests" [
        testCaseAsync "use normal disposable" <| async {
            let data = 42
            let! actual = asyncResult {
                use d = makeDisposable ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "use! normal wrapped disposable" <| async {
            let data = 42
            let! actual = asyncResult {
                use! d = makeDisposable () |> Result.Ok
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "use null disposable" <| async {
            let data = 42
            let! actual = asyncResult {
                use d = null
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]


[<Tests>]
let ``AsyncResultCE loop Tests`` =
    testList "AsyncResultCE loop Tests" [
        testCaseAsync "while" <| async {
            let data = 42
            let mutable index = 0
            let! actual = asyncResult {
                while index < 10 do
                    index <- index + 1
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "for in" <| async {
            let data = 42
            let! actual = asyncResult {
                for i in [1..10] do
                    ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseAsync "for to" <| async {
            let data = 42
            let! actual = asyncResult {
                for i = 1 to 10 do
                    ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]