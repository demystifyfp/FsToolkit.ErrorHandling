namespace Expects.JobResult

module Expect =
  open Expect
  open Expecto
  open Hopac

  let hasJobValue v jobX =
    let x = run jobX
    if v = x then
      ()
    else Tests.failtestf "Expected %A, was %A." v x


  let hasJobOkValue v jobX = 
    let x = run jobX
    hasOkValue v x


  let hasJobErrorValue v jobX = 
    let x = run jobX
    hasErrorValue v x
