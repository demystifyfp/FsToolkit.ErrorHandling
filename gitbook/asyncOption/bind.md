# AsyncOption.bind

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('TInput -> Async<'TOutput option>) -> Async<'TInput option> -> Async<'TOutput option>
```

## Examples

Take the following function for example

```fsharp
type Account =
  { EmailAddress : string
    Name : string }

// string -> Async<Account option>
let lookupAccountByEmail email = async {
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
let asyncOpt : Async<Account option> =
    AsyncOption.some "john@test.com" // Async<string option>
    |> AsyncOption.bind lookupAccountByEmail // Async<Account option>

// async { Some { EmailAddress = "john@test.com"; Name = "John Johnson" } }
```

### Example 2

```fsharp
let asyncOpt : Async<Account option> =
    AsyncOption.some "jerry@test.com" // Async<string option>
    |> AsyncOption.bind lookupAccountByEmail // Async<Account option>

// async { None }
```

### Example 3

```fsharp
let asyncOpt : Async<Account option> =
    Async.singleton None // Async<string option>
    |> AsyncOption.bind lookupAccountByEmail // Async<Account option>

// async { None }
```
