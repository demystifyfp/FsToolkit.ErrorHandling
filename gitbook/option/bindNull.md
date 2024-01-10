# Option.bindNull

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('T -> 'nullableValue) -> 'T option -> 'nullableValue option
```

## Examples


### Example 1

```fsharp
open System

let userInput = Some 12
let toNullable<'T> x = Nullable x

Option.bindNull toNullable userInput
// Some 12
```

### Example 2

```fsharp
open System

let userInput : Option<int> = None
let toNullable<'T> x = Nullable x

Option.bindNull toNullable userInput
// None
```

### Example 3

```fsharp
let userInput = Some 12
Option.bindNull string userInput
// Some "12"
```


