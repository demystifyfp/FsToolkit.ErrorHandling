module TaskResultCETests



open Expecto
open SampleDomain
open TestData
open FsToolkit.ErrorHandling
open System.Threading.Tasks

#if NETSTANDARD2_0 || NET5_0
open FSharp.Control.Tasks
#endif

[<Tests>]
let ``TaskResultCE return Tests`` =
    testList
        "TaskResultCE  Tests"
        [ testCaseTask "Return string"
          <| fun () ->
              task {
                  let data = "Foo"
                  let! actual = taskResult { return data }
                  Expect.equal actual (Result.Ok data) "Should be ok"
              } ]


[<Tests>]
let ``TaskResultCE return! Tests`` =
    testList
        "TaskResultCE return! Tests"
        [ testCaseTask "Return Ok Result"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let data = Result.Ok innerData
                  let! actual = taskResult { return! data }

                  Expect.equal actual (data) "Should be ok"
              }
          testCaseTask "Return Ok Choice"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let data = Choice1Of2 innerData
                  let! actual = taskResult { return! data }
                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }

          testCaseTask "Return Ok AsyncResult"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let data = Result.Ok innerData
                  let! actual = taskResult { return! Async.singleton data }

                  Expect.equal actual (data) "Should be ok"
              }
          testCaseTask "Return Ok TaskResult"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let data = Result.Ok innerData
                  let! actual = taskResult { return! Task.FromResult data }

                  Expect.equal actual (data) "Should be ok"
              }
          testCaseTask "Return Async"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let! actual = taskResult { return! Async.singleton innerData }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }
          testCaseTask "Return Task Generic"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let! actual = taskResult { return! Task.singleton innerData }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }
          testCaseTask "Return Task"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let! actual = taskResult { return! Task.FromResult innerData :> Task }

                  Expect.equal actual (Result.Ok()) "Should be ok"
              }
          testCaseTask "Return ValueTask Generic"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let! actual = taskResult { return! ValueTask.FromResult innerData }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }
          testCaseTask "Return ValueTask"
          <| fun () ->
              task {
                  let! actual = taskResult { return! ValueTask.CompletedTask }

                  Expect.equal actual (Result.Ok()) "Should be ok"
              }
#if NETSTANDARD2_0
          testCaseTask "Return Ply"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let! actual = taskResult { return! Unsafe.uply { return innerData } }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }
#endif
          ]


[<Tests>]
let ``TaskResultCE bind Tests`` =
    testList
        "TaskResultCE bind Tests"
        [ testCaseTask "Bind Ok Result"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let data = Result.Ok innerData

                  let! actual =
                      taskResult {
                          let! data = data
                          return data
                      }

                  Expect.equal actual (data) "Should be ok"

              }
          testCaseTask "Bind Ok Choice"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let data = Choice1Of2 innerData

                  let! actual =
                      taskResult {
                          let! data = data
                          return data
                      }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }


          testCaseTask "Bind Ok AsyncResult"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let data = Result.Ok innerData |> Async.singleton

                  let! actual =
                      taskResult {
                          let! data = data
                          return data
                      }

                  Expect.equal actual (data |> Async.RunSynchronously) "Should be ok"
              }
          testCaseTask "Bind Ok TaskResult"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let data = Result.Ok innerData |> Task.singleton

                  let! actual =
                      taskResult {
                          let! data = data
                          return data
                      }

                  Expect.equal actual (data.Result) "Should be ok"
              }
          testCaseTask "Bind Async"
          <| fun () ->
              task {
                  let innerData = "Foo"

                  let! actual =
                      taskResult {
                          let! data = Async.singleton innerData
                          return data
                      }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }
          testCaseTask "Bind Task Generic"
          <| fun () ->
              task {
                  let innerData = "Foo"

                  let! actual =
                      taskResult {
                          let! data = Task.FromResult innerData
                          return data
                      }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }
          testCaseTask "Bind Task"
          <| fun () ->
              task {
                  let innerData = "Foo"
                  let! actual = taskResult { do! Task.FromResult innerData :> Task }

                  Expect.equal actual (Result.Ok()) "Should be ok"
              }
          testCaseTask "Bind ValueTask Generic"
          <| fun () ->
              task {
                  let innerData = "Foo"

                  let! actual =
                      taskResult {
                          let! data = ValueTask.FromResult innerData
                          return data
                      }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }
          testCaseTask "Bind ValueTask"
          <| fun () ->
              task {
                  let! actual = taskResult { do! ValueTask.CompletedTask }

                  Expect.equal actual (Result.Ok()) "Should be ok"
              }
#if NETSTANDARD2_0
          testCaseTask "Bind Ply"
          <| fun () ->
              task {
                  let innerData = "Foo"

                  let! actual =
                      taskResult {
                          let! data = Unsafe.uply { return innerData }
                          return data
                      }

                  Expect.equal actual (Result.Ok innerData) "Should be ok"
              }
#endif
          ]


[<Tests>]
let ``TaskResultCE combine/zero/delay/run Tests`` =
    testList
        "TaskResultCE combine/zero/delay/run Tests"
        [ testCaseTask "Zero/Combine/Delay/Run"
          <| fun () ->
              task {
                  let data = 42

                  let! actual =
                      taskResult {
                          let result = data
                          if true then ()
                          return result
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              } ]



[<Tests>]
let ``TaskResultCE try Tests`` =
    testList
        "TaskResultCE try Tests"
        [ testCaseTask "Try With"
          <| fun () ->
              task {
                  let data = 42

                  let! actual =
                      taskResult {
                          let data = data

                          try
                              ()
                          with
                          | _ -> ()

                          return data
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              }
          testCaseTask "Try Finally"
          <| fun () ->
              task {
                  let data = 42

                  let! actual =
                      taskResult {
                          let data = data

                          try
                              ()
                          finally
                              ()

                          return data
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              } ]

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = () }

[<Tests>]
let ``TaskResultCE using Tests`` =
    testList
        "TaskResultCE using Tests"
        [ testCaseTask "use normal disposable"
          <| fun () ->
              task {
                  let data = 42

                  let! actual =
                      taskResult {
                          use d = makeDisposable ()
                          return data
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              }
          testCaseTask "use! normal wrapped disposable"
          <| fun () ->
              task {
                  let data = 42

                  let! actual =
                      taskResult {
                          use! d = makeDisposable () |> Result.Ok
                          return data
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              }
          testCaseTask "use null disposable"
          <| fun () ->
              task {
                  let data = 42

                  let! actual =
                      taskResult {
                          use d = null
                          return data
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              } ]


[<Tests>]
let ``TaskResultCE loop Tests`` =
    testList
        "TaskResultCE loop Tests"
        [ testCaseTask "while"
          <| fun () ->
              task {
                  let data = 42
                  let mutable index = 0

                  let! actual =
                      taskResult {
                          while index < 10 do
                              index <- index + 1

                          return data
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              }
          testCaseTask "while fail"
          <| fun () ->
              task {
                  let data = 42
                  let mutable index = 0

                  let! actual =
                      taskResult {
                          while index < 10 do
                              index <- index + 1

                          return data
                      }

                  let mutable loopCount = 0
                  let mutable wasCalled = false

                  let sideEffect () =
                      wasCalled <- true
                      "ok"

                  let expected = Error "error"

                  let data =
                      [ Ok "42"
                        Ok "1024"
                        expected
                        Ok "1M"
                        Ok "1M"
                        Ok "1M" ]

                  let! actual =
                      taskResult {
                          while loopCount < data.Length do
                              let! x = data.[loopCount]
                              loopCount <- loopCount + 1

                          return sideEffect ()
                      }

                  Expect.equal loopCount 2 "Should only loop twice"
                  Expect.equal actual expected "Should be an error"
                  Expect.isFalse wasCalled "No additional side effects should occur"
              }
          testCaseTask "for in"
          <| fun () ->
              task {
                  let data = 42

                  let! actual =
                      taskResult {
                          for i in [ 1 .. 10 ] do
                              ()

                          return data
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              }
          testCaseTask "for to"
          <| fun () ->
              task {
                  let data = 42

                  let! actual =
                      taskResult {
                          for i = 1 to 10 do
                              ()

                          return data
                      }

                  Expect.equal actual (Result.Ok data) "Should be ok"
              }
          testCaseTask "for in fail"
          <| fun () ->
              task {

                  let mutable loopCount = 0
                  let expected = Error "error"

                  let data =
                      [ Ok "42"
                        Ok "1024"
                        expected
                        Ok "1M"
                        Ok "1M"
                        Ok "1M" ]

                  let! actual =
                      taskResult {
                          for i in data do
                              let! x = i
                              loopCount <- loopCount + 1
                              ()

                          return "ok"
                      }

                  Expect.equal loopCount 2 "Should only loop twice"
                  Expect.equal actual expected "Should be an error"
              } ]


[<Tests>]
let ``TaskResultCE applicative tests`` =
    testList
        "TaskResultCE applicative tests"
        [ testCaseTask "Happy Path TaskResult"
          <| fun () ->
              task {
                  let! actual =
                      taskResult {
                          let! a = TaskResult.retn 3
                          and! b = TaskResult.retn 2
                          and! c = TaskResult.retn 1
                          return a + b - c
                      }

                  Expect.equal actual (Ok 4) "Should be ok"
              }

          testCaseTask "Happy Path AsyncResult"
          <| fun () ->
              task {
                  let! actual =
                      taskResult {
                          let! a = AsyncResult.retn 3
                          and! b = AsyncResult.retn 2
                          and! c = AsyncResult.retn 1
                          return a + b - c
                      }

                  Expect.equal actual (Ok 4) "Should be ok"
              }


          testCaseTask "Happy Path Result"
          <| fun () ->
              task {
                  let! actual =
                      taskResult {
                          let! a = Result.Ok 3
                          and! b = Result.Ok 2
                          and! c = Result.Ok 1
                          return a + b - c
                      }

                  Expect.equal actual (Ok 4) "Should be ok"
              }

          testCaseTask "Happy Path Choice"
          <| fun () ->
              task {
                  let! actual =
                      taskResult {
                          let! a = Choice1Of2 3
                          and! b = Choice1Of2 2
                          and! c = Choice1Of2 1
                          return a + b - c
                      }

                  Expect.equal actual (Ok 4) "Should be ok"
              }

          testCaseTask "Happy Path Async"
          <| fun () ->
              task {
                  let! actual =
                      taskResult {
                          let! a = Async.singleton 3 //: Async<int>
                          and! b = Async.singleton 2 //: Async<int>
                          and! c = Async.singleton 1 //: Async<int>
                          return a + b - c
                      }

                  Expect.equal actual (Ok 4) "Should be ok"
              }

          testCaseTask "Happy Path 2 Async"
          <| fun () ->
              task {
                  let! actual =
                      taskResult {
                          let! a = Async.singleton 3 //: Async<int>
                          and! b = Async.singleton 2 //: Async<int>
                          return a + b
                      }

                  Expect.equal actual (Ok 5) "Should be ok"
              }

          testCaseTask "Happy Path 2 Task"
          <| fun () ->
              task {
                  let! actual =
                      taskResult {
                          let! a = Task.FromResult 3
                          and! b = Task.FromResult 2
                          return a + b
                      }

                  Expect.equal actual (Ok 5) "Should be ok"
              }
          let specialCaseTask returnValue =
#if NETSTANDARD2_0
              Unsafe.uply { return returnValue }
#else
              Task.FromResult returnValue
#endif

          testCaseTask "Happy Path Result/Choice/AsyncResult/Ply/ValueTask"
          <| fun () ->
              task {
                  let! actual =
                      taskResult {
                          let! a = Ok 3
                          and! b = Choice1Of2 2
                          and! c = Ok 1 |> Async.singleton
                          and! d = specialCaseTask (Ok 3)
                          and! e = ValueTask.FromResult(Ok 5)
                          return a + b - c - d + e
                      }

                  Expect.equal actual (Ok 6) "Should be ok"
              }

          testCaseTask "Fail Path Result"
          <| fun () ->
              task {
                  let expected = Error "TryParse failure"

                  let! actual =
                      taskResult {
                          let! a = Ok 3
                          and! b = Ok 2
                          and! c = expected
                          return a + b - c
                      }

                  Expect.equal actual expected "Should be Error"
              }

          testCaseTask "Fail Path Choice"
          <| fun () ->
              task {
                  let errorMsg = "TryParse failure"

                  let! actual =
                      taskResult {
                          let! a = Choice1Of2 3
                          and! b = Choice1Of2 2
                          and! c = Choice2Of2 errorMsg
                          return a + b - c
                      }

                  Expect.equal actual (Error errorMsg) "Should be Error"
              }

          testCaseTask "Fail Path Result/Choice/AsyncResult"
          <| fun () ->
              task {
                  let errorMsg = "TryParse failure"

                  let! actual =
                      taskResult {
                          let! a = Choice1Of2 3
                          and! b = Ok 2 |> Async.singleton
                          and! c = Error errorMsg
                          return a + b - c
                      }

                  Expect.equal actual (Error errorMsg) "Should be Error"
              } ]
