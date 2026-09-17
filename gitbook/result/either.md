# Result.either

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('a -> 'b) -> ('c -> 'b) 
    -> Result<'a, 'c> -> 'b
```

## Examples

### Example 1

`either` can be used to convert `Result` to another similar type, such as `Choice`:

```fsharp
let choice1 = Ok 42 |> Result.either Choice1Of2 Choice2Of2
// Choice1Of2 42

let choice2 = Error "An error occurred" |> Result.either Choice1Of2 Choice2Of2
// Choice2Of2 "An error occurred"
```

### Example 2

In a typical web application, if there is any request validation error, we send `HTTP 400 Bad Request` as response and `HTTP 200 OK` for a successful operation.

Given the following function:

```fsharp
// string -> Result<int, string>
let tryParseInt str =
    match str |> Option.tryParse<int> with
    | None -> Error $"unable to parse '{str}' to integer"
    | Some x -> Ok x
```

And the following fake HTTP response type:

```fsharp
type HttpResponse<'a, 'b> =
    | Ok of 'a
    | BadRequest of 'b
```

Then using `Result.either`, we can do the following

```fsharp
// HttpRequest -> HttpResponse<int,string>
let handler httpRequest =
    // reading the input from the HTTP request
    let inputStr = httpRequest ... 
    inputStr |> tryParseInt |> Result.either Ok BadRequest
```
