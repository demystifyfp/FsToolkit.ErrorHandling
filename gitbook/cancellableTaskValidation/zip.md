## CancellableTaskValidation.zip

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableTaskValidation<'left, 'error>
  -> CancellableTaskValidation<'right, 'error>
  -> CancellableTaskValidation<'left * 'right, 'error>
```

Takes two `CancellableTaskValidation` values, runs them **serially** (left then right), and returns a tuple of the results. Errors from both computations are **accumulated** — if both return `Error`, all errors are collected into a single list.

Unlike [`parallelZip`](../cancellableTaskValidation/parallelZip.md), computations run one after the other.

## Examples

### Example 1

Combining two validation results into a tuple:

```fsharp
let validateUserAndRole (userId: UserId) : CancellableTaskValidation<User * Role, string> =
    CancellableTaskValidation.zip
        (fetchAndValidateUser userId)
        (fetchAndValidateRole userId)
```

### Example 2

Using `zip` inside a larger computation expression:

```fsharp
let buildProfile (userId: UserId) : CancellableTaskValidation<UserProfile, string> =
    cancellableTaskValidation {
        let! user, preferences =
            CancellableTaskValidation.zip
                (fetchUser userId)
                (fetchPreferences userId)
        return UserProfile.create user preferences
    }
```

### Example 3

Accumulating errors from two independent validations:

```fsharp
let validateForm (form: SignupForm) : CancellableTaskValidation<ValidEmail * ValidUsername, string> =
    CancellableTaskValidation.zip
        (validateEmail form.Email)     // e.g. Error ["Invalid email format"]
        (validateUsername form.Username) // e.g. Error ["Username too short"]
    // If both fail, result is Error ["Invalid email format"; "Username too short"]
```
