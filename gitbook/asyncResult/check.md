# Result.check

Namespace: `FsToolkit.ErrorHandling`

The intent of check is to allow an Ok result value to be validated. 

`check` takes a validation function of the form `'ok -> Async<Result<unit, 'error>>` and a result of the form `Async<Result<'ok, 'error>>`. 

If the async-wrapped result is `Ok x` then the validation function is applied, and if the validation function returns an error, this new async-wrapped error is returned. Otherwise, the original async-wrapped `Ok x` result is returned. If the original async-wrapped result is an Error, the original async-wrapped result is returned.

## Function Signature
```fsharp
('ok -> Async<Result<unit,'error>>) -> Async<Result<'ok,'error>> -> Async<Result<'ok,'error>>
```

## Examples

### Example 1

Given the following function that returns true for the id `123`
```fsharp
checkEnabled : int -> Async<bool>
```

```fsharp
AsyncResult.ok (
    {|
        PolicyId = 123
        AccessPolicyName = "UserCanAccessResource"
    |}

)
|> AsyncResult.check (fun policy ->
    asyncResult {
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
// AsyncResult.Ok {| AccessPolicyName = "UserCanAccessResource"; IsEnabled = true; |}
```

### Example 2

```fsharp
AsyncResult.ok (
    {|
        PolicyId = 456
        AccessPolicyName = "UserCanAccessResource"
    |}

)
|> AsyncResult.check (fun policy ->
    asyncResult {
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

// AsyncResult.Error "The policy UserCanAccessResource cannot be used because its disabled."
```
