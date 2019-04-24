## ResultOption Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.ResultOption`

FsToolkit.ErrorHandling provides the standard infix operators for the `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) functions of the `Result<Option<_>,_>` type.

In addition to these, it also offers an another infix operator `<*^>` for usage with normal `Result` values (without an inner `Option`). It has the following function signature:

```
Result<('a -> 'b) option, 'c> -> Result<'a, 'c> 
  -> Result<'b option, 'c>
```

## Examples

### Example 1

Assume that we have the following function:

```fsharp
// int -> int -> int -> int
let add a b c = a + b + c
```

Then using the infix operators, we can do the following:

```fsharp
// Ok (Some 42)
let opResult : Result<int option, string> =
  add
  <!> (Ok (Some 30)) 
  <*> (Ok (Some 10)) 
  <*> (Ok (Some 2)) 
```

If we have values that are not `Option` wrapped, we can use the `<*^>` operator:

```fsharp
let opResult : Result<int option, string> =
  add
  <!> (Ok (Some 30)) 
  <*^> (Ok 10) 
  <*> (Ok (Some 2))
```

### Example 2

The [ResultOption.map2 example](../resultOption/map2.md#example-2) can be written using the infix operators as below:

```fsharp
let toCreatePostRequest (dto : CreatePostRequestDto) = 

  // Result<Location option, string>
  let locationR = 
    location
    <!> Option.traverseResult Latitude.TryCreate dto.Latitude
    <*> Option.traverseResult Longitude.TryCreate dto.Longitude

  // Result<Tweet, string>
  let tweetR = Tweet.TryCreate dto.Tweet

  // Result<CreatePostRequest, string>
  Result.map2 createPostRequest tweetR locationR
```
