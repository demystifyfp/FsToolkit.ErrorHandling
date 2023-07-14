# Task.ofUnit

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task` into a `Task<unit>`

## Function Signature

```fsharp
Task -> Task<unit>
```

## Examples

### Example 1

```fsharp
let result = Task.ofUnit (task { return () })
// task { return () }
```
