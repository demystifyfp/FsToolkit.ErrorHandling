# List.sequenceOptionM

Namespace: `FsToolkit.ErrorHandling`

Applies the monadic function `id` to each element in the input list, and returns the result as an option. If any element in the list is None, the entire result will be None.

## Function Signature

```fsharp
'a option list -> 'a list option
```

## Examples

### Example 1

```fsharp
let myList =
    [
        Some 123
        Some 456
        Some 789
    ]

List.sequenceOptionM myList
// Some [123; 456; 789]
```

### Example 2

```fsharp
let myList =
    [
        Some 123
        None
        Some 789
    ]

List.sequenceOptionM myList
// None
```
