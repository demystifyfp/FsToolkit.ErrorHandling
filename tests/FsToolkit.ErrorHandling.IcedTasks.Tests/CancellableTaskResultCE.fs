namespace FsToolkit.ErrorHandling.IcedTasks.Tests
open Expecto
open FsToolkit.ErrorHandling
open System.Threading
open System.Threading.Tasks
open IcedTasks
module CancellableTaskResultCE =
    

    [<Tests>]
    let cancellableTaskResultBuilderTests =
        testList
            "CancellableTaskResultBuilder" [ 
                testList "Return" [
                    testCaseTask "return" <| fun () -> task {
                        let data = 42
                        let ctr = 
                            cancellableTaskResult {
                                return data
                            }
                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return value"
                    }
                ]
                testList "ReturnFrom" [
                    testCaseTask "return!" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! cancellableTaskResult { return data } 
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! cancellableTaskResult"
                    }
                    testCaseTask "return! taskResult" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! taskResult { return data } 
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! taskResult"
                    }
                    testCaseTask "return! asyncResult" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! asyncResult { return data } 
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! asyncResult"
                    }
                    testCaseTask "return! asyncChoice" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! async { return Choice1Of2 data } 
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! asyncChoice"
                    }

                    testCaseTask "return! valueTaskResult" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! ValueTask.FromResult(Ok data)
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! valueTaskResult"
                    }
                    testCaseTask "return! result" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! Ok data
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! result"
                    }
                    testCaseTask "return! choice" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! Choice1Of2 data
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! choice"
                    }


                    testCaseTask "return! task<'T>" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! task { return data }
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! task<'T>"
                    }

                    testCaseTask "return! task" <| fun () -> task {
                        let ctr = cancellableTaskResult {
                            return! Task.CompletedTask
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok ()) "Should be able to Return! task"
                    }


                    testCaseTask "return! valuetask<'T>" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! ValueTask.FromResult data
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! valuetask<'T>"
                    }

                    testCaseTask "return! valuetask" <| fun () -> task {
                        let ctr = cancellableTaskResult {
                            return! ValueTask.CompletedTask
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok ()) "Should be able to Return! valuetask"
                    }

                    testCaseTask "return! async<'T>" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! async { return data }
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! async<'T>"
                    }
                    testCaseTask "return! ColdTask<'T>" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! coldTask { return data }
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! ColdTask<'T>"
                    }

                    testCaseTask "return! ColdTask" <| fun () -> task {

                        let ctr = cancellableTaskResult {
                            return! fun () -> Task.CompletedTask
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok ()) "Should be able to Return! ColdTask"
                    }

                    testCaseTask "return! CancellableTask<'T>" <| fun () -> task {
                        let data = 42
                        let ctr = cancellableTaskResult {
                            return! cancellableTask { return data }
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok data) "Should be able to Return! CancellableTask<'T>"
                    }

                    testCaseTask "return! CancellableTask" <| fun () -> task {

                        let ctr = cancellableTaskResult {
                            return! fun ct -> Task.CompletedTask
                        }

                        let! actual = ctr CancellationToken.None
                        Expect.equal actual (Ok ()) "Should be able to Return! CancellableTask"
                    }

                    // testCaseTask "return! TaskLike" <| fun () -> task {
                    //     let ctr = cancellableTaskResult {
                    //         return! Task.Yield()
                    //     }

                    //     let! actual = ctr CancellationToken.None
                    //     Expect.equal actual (Ok ()) "Should be able to Return! CancellableTask"
                    // }

                ]

                // testList "Binds" [
                //     testCaseAsync "let!" <| async {
                //         let data = 42
                //         let! actual = parallelAsync {

                //             let! someValue = async.Return data
                //             return someValue
                //         }
                //         Expect.equal actual data "Should be able to Return! value"
                //     }
                //     testCaseAsync "do!" <| async {
                //         do! parallelAsync {
                //             do! async.Return ()
                //         }
                //     }

                // ]
                // testList "Zero/Combine/Delay" [
                //     testCaseAsync "if statement" <| async {
                //         let data = 42
                //         let! actual = parallelAsync {
                //             let result = data
                //             if true then ()
                //             return result
                //         }
                //         Expect.equal actual data "Zero/Combine/Delay should work"
                //     }
                // ]
                // testList "TryWith" [
                //     testCaseAsync "try with" <| async {
                //         let data = 42
                //         let! actual = parallelAsync {
                //             let data = data
                //             try ()
                //             with _ -> ()

                //             return data
                //         }
                //         Expect.equal actual data "TryWith should work"
                //     }
                // ]


                // testList "TryFinally" [
                //     testCaseAsync "try finally" <| async {
                //         let data = 42
                //         let! actual = parallelAsync {
                //             let data = data
                //             try ()
                //             finally ()

                //             return data
                //         }
                //         Expect.equal actual data "TryFinally should work"
                //     }
                // ]

                // testList "Using" [
                //     testCaseAsync "use" <| async {
                //         let data = 42

                //         let! actual =
                //             parallelAsync {
                //                 use d = TestHelpers.makeDisposable ()
                //                 return data
                //             }

                //         Expect.equal actual data "Should be able to use use"
                //     }
                //     testCaseAsync "use!" <| async {
                //         let data = 42

                //         let! actual =
                //             parallelAsync {
                //                 use! d = TestHelpers.makeDisposable () |> async.Return
                //                 return data
                //             }

                //         Expect.equal actual data "Should be able to use use"
                //     }
                //     testCaseAsync "null" <| async {
                //         let data = 42

                //         let! actual =
                //             parallelAsync {
                //                 use d = null
                //                 return data
                //             }

                //         Expect.equal actual data "Should be able to use use"
                //     }
                // ]

                // testList "While" [
                //     testCaseAsync "while" <| async {
                //         let loops = 10
                //         let mutable index = 0

                //         let! actual =
                //             parallelAsync {
                //                 while index < loops do
                //                     index <- index + 1

                //                 return index
                //             }

                //         Expect.equal actual loops "Should be ok"
                //     }
                // ]

                // testList "For" [
                //     testCaseAsync "for in" <| async {
                //         let loops = 10
                //         let mutable index = 0

                //         let! actual =
                //             parallelAsync {
                //                 for i in [ 1 .. 10 ] do
                //                     index <- i + i

                //                 return index
                //             }

                //         Expect.equal actual index "Should be ok"
                //     }


                //     testCaseAsync "for to" <| async {
                //         let loops = 10
                //         let mutable index = 0

                //         let! actual =
                //             parallelAsync {
                //                 for i = 1 to loops do
                //                     index <- i + i

                //                 return index
                //             }

                //         Expect.equal actual index "Should be ok"
                //     }
                // ]

                // testList "MergeSources" [
                //     testCaseAsync "and!" <| async {
                //         let data = 42
                //         let! actual = parallelAsync {
                //             let! r1 = async.Return data
                //             and! r2 = async.Return data
                //             and! r3 = async.Return data

                //             return r1 + r2 + r3
                //         }

                //         Expect.equal actual 126 "and! works"
                //     }
                // ]
            //   testCaseAsync "Simple Return"
            //   <| async {
            //       let foo = cancellableTask { return "lol" }
            //       let! result = foo |> Async.AwaitCancellableTask

            //       Expect.equal result "lol" ""
            //   }

            //   testCaseAsync "Simple Cancellation"
            //   <| async {
            //       do!
            //           Expect.CancellationRequested(
            //               cancellableTask {

            //                   let foo = cancellableTask { return "lol" }
            //                   use cts = new CancellationTokenSource()
            //                   cts.Cancel()
            //                   let! result = foo cts.Token
            //                   Expect.equal result "lol" ""
            //               }
            //           )
            //   }


            //   testCaseAsync "CancellableTasks are lazily evaluated"
            //   <| async {

            //       let mutable someValue = null

            //       do!
            //           Expect.CancellationRequested(
            //               cancellableTask {
            //                   let fooColdTask = cancellableTask { someValue <- "lol" }
            //                   do! Async.Sleep(100)
            //                   Expect.equal someValue null ""
            //                   use cts = new CancellationTokenSource()
            //                   cts.Cancel()
            //                   let fooAsync = fooColdTask cts.Token
            //                   do! Async.Sleep(100)
            //                   Expect.equal someValue null ""

            //                   do! fooAsync

            //                   Expect.equal someValue "lol" ""
            //               }
            //           )

            //       Expect.equal someValue null ""
            //   }
            //   testCaseAsync "Can extract context's CancellationToken via CancellableTask.getCancellationToken"
            //   <| async {
            //       let fooTask =
            //           cancellableTask {
            //               let! ct = CancellableTask.getCancellationToken
            //               return ct
            //           }

            //       use cts = new CancellationTokenSource()
            //       let! result = fooTask cts.Token |> Async.AwaitTask
            //       Expect.equal result cts.Token ""
            //   }

            //   testCaseAsync
            //       "Can extract context's CancellationToken via CancellableTask.getCancellationToken in a deeply nested CE"
            //   <| async {
            //       do!
            //           Expect.CancellationRequested(
            //               cancellableTask {
            //                   let fooTask =
            //                       cancellableTask {
            //                           return!
            //                               cancellableTask {
            //                                   do!
            //                                       cancellableTask {
            //                                           let! ct = CancellableTask.getCancellationToken
            //                                           do! Task.Delay(1000, ct)
            //                                       }
            //                               }
            //                       }

            //                   use cts = new CancellationTokenSource()
            //                   cts.CancelAfter(100)
            //                   do! fooTask cts.Token
            //               }
            //           )

            //   }

            //   testCaseAsync "pass along CancellationToken to async bind"
            //   <| async {

            //       let fooTask =
            //           cancellableTask {
            //               let! result =
            //                   async {
            //                       let! ct = Async.CancellationToken
            //                       return ct
            //                   }

            //               return result
            //           }

            //       use cts = new CancellationTokenSource()
            //       let! passedct = fooTask cts.Token |> Async.AwaitTask
            //       Expect.equal passedct cts.Token ""
            //   }


            //   testCaseAsync "Can Bind CancellableTask"
            //   <| async {
            //       let fooTask: CancellableTask = fun ct -> Task.FromResult()
            //       let outerTask = cancellableTask { do! fooTask }
            //       use cts = new CancellationTokenSource()
            //       do! outerTask cts.Token |> Async.AwaitTask
            //   // Compiling is a sufficient Expect
            //   }
            //   testCaseAsync "Can ReturnFrom CancellableTask"
            //   <| async {
            //       let fooTask: CancellableTask = fun ct -> Task.FromResult()
            //       let outerTask = cancellableTask { return! fooTask }
            //       use cts = new CancellationTokenSource()
            //       do! outerTask cts.Token |> Async.AwaitTask
            //   // Compiling is a sufficient Expect
            //   }

            //   testCaseAsync "Can Bind CancellableTask<T>"
            //   <| async {
            //       let expected = "lol"
            //       let fooTask: CancellableTask<_> = fun ct -> Task.FromResult expected

            //       let outerTask =
            //           cancellableTask {
            //               let! result = fooTask
            //               return result
            //           }

            //       use cts = new CancellationTokenSource()
            //       let! actual = outerTask cts.Token |> Async.AwaitTask
            //       Expect.equal actual expected ""
            //   }

            //   testCaseAsync "Can ReturnFrom CancellableTask<T>"
            //   <| async {
            //       let expected = "lol"
            //       let fooTask: CancellableTask<_> = fun ct -> Task.FromResult expected
            //       let outerTask = cancellableTask { return! fooTask }
            //       use cts = new CancellationTokenSource()
            //       let! actual = outerTask cts.Token |> Async.AwaitTask
            //       Expect.equal actual expected ""
            //   }


            //   testCaseAsync "Can Bind Task"
            //   <| async {
            //       let outerTask = cancellableTask { do! Task.FromResult() }
            //       use cts = new CancellationTokenSource()
            //       do! outerTask cts.Token |> Async.AwaitTask
            //   // Compiling is a sufficient Expect
            //   }
            //   testCaseAsync "Can ReturnFrom Task"
            //   <| async {
            //       let outerTask = cancellableTask { return! Task.FromResult() }
            //       use cts = new CancellationTokenSource()
            //       do! outerTask cts.Token |> Async.AwaitTask
            //   // Compiling is a sufficient Expect
            //   }

            //   testCaseAsync "Can Bind Task<T>"
            //   <| async {
            //       let expected = "lol"

            //       let outerTask =
            //           cancellableTask {
            //               let! result = Task.FromResult expected
            //               return result
            //           }

            //       use cts = new CancellationTokenSource()
            //       let! actual = outerTask cts.Token |> Async.AwaitTask
            //       Expect.equal actual expected ""
            //   }
            //   testCaseAsync "Can ReturnFrom Task<T>"
            //   <| async {
            //       let expected = "lol"
            //       let outerTask = cancellableTask { return! Task.FromResult expected }
            //       use cts = new CancellationTokenSource()
            //       let! actual = outerTask cts.Token |> Async.AwaitTask
            //       Expect.equal actual expected ""
            //   }

            //   testCaseAsync "Can Bind ColdTask<T>"
            //   <| async {
            //       let expected = "lol"

            //       let coldT = coldTask { return expected }

            //       let outerTask =
            //           cancellableTask {
            //               let! result = coldT
            //               return result
            //           }

            //       use cts = new CancellationTokenSource()
            //       let! actual = outerTask cts.Token |> Async.AwaitTask
            //       Expect.equal actual expected ""
            //   }
            //   testCaseAsync "Can ReturnFrom ColdTask<T>"
            //   <| async {
            //       let expected = "lol"
            //       let coldT = coldTask { return expected }
            //       let outerTask = cancellableTask { return! coldT }
            //       use cts = new CancellationTokenSource()
            //       let! actual = outerTask cts.Token |> Async.AwaitTask
            //       Expect.equal actual expected ""
            //   }


            //   testCaseAsync "Can Bind ColdTask"
            //   <| async {

            //       let coldT: ColdTask = fun () -> Task.FromResult()

            //       let outerTask =
            //           cancellableTask {
            //               let! result = coldT
            //               return result
            //           }

            //       use cts = new CancellationTokenSource()
            //       do! outerTask cts.Token |> Async.AwaitTask
            //   // Compiling is a sufficient Expect
            //   }
            //   testCaseAsync "Can ReturnFrom ColdTask"
            //   <| async {
            //       let coldT: ColdTask = fun () -> Task.FromResult()
            //       let outerTask = cancellableTask { return! coldT }
            //       use cts = new CancellationTokenSource()
            //       do! outerTask cts.Token |> Async.AwaitTask
            //   // Compiling is a sufficient Expect
            //   }


            //   testCaseAsync "Can Bind cold TaskLike"
            //   <| async {
            //       let fooTask = fun () -> Task.Yield()

            //       let outerTask =
            //           cancellableTask {
            //               let! result = fooTask
            //               return result
            //           }

            //       use cts = new CancellationTokenSource()
            //       do! outerTask cts.Token |> Async.AwaitTask
            //   // Compiling is sufficient expect
            //   }
            //   testCaseAsync "Can ReturnFrom cold TaskLike"
            //   <| async {
            //       let fooTask = fun () -> Task.Yield()
            //       let outerTask = cancellableTask { return! fooTask }
            //       use cts = new CancellationTokenSource()
            //       do! outerTask cts.Token |> Async.AwaitTask
            //   // Compiling is sufficient expect
            //   }

            //   testCaseAsync "Can Bind Async<T>"
            //   <| async {
            //       let expected = "lol"
            //       let fooTask = async.Return expected

            //       let outerTask =
            //           cancellableTask {
            //               let! result = fooTask
            //               return result
            //           }

            //       use cts = new CancellationTokenSource()
            //       let! actual = outerTask cts.Token |> Async.AwaitTask
            //       Expect.equal actual expected ""
            //   }
            //   testCaseAsync "Can ReturnFrom Async<T>"
            //   <| async {
            //       let expected = "lol"
            //       let fooTask = async.Return expected
            //       let outerTask = cancellableTask { return! fooTask }
            //       use cts = new CancellationTokenSource()
            //       let! actual = outerTask cts.Token |> Async.AwaitTask
            //       Expect.equal actual expected ""
            //   }

            //   testCase "CancellationToken flows from Async<unit> to CancellableTask<T> via Async.AwaitCancellableTask"
            //   <| fun () ->
            //       let innerTask = cancellableTask { return! CancellableTask.getCancellationToken }
            //       let outerAsync = async { return! innerTask |> Async.AwaitCancellableTask }

            //       use cts = new CancellationTokenSource()
            //       let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)
            //       Expect.equal actual cts.Token ""

            //   testCase "CancellationToken flows from Async<unit> to CancellableTask via Async.AwaitCancellableTask"
            //   <| fun () ->
            //       let mutable actual = CancellationToken.None
            //       let innerTask: CancellableTask = fun ct -> task { actual <- ct } :> Task
            //       let outerAsync = async { return! innerTask |> Async.AwaitCancellableTask }

            //       use cts = new CancellationTokenSource()
            //       Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)
            //       Expect.equal actual cts.Token ""


            //   testCase "AsyncBuilder can Bind CancellableTask<T>"
            //   <| fun () ->
            //       let innerTask = cancellableTask { return! CancellableTask.getCancellationToken }

            //       let outerAsync =
            //           async {
            //               let! result = innerTask
            //               return result
            //           }

            //       use cts = new CancellationTokenSource()
            //       let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)
            //       Expect.equal actual cts.Token ""


            //   testCase "AsyncBuilder can ReturnFrom CancellableTask<T>"
            //   <| fun () ->
            //       let innerTask = cancellableTask { return! CancellableTask.getCancellationToken }
            //       let outerAsync = async { return! innerTask }

            //       use cts = new CancellationTokenSource()
            //       let actual = Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)
            //       Expect.equal actual cts.Token ""


            //   testCase "AsyncBuilder can Bind CancellableTask"
            //   <| fun () ->
            //       let mutable actual = CancellationToken.None
            //       let innerTask: CancellableTask = fun ct -> task { actual <- ct } :> Task
            //       let outerAsync = async { do! innerTask }

            //       use cts = new CancellationTokenSource()
            //       Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)
            //       Expect.equal actual cts.Token ""
            //   testCase "AsyncBuilder can ReturnFrom CancellableTask"
            //   <| fun () ->
            //       let mutable actual = CancellationToken.None
            //       let innerTask: CancellableTask = fun ct -> task { actual <- ct } :> Task
            //       let outerAsync = async { return! innerTask }

            //       use cts = new CancellationTokenSource()
            //       Async.RunSynchronously(outerAsync, cancellationToken = cts.Token)
            //       Expect.equal actual cts.Token "" 
            ]