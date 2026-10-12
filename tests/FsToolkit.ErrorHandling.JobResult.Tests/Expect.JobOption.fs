namespace Expect.JobOption

module Expect =
    open Expecto
    open Hopac

    let hasJobValue v jobX =
        let x = run jobX

        if v = x then () else failtestf "Expected %A, was %A." v x

    let hasJobSomeValue v jobX =
        let x = run jobX
        TestHelpers.Expect.hasSomeValue v x

    let hasJobNoneValue jobX =
        let x = run jobX
        TestHelpers.Expect.hasNoneValue x
