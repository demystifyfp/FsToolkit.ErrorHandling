# Option.bindNull

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('T -> 'nullableValue) -> 'T option -> 'nullableValue option
```

## Examples

Take the following function for example

```fsharp
// string -> string
let addWorld (s: Nullable<string>) = 
    s + " world"
```

```
