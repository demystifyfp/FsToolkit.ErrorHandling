## Option.traverseResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Result<'b,'c>) -> 'a option -> Result<'b option, 'c>
```

Note that `traverse` is the same as `map >> sequence`. See also [Option.sequenceResult](sequenceResult.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

If we have a value of type `string option` and want to call the `tryParseInt` function that we defined in the [Result.map2 example](../result/map2.md#example-1), we can achieve it using the `traverseResult` function as below:

```fsharp
Some "42" |> Option.traverseResult tryParseInt
// Ok (Some 42)

None |> Option.traverseResult tryParseInt
// Ok None

Some "foo" |> Option.traverseResult tryParseInt
// Error "unable to parse 'foo' to integer"
```

### Example 2

The `CreatePostRequest` type that we defined in [Result.map3 example](../result/map3.md#createpostrequest) contains a `Location option`. The corresponding DTO objects would look like this:

```fsharp
type LocationDto = {
  Latitude : float
  Longitude : float
}

type CreatePostRequestDto = {
  Tweet : string
  Location : LocationDto option
}
```

Let's assume that we have this function to convert a `LocationDto` to a `Location`:

```fsharp
// LocationDto -> Result<Location, string>
let locationFromDto (dto : LocationDto) = result {
  let! lat = Latitude.TryCreate dto.Latitude
  let! lng = Longitude.TryCreate dto.Longitude
  return {Location.Latitude = lat; Longitude = lng}
}
```

Then in order to create a similar function to convert a `CreatePostRequestDto` to a `CreatePostRequest`, we can make use of `traverseResult` as below:

```fsharp
let createPostRequestFromDto (dto : CreatePostRequestDto) = result {
    // Parse the location DTO option to a Location option,
    // returning an error if it's Some and invalid
    let! location =
      dto.Location |> Option.traverseResult locationDtoToLocation

    let! tweet = Tweet.TryCreate dto.Tweet
    return {
      Tweet = tweet
      Location = location
    }
  }
```

See also the [example 2](../resultOption/map2.md#example-2) of ResultOption.map2.