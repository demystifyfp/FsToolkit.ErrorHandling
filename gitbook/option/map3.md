# Option.map3

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the values of three options if they are `Some`. If any option is `None`, return `None`.

## Function Signature

```fsharp
('TInput1 -> 'TInput2 -> 'TInput3 -> 'TOutput) -> 'TInput1 option -> 'TInput2 option -> 'TInput3 option -> 'TOutput option
```

## Examples

### Example 1

```fsharp
Option.map3 (fun x y z -> x + y + z) (Some 1) (Some 2) (Some 3)

// Some 6
```

### Example 2

```fsharp
Option.map3 (fun x y z -> x + y + z) (Some 1) (Some 2) None

// None
```

### Example 3

```fsharp
Option.map3 (fun x y z -> x + y + z) (Some 1) None (Some 3)

// None
```

### Example 4

```fsharp
Option.map3 (fun x y z -> x + y + z) None (Some 2) (Some 3)

// None
```
