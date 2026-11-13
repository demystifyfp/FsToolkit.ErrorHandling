# Task.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the value of a task.

## Function Signature

```fsharp
('input-> 'output) -> Task<'input> -> Task<'output>
```

## Examples

### Example 1

```fsharp
Task.map (fun x -> x + 1) (Task.result 1)

// task { 2 }
```


