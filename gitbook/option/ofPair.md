# Option.ofPair

Namespace: `FsToolkit.ErrorHandling`

Transforms a `bool * 'T` value to `'T Option`.

## Function Signature

```fsharp
bool * 'T -> 'T Option
```

## Examples

### Example 1

```fsharp
let opt = Option.ofPair (true, 1) 
// Some 1
```

### Example 2

```fsharp
let opt = Option.ofPair (false, 1)
// None
```

### Example 3

Instead of using this code snippet,

```fsharp
match Int32.TryParse "12" with
| true, x -> x
| false, _ -> 0

// 12
```

you could use `Option.ofPair` if it better suits your use case

```fsharp
match Int32.TryParse "12" |> Option.ofPair with
| Some x -> x
| None -> 0

// 12
```
