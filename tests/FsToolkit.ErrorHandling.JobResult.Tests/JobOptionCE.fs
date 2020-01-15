module JobOptionCETests 

open Expecto
open FsToolkit.ErrorHandling
open Hopac
open System.Threading.Tasks

let makeDisposable () =
    { new System.IDisposable
        with member this.Dispose() = () }

[<Tests>]
let ceTests = 
    testList "Job Option CE" [
        testCaseJob "Return value" <| job {
            let expected = Some 42
            let! actual = jobOption  {
                return 42
            }
            Expect.equal actual expected "Should return value wrapped in option"
        }
        testCaseJob "ReturnFrom Some" <| job {
            let expected = Some 42
            let! actual = jobOption  {
                return! (Some 42)
            }
            Expect.equal actual expected "Should return value wrapped in option"
        }
        testCaseJob "ReturnFrom None" <| job {
            let expected = None
            let! actual = jobOption  {
                return! None
            }
            Expect.equal actual expected "Should return value wrapped in option"
        }
        
        testCaseJob "ReturnFrom Async None" <| job {
            let expected = None
            let! actual = jobOption  {
                return! (async.Return None)
            }
            Expect.equal actual expected "Should return value wrapped in option"
        }

        testCaseJob "ReturnFrom Task None" <| job {
            let expected = None
            let! actual = jobOption  {
                return! (Task.FromResult None)
            }
            Expect.equal actual expected "Should return value wrapped in option"
        }

        testCaseJob "ReturnFrom Job None" <| job {
            let expected = None
            let! actual = jobOption  {
                return! (Job.result None)
            }
            Expect.equal actual expected "Should return value wrapped in option"
        }

        testCaseJob "Bind Some" <| job {
            let expected = Some 42
            let! actual = jobOption {
                let! value = Some 42
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        }
        testCaseJob "Bind None" <| job {
            let expected = None
            let! actual = jobOption {
                let! value = None
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        }
        testCaseJob "Bind Async None" <| job {
            let expected = None
            let! actual = jobOption {
                let! value = async.Return(None)
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        }
        testCaseJob "Bind Task None" <| job {
            let expected = None
            let! actual = jobOption {
                let! value = Task.FromResult None
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        }

        testCaseJob "Bind Job None" <| job {
            let expected = None
            let! actual = jobOption {
                let! value = Job.result None
                return value
            }
            Expect.equal actual expected "Should bind value wrapped in option"
        }

        testCaseJob "Zero/Combine/Delay/Run" <| job {
            let data = 42
            let! actual = jobOption {
                let result = data
                if true then ()
                return result
            }
            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseJob "Try With" <| job {
            let data = 42
            let! actual = jobOption {
                try 
                    return data
                with e -> 
                    return raise e
            }
            Expect.equal actual (Some data) "Try with failed"
        }
        testCaseJob "Try Finally" <| job {
            let data = 42
            let! actual = jobOption {
                try 
                    return data
                finally
                    ()
            }
            Expect.equal actual (Some data) "Try with failed"
        }
        testCaseJob "Using null" <| job {
            let data = 42
            let! actual = jobOption {
                use d = null
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseJob "Using disposeable" <| job {
            let data = 42
            let! actual = jobOption {
                use d = makeDisposable ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseJob "Using bind disposeable" <| job {
            let data = 42
            let! actual = jobOption {
                use! d = (makeDisposable () |> Some)
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseJob "While" <| job {
            let data = 42
            let mutable index = 0
            let! actual = jobOption {
                while index < 10 do
                    index <- index + 1
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseJob "For in" <| job {
            let data = 42
            let! actual = jobOption {
                for i in [1..10] do
                    ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        }
        testCaseJob "For to" <| job {
            let data = 42
            let! actual = jobOption {
                for i = 1 to 10 do
                    ()
                return data
            }
            Expect.equal actual (Some data) "Should be ok"
        }
    ]

