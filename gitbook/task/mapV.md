# Task.map

Namespace: `FsToolkit.ErrorHandling`

Like [Task.map](../task/map.md), but taking a `ValueTask<'a>` as input

## Function Signature

```fsharp
('input-> 'output) -> ValueTask<'input> -> Task<'output>
```

## Examples

### Example 1

```fsharp
Task.mapV (fun x -> x + 1) (ValueTask.FromResult(1))

// task { 2 }
```
