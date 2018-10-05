## Option.traverseResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Result<'b,'c>) -> 'a option -> Result<'b option, 'c>
```

## Examples

### Example 1

If we have a value of type `string option` and wants to call the `tryParseInt` function that we defined in the [Result.map2 example](../result/map2.md#example-1), we can acheive it using the `traverseResult` function as below

```fsharp
// returns - Ok (Some 42)
Option.traverseResult tryParseInt (Some "42") 

// returns - Ok None
Option.traverseResult tryParseInt None

// returns - Error "unable to parse 'foo' to integer"
Option.traverseResult tryParseInt (Some "foo") 
```

### Example 2

The `CreatePostRequest` type that we defined in [Result.map3 example](../result/map3.md#createpostrequest) takes `Location` as option. 

The corresponding DTO object would look like 

#### CreatePostRequestDto

```fsharp
type LocationDto = {
  Latitude : double
  Longitude : double
}

type CreatePostRequestDto = {
  Tweet : string
  Location : LocationDto option
}
```

Let's assume that we have a function `locationDtoToLocation`

```fsharp
open FsToolkit.ErrorHandling.CE.Result

// LocationDto -> Result<Location, string>
let locationDtoToLocation (dto : LocationDto) = result {
  let! lat = Latitude.TryCreate dto.Latitude
  let! lng = Longitude.TryCreate dto.Longitude
  return {Location.Latitude = lat; Longitude = lng}
}
```

Then to define a static member function `ToCreatePostRequest`, we can make use of `traverseResult` function as below

```fsharp
type CreatePostRequestDto = {
  ...
} with 
  static member ToCreatePostRequest(dto : CreatePostRequestDto) = result {
    // Location option
    let! location =
      Option.traverseResult locationDtoToLocation dto.Location

    let! tweet = Tweet.TryCreate dto.Tweet
    return {
      Tweet = tweet
      Location = location
    }
  }
```

See also the [example 2](../resultOption/map2.md#example-2) of ResultOption.map2.