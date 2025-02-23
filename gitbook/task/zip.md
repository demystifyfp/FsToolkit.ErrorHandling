# Task.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two tasks and returns a tuple of the pair

## Function Signature

```fsharp
Task<'left> -> Task<'right> -> Task<('left * 'right)>
```

## Examples

### Example 1

```fsharp
let left = Task.singleton 123
let right = Task.singleton "abc"

Task.zip left right
// task { (123, "abc") }
```

