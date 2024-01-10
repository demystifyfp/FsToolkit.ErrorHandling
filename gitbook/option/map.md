# Option.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the value of an option if it is `Some`. If the option is `None`, return `None`.

## Function Signature

```fsharp
('TInput-> 'TOutput) -> 'TInput option -> 'TOutput option
```

## Examples

### Example 1

```fsharp
Option.map (fun x -> x + 1) (Some 1)

// Some 2
```

### Example 2

```fsharp
Option.map (fun x -> x + 1) None

// None
```

