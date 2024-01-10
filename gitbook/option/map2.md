# Option.map2

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the values of two options if they are `Some`. If either option is `None`, return `None`.

## Function Signature

```fsharp
('TInput1 -> 'TInput2 -> 'TOutput) -> 'TInput1 option -> 'TInput2 option -> 'TOutput option
```

## Examples

### Example 1

```fsharp
Option.map2 (fun x y -> x + y) (Some 1) (Some 2)

// Some 3
```

### Example 2

```fsharp
Option.map2 (fun x y -> x + y) (Some 1) None

// None
```

### Example 3

```fsharp
Option.map2 (fun x y -> x + y) None (Some 2)

// None
```
