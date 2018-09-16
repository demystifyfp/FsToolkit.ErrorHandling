## Result.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Result<('a -> 'b), 'c> -> Result<'a, 'c> -> Result<'b, 'c>
```

## Examples:

### Example 1

Assume that we have a function to find the remaining characters of a [Tweet](../result/map3.md#tweet).

```fsharp
// Tweet -> int
let remainingCharacters (tweet : Tweet) =
  280 - tweet.Value.Length
```

If we want a function that takes a plain string and have to use the above function to compute the remaining character, we can achieve it using the `apply` function as below

```fsharp
// string -> Result<int,string>
let remainingCharacters2 (tweetStr : string) =
  Tweet.TryCreate tweet
  |> Result.apply (Ok remainingCharacters)
```

Alternatively, you can achieve the same using the Result's in-built `map` function like

```fsharp
// string -> Result<int,string>
let remainingCharacters2 (tweetStr : string) =
  Tweet.TryCreate tweet
  |> Result.map remainingCharacters
```

> We can get most out of the `apply` function when we use it over a multi-parameter function using its `infix` operator. FsToolkit.ErrorHandling provides this operator as well and the documentation for this can be found [here](TODO).