# TaskOption.bind

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('input -> Task<'output option>) -> Task<'input option> -> Task<'output option>
```

## Examples

Take the following function for example

```fsharp
type Account =
  { EmailAddress : string
    Name : string }

// string -> Task<Account option>
let lookupAccountByEmail email = task {
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
let taskOpt : Task<Account option> =
    TaskOption.some "john@test.com" // Task<string option>
    |> TaskOption.bind lookupAccountByEmail // Task<Account option>

// task { Some { EmailAddress = "john@test.com"; Name = "John Johnson" } }
```

### Example 2

```fsharp
let taskOpt : Task<Account option> =
    TaskOption.some "jerry@test.com" // Task<string option>
    |> TaskOption.bind lookupAccountByEmail // Task<Account option>

// task { None }
```

### Example 3

```fsharp
let taskOpt : Task<Account option> =
    Task.singleton None // Task<string option>
    |> TaskOption.bind lookupAccountByEmail // Task<Account option>

// task { None }
```
