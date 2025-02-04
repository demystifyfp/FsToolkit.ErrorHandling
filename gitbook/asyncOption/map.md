# AsyncOption.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the value of an async option if it is `Some`. If the option is `None`, return `None`.

## Function Signature

```fsharp
('TInput -> 'TOutput) -> AsyncOption<'TInput> -> AsyncOption<'TOutput>
```

## Examples

### Example 1

```fsharp
AsyncOption.map (fun x -> x + 1) (AsyncOption.some 1)

// async { Some 2 }
```

### Example 2

```fsharp
AsyncOption.map (fun x -> x + 1) (Async.singleton None)

// async { None }
```

