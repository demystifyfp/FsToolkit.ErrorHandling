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
        | WebSharperPending of WebSharperPending.Cause

    type private FlatTest = {
        Name: string
        Kind: Kind
        Focused: bool
        Body: Async<unit>
    }

    type private Outcome =
        | Passed
        | Failed of string
        | TimedOut
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
        let problems = [
            for name, _ in WebSharperPending.tests do
                match
                    tests
                    |> List.tryFind (fun test -> test.Name = name)
                with
                | None -> $"WebSharperPending names a test that does not exist: {name}"
                | Some test when test.Kind = SourcePending ->
                    $"WebSharperPending names a test that the source marks pending: {name}"
                | Some _ -> ()

            for name in
                duplicates (
                    WebSharperPending.tests
                    |> List.map fst
                ) do
                $"WebSharperPending names this test more than one time: {name}"
        ]

        let tests =
            tests
            |> List.map (fun test ->
                match
                    WebSharperPending.tests
                    |> List.tryFind (fun (name, _) -> name = test.Name)
                with
                | Some(_, cause) when test.Kind = Run -> {
                      test with
                          Kind = WebSharperPending cause
                  }
                | _ -> test
            )

        tests, problems

    let private run (test: FlatTest) =
        async {
            let body =
                async {
                    try
                        do! test.Body
                        return None
                    with e ->
                        return Some e.Message
                }

            try
                let! child = Async.StartChild(body, testTimeoutMilliseconds)

                match! child with
                | None -> return Passed
                | Some message -> return Failed message
            with :? System.TimeoutException ->
                return TimedOut
        }

    [<Inline "process.stdout.write('', () => process.exit($code))">]
    let private exitAfterOutput (code: int) : unit = X<unit>

    let runTests (_args: string[]) (tests: TestCase) : obj =
        async {
            let anyFocused = hasFocus tests
            let all, pendingProblems = applyWebSharperPending (flatten "" false false tests)

            // A test that times out can still run, so the run stops at the first timeout.
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

                        let results =
                            (test, outcome)
                            :: results

                        if outcome = TimedOut then
                            return List.rev results
                        else
                            return! runAll rest results
                }

            let! results = runAll all []

            let problems = [
                yield! pendingProblems

                for name in
                    duplicates (
                        all
                        |> List.map (fun test -> test.Name)
                    ) do
                    $"Two or more tests have the name: {name}"

                if anyFocused then
                    "The run has focused tests. Remove ftestCase, ftestCaseAsync, and ftestList."

                for test, outcome in results do
                    match test.Kind, outcome with
                    | _, TimedOut ->
                        $"{test.Name} did not finish in {testTimeoutMilliseconds} ms. The run stopped."
                    | WebSharperPending _, Passed ->
                        $"{test.Name} passes now. Remove it from WebSharperPending.fs."
                    | WebSharperPending cause, Failed message when
                        not (WebSharperPending.Cause.explains cause message)
                        ->
                        $"{test.Name} fails for another cause than the one in WebSharperPending.fs: {message}"
                    | _ -> ()
            ]

            let failed = [
                for test, outcome in results do
                    match test.Kind, outcome with
                    | Run, Failed message -> test.Name, message
                    | Run, TimedOut -> test.Name, $"did not finish in {testTimeoutMilliseconds} ms"
                    | _ -> ()
            ]

            let passed =
                results
                |> List.filter (fun (test, outcome) ->
                    test.Kind = Run
                    && outcome = Passed
                )
                |> List.length

            let pendingOnWebSharper = [
                for test, _ in results do
                    match test.Kind with
                    | WebSharperPending cause -> test.Name, WebSharperPending.Cause.reason cause
                    | _ -> ()
            ]

            for name, reason in pendingOnWebSharper do
                printfn "PENDING %s [pending on WebSharper: %s]" name reason

            for name, message in failed do
                printfn "FAILED %s\n    %s" name message

            for problem in problems do
                printfn "ERROR %s" problem

            printfn
                "WebSharper: %d passed, %d failed, %d pending on WebSharper, %d not run, %d of %d tests reached."
                passed
                failed.Length
                pendingOnWebSharper.Length
                (results
                 |> List.filter (fun (_, outcome) -> outcome = NotRun)
                 |> List.length)
                results.Length
                all.Length

            let succeeded =
                failed.IsEmpty
                && problems.IsEmpty
                && passed > 0

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
