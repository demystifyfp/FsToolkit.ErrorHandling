namespace Contoso.Consumer

open System
open System.Threading.Tasks
open Contoso.Internal.ErrorHandling

module Library =

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
        Contoso.Internal.ErrorHandling.Result.map (fun x -> x * 2) value
