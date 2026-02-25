# Option.tryGetValue

Namespace: `FsToolkit.ErrorHandling`

`tryGetValue` tries to get the value associated with a key from a dictionary. Returns `Some value` if the key exists, or `None` if it does not.

> **Note:** This function is not available when using Fable (JavaScript/Python compilation).

## Function Signature

```fsharp
'key -> ^Dictionary -> ^value option
```

where `^Dictionary` has a member `TryGetValue: 'key * byref<^value> -> bool`.

## Examples

### Example 1 — Key exists

```fsharp
open System.Collections.Generic

let dict = Dictionary<string, int>()
dict["apples"] <- 5
dict["bananas"] <- 3

let result : int option =
    dict
    |> Option.tryGetValue "apples"

// Some 5
```

### Example 2 — Key does not exist

```fsharp
open System.Collections.Generic

let dict = Dictionary<string, int>()
dict["apples"] <- 5

let result : int option =
    dict
    |> Option.tryGetValue "bananas"

// None
```
