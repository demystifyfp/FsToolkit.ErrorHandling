## Validation Infix Operators

FsToolkit.ErrorHandling provides the standard infix operators for the map (`<!>`), apply (`<*>`) & bind (`>>=`) functions to work with `Result<a, 'b list>`.

It also provide a varaint (`<!^>`, `<*^>`) on these operators to make it seamlessly work with `Result<'a, 'b>` type. 

Refer the examples below to understand how it works.

Namespace: `FsToolkit.ErrorHandling.Operator.Validation`

## Examples

### Example 1

Assume that we have below types and `createPostRequest` function

```fsharp
type Latitude = private Latitude of double with
  // ...
  // double -> Result<Latitude, string list>
  static member TryCreate (lat : double) =
    // ...

type Longitude = private Longitude of double with
  // ...
  // double -> Result<Longitude, string list>
  static member TryCreate (lng : double) =
    // ...

type Tweet = private Tweet of string with
  // ...
  // string -> Result<Tweet, string list>
  static member TryCreate (tweet : string) =
    // ...

// Latitude -> Longitude -> Tweet -> CreatePostRequest
let createPostRequest lat long tweet =
  // ...
```

We can make use of the standard operators in the Validation Operators module to perform the validation of the incoming request and capture all the errors as below

```fsharp
open FsToolkit.ErrorHandling.Operator.Validation

// double -> double -> string -> Result<CreatePostRequest, string list>
let validateCreatePostRequest lat lng tweet = 
  createPostRequest
  <!> Latitude.TryCreate lat
  <*> Longitude.TryCreate lng
  <*> Tweet.TryCreat tweet
```

If we call the `validateCreatePostRequest` function we can all the errors in the single shot. 
```
> validateCreatePostRequest 300. 400. ""

Error
    ["300.0 is a invalid latitude value"
     "400.0 is a invalid longitude value"
     "Tweet shouldn't be empty"]
```

### Example 2

In the above Example 1, all the `TryCreate` functions return a string list for error (`Result<'a, string list>`). If in case, these functions return a `Result<'a, string>` we can make use of the other operators (`<*^>` & `<!^>`) operators to achieve what we did there. 


```fsharp
type Latitude = private Latitude of double with
  // ...
  // double -> Result<Latitude, string>
  static member TryCreate (lat : double) =
    // ...

type Longitude = private Longitude of double with
  // ...
  // double -> Result<Longitude, string>
  static member TryCreate (lng : double) =
    // ...

type Tweet = private Tweet of string with
  // ...
  // string -> Result<Tweet, string>
  static member TryCreate (tweet : string) =
    // ...
```


```fsharp
open FsToolkit.ErrorHandling.Operator.Validation

// double -> double -> string -> Result<CreatePostRequest, string list>
let validateCreatePostRequest lat lng tweet = 
  createPostRequest
  <!^> Latitude.TryCreate lat
  <*^> Longitude.TryCreate lng
  <*^> Tweet.TryCreat tweet
```