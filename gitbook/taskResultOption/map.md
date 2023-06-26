## TaskResultOption.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Task<Result<'a option, 'c>> 
  -> Task<Result<'b option, 'c>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `taskResultOption` computation expression](ce.md).

### Example 1

Given the function

```fsharp
getUserById : UserId -> Task<Result<User option, exn>>
```

Then using the `TaskResultOption.map` we can get the name of the user like this:

```fsharp
// Task<Result<PersonName option>, exn>
getUserById sampleUserId
|> TaskResultOption.map (fun user -> user.Name)
```

