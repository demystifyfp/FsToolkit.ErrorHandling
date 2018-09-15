## Result.fold

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b) -> ('c -> 'b) -> Result<'a, 'c> -> 'b
```

## Examples:

### Basic Example

Let's assume that we have a function `tryParseInt`

```fsharp
open System

// string -> Result<int, string>
let tryParseInt str =
  match Int32.TryParse str with
  | true, x -> Ok x
  | false, _ -> 
    Error (sprintf "unable to parse '%s' to integer" str)
```

If we want to return the actual value for success and a default value `0` in failure, we can achieve it as below

```fsharp
// string -> int
let tryParseIntOrDefault str =
  str
  |> tryParseInt
  |> Result.fold id (fun _ -> 0)

tryParseIntOrDefault "42" // returns - 12
tryParseIntOrDefault "foobar" // returns - 0
```

### A Real World Example

In a typical web application, if there is any request validation error, we send `HTTP 400 Bad Request` as response and `HTTP 200 OK` for successful operation

In the above `tryParseInt` example, if we emulate the same using a fake HTTP response type

```fsharp
type HttpResponse<'a, 'b> =
  | OK of 'a
  | BadRequest of 'b
```

Then using `Result.fold`, we can do the following

```fsharp
// HttpRequest -> HttpResponse<int,string>
let handler httpRequest =
  // reading the input from the HTTP request
  let inputStr = httpRequest ... 

  Result.fold OK BadRequest (tryParseInt inputStr)
```


