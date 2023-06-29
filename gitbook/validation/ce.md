## Validation Computation Expression

Namespace: `FsToolkit.ErrorHandling`

The `Validation` type is defined as:

```fsharp
type Validation<'a,'err> = Result<'a, 'err list>
```

This CE can take advantage of the [and! operator](https://github.com/fsharp/fslang-suggestions/issues/579) to join multiple error results into a list.

## Examples:

```fsharp
// string -> Result<int, string>
let tryParseInt str =
  match System.Int32.TryParse str with
  | true, x -> Ok x
  | false, _ ->
    Error (sprintf "unable to parse '%s' to integer" str)
```

### Example 1

The example from [Validation.map3](../validation/map3.md#example-1) can be solved using the `validation` computation expression as below:

```fsharp
// Validation<int, string>
let addResult = validation {
  let! x = tryParseInt "35"
  and! y = tryParseInt "5"
  and! z = tryParseInt "2"
  return add x y z
}
// Ok 42
```

### Validation "Gotchas"

If you place any 'let!' after your 'and!'s, you will lose out on your error joining

```fsharp
// Validation<int, string>
let addResult = validation {
    let! x = tryParseInt "1"
    and! y = tryParseInt "str1"
    let! z = tryParseInt "str2"
    return x + y + z
}
// Error ["unable to parse 'str1' to integer"]
```

### Combining CE's
Sometimes you need to break apart your computational expressions for readability. You can still join error results from separate validation expressions.

```fsharp
// use existing code found above

// Validation<int, string>
let addResult1 = validation {
    let! x = tryParseInt "1"
    and! y = tryParseInt "str1"
    return x + y
}
// Error ["unable to parse 'str1' to integer"]

// Validation<int, string>
let addResult2 = validation {
    let! x = tryParseInt "1"
    and! y = tryParseInt "str2"
    return x + y
}
// Error ["unable to parse 'str2' to integer"]

let combinedResult = 
    validation {
        let! x = addResult1
        and! y = addResult2
        return x + y
    }
    
// Error ["unable to parse 'str1' to integer"
//        "unable to parse 'str2' to integer"]
```
