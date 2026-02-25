## CancellableTaskValidation.parallelZip

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableTaskValidation<'left, 'error>
  -> CancellableTaskValidation<'right, 'error>
  -> CancellableTaskValidation<'left * 'right, 'error>
```

Takes two `CancellableTaskValidation` values, starts them **concurrently**, and returns a tuple of the results once both complete. Errors from both computations are **accumulated** — if both return `Error`, all errors are collected into a single list.

Unlike [`zip`](../cancellableTaskValidation/zip.md), both computations are started before waiting for either to complete, which can improve throughput when the two operations are independent.

## Examples

### Example 1

Fetching two independent resources in parallel while collecting any errors:

```fsharp
let validateUserAndRole (userId: UserId) : CancellableTaskValidation<User * Role, string> =
    CancellableTaskValidation.parallelZip
        (fetchAndValidateUser userId)
        (fetchAndValidateRole userId)
```

### Example 2

Using `parallelZip` inside a computation expression:

```fsharp
let buildProfile (userId: UserId) : CancellableTaskValidation<UserProfile, string> =
    cancellableTaskValidation {
        let! user, preferences =
            CancellableTaskValidation.parallelZip
                (fetchUser userId)
                (fetchPreferences userId)
        return UserProfile.create user preferences
    }
```

### Example 3

Concurrently validating independent fields and accumulating all errors:

```fsharp
let validateSignupForm (form: SignupForm) : CancellableTaskValidation<ValidEmail * ValidUsername, string> =
    CancellableTaskValidation.parallelZip
        (validateEmail form.Email)       // e.g. Error ["Invalid email format"]
        (validateUsername form.Username) // e.g. Error ["Username too short"]
    // Both run concurrently; if both fail: Error ["Invalid email format"; "Username too short"]
```
