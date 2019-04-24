## ResultOption.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```F#
('a -> 'b) -> Result<'a option, 'c> -> Result<'b option, 'c>
```

`ResultOption.map` is the same as `Result.map Option.map`.

### Example 1

Given the following functions:

```f#
getTweet : PostId -> Result<Tweet option, _>
remainingCharacters : Tweet -> int
```

You can get the number of remaining characters as below:

```fsharp
getTweetByPostId somePostId
|> ResultOption.map remainingCharacters
```

