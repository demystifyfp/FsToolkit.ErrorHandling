## TaskValidation Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.TaskValidation`

FsToolkit.ErrorHandling provides the standard infix operators for `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) to work with `Result<'a, 'b list>`.

There are also variants of the `map` and `apply` operators  (`<!^>` and `<*^>`) that accept `Result<'a, 'b>` (non-list) as the right-hand argument.

## Examples

### Example 1

Assume that we have following types and functions:

```fsharp
type Latitude = private Latitude of float with
  // float -> Task<Result<Latitude, string list>>
  static member TryCreate (lat : float) =
    // ...

type Longitude = private Longitude of float with
  // float -> Task<Result<Longitude, string list>>
  static member TryCreate (lng : float) =
    // ...

type Tweet = private Tweet of string with
  // string -> Task<Result<Tweet, string list>>
  static member TryCreate (tweet : string) =
    // ...

// Latitude -> Longitude -> Tweet -> CreatePostRequest
let createPostRequest lat long tweet =
  // ...
```

We can make use of the standard operators in the TaskValidation Operators module to perform the taskValidation of the incoming request and capture all the errors as shown below:

```fsharp
open FsToolkit.ErrorHandling.Operator.TaskValidation

// float -> float -> string -> Task<Result<CreatePostRequest, string list>>
let validateCreatePostRequest lat lng tweet = 
  createPostRequest
  <!> Latitude.TryCreate lat
  <*> Longitude.TryCreate lng
  <*> Tweet.TryCreate tweet
```

By using the `TaskValidation` operators instead of the `Result` operators, we collect all the errors:
```fsharp
validateCreatePostRequest 300. 400. ""
// Error
     ["300.0 is a invalid latitude value"
      "400.0 is a invalid longitude value"
      "Tweet shouldn't be empty"]
```

### Example 2

In the above example, all the `TryCreate` functions return a string list as the error type (`Task<Result<'a, string list>>`). If these functions instead returned `Task<Result<'a, string>>` (only a single error), we can use `<*^>` and `<!^>` to get the same result:


```fsharp
open FsToolkit.ErrorHandling.Operator.TaskValidation

// float -> float -> string -> Task<Result<CreatePostRequest, string list>>
let validateCreatePostRequest lat lng tweet = 
  createPostRequest
  <!^> Latitude.TryCreate lat
  <*^> Longitude.TryCreate lng
  <*^> Tweet.TryCreate tweet
```
