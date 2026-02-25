# CancellableValueTaskOption.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the value of a cancellable value task option if it is `Some`. If the option is `None`, return `None`.

## Function Signature

```fsharp
('input -> 'output) -> CancellableValueTask<'input option> -> CancellableValueTask<'output option>
```

## Examples

### Example 1

```fsharp
CancellableValueTaskOption.map (fun x -> x + 1) (CancellableValueTaskOption.some 1)

// cancellableValueTask { Some 2 }
```

### Example 2

```fsharp
CancellableValueTaskOption.map (fun x -> x + 1) (CancellableValueTask.singleton None)

// cancellableValueTask { None }
```
