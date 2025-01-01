# Result.check

Namespace: `FsToolkit.ErrorHandling`

The intent of check is to allow an Ok result value to be validated. 

`check` takes a validation function of the form `'ok -> Task<Result<unit, 'error>>` and a result of the form `Task<Result<'ok, 'error>>`. 

If the task-wrapped result is `Ok x` then the validation function is applied, and if the validation function returns an error, this new task-wrapped error is returned. Otherwise, the original task-wrapped `Ok x` result is returned. If the original task-wrapped result is an Error, the original task-wrapped result is returned.

## Function Signature
```fsharp
('ok -> Task<Result<unit,'error>>) -> Task<Result<'ok,'error>> -> Task<Result<'ok,'error>>
```

## Examples

Given the following function that returns true for the id `123`
```fsharp
checkEnabled : int -> Task<bool>
```

### Example 1

```fsharp
TaskResult.ok (
    {|
        PolicyId = 123
        AccessPolicyName = "UserCanAccessResource"
    |}

)
|> TaskResult.check (fun policy ->
    taskResult {
        let! isEnabled = checkEnabled policy.PolicyId

        return
            if not isEnabled then
                Error(
                    $"The policy {policy.AccessPolicyName} cannot be used because its disabled."
                )
            else
                Ok()

    }
)
// TaskResult.Ok {| AccessPolicyName = "UserCanAccessResource"; IsEnabled = true; |}
```

### Example 2

```fsharp
TaskResult.ok (
    {|
        PolicyId = 456
        AccessPolicyName = "UserCanAccessResource"
    |}

)
|> TaskResult.check (fun policy ->
    taskResult {
        let! isEnabled = checkEnabled policy.PolicyId

        return
            if not isEnabled then
                Error(
                    $"The policy {policy.AccessPolicyName} cannot be used because its disabled."
                )
            else
                Ok()

    }
)

// TaskResult.Error "The policy UserCanAccessResource cannot be used because its disabled."
```
