## Result Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples:

### Example 1

The example from [Result.map3](../result/map3.md#example-1) can be solved using the `result` computation expression as below:

```fsharp
// Result<int, string>
let addResult = result {
  let! x = tryParseInt "35"
  let! y = tryParseInt "5"
  let! z = tryParseInt "2"
  return add x y z
}
```

### Example 2

The example from [Result.map3](../result/map3.md#a-example-2) can be solved using the `result` computation expression as below:

```fsharp
// Result<CreatePostRequest,string>
let createPostRequestResult = result {
  let! lat = Latitude.TryCreate 13.067439
  let! lng = Longitude.TryCreate 80.237617
  let! tweet = Tweet.TryCreate "Hello, World!"
  return createPostRequest userId lat lng tweet
}
```

### Example 3

Given the following functions:

```fsharp
tryGetUser : string -> User option
isPwdValid : string -> User -> bool
authorize : User -> Result<unit, AuthError>
createAuthToken : User -> AuthToken
```

Here's how a simple login use-case can be written (using some helpers from the `Result` module):

```fsharp
type LoginError = InvalidUser | InvalidPwd | Unauthorized of AuthError

let login (username : string) (password : string) : Result<AuthToken, LoginError> =
  result {
    // requireSome unwraps a Some value or gives the specified error if None
    let! user = username |> tryGetUser |> Result.requireSome InvalidUser

    // requireTrue gives the specified error if false
    do! user |> isPwdValid password |> Result.requireTrue InvalidPwd

    // Error value is wrapped/transformed (Unauthorized has signature AuthError -> LoginError)
    do! user |> authorize |> Result.mapError Unauthorized

    return user |> createAuthToken
  }
```

