# Result.ofChoice

Namespace: `FsToolkit.ErrorHandling`

Transforms F#'s `Choice` value to `Result`.

## Function Signature

```fsharp
Choice<'a,'b> -> Result<'a, 'b>
```

## Examples

### Example 1

```fsharp
Result.ofChoice (Choice1Of2 42)
// Ok 42

Result.ofChoice (Choice2Of2 "Something went wrong!") 
// Error "Something went wrong!"
```
