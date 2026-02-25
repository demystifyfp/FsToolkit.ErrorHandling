## CancellableTaskResult.zip

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableTaskResult<'left, 'Error>
  -> CancellableTaskResult<'right, 'Error>
  -> CancellableTaskResult<'left * 'right, 'Error>
```

Takes two `CancellableTaskResult` values, runs them **serially** (left then right), and returns a tuple of the results. If either computation returns an `Error`, the error is returned and the remaining computation (if any) is not executed.

## Examples

### Example 1

Fetching two pieces of data serially and combining them:

```fsharp
let getUserAndPermissions (userId: UserId) : CancellableTaskResult<User * Permission list, string> =
    CancellableTaskResult.zip
        (fetchUser userId)
        (fetchPermissions userId)
```

### Example 2

Using `zip` as part of a larger pipeline:

```fsharp
let loadDashboard (userId: UserId) : CancellableTaskResult<Dashboard, string> =
    cancellableTaskResult {
        let! user, settings =
            CancellableTaskResult.zip
                (fetchUser userId)
                (fetchUserSettings userId)
        return buildDashboard user settings
    }
```

### Example 3

Combining two independent lookups, short-circuiting on the first error:

```fsharp
let validateCredentials (username: string) (password: string) : CancellableTaskResult<User * Role, AuthError> =
    CancellableTaskResult.zip
        (lookupUser username)          // returns Error if user not found
        (lookupRole username)          // skipped if lookupUser fails
```
