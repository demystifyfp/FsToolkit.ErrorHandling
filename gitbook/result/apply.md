## Result.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<('a -> 'b), 'c> -> Result<'a, 'c> -> Result<'b, 'c>
```

## Examples

### Example 1

Note: The `apply` function is most useful in its infix operator form `<*>` when using it over a multi-parameter function. Examples of this is shown in [the documentation for this operator](../result/operators.md). The example below is a bit artificial since `map` is arguably better.

Assume that we have a function to find the remaining characters of a [Tweet](../result/map3.md#tweet):

```fsharp
// Tweet -> int
let remainingCharacters (tweet : Tweet) =
  280 - tweet.Value.Length
```

If we want a function that takes a plain string instead, we can achieve it using the `apply` function:

```fsharp
// string -> Result<int,string>
let remainingCharactersStr (tweetStr : string) =
  Tweet.TryCreate tweet
  |> Result.apply (Ok remainingCharacters)
```

But as mentioned, using the `map`  function is simpler in this case:

```fsharp
// string -> Result<int,string>
let remainingCharactersStr (tweetStr : string) =
  Tweet.TryCreate tweet
  |> Result.map remainingCharacters
```

