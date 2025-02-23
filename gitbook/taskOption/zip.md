# TaskOption.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two options and returns a tuple of the pair or None if either are None

## Function Signature

```fsharp
Task<'left option> -> Task<'right option> -> Task<('left * 'right) option>
```

## Examples

### Example 1

```fsharp
let left = TaskOption.some 123
let right = TaskOption.some "abc"

TaskOption.zip left right
// task { Some (123, "abc") }
```

### Example 2

```fsharp
let left = TaskOption.some 123
let right = TaskOption.singleton None

TaskOption.zip left right
// task { None }
```
