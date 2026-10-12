# TaskValueOption.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two voptions and returns a tuple of the pair or ValueNone if either are ValueNone

## Function Signature

```fsharp
Task<'left voption> -> Task<'right voption> -> Task<('left * 'right) voption>
```

## Examples

### Example 1

```fsharp
let left = TaskValueOption.some 123
let right = TaskValueOption.some "abc"

TaskValueOption.zip left right
// task { ValueSome (123, "abc") }
```

### Example 2

```fsharp
let left = TaskValueOption.some 123
let right = TaskValueOption.none

TaskValueOption.zip left right
// task { ValueNone }
```
