module AsyncResultCETests 


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif
open SampleDomain
open TestData
open TestHelpers
open System.Threading.Tasks
open FsToolkit.ErrorHandling


let ``AsyncResultCE return Tests`` =
    testList "AsyncResultCE  Tests" [
        testCaseAsync "Return string" <| async {
            let data = "Foo"
            let! actual = asyncResult { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]



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
        testCaseAsync "Return Async" <| async {
            let innerData = "Foo"
            let! actual = asyncResult { return! Async.singleton innerData }
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        #if !FABLE_COMPILER
        testCaseAsync "Return Ok TaskResult" <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = asyncResult { return! Task.FromResult data }
        
            Expect.equal actual (data) "Should be ok"
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
        #endif

        
    ]



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
            let data = Result.Ok innerData |> Async.singleton
            let! actual = asyncResult { 
                let! data = data
                return data 
            }        
            let! data = data
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
        

        #if !FABLE_COMPILER        
        testCaseAsync "Bind Ok TaskResult" <| async {
            let innerData = "Foo"
            let data = Result.Ok innerData |> Task.FromResult
            let! actual = asyncResult { 
                let! data = data
                return data 
            }        
        
            Expect.equal actual (data.Result) "Should be ok"
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
        #endif
    ]



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
        #if !FABLE_COMPILER
        // Fable can't handle null disposables you get
        // TypeError: Cannot read property 'Dispose' of null
        testCaseAsync "use null disposable" <| async {
            let data = 42
            let! actual = asyncResult {
                use d = null
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        #endif
    ]



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
        testCaseAsync "for in fail" <| async {
            let mutable loopCount = 0
            let expected = Error "error"
            let data = [
                Ok "42"
                Ok "1024"
                expected
                Ok "1M"
            ]
            let! actual = asyncResult {
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

let allTests = testList "AsyncResultCETests" [
    ``AsyncResultCE return Tests``
    ``AsyncResultCE return! Tests``
    ``AsyncResultCE bind Tests``
    ``AsyncResultCE combine/zero/delay/run Tests``
    ``AsyncResultCE try Tests``
    ``AsyncResultCE using Tests``
    ``AsyncResultCE loop Tests``
]