## ResultOption.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b) -> Result<'a option, 'c> -> Result<'b option, 'c>
```

### Example 1

```fsharp
// Tweet -> int
let remainingCharacters (tweet : Tweet) =
  280 - tweet.Value.Length

// PostId -> Result<Tweet option, string>
let getTweetByPostId postId =
  Ok (Some tweet)


getTweetByPostId somePostId // Result<Tweet option, string>
|> ResultOption.map remainingCharacters // Result<int option, string>
```