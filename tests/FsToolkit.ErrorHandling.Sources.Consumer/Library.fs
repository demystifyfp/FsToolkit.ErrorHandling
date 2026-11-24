namespace Contoso.Consumer

open System
open System.Threading.Tasks
open Contoso.Internal.ErrorHandling
#if TEST_RUNTIME_ASYNC
open Contoso.Internal.ErrorHandling.Polyfill.RuntimeTask
#endif

module Library =

#if TEST_RUNTIME_ASYNC
    let private taskResult = RuntimeTaskResults.taskResult
    let private backgroundTaskResult = RuntimeTaskResults.backgroundTaskResult
    let private taskOption = RuntimeTaskOptions.taskOption
    let private backgroundTaskOption = RuntimeTaskOptions.backgroundTaskOption
    let private taskValueOption = RuntimeTaskValueOptions.taskValueOption

    let private backgroundTaskValueOption =
        RuntimeTaskValueOptions.backgroundTaskValueOption

    let private taskResultOption = RuntimeTaskResultOptions.taskResultOption
    let private taskValidation = RuntimeTaskValidations.taskValidation

    let private backgroundTaskValidation =
        RuntimeTaskValidations.backgroundTaskValidation
#endif

    let private parseInt (input: string) : Result<int, string> =
        match Int32.TryParse input with
        | true, value -> Ok value
        | false, _ -> Error $"'{input}' is not a number"

    let parsePositive (input: string) : Result<int, string> =
        result {
            let! value = parseInt input
            do! Req.isTrue "The number must be positive." (value > 0)
            return value
        }

    let parsePositiveTask (input: Task<string>) : Task<Result<int, string>> =
        taskResult {
            let! text = input
            return! parsePositive text
        }

    let backgroundResult (input: Task<Result<int, string>>) = backgroundTaskResult { return! input }

    let options (input: Task<int option>) =
        taskOption { return! input }, backgroundTaskOption { return! input }

    let valueOptions (input: Task<int voption>) =
        taskValueOption { return! input }, backgroundTaskValueOption { return! input }

    let resultOption (input: Task<Result<int option, string>>) = taskResultOption { return! input }

    let validations (input: Task<Result<int, string list>>) =
        taskValidation { return! input }, backgroundTaskValidation { return! input }

    let validatePerson (name: string) (age: string) : Result<string * int, string list> =
        validation {
            let! validName =
                if String.IsNullOrWhiteSpace name then
                    Error "The name is empty."
                else
                    Ok name

            and! validAge = parsePositive age
            return validName, validAge
        }

    let parsePositiveAsync (input: string) : Async<Result<int, string>> =
        asyncResult {
            let! text = FsToolkit.ErrorHandling.Async.singleton input
            let! value = parsePositive text
            return value
        }

    let double (value: Result<int, string>) : Result<int, string> =
        Contoso.Internal.ErrorHandling.Result.eitherMap (fun x -> x * 2) id value
