## JobOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<('a -> 'b) option> -> Job<'a option> -> Job<'b option>
```

## Examples

### Example 1

```fsharp
let f = Job.result (Some (fun x -> x + 1))
let x = Job.result (Some 41)

JobOption.apply f x
// job { return Some 42 }
```

### Example 2

```fsharp
let f = Job.result None
let x = Job.result (Some 41)

JobOption.apply f x
// job { return None }
```
