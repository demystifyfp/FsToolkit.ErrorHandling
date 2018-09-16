## ResultOption Infix Operators

FsToolkit.ErrorHandling provides the standard infix operators for the map (`<!>`), apply (`<*>`) & bind (`>>=`) functions of `ResultOption`.

Namespace: `FsToolkit.ErrorHandling.Operator.ResultOption`

In addition to these, it also offers an another infix operator `<*^>` which has the following function singature.

```
Result<('a -> 'b) option, 'c> -> Result<'a, 'c> 
  -> Result<'b option, 'c>
```

## Examples

### Example 1

Assume that we have the following function,

```fsharp
// int -> int -> int -> int
let add a b c = a + b + c
```

Then using `ResultOption` infix operators, we can achieve the following

```fsharp
// returns - Ok (Some 42)
let opResult : Result<int option, string> =
  add
  <!> (Ok (Some 30)) 
  <*> (Ok (Some 10)) 
  <*> (Ok (Some 2)) 
```

Using the `<*^>` operator, the above code snippet can be written as 

```fsharp
let opResult : Result<int option, string> =
  add3
  <!> (Ok (Some 30)) 
  <*^> (Ok 10) 
  <*^> (Ok 2)
```

### Example 2

The [ResultOption.map2 example](../resultOption/map2.md#example-2) can be written using ResultOption infix operators as below

```fsharp
let toCreatePostRequest (dto :CreatePostRequestDto) = 

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
