module AsyncSeq

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open FSharp.Control
open FsToolkit.ErrorHandling

let allTests =
    testList "AsyncOption.map Tests" [
        testCaseAsync "simple case 1"
        <| async {
            let xs = asyncSeq {
                1
                2
                3
            }

            let! actual = asyncResult {
                let mutable sum = 0

                for x in xs do
                    sum <- sum + x

                return sum
            }

            let expected = Ok 6

            Expect.equal actual expected ""
        }

        testCaseAsync "simple case 2"
        <| async {
            let xs = asyncSeq {
                Ok 1
                Ok 2
                Error "oh no"
                failwith "Evaluated too far"
            }

            let! actual = asyncResult {
                let mutable sum = 0

                for x in xs do
                    sum <- sum + x

                return sum
            }

            let expected = Error "oh no"

            Expect.equal actual expected ""
        }
    ]
