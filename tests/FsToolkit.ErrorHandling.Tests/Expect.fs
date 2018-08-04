module Expect

open Expecto
let hasOkValuePredicate x f =
  match x with
  | Result.Ok v -> 
    if f v then 
      () 
    else 
      Tests.failtestf "Predicate failed for Ok's Value"
  | Result.Error x -> 
    Tests.failtestf "Expected Ok, was Error(%A)." x

let hasErrorValue x v =
  match x with
  | Ok x -> 
    Tests.failtestf "Expected Error, was Ok(%A)." x
  | Error x when x = v -> ()
  | Error x -> 
    Tests.failtestf "Expected Error(%A), was Error(%A)." v x

