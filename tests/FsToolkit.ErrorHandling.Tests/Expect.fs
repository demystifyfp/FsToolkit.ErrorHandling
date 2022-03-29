namespace TestHelpers

#if FABLE_COMPILER
module Tests =
    let failtestf = failwithf
#endif

module Expect =
#if FABLE_COMPILER
    open Fable.Mocha
#else
    open Expecto
    open System.Threading.Tasks

#endif

#if NETSTANDARD2_0 || NET5_0 || NET5
    //     failwith "LOL"
    open FSharp.Control.Tasks
#endif


#if FABLE_COMPILER
    let isOk x message =
        match x with
        | Ok _ -> ()
        | Result.Error x -> Tests.failtestf "%s. Expected Ok, was Error(%A)." message x
#endif

    let hasErrorValue v x =
        match x with
        | Ok x -> Tests.failtestf "Expected Error, was Ok(%A)." x
        | Error x when x = v -> ()
        | Error x -> Tests.failtestf "Expected Error(%A), was Error(%A)." v x

    let hasOkValue v x =
        match x with
        | Ok x when x = v -> ()
        | Ok x -> Tests.failtestf "Expected Ok(%A), was Ok(%A)." v x
        | Error x -> Tests.failtestf "Expected Ok, was Error(%A)." x

    let hasSomeValue v x =
        match x with
        | Some x when x = v -> ()
        | Some x -> Tests.failtestf "Expected Some(%A), was Some(%A)." v x
        | None -> Tests.failtestf "Expected Some, was None."

    let hasNoneValue x =
        match x with
        | None -> ()
        | Some _ -> Tests.failtestf "Expected None, was Some."

    let hasAsyncValue v asyncX =
        async {
            let! x = asyncX

            if v = x then
                ()
            else
                Tests.failtestf "Expected %A, was %A." v x
        }

    let hasAsyncOkValue v asyncX =
        async {
            let! x = asyncX
            hasOkValue v x
        }

    let hasAsyncErrorValue v asyncX =
        async {
            let! x = asyncX
            hasErrorValue v x
        }

    let hasAsyncSomeValue v asyncX =
        async {
            let! x = asyncX
            hasSomeValue v x
        }

    let hasAsyncNoneValue asyncX =
        async {
            let! x = asyncX
            hasNoneValue x
        }

#if !FABLE_COMPILER

    let hasTaskValue v taskX =
        let x =
            taskX |> Async.AwaitTask |> Async.RunSynchronously

        if v = x then
            ()
        else
            Tests.failtestf "Expected %A, was %A." v x

    let hasTaskOkValue v (taskX: Task<_>) =
        task {
            let! x = taskX
            hasOkValue v x
        }

    let hasTaskOkValueSync v taskX =
        let x =
            taskX |> Async.AwaitTask |> Async.RunSynchronously

        hasOkValue v x

    let hasTaskNoneValue taskX =
        let x =
            taskX |> Async.AwaitTask |> Async.RunSynchronously

        hasNoneValue x

    let hasTaskErrorValue v (taskX: Task<_>) =
        task {
            let! x = taskX
            hasErrorValue v x
        }

    let hasTaskErrorValueSync v taskX =
        let x =
            taskX |> Async.AwaitTask |> Async.RunSynchronously

        hasErrorValue v x

    let hasTaskSomeValue v taskX =
        let x =
            taskX |> Async.AwaitTask |> Async.RunSynchronously

        hasSomeValue v x

#endif

    let same expected actual =
        Expect.equal actual expected "expected and actual should be same"
