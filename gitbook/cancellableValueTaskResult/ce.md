## CancellableValueTaskResult Computation Expression

Namespace: `FsToolkit.ErrorHandling`


## CancellableValueTaskResult Examples

### Example 1

The initial [motivating example](../README.md) is perhaps more realistic if some functions are asynchronous. Given the following functions:

```fsharp
tryGetUser : string -> CancellableValueTask<User option>
isPwdValid : string -> User -> bool
authorize : User -> CancellableValueTask<Result<unit, AuthError>>
createAuthToken : User -> Result<AuthToken, TokenError>
```

A login flow can be implemented as below using the `cancellableValueTaskResult` CE and a few [helpers](others.md):

```fsharp
type LoginError = 
	| InvalidUser
	| InvalidPwd
	| Unauthorized of AuthError
	| TokenErr of TokenError

let login (username: string) (password: string) : CancellableValueTask<Result<AuthToken, LoginError>> =
  cancellableValueTaskResult {
    let! user = username |> tryGetUser |> CancellableValueTaskResult.requireSome InvalidUser
    do! user |> isPwdValid password |> Result.requireTrue InvalidPwd
    do! user |> authorize |> CancellableValueTaskResult.mapError Unauthorized
    return! user |> createAuthToken |> Result.mapError TokenErr
  }
```

## BackgroundCancellableValueTaskResult

You also have an option to use the `backgroundCancellableValueTaskResult` computation expression. It will still use the CancellableValueTaskResult type, but it will escape to a background thread where necessary.
