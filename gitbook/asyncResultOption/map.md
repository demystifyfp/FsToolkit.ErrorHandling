## AsyncResultOption.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b) -> Async<Result<'a option, 'c>> 
  -> Async<Result<'b option, 'c>>
```

## Examples

### Example 1

```fsharp
// UserId -> Async<Result<User option, Exception>>
let getUserById (userId : UserId) = async {
  // ...
}
```

Using the `AsyncResultOption.map` we can achieve the following, to get the name of the user

```fsharp
// Async<Result<PersonName option>, Exception>
getUserById sampleUserId
|> AsyncResultOption.map (fun user -> user.Name)
```