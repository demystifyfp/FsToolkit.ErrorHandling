module JobResultCETests 



open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open System.Threading.Tasks
open Hopac

let testCaseJob name test = testCaseAsync name (Job.toAsync test)
let ptestCaseJob name test = ptestCaseAsync name (Job.toAsync test)
let ftestCaseJob name test = ftestCaseAsync name (Job.toAsync test)

[<Tests>]
let ``JobResultCE return Tests`` =
    testList "JobResultCE  Tests" [
        testCaseJob "Return string" <| job {
            let data = "Foo"
            let! actual = jobResult { return data }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]


[<Tests>]
let ``JobResultCE return! Tests`` =
    testList "JobResultCE return! Tests" [
        testCaseJob "Return Ok Result" <| job {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = jobResult { return! data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseJob "Return Ok Choice" <| job {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = jobResult { return! data }
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

        testCaseJob "Return Ok AsyncResult" <| job {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = jobResult { return! Async.singleton data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseJob "Return Ok TaskResult" <| job {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = jobResult { return! Task.FromResult data }
        
            Expect.equal actual (data) "Should be ok"
        }
        testCaseJob "Return Async" <| job {
            let innerData = "Foo"
            let! actual = jobResult { return! Async.singleton innerData }
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseJob "Return Task Generic" <| job {
            let innerData = "Foo"
            let! actual = jobResult { return! Task.singleton innerData }
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseJob "Return Task" <| job {
            let innerData = "Foo"
            let! actual = jobResult { return! Task.FromResult innerData :> Task }
        
            Expect.equal actual (Result.Ok ()) "Should be ok"
        }
    ]


[<Tests>]
let ``JobResultCE bind Tests`` =
    testList "JobResultCE bind Tests" [
        testCaseJob "Bind Ok Result" <| job {
            let innerData = "Foo"
            let data = Result.Ok innerData
            let! actual = jobResult { 
                let! data = data
                return data 
            }        
            Expect.equal actual (data) "Should be ok"

        }
        testCaseJob "Bind Ok Choice" <| job {
            let innerData = "Foo"
            let data = Choice1Of2 innerData
            let! actual = jobResult { 
                let! data = data
                return data 
            }        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }

 
        testCaseJob "Bind Ok AsyncResult" <| job {
            let innerData = "Foo"
            let data = Result.Ok innerData |> Async.singleton
            let! actual = jobResult { 
                let! data = data
                return data 
            }        
        
            Expect.equal actual (data |> Async.RunSynchronously) "Should be ok"
        }
        testCaseJob "Bind Ok TaskResult" <| job {
            let innerData = "Foo"
            let data = Result.Ok innerData |> Task.singleton
            let! actual = jobResult { 
                let! data = data
                return data 
            }        
        
            Expect.equal actual (data.Result) "Should be ok"
        }
        testCaseJob "Bind Async" <| job {
            let innerData = "Foo"
            let! actual = jobResult { 
                let! data = Async.singleton innerData
                return data 
            }        
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseJob "Bind Task Generic" <| job {
            let innerData = "Foo"
            let! actual = jobResult { 
                let! data = Task.FromResult innerData
                return data 
            }        
        
            Expect.equal actual (Result.Ok innerData) "Should be ok"
        }
        testCaseJob "Bind Task" <| job {
            let innerData = "Foo"
            let! actual = jobResult { 
                do! Task.FromResult innerData :> Task
            }        
        
            Expect.equal actual (Result.Ok ()) "Should be ok"
        }
    ]


[<Tests>]
let ``JobResultCE combine/zero/delay/run Tests`` =
    testList "JobResultCE combine/zero/delay/run Tests" [
        testCaseJob "Zero/Combine/Delay/Run" <| job {
            let data = 42
            let! actual = jobResult {
                let result = data
                if true then ()
                return result
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]



[<Tests>]
let ``JobResultCE try Tests`` =
    testList "JobResultCE try Tests" [
        testCaseJob "Try With" <| job {
            let data = 42
            let! actual = jobResult {
                let data = data
                try ()
                with _ -> ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseJob "Try Finally" <| job {
            let data = 42
            let! actual = jobResult {
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
let ``JobResultCE using Tests`` =
    testList "JobResultCE using Tests" [
        testCaseJob "use normal disposable" <| job {
            let data = 42
            let! actual = jobResult {
                use d = makeDisposable ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseJob "use! normal wrapped disposable" <| job {
            let data = 42
            let! actual = jobResult {
                use! d = makeDisposable () |> Result.Ok
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseJob "use null disposable" <| job {
            let data = 42
            let! actual = jobResult {
                use d = null
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
    ]


[<Tests>]
let ``JobResultCE loop Tests`` =
    testList "JobResultCE loop Tests" [
        testCaseJob "while" <| job {
            let data = 42
            let mutable index = 0
            let! actual = jobResult {
                while index < 10 do
                    index <- index + 1
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseJob "for in" <| job {
            let data = 42
            let! actual = jobResult {
                for i in [1..10] do
                    ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }
        testCaseJob "for to" <| job {
            let data = 42
            let! actual = jobResult {
                for i = 1 to 10 do
                    ()
                return data
            }
            Expect.equal actual (Result.Ok data) "Should be ok"
        }        
        testCaseJob "for in fail" <| job {
            
            let mutable loopCount = 0
            let expected = Error "error"
            let data = [
                Ok "42"
                Ok "1024"
                expected
                Ok "1M"
            ]
            let! actual = jobResult {
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