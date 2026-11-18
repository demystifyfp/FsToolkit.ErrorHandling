namespace Fable.Pyxpecto

open WebSharper
open WebSharper.JavaScript
open FsToolkit.ErrorHandling.WebSharper.TestAdapter

type FocusState =
    | Normal
    | Pending
    | Focused

type TestCase =
    | SyncTest of string * (unit -> unit) * FocusState
    | AsyncTest of string * Async<unit> * FocusState
    | TestList of string * TestCase list * FocusState

[<AutoOpen>]
module Test =
    let testCase name body = SyncTest(name, body, Normal)
    let ptestCase name body = SyncTest(name, body, Pending)
    let ftestCase name body = SyncTest(name, body, Focused)
    let testCaseAsync name body = AsyncTest(name, body, Normal)
    let ptestCaseAsync name body = AsyncTest(name, body, Pending)
    let ftestCaseAsync name body = AsyncTest(name, body, Focused)
    let testList name tests = TestList(name, tests, Normal)
    let ftestList name tests = TestList(name, tests, Focused)
    let ptestList name tests = TestList(name, tests, Pending)

module Expect =
    let equal (actual: 'a) (expected: 'a) (message: string) : unit =
        if
            actual
            <> expected
        then
            failwithf "%s. Expected: %A. Actual: %A." message expected actual

    let notEqual (actual: 'a) (expected: 'a) (message: string) : unit =
        if actual = expected then
            failwithf "%s. Expected a value other than %A." message expected

    let isTrue (condition: bool) (message: string) = equal condition true message
    let isFalse (condition: bool) (message: string) = equal condition false message

    let isOk (x: Result<'ok, 'error>) (message: string) =
        match x with
        | Ok _ -> ()
        | Error e -> failwithf "%s. Expected Ok, was Error(%A)." message e

    let isError (x: Result<'ok, 'error>) (message: string) =
        match x with
        | Error _ -> ()
        | Ok value -> failwithf "%s. Expected Error, was Ok(%A)." message value

    let wantOk (x: Result<'ok, 'error>) (message: string) : 'ok =
        match x with
        | Ok value -> value
        | Error e -> failwithf "%s. Expected Ok, was Error(%A)." message e

    let wantError (x: Result<'ok, 'error>) (message: string) : 'error =
        match x with
        | Error e -> e
        | Ok value -> failwithf "%s. Expected Error, was Ok(%A)." message value

    let isSome (x: 'a option) (message: string) =
        match x with
        | Some _ -> ()
        | None -> failwithf "%s. Expected Some, was None." message

    let wantSome (x: 'a option) (message: string) : 'a =
        match x with
        | Some value -> value
        | None -> failwithf "%s. Expected Some, was None." message

    let isNone (x: 'a option) (message: string) =
        match x with
        | None -> ()
        | Some value -> failwithf "%s. Expected None, was Some(%A)." message value

module Pyxpecto =
    type private FlatTest = {
        Name: string
        State: FocusState
        Run: Async<unit>
    }

    type private Outcome =
        | Passed
        | Failed of string
        | Skipped

    let private combine parent own =
        match parent, own with
        | Pending, _
        | _, Pending -> Pending
        | Focused, _
        | _, Focused -> Focused
        | Normal, Normal -> Normal

    let rec private flatten
        (path: string)
        (parentState: FocusState)
        (test: TestCase)
        : FlatTest list =
        let join name =
            if path = "" then
                name
            else
                path
                + " / "
                + name

        match test with
        | SyncTest(name, body, state) -> [
            {
                Name = join name
                State = combine parentState state
                Run = async { body () }
            }
          ]
        | AsyncTest(name, body, state) -> [
            {
                Name = join name
                State = combine parentState state
                Run = body
            }
          ]
        | TestList(name, tests, state) ->
            tests
            |> List.collect (flatten (join name) (combine parentState state))

    let private applyWebSharperPending (tests: FlatTest list) =
        let unknown =
            WebSharperPending.tests
            |> List.filter (fun (name, _) ->
                tests
                |> List.forall (fun test ->
                    test.Name
                    <> name
                )
            )

        let tests =
            tests
            |> List.map (fun test ->
                match
                    WebSharperPending.tests
                    |> List.tryFind (fun (name, _) -> name = test.Name)
                with
                | Some(_, reason) -> {
                    test with
                        Name =
                            test.Name
                            + " [pending on WebSharper: "
                            + reason
                            + "]"
                        State = Pending
                  }
                | None -> test
            )

        tests, unknown

    let private run (test: FlatTest) =
        async {
            try
                do! test.Run
                return Passed
            with e ->
                return Failed e.Message
        }

    [<Inline "process.exit($code)">]
    let private exitWith (code: int) : unit = X<unit>

    let runTests (_args: string[]) (tests: TestCase) : obj =
        async {
            let all, unknown = applyWebSharperPending (flatten "" Normal tests)

            let anyFocused =
                all
                |> List.exists (fun test -> test.State = Focused)

            let mutable results = []

            for test in all do
                let! outcome =
                    match test.State with
                    | Pending -> async { return Skipped }
                    | Normal when anyFocused -> async { return Skipped }
                    | Normal
                    | Focused -> run test

                results <-
                    (test, outcome)
                    :: results

            let results = List.rev results

            let failed =
                results
                |> List.choose (fun (test, outcome) ->
                    match outcome with
                    | Failed message -> Some(test.Name, message)
                    | _ -> None
                )

            let passed =
                results
                |> List.filter (fun (_, outcome) -> outcome = Passed)
                |> List.length

            let pending =
                results
                |> List.filter (fun (test, _) -> test.State = Pending)
                |> List.length

            let skipped =
                results
                |> List.filter (fun (_, outcome) -> outcome = Skipped)
                |> List.length

            for test, _ in
                results
                |> List.filter (fun (test, _) -> test.State = Pending) do
                printfn "PENDING %s" test.Name

            for name, message in failed do
                printfn "FAILED %s\n    %s" name message

            for name, _ in unknown do
                printfn "ERROR WebSharperPending names a test that does not exist: %s" name

            if anyFocused then
                printfn
                    "ERROR The run has focused tests. Remove ftestCase, ftestCaseAsync, and ftestList."

            printfn
                "WebSharper: %d passed, %d failed, %d pending, %d skipped because other tests are focused, %d total."
                passed
                failed.Length
                pending
                (skipped
                 - pending)
                results.Length

            let succeeded =
                failed.IsEmpty
                && unknown.IsEmpty
                && passed > 0
                && not anyFocused

            exitWith (if succeeded then 0 else 1)
        }
        |> Async.Start

        box 0

// Expecto.fs of FsToolkit.ErrorHandling.Tests opens Expecto outside its .NET-only block. Fable gets the namespace from the Expecto package.
namespace Expecto

module WebSharperPlaceholder =
    let value = ()

namespace Fable.Core

open WebSharper
open WebSharper.JavaScript

module JsInterop =
    [<Inline "$x">]
    let (!!) (x: 'a) : 'b = X<'b>

    [<Inline "(0, eval)($code)">]
    let emitJsStatement (_args: 'a) (code: string) : unit = X<unit>
