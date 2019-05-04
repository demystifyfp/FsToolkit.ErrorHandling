## AsyncResultOption.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Async<Result<'a option, 'c>> 
  -> Async<Result<'b option, 'c>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `asyncResultOption` computation expression](ce.md).

### Example 1

Given the function

```fsharp
getUserById : UserId -> Async<Result<User option, exn>>
```

Then using the `AsyncResultOption.map` we can get the name of the user like this:

```fsharp
// Async<Result<PersonName option>, exn>
getUserById sampleUserId
|> AsyncResultOption.map (fun user -> user.Name)
```

