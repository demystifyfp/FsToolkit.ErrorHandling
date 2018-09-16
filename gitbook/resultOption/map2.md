## ResultOption.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c) -> Result<'a option, 'd> -> Result<'b option, 'd> 
  -> Result<'c option, 'd>
```

### Example 1

Assume that we have the following function,

```fsharp
// int -> int -> int
let add a b = a + b
```

Then using `ResultOption.map2` function, we can achieve the following

```fsharp
// returns - Ok (Some 42)
ResultOption.map2 add (Ok (Some 40)) (Ok (Some 2)) 
```

### Example 2

Let's assume that we have the following types and function in addition to what we defined in the [Result.map2 example](../result/map2.md#example-2)

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
  Latitude : double option
  Longitude : double option
}
```

To transform the `CreatePostRequestDto` to `CreatePostRequest` with validation, we can do the following using the `Option.traverseResult`, `ResultOption.map2` & `Result.map2`.


```fsharp
// CreatePostRequestDto -> Result<CreatePostRequest, string>
let toCreatePostRequest (dto :CreatePostRequestDto) = 

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