module Expecto

open Expecto
open System
open System.Threading.Tasks

#if !FABLE_COMPILER
let testCaseTask name test =
    testCaseAsync
        name
        (async {
            return!
                test ()
                |> Async.AwaitTask
        })

let ptestCaseTask name test =
    ptestCaseAsync
        name
        (async {
            return!
                test ()
                |> Async.AwaitTask
        })

let ftestCaseTask name test =
    ftestCaseAsync
        name
        (async {
            return!
                test ()
                |> Async.AwaitTask
        })

module Expect =
    open Expecto

    /// Expects the passed function to throw `'texn`.
    [<RequiresExplicitTypeArguments>]
    let throwsTAsync<'a, 'texn when 'texn :> exn> (f: Async<'a>) message =
        async {
            let! thrown =
                async {
                    try
                        let! r = f
                        return Choice2Of2 r
                    with e ->
                        return Choice1Of2 e
                }

            match thrown with
            | Choice1Of2 e when not (typeof<'texn>.IsAssignableFrom(e.GetType())) ->
                Tests.failtestf
                    "%s. Expected f to throw an exn of type %s, but one of type %s was thrown."
                    message
                    (typeof<'texn>.FullName)
                    (e.GetType().FullName)
            | Choice1Of2 _ -> ()
            | Choice2Of2 result ->
                Tests.failtestf "%s. Expected f to throw. returned %A" message result
        }

type Expect =

    static member CancellationRequested(operation: Async<'a>) =
        Expect.throwsTAsync<'a, OperationCanceledException> (operation) "Should have been cancelled"

    static member CancellationRequested(operation: Task<_>) =
        Expect.CancellationRequested(Async.AwaitTask operation)
        |> Async.StartImmediateAsTask

#endif
