## Result Computation Expression

Namespace: `FsToolkit.ErrorHandling.CE.Result`

## Examples:

### Basic Example

The example that we saw in the [Result.map3](../result/map3.md#basic-example) can be solved using the `result` computation expression as below

```fsharp
open FsToolkit.ErrorHandling.CE.Result

// Result<int, string>
let addResult = result {
  let! x = tryParseInt "35"
  let! y = tryParseInt "5"
  let! z = tryParseInt "2"
  return add x y z
} // returns - Ok 42
 
```

### A Real World Example

The example that we saw in the [Result.map3](../result/map3.md#a-real-world-example) can be solved using the `result` computation expression as below

```fsharp
// Result<CreatePostRequest,string>
let createPostRequestResult = result {
  let! lat = Latitude.TryCreate 13.067439
  let! lng = Longitude.TryCreate 80.237617
  let! tweet = Tweet.TryCreate "Hello, World!"
  return createPostRequest userId lat lng tweet
} 
```