## ResultOption.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> Result<'a option, 'd>
  -> Result<'b option, 'd>
  -> Result<'c option, 'd>
```

### Example 1

Given the following function:

```fsharp
let add : int -> int -> int
```

Then using `ResultOption.map2`, we can do the following:

```fsharp
ResultOption.map2 add (Ok (Some 40)) (Ok (Some 2)) 
// Ok (Some 42)
```

### Example 2

Let's assume that we have the following types and functions in addition to what we defined in the [Result.map2 example](../result/map2.md#example-2):

```fsharp
type CreatePostRequest = {
  Tweet : Tweet
  Location : Location option
}

// Tweet -> Location option -> CreatePostRequest
let createPostRequest tweet location =
  {Tweet = tweet; Location = location}

type CreatePostRequestDto = {
  Tweet : string
  Latitude : float option
  Longitude : float option
}
```

We can then create a function transforming a `CreatePostRequestDto` to a `CreatePostRequest`, using `Option.traverseResult`, `ResultOption.map2`, and `Result.map2`:


```fsharp
// CreatePostRequestDto -> Result<CreatePostRequest, string>
let toCreatePostRequest (dto : CreatePostRequestDto) = 

  // Result<Latitude option, string>
  let latR =
    dto.Latitude
    |> Option.traverseResult Latitude.TryCreate

  // Result<Longitude option, string>
  let lngR =
    dto.Longitude
    |> Option.traverseResult Longitude.TryCreate

  // Result<Location option, string>
  let locationR =
    ResultOption.map2 location latR lngR

  // Result<Tweet, string>
  let tweetR = Tweet.TryCreate dto.Tweet

  // Result<CreatePostRequest, string>
  Result.map2 createPostRequest tweetR locationR
```

Note that this example can also be written using the `result` and `resultOption` computation expressions, which would allow you to skip the `map2` functions. See for example the [resultOption CE example](../result/ce.md#example-2).