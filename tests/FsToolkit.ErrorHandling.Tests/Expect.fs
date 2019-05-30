namespace TestHelpers 

#if FABLE_COMPILER 
module Tests =
    let failtestf = failwithf 
#endif

module Expect =
#if FABLE_COMPILER 
  open Fable.Mocha
#else
  open Expecto
#endif


  #if FABLE_COMPILER
  let isOk x message =
    match x with
    | Ok _ -> ()
    | Result.Error x ->
      Tests.failtestf "%s. Expected Ok, was Error(%A)." message x
  #endif

  let hasErrorValue v x =
    match x with
    | Ok x -> 
      Tests.failtestf "Expected Error, was Ok(%A)." x
    | Error x when x = v -> ()
    | Error x -> 
      Tests.failtestf "Expected Error(%A), was Error(%A)." v x

  let hasOkValue v x =
    match x with
    | Ok x when x = v -> ()
    | Ok x ->
      Tests.failtestf "Expected Ok(%A), was Ok(%A)." v x
    | Error x -> 
      Tests.failtestf "Expected Ok, was Error(%A)." x

  let hasAsyncValue v asyncX = async {
    let! x = asyncX
    if v = x then
      ()
    else Tests.failtestf "Expected %A, was %A." v x
  }

  let hasTaskValue v taskX =
    let x =  taskX |> Async.AwaitTask |> Async.RunSynchronously
    if v = x then
      ()
    else Tests.failtestf "Expected %A, was %A." v x


  let hasAsyncOkValue v asyncX = async {
    let! x = asyncX
    hasOkValue v x
  }

  let hasTaskOkValue v taskX = 
    let x =  taskX |> Async.AwaitTask |> Async.RunSynchronously
    hasOkValue v x

  let hasAsyncErrorValue v asyncX =  async {
    let! x = asyncX
    hasErrorValue v x
  }

  let hasTaskErrorValue v taskX = 
    let x =  taskX |> Async.AwaitTask |> Async.RunSynchronously
    hasErrorValue v x

  let same expected actual =
    Expect.equal actual expected "expected and actual should be same"

