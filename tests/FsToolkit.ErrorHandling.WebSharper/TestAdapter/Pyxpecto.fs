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

    let isTrue (condition: bool) (message: string) = equal condition true message
    let isFalse (condition: bool) (message: string) = equal condition false message

    let isOk (x: Result<'ok, 'error>) (message: string) =
        match x with
        | Ok _ -> ()
        | Error e -> failwithf "%s. Expected Ok, was Error(%A)." message e

    let wantOk (x: Result<'ok, 'error>) (message: string) : 'ok =
        match x with
        | Ok value -> value
        | Error e -> failwithf "%s. Expected Ok, was Error(%A)." message e

    let wantError (x: Result<'ok, 'error>) (message: string) : 'error =
        match x with
        | Error e -> e
        | Ok value -> failwithf "%s. Expected Error, was Ok(%A)." message value

    let wantSome (x: 'a option) (message: string) : 'a =
        match x with
        | Some value -> value
        | None -> failwithf "%s. Expected Some, was None." message

    let isNone (x: 'a option) (message: string) =
        match x with
        | None -> ()
        | Some value -> failwithf "%s. Expected None, was Some(%A)." message value

module Pyxpecto =
    let private testTimeoutMilliseconds = 60000

    type private Kind =
        | Run
        | SourcePending
        | WebSharperPending of reason: string

    type private FlatTest = {
        Name: string
        Kind: Kind
        Focused: bool
        Body: Async<unit>
    }

    type private Outcome =
        | Passed
        | Failed of string
        | NotRun

    let rec private hasFocus test =
        match test with
        | SyncTest(_, _, Focused)
        | AsyncTest(_, _, Focused)
        | TestList(_, _, Focused) -> true
        | TestList(_, tests, _) -> List.exists hasFocus tests
        | SyncTest _
        | AsyncTest _ -> false

    let rec private flatten
        (path: string)
        (pending: bool)
        (focused: bool)
        (test: TestCase)
        : FlatTest list =
        let join name =
            if path = "" then
                name
            else
                path
                + " / "
                + name

        let leaf name state body = [
            {
                Name = join name
                Kind =
                    if
                        pending
                        || state = Pending
                    then
                        SourcePending
                    else
                        Run
                Focused =
                    focused
                    || state = Focused
                Body = body
            }
        ]

        match test with
        | SyncTest(name, body, state) -> leaf name state (async { body () })
        | AsyncTest(name, body, state) -> leaf name state body
        | TestList(name, tests, state) ->
            tests
            |> List.collect (
                flatten
                    (join name)
                    (pending
                     || state = Pending)
                    (focused
                     || state = Focused)
            )

    let private duplicates (names: string list) =
        names
        |> List.countBy id
        |> List.filter (fun (_, count) -> count > 1)
        |> List.map fst

    let private applyWebSharperPending (tests: FlatTest list) =
        let unknown =
            WebSharperPending.tests
            |> List.map fst
            |> List.filter (fun name ->
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
                | Some(_, reason) when test.Kind = Run -> {
                      test with
                          Kind = WebSharperPending reason
                  }
                | _ -> test
            )

        tests, unknown

    let private run (test: FlatTest) =
        async {
            try
                let! child = Async.StartChild(test.Body, testTimeoutMilliseconds)
                do! child
                return Passed
            with e ->
                return Failed e.Message
        }

    [<Inline "process.stdout.write('', () => process.exit($code))">]
    let private exitAfterOutput (code: int) : unit = X<unit>

    let runTests (_args: string[]) (tests: TestCase) : obj =
        async {
            let anyFocused = hasFocus tests
            let all, unknown = applyWebSharperPending (flatten "" false false tests)

            let duplicateNames =
                duplicates (
                    all
                    |> List.map (fun test -> test.Name)
                )

            let duplicatePending =
                duplicates (
                    WebSharperPending.tests
                    |> List.map fst
                )

            let rec runAll (remaining: FlatTest list) results =
                async {
                    match remaining with
                    | [] -> return List.rev results
                    | test :: rest ->
                        let! outcome =
                            match test.Kind with
                            | SourcePending -> async { return NotRun }
                            | Run when
                                anyFocused
                                && not test.Focused
                                ->
                                async { return NotRun }
                            | Run
                            | WebSharperPending _ -> run test

                        return!
                            runAll
                                rest
                                ((test, outcome)
                                 :: results)
                }

            let! results = runAll all []

            let count predicate =
                results
                |> List.filter predicate
                |> List.length

            let passed =
                count (fun (test, outcome) ->
                    test.Kind = Run
                    && outcome = Passed
                )

            let failed =
                results
                |> List.choose (fun (test, outcome) ->
                    match test.Kind, outcome with
                    | Run, Failed message -> Some(test.Name, message)
                    | _ -> None
                )

            let pendingOnWebSharper =
                results
                |> List.choose (fun (test, outcome) ->
                    match test.Kind with
                    | WebSharperPending reason -> Some(test.Name, reason, outcome)
                    | _ -> None
                )

            let passedOnWebSharper =
                pendingOnWebSharper
                |> List.filter (fun (_, _, outcome) -> outcome = Passed)

            for name, reason, _ in pendingOnWebSharper do
                printfn "PENDING %s [pending on WebSharper: %s]" name reason

            for name, message in failed do
                printfn "FAILED %s\n    %s" name message

            for name, _, _ in passedOnWebSharper do
                printfn "ERROR %s passes now. Remove it from WebSharperPending.fs." name

            for name in unknown do
                printfn "ERROR WebSharperPending names a test that does not exist: %s" name

            for name in duplicateNames do
                printfn "ERROR Two or more tests have the name: %s" name

            for name in duplicatePending do
                printfn "ERROR WebSharperPending names this test more than one time: %s" name

            if anyFocused then
                printfn
                    "ERROR The run has focused tests. Remove ftestCase, ftestCaseAsync, and ftestList."

            printfn
                "WebSharper: %d passed, %d failed, %d pending on WebSharper, %d not run, %d total."
                passed
                failed.Length
                pendingOnWebSharper.Length
                (count (fun (_, outcome) -> outcome = NotRun))
                results.Length

            let succeeded =
                failed.IsEmpty
                && passedOnWebSharper.IsEmpty
                && unknown.IsEmpty
                && duplicateNames.IsEmpty
                && duplicatePending.IsEmpty
                && passed > 0
                && not anyFocused

            exitAfterOutput (if succeeded then 0 else 1)
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
    let emitJsStatement (_args: unit) (code: string) : unit = X<unit>
