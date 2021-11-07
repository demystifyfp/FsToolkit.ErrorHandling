module AsyncOptionCETests


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif
open FsToolkit.ErrorHandling
open System.Threading.Tasks

let makeDisposable () =
    { new System.IDisposable with
        member this.Dispose() = () }

let ceTests =
    testList
        "CE Tests"
        [ testCaseAsync "Return value"
          <| async {
              let expected = Some 42
              let! actual = asyncOption { return 42 }
              Expect.equal actual expected "Should return value wrapped in option"
             }
          testCaseAsync "ReturnFrom Some"
          <| async {
              let expected = Some 42
              let! actual = asyncOption { return! (Some 42) }
              Expect.equal actual expected "Should return value wrapped in option"
             }
          testCaseAsync "ReturnFrom None"
          <| async {
              let expected = None
              let! actual = asyncOption { return! None }
              Expect.equal actual expected "Should return value wrapped in option"
             }

          testCaseAsync "ReturnFrom Async None"
          <| async {
              let expected = None
              let! actual = asyncOption { return! (async.Return None) }
              Expect.equal actual expected "Should return value wrapped in option"
             }
          testCaseAsync "ReturnFrom Async"
          <| async {
              let expected = Some 42
              let! actual = asyncOption { return! async { return 42 } }
              Expect.equal actual expected "Should return value wrapped in option"
             }

#if !FABLE_COMPILER
          testCaseAsync "ReturnFrom Task None"
          <| async {
              let expected = None
              let! actual = asyncOption { return! (Task.FromResult None) }
              Expect.equal actual expected "Should return value wrapped in option"
             }
          testCaseAsync "ReturnFrom Task"
          <| async {
              let expected = Some 42
              let! actual = asyncOption { return! (Task.FromResult 42) }
              Expect.equal actual expected "Should return value wrapped in option"
             }
#endif
          testCaseAsync "Bind Some"
          <| async {
              let expected = Some 42

              let! actual =
                  asyncOption {
                      let! value = Some 42
                      return value
                  }

              Expect.equal actual expected "Should bind value wrapped in option"
             }
          testCaseAsync "Bind None"
          <| async {
              let expected = None

              let! actual =
                  asyncOption {
                      let! value = None
                      return value
                  }

              Expect.equal actual expected "Should bind value wrapped in option"
             }
          testCaseAsync "Bind Async None"
          <| async {
              let expected = None

              let! actual =
                  asyncOption {
                      let! value = async.Return(None)
                      return value
                  }

              Expect.equal actual expected "Should bind value wrapped in option"
             }
          testCaseAsync "Bind Async"
          <| async {
              let expected = Some 42

              let! actual =
                  asyncOption {
                      let! value = async.Return(42)
                      return value
                  }

              Expect.equal actual expected "Should bind value wrapped in option"
             }

#if !FABLE_COMPILER
          testCaseAsync "Bind Task None"
          <| async {
              let expected = None

              let! actual =
                  asyncOption {
                      let! value = Task.FromResult None
                      return value
                  }

              Expect.equal actual expected "Should bind value wrapped in option"
             }
          testCaseAsync "Bind Task"
          <| async {
              let expected = Some 42

              let! actual =
                  asyncOption {
                      let! value = Task.FromResult 42
                      return value
                  }

              Expect.equal actual expected "Should bind value wrapped in option"
             }
#endif

          testCaseAsync "Zero/Combine/Delay/Run"
          <| async {
              let data = 42

              let! actual =
                  asyncOption {
                      let result = data
                      if true then ()
                      return result
                  }

              Expect.equal actual (Some data) "Should be ok"
             }
          testCaseAsync "Try With"
          <| async {
              let data = 42

              let! actual =
                  asyncOption {
                      try
                          return data
                      with
                      | e -> return raise e
                  }

              Expect.equal actual (Some data) "Try with failed"
             }
          testCaseAsync "Try Finally"
          <| async {
              let data = 42

              let! actual =
                  asyncOption {
                      try
                          return data
                      finally
                          ()
                  }

              Expect.equal actual (Some data) "Try with failed"
             }
          testCaseAsync "Using null"
          <| async {
              let data = 42

              let! actual =
                  asyncOption {
                      use d = null
                      return data
                  }

              Expect.equal actual (Some data) "Should be ok"
             }
          testCaseAsync "Using disposeable"
          <| async {
              let data = 42

              let! actual =
                  asyncOption {
                      use d = makeDisposable ()
                      return data
                  }

              Expect.equal actual (Some data) "Should be ok"
             }
          testCaseAsync "Using bind disposeable"
          <| async {
              let data = 42

              let! actual =
                  asyncOption {
                      use! d = (makeDisposable () |> Some)
                      return data
                  }

              Expect.equal actual (Some data) "Should be ok"
             }
          testCaseAsync "While"
          <| async {
              let data = 42
              let mutable index = 0

              let! actual =
                  asyncOption {
                      while index < 10 do
                          index <- index + 1

                      return data
                  }

              Expect.equal actual (Some data) "Should be ok"
             }
          testCaseAsync "For in"
          <| async {
              let data = 42

              let! actual =
                  asyncOption {
                      for i in [ 1 .. 10 ] do
                          ()

                      return data
                  }

              Expect.equal actual (Some data) "Should be ok"
             }
          testCaseAsync "For to"
          <| async {
              let data = 42

              let! actual =
                  asyncOption {
                      for i = 1 to 10 do
                          ()

                      return data
                  }

              Expect.equal actual (Some data) "Should be ok"
             } ]


let allTests =
    testList "Async Option CE tests" [ ceTests ]
