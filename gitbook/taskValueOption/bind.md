# TaskValueOption.bind

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('input -> Task<'output voption>) -> Task<'input voption> -> Task<'output voption>
```

## Examples

Take the following function for example

```fsharp
type Account =
  { EmailAddress : string
    Name : string }

// string -> Task<Account voption>
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
  
  return 
    accounts
    |> Map.tryFind email
    |> Option.toValueOption
}
```

### Example 1

```fsharp
let taskOpt : Task<Account voption> =
    TaskValueOption.valueSome "john@test.com" // Task<string voption>
    |> TaskValueOption.bind lookupAccountByEmail // Task<Account voption>

// task { ValueSome { EmailAddress = "john@test.com"; Name = "John Johnson" } }
```

### Example 2

```fsharp
let taskVOpt : Task<Account voption> =
    TaskValueOption.some "jerry@test.com" // Task<string voption>
    |> TaskValueOption.bind lookupAccountByEmail // Task<Account voption>

// task { ValueNone }
```

### Example 3

```fsharp
let taskVOpt : Task<Account voption> =
    Task.singleton ValueNone // Task<string voption>
    |> TaskValueOption.bind lookupAccountByEmail // Task<Account voption>

// task { ValueNone }
```
