## Option.sequenceResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Result<'a,'b> option -> Result<'a option, 'b>
```

## Examples

### Example 1

```fsharp
let r1 : Result<int option, string> = 
  Option.sequenceResult (Some (Ok 42))
// returns - Ok (Some 42)

let r2 : Result<int option, string> = 
  Option.sequenceResult (Some (Error "something went wrong"))
// returns - Error "something went wrong"

let r3 : Result<int option, string> = 
  Option.sequenceResult None
// returns - Ok None 
```