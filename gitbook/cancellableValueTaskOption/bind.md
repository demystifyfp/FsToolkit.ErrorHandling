# CancellableValueTaskOption.bind

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('input -> CancellableValueTask<'output option>) -> CancellableValueTask<'input option> -> CancellableValueTask<'output option>
```

## Examples

Take the following function for example

```fsharp
type Account =
  { EmailAddress : string
    Name : string }

// string -> CancellableValueTask<Account option>
let lookupAccountByEmail email = cancellableValueTask {
  let john = { EmailAddress = "john@test.com"; Name = "John Johnson" }
  let jeff = { EmailAddress = "jeff@test.com"; Name = "Jeff Jefferson" }
  let jack = { EmailAddress = "jack@test.com"; Name = "Jack Jackson" }
  
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
let taskOpt : CancellableValueTask<Account option> =
    CancellableValueTaskOption.some "john@test.com" // CancellableValueTask<string option>
    |> CancellableValueTaskOption.bind lookupAccountByEmail // CancellableValueTask<Account option>

// cancellableValueTask { Some { EmailAddress = "john@test.com"; Name = "John Johnson" } }
```

### Example 2

```fsharp
let taskOpt : CancellableValueTask<Account option> =
    CancellableValueTaskOption.some "jerry@test.com" // CancellableValueTask<string option>
    |> CancellableValueTaskOption.bind lookupAccountByEmail // CancellableValueTask<Account option>

// cancellableValueTask { None }
```

### Example 3

```fsharp
let taskOpt : CancellableValueTask<Account option> =
    CancellableValueTask.singleton None // CancellableValueTask<string option>
    |> CancellableValueTaskOption.bind lookupAccountByEmail // CancellableValueTask<Account option>

// cancellableValueTask { None }
```
