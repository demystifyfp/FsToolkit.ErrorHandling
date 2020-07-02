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


[<Tests>]
let ``AsyncResultCE applicative tests`` =
    testList "JobResultCE applicative tests" [
        testCaseJob "Happy Path JobResult" <| job {
            let! actual = jobResult {
                let! a = JobResult.retn 3
                and! b = JobResult.retn 2
                and! c = JobResult.retn 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        }
        testCaseJob "Happy Path AsyncResult" <| job {
            let! actual = jobResult {
                let! a = AsyncResult.retn 3
                and! b = AsyncResult.retn 2
                and! c = AsyncResult.retn 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseJob "Happy Path TaskResult" <| job {
            let! actual = jobResult {
                let! a = TaskResult.retn 3
                and! b = TaskResult.retn 2
                and! c = TaskResult.retn 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        }


        testCaseJob "Happy Path Result" <| job {
            let! actual = jobResult {
                let! a = Result.Ok 3
                and! b = Result.Ok 2
                and! c = Result.Ok 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseJob "Happy Path Choice" <| job {
            let! actual = jobResult {
                let! a = Choice1Of2 3
                and! b = Choice1Of2 2
                and! c = Choice1Of2 1
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseJob "Happy Path Async" <| job {
            let! actual = jobResult {
                let! a = Async.singleton 3 //: Async<int>
                and! b = Async.singleton 2 //: Async<int>
                and! c = Async.singleton 1 //: Async<int>
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseJob "Happy Path 2 Async" <| job {
            let! actual = jobResult {
                let! a = Async.singleton 3 //: Async<int>
                and! b = Async.singleton 2 //: Async<int>
                return a + b 
            }
            Expect.equal actual (Ok 5) "Should be ok"
        }

        testCaseJob "Happy Path 2 Task" <| job {
            let! actual = jobResult {
                let! a = Task.FromResult 3 
                and! b = Task.FromResult 2 
                return a + b 
            }
            Expect.equal actual (Ok 5) "Should be ok"
        }

        testCaseJob "Happy Path Result/Choice/AsyncResult" <| job {
            let! actual = jobResult {
                let! a = Ok 3
                and! b = Choice1Of2 2
                and! c = Ok 1 |> Async.singleton
                return a + b - c
            }
            Expect.equal actual (Ok 4) "Should be ok"
        }

        testCaseJob "Fail Path Result" <| job {
            let expected = Error "TryParse failure"
            let! actual = jobResult {
                let! a = Ok 3
                and! b = Ok 2
                and! c = expected
                return a + b - c
            }
            Expect.equal actual expected "Should be Error"
        }
            
        testCaseJob "Fail Path Choice" <| job {
            let errorMsg = "TryParse failure"
            let! actual = jobResult {
                let! a = Choice1Of2 3
                and! b = Choice1Of2 2
                and! c = Choice2Of2 errorMsg
                return a + b - c
            }
            Expect.equal actual (Error errorMsg) "Should be Error"
        }

        testCaseJob "Fail Path Result/Choice/AsyncResult" <| job {
            let errorMsg = "TryParse failure"
            let! actual = jobResult {
                let! a = Choice1Of2 3
                and! b = Ok 2 |> Async.singleton
                and! c = Error errorMsg
                return a + b - c
            }
            Expect.equal actual (Error errorMsg) "Should be Error"
        }
    ]
