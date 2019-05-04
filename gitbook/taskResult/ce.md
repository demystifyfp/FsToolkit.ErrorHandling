## TaskResult Computation Expression

Namespace: `FsToolkit.ErrorHandling`


## Examples

### Example 1

The initial [motivating example](../../) is perhaps more realistic if some functions are asynchronous. Given the following functions:

```fsharp
tryGetUser : string -> Task<User option>
isPwdValid : string -> User -> bool
authorize : User -> Task<Result<unit, AuthError>>
createAuthToken : User -> Result<AuthToken, TokenError>
```

A login flow can be implemented as below using the `taskResult` CE and a few [helpers](others.md):

```fsharp
type LoginError = 
	| InvalidUser
	| InvalidPwd
	| Unauthorized of AuthError
	| TokenErr of TokenError

let login (username: string) (password: string) : Task<Result<AuthToken, LoginError>> =
  taskResult {
    let! user = username |> tryGetUser |> TaskResult.requireSome InvalidUser
    do! user |> isPwdValid password |> Result.requireTrue InvalidPwd
    do! user |> authorize |> TaskResult.mapError Unauthorized
    return! user |> createAuthToken |> Result.mapError TokenErr
  }
```
