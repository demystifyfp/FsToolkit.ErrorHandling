# CancellableTaskOption.bind

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('input -> CancellableTask<'output option>) -> CancellableTask<'input option> -> CancellableTask<'output option>
```

## Examples

Take the following function for example

```fsharp
type Account =
  { EmailAddress : string
    Name : string }

// string -> CancellableTask<Account option>
let lookupAccountByEmail email = cancellableTask {
  let john = { EmailAddress = "john@test.com"; Name = "John Johnson" }
  let jeff = { EmailAddress = "jeff@test.com"; Name = "Jeff Jefferson" }
  let jack = { EmailAddress = "jack@test.com"; Name = "Jack Jackson" }
  
  // Just a map lookup, but imagine we look up an account in our database
  let accounts = Map.ofList [
      ("john@test.com", john)
      ("jeff@test.com", jeff)
      ("jack@test.com", jack)
  ]
  
  return Map.tryFind email accounts
}
```

### Example 1

```fsharp
let taskOpt : CancellableTask<Account option> =
    CancellableTaskOption.some "john@test.com" // CancellableTask<string option>
    |> CancellableTaskOption.bind lookupAccountByEmail // CancellableTask<Account option>

// cancellableTask { Some { EmailAddress = "john@test.com"; Name = "John Johnson" } }
```

### Example 2

```fsharp
let taskOpt : CancellableTask<Account option> =
    CancellableTaskOption.some "jerry@test.com" // CancellableTask<string option>
    |> CancellableTaskOption.bind lookupAccountByEmail // CancellableTask<Account option>

// cancellableTask { None }
```

### Example 3

```fsharp
let taskOpt : CancellableTask<Account option> =
    CancellableTask.singleton None // CancellableTask<string option>
    |> CancellableTaskOption.bind lookupAccountByEmail // CancellableTask<Account option>

// cancellableTask { None }
```
