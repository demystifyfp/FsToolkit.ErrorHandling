## Result.ofChoice

Transforms F#'s `Choice` value to `Result`.

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Choice<'a,'b> -> Result<'a, 'b>
```

## Examples:

### Basic Example

```fsharp
Result.ofChoice (Choice1Of2 42) // returns - Ok 42
```

```fsharp
Result.ofChoice (Choice2Of2 "Something went wrong!") 
// returns - Error "Something went wrong!"
```