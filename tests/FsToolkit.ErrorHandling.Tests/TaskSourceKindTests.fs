module TaskSourceKindTests

open System
open System.Threading
open System.Threading.Tasks
open Expecto
open FsToolkit.ErrorHandling

// Each source kind reaches Bind, BindReturn, ReturnFrom, and MergeSources in a different form.

let private pendingInt (value: int) : Task<int> =
    task {
        do! Task.Yield()
        return value
    }

let private pendingOk () : Task<Result<int, string>> =
    task {
        do! Task.Yield()
        return Ok 1
    }

let private pendingValidation () : Task<Validation<int, string>> =
    task {
        do! Task.Yield()
        return Ok 1
    }

let private pendingSome () : Task<int option> =
    task {
        do! Task.Yield()
        return Some 1
    }

let private pendingValueSome () : Task<int voption> =
    task {
        do! Task.Yield()
        return ValueSome 1
    }

let private pendingFault () : Task<int> =
    task {
        do! Task.Yield()
        return failwith "boom"
    }

let rec private innermost (e: exn) =
    match e with
    | :? AggregateException as a ->
        match a.InnerException with
        | null -> e
        | inner -> innermost inner
    | e -> e

let allTests =
    testList "Task builder source kinds" [
        testList "taskResult" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult 1
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = pendingInt 1
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = pendingOk ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = pendingOk ()
                            return a + b
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Error "a": Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Error "a": Result<int, string>) ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual = taskResult { return! Task.FromResult(Ok 1: Result<int, string>) }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = (Ok 1: Result<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual = taskResult { return! (Ok 1: Result<int, string>) }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = taskResult { return! Task.FromResult 1 }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskResult {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = taskResult { return! pendingInt 1 }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskResult {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskResult {
                            let! a = pendingFault ()
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        taskResult {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
        ]
        testList "backgroundTaskResult" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult 1
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = pendingInt 1
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = pendingOk ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Result<int, string>) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            and! b = pendingOk ()
                            return a + b
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Error "a": Result<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Error "a": Result<int, string>) ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult(Ok 1: Result<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult { return! Task.FromResult(Ok 1: Result<int, string>) }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = (Ok 1: Result<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual = backgroundTaskResult { return! (Ok 1: Result<int, string>) }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = backgroundTaskResult { return! Task.FromResult 1 }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        backgroundTaskResult {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Result<int, string>) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = backgroundTaskResult { return! pendingInt 1 }

                    Expect.equal actual (Ok 1: Result<int, string>) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        backgroundTaskResult {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        backgroundTaskResult {
                            let! a = pendingFault ()
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        backgroundTaskResult {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (Ok 2: Result<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
        ]
        testList "taskValidation" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            and! b = Task.FromResult(Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            and! b = Task.FromResult(Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult 1
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = pendingInt 1
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = pendingValidation ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Ok 3: Validation<int, string>) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            and! b = pendingValidation ()
                            return a + b
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.error "a": Validation<int, string>)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Error [ "a" ]: Validation<int, string>) ""
                }
            testCaseTask "and! collects the errors of two Value sources"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.error "a": Validation<int, string>)
                            and! b = (Validation.error "b": Validation<int, string>)
                            return a + b
                        }

                    Expect.equal
                        actual
                        (Error [
                            "a"
                            "b"
                        ]
                        : Validation<int, string>)
                        ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult(Validation.ok 1: Validation<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            return! Task.FromResult(Validation.ok 1: Validation<int, string>)
                        }

                    Expect.equal actual (Ok 1: Validation<int, string>) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = (Validation.ok 1: Validation<int, string>)
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation { return! (Validation.ok 1: Validation<int, string>) }

                    Expect.equal actual (Ok 1: Validation<int, string>) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = taskValidation { return! Task.FromResult 1 }

                    Expect.equal actual (Ok 1: Validation<int, string>) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValidation {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (Ok 2: Validation<int, string>) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = taskValidation { return! pendingInt 1 }

                    Expect.equal actual (Ok 1: Validation<int, string>) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskValidation {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskValidation {
                            let! a = pendingFault ()
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        taskValidation {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (Validation.ok 2: Validation<int, string>)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
        ]
        testList "taskOption" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            and! b = Task.FromResult(Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            and! b = Task.FromResult(Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult 1
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = pendingInt 1
                            and! b = (Some 2)
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = pendingSome ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (Some 3) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            and! b = pendingSome ()
                            return a + b
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (None: int option)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (None: int option) ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult(Some 1)
                            return a + 1
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual = taskOption { return! Task.FromResult(Some 1) }

                    Expect.equal actual (Some 1) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = (Some 1)
                            return a + 1
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual = taskOption { return! (Some 1) }

                    Expect.equal actual (Some 1) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = taskOption { return! Task.FromResult 1 }

                    Expect.equal actual (Some 1) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskOption {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (Some 2) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = taskOption { return! pendingInt 1 }

                    Expect.equal actual (Some 1) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskOption {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (Some 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskOption {
                            let! a = pendingFault ()
                            and! b = (Some 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        taskOption {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (Some 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
        ]
        testList "taskValueOption" [
            testCaseTask "and! takes a Task source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            and! b = Task.FromResult(ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Task source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Task source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Value source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            and! b = Task.FromResult(ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Value source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Value source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Plain source and a Task source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult(ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Plain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult 1
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! takes a Plain source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult 1
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! waits for a PendingPlain source and a Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = pendingInt 1
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! waits for a Task source and a PendingPlain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            and! b = pendingInt 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! waits for a PendingShaped source and a Plain source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = pendingValueSome ()
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueSome 3) ""
                }
            testCaseTask "and! waits for a Value source and a PendingShaped source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            and! b = pendingValueSome ()
                            return a + b
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "and! stops at a stopped Value source"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueNone: int voption)
                            and! b = Task.FromResult 2
                            return a + b
                        }

                    Expect.equal actual (ValueNone: int voption) ""
                }
            testCaseTask "let! of a Task source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult(ValueSome 1)
                            return a + 1
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "return! of a Task source"
            <| fun () ->
                task {
                    let! actual = taskValueOption { return! Task.FromResult(ValueSome 1) }

                    Expect.equal actual (ValueSome 1) ""
                }
            testCaseTask "let! of a Value source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = (ValueSome 1)
                            return a + 1
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "return! of a Value source"
            <| fun () ->
                task {
                    let! actual = taskValueOption { return! (ValueSome 1) }

                    Expect.equal actual (ValueSome 1) ""
                }
            testCaseTask "let! of a Plain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = Task.FromResult 1
                            return a + 1
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "return! of a Plain source"
            <| fun () ->
                task {
                    let! actual = taskValueOption { return! Task.FromResult 1 }

                    Expect.equal actual (ValueSome 1) ""
                }
            testCaseTask "let! of a PendingPlain source then return"
            <| fun () ->
                task {
                    let! actual =
                        taskValueOption {
                            let! a = pendingInt 1
                            return a + 1
                        }

                    Expect.equal actual (ValueSome 2) ""
                }
            testCaseTask "return! of a PendingPlain source"
            <| fun () ->
                task {
                    let! actual = taskValueOption { return! pendingInt 1 }

                    Expect.equal actual (ValueSome 1) ""
                }
            testCaseTask "and! with a faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskValueOption {
                            let! a = Task.FromException<int>(exn "boom")
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a pending faulted plain source faults"
            <| fun () ->
                task {
                    let computation () =
                        taskValueOption {
                            let! a = pendingFault ()
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with e ->
                                return (innermost e).Message
                        }

                    Expect.equal outcome "boom" ""
                }
            testCaseTask "and! with a canceled plain source is canceled"
            <| fun () ->
                task {
                    let computation () =
                        taskValueOption {
                            let! a = Task.FromCanceled<int>(CancellationToken(true))
                            and! b = (ValueSome 2)
                            return a + b
                        }

                    let! outcome =
                        task {
                            try
                                let! _ = computation ()
                                return "completed"
                            with
                            | :? OperationCanceledException -> return "canceled"
                            | e -> return (innermost e).Message
                        }

                    Expect.equal outcome "canceled" ""
                }
        ]
    ]
