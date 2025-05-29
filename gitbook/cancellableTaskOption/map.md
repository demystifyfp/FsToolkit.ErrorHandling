# CancellableTaskOption.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the value of a cancellable task option if it is `Some`. If the option is `None`, return `None`.

## Function Signature

```fsharp
('input -> 'output) -> CancellableTask<'input option> -> CancellableTask<'output option>
```

## Examples

### Example 1

```fsharp
CancellableTaskOption.map (fun x -> x + 1) (CancellableTaskOption.some 1)

// cancellableTask { Some 2 }
```

### Example 2

```fsharp
CancellableTaskOption.map (fun x -> x + 1) (CancellableTask.singleton None)

// cancellableTask { None }
```

