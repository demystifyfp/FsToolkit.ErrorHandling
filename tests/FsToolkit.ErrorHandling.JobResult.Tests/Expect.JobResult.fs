namespace Expects.JobResult

module Expect =
  open Expecto
  open Hopac

  let hasJobValue v jobX =
    let x = run jobX
    if v = x then
      ()
    else Tests.failtestf "Expected %A, was %A." v x


  let hasJobOkValue v jobX = 
    let x = run jobX
    TestHelpers.Expect.hasOkValue v x


  let hasJobErrorValue v jobX = 
    let x = run jobX
    TestHelpers.Expect.hasErrorValue v x
