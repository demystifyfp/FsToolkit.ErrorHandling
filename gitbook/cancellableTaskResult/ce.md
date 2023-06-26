## CancellableTaskResult Computation Expression

Namespace: `FsToolkit.ErrorHandling`


## CancellableTaskResult Examples

### Example 1

The initial [motivating example](../README.md) is perhaps more realistic if some functions are asynchronous. Given the following functions:

```fsharp
tryGetUser : string -> CancellableTask<User option>
isPwdValid : string -> User -> bool
authorize : User -> CancellableTask<Result<unit, AuthError>>
createAuthToken : User -> Result<AuthToken, TokenError>
```

A login flow can be implemented as below using the `cancellableTaskResult` CE and a few [helpers](others.md):

```fsharp
type LoginError = 
	| InvalidUser
	| InvalidPwd
	| Unauthorized of AuthError
	| TokenErr of TokenError

let login (username: string) (password: string) : Task<Result<AuthToken, LoginError>> =
  cancellableTaskResult {
    let! user = username |> tryGetUser |> CancellableTaskResult.requireSome InvalidUser
    do! user |> isPwdValid password |> Result.requireTrue InvalidPwd
    do! user |> authorize |> CancellableTaskResult.mapError Unauthorized
    return! user |> createAuthToken |> Result.mapError TokenErr
  }
```

## BackgroundCancellableTaskResults

You also have an option to use the 'backgroundCancellableTaskResult' computation expression. It will still use the CancellableTaskResult type, but it will escape to a background thread where necessary.
