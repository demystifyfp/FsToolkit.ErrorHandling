# ValueTaskValueOption.bind

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('input -> ValueTask<'output voption>) -> ValueTask<'input voption> -> ValueTask<'output voption>
```

## Examples

Take the following function for example

```fsharp
type Account =
  { EmailAddress : string
    Name : string }

// string -> ValueTask<Account voption>
let lookupAccountByEmail email = valueTask {
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
let result : ValueTask<Account voption> =
    ValueTaskValueOption.valueSome "john@test.com" // ValueTask<string voption>
    |> ValueTaskValueOption.bind lookupAccountByEmail // ValueTask<Account voption>

// valueTask { ValueSome { EmailAddress = "john@test.com"; Name = "John Johnson" } }
```

### Example 2

```fsharp
let result : ValueTask<Account voption> =
    ValueTaskValueOption.valueSome "jerry@test.com" // ValueTask<string voption>
    |> ValueTaskValueOption.bind lookupAccountByEmail // ValueTask<Account voption>

// valueTask { ValueNone }
```

### Example 3

```fsharp
let result : ValueTask<Account voption> =
    ValueTask<_>(ValueNone) // ValueTask<string voption>
    |> ValueTaskValueOption.bind lookupAccountByEmail // ValueTask<Account voption>

// valueTask { ValueNone }
```
