module Expect

open Expecto

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

let hasAsyncValue v asyncX =
  let x = Async.RunSynchronously asyncX
  if v = x then
    ()
  else Tests.failtestf "Expected %A, was %A." v x

let hasTaskValue v taskX =
  let x =  taskX |> Async.AwaitTask |> Async.RunSynchronously
  if v = x then
    ()
  else Tests.failtestf "Expected %A, was %A." v x


let hasAsyncOkValue v asyncX = 
  let x = Async.RunSynchronously asyncX
  hasOkValue v x

let hasTaskOkValue v taskX = 
  let x =  taskX |> Async.AwaitTask |> Async.RunSynchronously
  hasOkValue v x

let hasAsyncErrorValue v asyncX = 
  let x = Async.RunSynchronously asyncX
  hasErrorValue v x

let hasTaskErrorValue v taskX = 
  let x =  taskX |> Async.AwaitTask |> Async.RunSynchronously
  hasErrorValue v x

let same expected actual =
  Expect.equal actual expected "expected and actual should be same"

